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
        GetPickInformation,
        
        //not use
        Cancel,
        
        //server->client
        Pong,
        MatchFound,
        EnsureEnqueueMatch,
        NotifyCharacterChanged,
        NotifyCharacterPicked,
    }
}