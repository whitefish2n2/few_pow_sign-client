using System;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using NetTest;
using Plugins;
using UnityEngine;

namespace Codes.OutGame.Match
{
    public class MatchingManager
    {
        public static readonly MatchingManager Instance = new MatchingManager();
        
        private GameMode currentGameMode;

        public event Action<ErrorResponse> OnMatchFailed;
        public event Action OnMatchCanceled;
        private WebSocket ws;
        
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
            
            OnMatchFailed = null;
            OnMatchCanceled = null;
        }

        public void ChangeGameMode(GameMode newGameMode)
        {
            currentGameMode = newGameMode;
        }

        public async Task Match(int timeout)
        {
            ws = RequestClient.Instance.Matching((int)currentGameMode);
            ws.OnOpen += WsOpenHandler;
            ws.OnMessage += WsMessageHandler;
            ws.OnClose += WsCloseHandler;
            ws.OnError += WsErrorHandler;
            
            var success = await ConnectWithTimeout(ws,timeout);
            if(!success) OnMatchCanceled?.Invoke();
        }

        private async Task<bool> ConnectWithTimeout(NativeWebSocket.WebSocket ws, int timeout)
        {
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout*1000, cts.Token);

            var connectTask = ws.Connect();
    
            var finished = await Task.WhenAny(connectTask, timeoutTask);

            if (finished == timeoutTask)
            {
                Debug.LogWarning("WebSocket connect timed out!");
                try { await ws.Close(); } catch(Exception e) { Debug.LogException(e); }
                return false;
            }

            cts.Cancel(); // 연결 성공했으니 timeout task 중단
            return true;
        }

        private void WsMessageHandler(byte[] bytes)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + message + ")");
        }

        private void WsCloseHandler(WebSocketCloseCode closeCode)
        {
            
        }

        private void WsErrorHandler(string error)
        {
            Debug.LogError(error);
            OnMatchCanceled?.Invoke();
        }

        private void WsOpenHandler()
        {
            
        }
    }
}
