using System;
using System.Threading;
using System.Threading.Tasks;
using Codes.OutGame.PickCharacter.Dto;
using Cysharp.Threading.Tasks;
using NativeWebSocket;
using NetCode;
using NetTest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins;
using UnityEngine;

namespace Codes.OutGame.Match
{
    public class MatchingWsManager:MonoSingleton<MatchingWsManager>
    {
        
        private GameMode currentGameMode;
        
        public event Action OnMatchCanceled;

        public event Action OnMatchEnqueueLoadingStarted;

        public event Action<MatchFoundDto> OnMatchFound;
        public event Action OnMatchMakingStarted;

        public event Action<EnsureMatchEnqueueDto> OnEnsureMatchEnqueue;
        
        public event Action<CharacterPickNotifyDto> OnCharacterPickTemporaryNotify;
        public event Action<CharacterPickNotifyDto> OnCharacterPickNotify;

        public event Action OnTimeout;
        
        private WebSocket ws = null;
        private CancellationTokenSource keepAliveCts;//keep alive loop 끊기 위한 토큰
        
        public MatchingWebsocketState matchingState = MatchingWebsocketState.Close;
        protected override void Initialize()
        {
            return;
        }

        private void Update()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            if(ws!=null)
                ws.DispatchMessageQueue();
            #endif
        }

        //매칭 화면에 들어올때마다 OutGameMatchController가 한번씩 실행
        public void PrepareToNewMatch()
        {
            if (ws != null)
            {
                ws.OnOpen -= WsOpenHandler;
                ws.OnMessage -= WsMessageHandler;
                ws.OnClose -= WsCloseHandler;
                ws.OnError -= WsErrorHandler;

                _ = ws.Close();
                ws = null;
            }
        }

        public void ChangeGameMode(GameMode newGameMode)
        {
            currentGameMode = newGameMode;
        }

        public async UniTask ClickCharacter(string sessionId, string sessionUserKey, string characterId)
        {
            if (ws == null) return;
            var selectDto = new TryCharacterPickDto {sessionId = sessionId, sessionUserKey = sessionUserKey, characterId = characterId};
            var body = JsonConvert.SerializeObject(WsEventDto.SelectCharacterTemporary(selectDto));
            await ws.SendText(body).AsUniTask();
        }

        public async UniTask Match(int timeout)
        {
            bool isValidToken = await RequestClient.Instance.ValidateToken( TokenHolder.instance.GetJwt());
            if (!isValidToken)
            {
                bool successRefresh = await RequestClient.Instance.RefreshJwt();
                if (!successRefresh) {
                    OnMatchCanceled?.Invoke();
                return;
                }
            }
            OnMatchEnqueueLoadingStarted?.Invoke();
            
            matchingState = MatchingWebsocketState.Connecting;
            
            ws = RequestClient.Instance.GetMatchWebsocket(TokenHolder.instance.GetJwt());
            ws.OnOpen += WsOpenHandler;
            ws.OnMessage += WsMessageHandler;
            ws.OnClose += WsCloseHandler;
            ws.OnError += WsErrorHandler;
            
            var success = await ConnectWithTimeout(ws,timeout);
            Debug.LogWarning(success);
            if(!success) OnMatchCanceled?.Invoke();
            else
            {
                try
                {
                    matchingState = MatchingWebsocketState.Wait;
                    Debug.Log("Matching Websocket Open!");
                    await ws.SendText(JsonConvert.SerializeObject(WsEventDto.EnqueueMatch(currentGameMode))).AsUniTask();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void Cancel()
        {
            try
            {
                if (ws is { State: WebSocketState.Open })
                {
                    _ = ws.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"웹소켓 닫기 실패: {e}");
            }
        }

        private async UniTask<bool> ConnectWithTimeout(NativeWebSocket.WebSocket ws, int timeout)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            
            var cts = new CancellationTokenSource();
            var timeoutTask = UniTask.Delay(timeout*1000, cancellationToken: cts.Token);
            ws.OnOpen += () => tcs.TrySetResult(true);
            ws.OnError += (e) => Debug.LogError(e);
            _ = ws.Connect();
    
            var finished = await UniTask.WhenAny(tcs.Task, timeoutTask);
            if (!finished.hasResultLeft)
            {
                Debug.LogError("WebSocket connect timed out!");
                try { await ws.Close().AsUniTask(); } catch(Exception e) { Debug.LogException(e); }
                return false;
            }
            else
            {
                cts.Cancel();
                Debug.Log("WebSocket Connect Success!");
                
                keepAliveCts = new CancellationTokenSource();
                var o = KeepAliveLoop(ws, 5, "Match Ws Server",keepAliveCts.Token);
                return true;
            }

            
            return true;
        }

        private void WsMessageHandler(byte[] bytes)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + message + ")");
            WsEventDto dto = JsonConvert.DeserializeObject<WsEventDto>(message);
            var jToken = dto.Message as JToken ?? JToken.FromObject(dto.Message);
            try
            {
                switch (dto.Type)
                {
                    case WsEventType.Pong:
                    {
                        Debug.Log("Pong");
                        break;
                    }
                    case WsEventType.MatchFound:
                    {
                        var matchFoundDto = jToken.ToObject<MatchFoundDto>();
                        OnMatchFound?.Invoke(matchFoundDto);
                        break;
                    }
                    case WsEventType.EnsureEnqueueMatch:
                    {
                        var ensureEnqueueDto = jToken.ToObject<EnsureMatchEnqueueDto>();
                        OnMatchMakingStarted?.Invoke();
                        OnEnsureMatchEnqueue?.Invoke(ensureEnqueueDto);
                        break;
                    }
                    case WsEventType.NotifyCharacterPicked:
                    {
                        var notifyDto = jToken.ToObject<CharacterPickNotifyDto>();
                        OnCharacterPickNotify?.Invoke(notifyDto);
                        break;
                    }
                    case WsEventType.NotifyCharacterChanged:
                    {
                        var notifyDto = jToken.ToObject<CharacterPickNotifyDto>();
                        OnCharacterPickTemporaryNotify?.Invoke(notifyDto);
                        break;
                    }
                    case WsEventType.PickCharacterFailed:
                    {
                        //todo
                        break;
                    }
                    case WsEventType.PickCharacterSuccess:
                    {
                        //todo
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            
        }

        private async void WsCloseHandler(WebSocketCloseCode closeCode)
        {
            Debug.LogWarning("Match Websocket Closed!  close code: " + closeCode);
            ws.OnOpen -= WsOpenHandler;
            ws.OnMessage -= WsMessageHandler;
            ws.OnError -= WsErrorHandler;
            ws.OnClose -= WsCloseHandler;
            try
            {
                switch (closeCode)
                {
                    case WebSocketCloseCode.Normal:
                        OnMatchCanceled?.Invoke();
                        break;
                    case WebSocketCloseCode.PolicyViolation:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                OnMatchCanceled?.Invoke();
            }
            
        }

        private void WsErrorHandler(string error)
        {
            Debug.LogError("[MatchWebSocket] : "+error);
        }

        private async void WsOpenHandler()
        {
            
        }
        
        async UniTask KeepAliveLoop(WebSocket websocket, int intervalSeconds, string indicator,CancellationToken token) {
            while (websocket.State == WebSocketState.Open &&  !token.IsCancellationRequested) {
                try
                {
                    Debug.Log($"Send Ping to {indicator}");
                    await websocket.SendText(JsonConvert.SerializeObject(WsEventDto.Ping())).AsUniTask();
                } catch (Exception e) {
                    Debug.LogError($"Ping failed to:{indicator} " + e);
                }
                
                try {
                    await UniTask.Delay(intervalSeconds * 1000, cancellationToken: token);
                } catch (TaskCanceledException) {
                    break;
                }
            }
        }

        protected override async void OnDestroy()
        {
            try
            {
                keepAliveCts?.Cancel();
                if(ws.State == WebSocketState.Open)
                    await ws.Close();   
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
