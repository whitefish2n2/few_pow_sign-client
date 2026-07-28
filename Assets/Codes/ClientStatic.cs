using System;
using System.IO;
using Codes.Util.Annotation;
using UnityEngine;

namespace Codes
{
    public class ClientStatic
    {
        public static ClientStatic Instance{get;private set;} = new();
        
        [ReadOnly] public string MatchServerHost = "localhost";
        

        public string MatchServerBaseUrl => "http://" + MatchServerHost;
        public string MatchWebsocketBaseUrl => "ws://" + MatchServerHost;

        public string GetFullServerUrl()
        {
            return MatchServerBaseUrl + ":" + MatchServerPort;
        }

        public string GetFullWebSocketUrl()
        {
            return MatchWebsocketBaseUrl + ":" + MatchWebsocketPort;
        }

        public readonly int MatchWebsocketPort = 25565;
        public readonly int MatchServerPort = 25565;
        
        public string authId;
        public string authPassword;//todo: 굳이 static에 저장할필요 없을듯
        public string authName;
        public long accountCreatedAt;

        public string dedicatedBaseUrl;

        [Serializable]
        private class ServerConfig
        {
            public string matchServerHost;
            public int matchServerPort;
            public int matchWebsocketPort;
        }
        private ClientStatic()
        {
            MatchServerHost = "localhost";
            MatchServerPort = 25565;
            MatchWebsocketPort = 25565;

            string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");
            if (File.Exists(configPath))
            {
                var config = JsonUtility.FromJson<ServerConfig>(File.ReadAllText(configPath));
                if (config != null)
                {
                    if (!string.IsNullOrEmpty(config.matchServerHost)) MatchServerHost = config.matchServerHost;
                    if (config.matchServerPort > 0) MatchServerPort = config.matchServerPort;
                    if (config.matchWebsocketPort > 0) MatchWebsocketPort = config.matchWebsocketPort;
                }
            }
        }
    }
}

