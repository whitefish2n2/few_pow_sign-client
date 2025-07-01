using System;
using System.Threading;
using System.Threading.Tasks;
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

        public event Action OnTimeout;
        
        private WebSocket ws = null;
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

        //매칭 화면에 들어올때마다 MatchingUiManager가 한번씩 실행
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

        public async Task Match(int timeout)
        {
            OnMatchEnqueueLoadingStarted?.Invoke();
            
            matchingState = MatchingWebsocketState.Connecting;
            
            ws = RequestClient.Instance.GetMatchWebsocket();
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
                    await ws.SendText(JsonConvert.SerializeObject(WsEventDto.EnqueueMatch(currentGameMode)));
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

        private async Task<bool> ConnectWithTimeout(NativeWebSocket.WebSocket ws, int timeout)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout*1000, cts.Token);
            ws.OnOpen += () => tcs.TrySetResult(true);
            ws.OnError += (e) => tcs.TrySetResult(false);

            _ = ws.Connect();
    
            var finished = await Task.WhenAny(tcs.Task, timeoutTask);

            if (finished == timeoutTask)
            {
                Debug.LogError("WebSocket connect timed out!");
                try { await ws.Close(); } catch(Exception e) { Debug.LogException(e); }
                return false;
            }
            if (tcs.Task.Result)
            {
                cts.Cancel();
                Debug.Log("WebSocket Connect Success!");
                _ = KeepAliveLoop(ws, 5, "Match Ws Server");
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
                        MatchMakeStatic.Instance.userWebsocketKey = ensureEnqueueDto.key;
                        OnMatchMakingStarted?.Invoke();
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
                        await RequestClient.Instance.RefreshJwt(s =>
                            {
                                TokenHolder.instance.SetToken(s.data, TokenHolder.instance.GetRefreshToken());
                                OnMatchCanceled?.Invoke();
                            },
                            (e) =>
                            {
                                Debug.LogError("[MatchingManager.cs] : [Error While Refreshing Token]");
                                ClientMonoStatic.Instance.HandleCriticalOrShouldLoginError(e);
                            },
                            () => { OnTimeout?.Invoke(); }
                        );
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
        
        async Task KeepAliveLoop(WebSocket websocket, int intervalSeconds, string indicator) {
            while (websocket.State == WebSocketState.Open) {
                try
                {
                    Debug.Log($"Send Ping to {indicator}");
                    await websocket.SendText(JsonConvert.SerializeObject(WsEventDto.Ping()));
                } catch (Exception e) {
                    Debug.LogError($"Ping failed to:{indicator} " + e);
                    break;
                }
                await Task.Delay(intervalSeconds * 1000);
            }
        }
    }
}
