using System;
using System.Collections.Generic;
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
    public class OutGameWsManager:MonoSingleton<OutGameWsManager>
    {
        
        private GameMode currentGameMode;
        
        public event Action OnJoinLobbySuccess;
        public event Action OnMatchCanceled;
        public event Action OnMatchEnqueueLoadingStarted;
        public event Action<MatchFoundDto> OnMatchFound;
        public event Action OnMatchMakingStarted;
        public event Action<EnsureMatchEnqueueDto> OnEnsureMatchEnqueue;
        public event Action<CharacterPickNotifyDto> OnCharacterPickTemporaryNotify;
        public event Action<CharacterPickNotifyDto> OnCharacterPickNotify;
        public event Action<List<AnotherPlayerInfoDto>> OnGotTeamPlayerInformation;
        public event Action<StartGameDto> OnStartGame;
        public event Action OnTimeout;
        

        public event Action<String> OnForcedLogout;

        public event Action<String> OnConnectionFatalError;
        
        private WebSocket ws = null;
        private CancellationTokenSource keepAliveCts;//keep alive loop 끊기 위한 토큰
        
        public MatchingWebsocketState socketState = MatchingWebsocketState.Close;
        private bool isReconnecting = false;//재연결용 플래그
        protected override void Initialize()
        {
            return;
        }

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            #if !UNITY_WEBGL
            if(ws!=null)
                ws.DispatchMessageQueue();
            #endif
        }

        //매칭 화면에 들어올때마다 OutGameMatchController가 한번씩 실행
        public void PrepareToNewMatch()
        {
            DisposeCurrentWebSocket();
            if(ws == null)
                _ = JoinLobby(15);
        }
        
        private void DisposeCurrentWebSocket()
        {
            keepAliveCts?.Cancel();
            keepAliveCts?.Dispose();
            keepAliveCts = null;

            if (ws != null)
            {
                ws.OnOpen -= WsOpenHandler;
                ws.OnMessage -= WsMessageHandler;
                ws.OnClose -= WsCloseHandler;
                ws.OnError -= WsErrorHandler;

                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting)
                {
                    _ = ws.Close();
                }
                ws = null;
            }
            socketState = MatchingWebsocketState.Close;
        }

        public void ChangeGameMode(GameMode newGameMode)
        {
            currentGameMode = newGameMode;
        }

        public async UniTask ClickCharacter(string characterId)
        {
            if (ws == null) return;
            var id = CharacterInfoLoader.Instance.CharacterIdToId(characterId);
            var selectDto = new TryCharacterPickDto { characterId = id.ToString() };
            var body = JsonConvert.SerializeObject(WsEventDto.SelectCharacterTemporary(selectDto));
            await ws.SendText(body).AsUniTask();
        }

        public async UniTask LockInCharacter(string characterId)
        {
            if (ws == null) return;
            var id = CharacterInfoLoader.Instance.CharacterIdToId(characterId);
            var selectDto = new TryCharacterPickDto { characterId = id.ToString() };
            var body = JsonConvert.SerializeObject(WsEventDto.LockInCharacter(selectDto));
            await ws.SendText(body).AsUniTask();
        }

        public async UniTask<bool> JoinLobby(int timeout)
        {
            bool isValidToken = await RequestClient.Instance.ValidateToken( TokenHolder.instance.GetJwt());
            if (!isValidToken)
            {
                bool successRefresh = await RequestClient.Instance.RefreshJwt();
                if (!successRefresh) {
                    return false;
                }
            }
            socketState = MatchingWebsocketState.Connecting;
            DisposeCurrentWebSocket();
            ws = RequestClient.Instance.GetMatchWebsocket(TokenHolder.instance.GetJwt());
            
            ws.OnOpen -= WsOpenHandler;
            ws.OnMessage -= WsMessageHandler;
            ws.OnClose -= WsCloseHandler;
            ws.OnError -= WsErrorHandler;

            ws.OnOpen += WsOpenHandler;
            ws.OnMessage += WsMessageHandler;
            ws.OnClose += WsCloseHandler;
            ws.OnError += WsErrorHandler;
            
            var success = await ConnectWithTimeout(ws,timeout);
            Debug.LogWarning(success);

            if (!isReconnecting)//비정상 종료 후 재연결하는 루트에서는 제공하지 않음
            {
                //연결 실패시 재시도 제공
                if(!success){ AlertManager.Instance.AlertRetryableError(ErrorResponse.ServerTimeout,
                        () => { _ = JoinLobby(15).AsUniTask();} );
                    return false;
                }
            }
            
            

            try
            {
                socketState = MatchingWebsocketState.Lobby;
                Debug.Log("Websocket Open!");
                OnJoinLobbySuccess?.Invoke();
                await ws.SendText(JsonConvert.SerializeObject(WsEventDto.JoinLobby())).AsUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return true;


        }
        public async UniTask Match(int timeout)
        {
            
            if (ws == null || ws.State != WebSocketState.Open)
            {
                Debug.LogWarning("웹소켓이 아직 연결되지 않았습니다. 매칭을 시작할 수 없습니다.");
                return; 
            }
            bool isValidToken = await RequestClient.Instance.ValidateToken( TokenHolder.instance.GetJwt());
            if (!isValidToken)
            {
                bool successRefresh = await RequestClient.Instance.RefreshJwt();
                if (!successRefresh) {
                    OnTimeout?.Invoke();
                    return;
                }
            }
            try
            {
                await ws.SendText(JsonConvert.SerializeObject(WsEventDto.EnqueueMatch(currentGameMode))).AsUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            OnMatchEnqueueLoadingStarted?.Invoke();
        }

        public async UniTask Cancel()
        {
            try
            {
                await ws.SendText(JsonConvert.SerializeObject(WsEventDto.CancelMatch())).AsUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError($"게임 캔슬 요청 실패: {e}");
            }
        }

        private async UniTask<bool> ConnectWithTimeout(WebSocket ws, int timeout)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();
            
            ///로컬 핸들러
            void OnOpen() => tcs.TrySetResult(true);
            void OnClose(WebSocketCloseCode code) => tcs.TrySetResult(false);
            void OnError(string errMsg)
            {
                tcs.TrySetResult(false);
                Debug.LogError(errMsg);
            }

            ws.OnOpen += OnOpen;
            ws.OnClose += OnClose;
            ws.OnError += OnError;
            
            var timeoutTask = UniTask.Delay(timeout*1000, cancellationToken: cts.Token);
            
            _ = ws.Connect();
    
            var finished = await UniTask.WhenAny(tcs.Task, timeoutTask);
            
            // 핸들러 정리
            ws.OnOpen -= OnOpen;
            ws.OnClose -= OnClose;
            ws.OnError -= OnError;
            
            if (!finished.hasResultLeft)
            {
                Debug.LogError("WebSocket connect timed out!");
                try { await ws.Close().AsUniTask(); } catch(Exception e) { Debug.LogException(e); }
                OnTimeout?.Invoke();
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
                        Debug.Log("Triggered: Pong");
                        break;
                    }
                    case WsEventType.StartMatch:
                    {
                        Debug.Log("Triggered: StartMatch");
                        var startGameDto = jToken.ToObject<StartGameDto>();
                        if (startGameDto.players != null)
                            foreach (var p in startGameDto.players) p.characterId = ResolveCharacterId(p.characterId);
                        OnStartGame?.Invoke(startGameDto);
                        break;
                    }
                    case WsEventType.EnsureEnqueueMatch:
                    {
                        Debug.Log("Triggered: EnsureEnqueueMatch");
                        var ensureEnqueueDto = jToken.ToObject<EnsureMatchEnqueueDto>();
                        OnMatchMakingStarted?.Invoke();
                        OnEnsureMatchEnqueue?.Invoke(ensureEnqueueDto);
                        break;
                    }
                    case WsEventType.CancelSuccess:
                    {
                        Debug.Log("Triggered: CancelSuccess");
                        OnMatchCanceled?.Invoke();
                        break;
                    }
                    case WsEventType.NotifyCharacterPicked:
                    {
                        Debug.Log("Triggered: NotifyCharacterPicked");
                        var notifyDto = jToken.ToObject<CharacterPickNotifyDto>();
                        notifyDto.characterId = ResolveCharacterId(notifyDto.characterId);
                        OnCharacterPickNotify?.Invoke(notifyDto);
                        break;
                    }
                    case WsEventType.NotifyCharacterChanged:
                    {
                        Debug.Log("Triggered: NotifyCharacterChanged");
                        var notifyDto = jToken.ToObject<CharacterPickNotifyDto>();
                        notifyDto.characterId = ResolveCharacterId(notifyDto.characterId);
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
                    case WsEventType.GameTeamPlayerInformation:
                    {
                        Debug.Log("Triggered: GameTeamPlayerInformation");
                        var infoDto = jToken.ToObject<List<AnotherPlayerInfoDto>>();
                        foreach (var p in infoDto) p.characterId = ResolveCharacterId(p.characterId);
                        OnGotTeamPlayerInformation?.Invoke(infoDto);
                        break;
                    }
                    case WsEventType.MatchFound:
                    {
                        Debug.Log("Triggered: MatchFound");
                        var matchFoundDto = jToken.ToObject<MatchFoundDto>();
                        if (matchFoundDto.teamPlayers != null)
                            foreach (var p in matchFoundDto.teamPlayers) p.characterId = ResolveCharacterId(p.characterId);
                        OnMatchFound?.Invoke(matchFoundDto);
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
            if (isReconnecting) return;
            Debug.LogWarning("Global Websocket Closed! close code: " + (int)closeCode);
    
            // 1. 이벤트 핸들러 해제 및 핑 루프 정지
            ws.OnOpen -= WsOpenHandler;
            ws.OnMessage -= WsMessageHandler;
            ws.OnClose -= WsCloseHandler;
            ws.OnError -= WsErrorHandler;
            keepAliveCts?.Cancel();
    
            // 2. 서버가 의도적으로 끊은 경우 (약속된 Close Code 확인)
            if (closeCode == WebSocketCloseCode.PolicyViolation || closeCode == WebSocketCloseCode.DuplicateLogin) 
            {
                OnForcedLogout?.Invoke("다른 기기에서 접속하여 연결이 종료되었습니다.");
                return;
            }
            if (closeCode == WebSocketCloseCode.TransferToGame)
            {
                Debug.Log("인게임 전환을 위한 연결 종료.");
                socketState = MatchingWebsocketState.Close;
                return; // 아무 에러도 띄우지 않고, 재연결도 시도하지 않고 종료!
            }

            // 3. 비정상 종료 시 재연결 시도
            bool reconnected = await TryReconnectAsync(maxAttempts: 3);
    
            if (reconnected)
            {
                Debug.Log("재연결 성공! 하던 작업 계속 진행");
                // 블로킹 UI 해제 이벤트 발생
            }
            else
            {
                // 4. 최후의 수단: 로그인 화면으로 킥
                OnConnectionFatalError?.Invoke("네트워크 연결이 불안정하여 로그인 화면으로 돌아갑니다.");
            }
            
        }
        private async UniTask<bool> TryReconnectAsync(int maxAttempts)
        {
            isReconnecting = true;
            // 화면을 가리는 UI 시스템 호출 (예: UIManager.ShowReconnectingOverlay())
    
            for (int i = 1; i <= maxAttempts; i++)
            {
                Debug.Log($"재연결 시도 중... ({i}/{maxAttempts})");
                await UniTask.Delay(2000); // 2초 대기 후 시도 (Backoff)
        
                // 여기에 웹소켓 Connect 및 인증(Token) 절차 재실행
                var success = await JoinLobby(10); 
                if (success)
                {
                    isReconnecting = false;
                    // UIManager.HideReconnectingOverlay();
                    return true;
                }
            }
            isReconnecting = false;
            // UIManager.HideReconnectingOverlay();
            return false;
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

        protected override void OnDestroy()
        {
            ForceCloseOnExit();
            base.OnDestroy();
        }

        private void OnApplicationQuit()
        {
            ForceCloseOnExit();
        }

        private void ForceCloseOnExit()
        {
            try
            {
                if (keepAliveCts != null)
                {
                    keepAliveCts.Cancel();
                    keepAliveCts.Dispose();
                    keepAliveCts = null;
                }
                
                if (ws != null)
                {
                    ws.OnOpen -= WsOpenHandler;
                    ws.OnMessage -= WsMessageHandler;
                    ws.OnClose -= WsCloseHandler;
                    ws.OnError -= WsErrorHandler;

                    if (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting)
                    {
                        _ = ws.Close(); 
                    }
                    ws = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"종료 중 웹소켓 해제 실패: {e}");
            }
        }

        public bool IsConnected()
        {
            return ws is { State: WebSocketState.Open };
        }
        
        private string ResolveCharacterId(string idStr)
        {
            if (string.IsNullOrEmpty(idStr)) return idStr;
            if (!int.TryParse(idStr, out var id)) return idStr;
            return CharacterInfoLoader.Instance.IdToCharacterId(id);
        }

    }
}
