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
        
        //not use
        Cancel,
        
        //server->client
        Pong,
        MatchFound,
        EnsureEnqueueMatch,
    }
}