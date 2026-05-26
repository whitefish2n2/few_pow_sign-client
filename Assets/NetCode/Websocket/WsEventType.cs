using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetCode
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WsEventType
    {
        //client->server
        Ping,
        EnqueueMatch,
        
        PickCharacter,
        PickCharacterTemporary,
        Cancel,
        
        //server->client
        JoinLobby,
        Pong,
        EnsureEnqueueMatch,
        MatchFound,
        Dodged,
        StartMatch,
        NotifyCharacterChanged,
        NotifyCharacterPicked,
        PickCharacterFailed,
        PickCharacterSuccess,
        GameTeamPlayerInformation,//GetGameInformation의 응답
        CancelSuccess//Cancel 요청의 응답
    }
}