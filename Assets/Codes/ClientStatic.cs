namespace Codes
{
    public class ClientStatic
    {
        public static ClientStatic Instance{get;private set;} = new();
        
        public readonly string MatchServerBaseUrl = "http://localhost";//todo: 배포후 변경/배포기 만들기(생성자에서 json을 불러오는 로직을 구현한다든지 ㄱㄱ)
        public readonly int MatchServerPort = 25565;

        public string GetFullUrl()
        {
            return MatchServerBaseUrl + ":" + MatchServerPort;
        }
        
        public readonly string MatchWebsocketBaseUrl = "ws://localhost";
        public readonly int MatchWebsocketPort = 25565;
        
        public string authId;
        public string authPassword;//todo: 굳이 static에 저장할필요 없을듯
        public string authName;

        public string dedicatedBaseUrl;
    }
}
