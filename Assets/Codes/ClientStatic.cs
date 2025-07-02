using System;
using Codes.Util.Annotation;

namespace Codes
{
    public class ClientStatic
    {
        public static ClientStatic Instance{get;private set;} = new();
        
        public readonly string MatchServerBaseUrl = "http://localhost";//todo: 배포후 변경/배포기 만들기(생성자에서 json을 불러오는 로직을 구현한다든지 ㄱㄱ)
        public readonly int MatchServerPort = 8080;

        public string GetFullUrl()
        {
            return MatchServerBaseUrl + ":" + MatchServerPort;
        }
        
        public readonly string MatchWebsocketBaseUrl = "ws://localhost";
        public readonly int MatchWebsocketPort = 8080;
        
        public string authId;//todo: 굳이 static에 저장할필요 없을듯
        public string authPassword;
        public string authName;
    
        
        [ReadOnly] public string jwt;
        [ReadOnly] public string refreshToken;
        [ReadOnly] public string username;
        [ReadOnly] public UInt64 userPrivateKey;
        [ReadOnly] public sbyte userPublicKey;
        [ReadOnly] public string sessionConnectToken;
        [ReadOnly] public string sessionKey;

        public string dedicatedBaseUrl;
        public string dedicatedPort;

        public UInt16 sessionIndex;

    }
}
