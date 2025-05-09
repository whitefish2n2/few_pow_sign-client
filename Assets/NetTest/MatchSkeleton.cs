using System;
using System.Net.Http;
using NativeWebSocket;
using System.Text;
using NetCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
namespace NetTest
{
    public class MatchSkeleton : MonoBehaviour
    {
        [SerializeField] private string websocketUrl;
        [SerializeField] private int modeIndex;
        private WebSocket websocket;

        public async void Click()
        {
            try
            {
                string url = $"{websocketUrl}/match-wait?token={NetTestStatic.instance.jwt}&userId={NetTestStatic.instance.authId}&gameMode={modeIndex}";
                websocket = new WebSocket(url);

                websocket.OnOpen += async () =>
                {
                    await websocket.SendText("fuck you server from client");
                    Debug.Log("Connection open!");
                };

                websocket.OnError += (e) => {
                    Debug.Log("Error! " + e);
                };

                websocket.OnClose += (e) => {
                    Debug.Log("Connection closed!");
                };

                websocket.OnMessage += (bytes) => {
                    string message = System.Text.Encoding.UTF8.GetString(bytes);
                    Debug.Log("Received OnMessage! (" + message + ")");
                };

                await websocket.Connect();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void Update()
        {
            if (websocket != null)
                websocket.DispatchMessageQueue();
        }

        public async void Cancel()
        {
            await websocket.SendText(JsonUtility.ToJson(new WsEventDto{Type = WsEventType.Cancel.ToString(),Message = ""}));
        }
    }
}
#endif