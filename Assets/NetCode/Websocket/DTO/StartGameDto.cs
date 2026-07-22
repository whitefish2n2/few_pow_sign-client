using System.Collections.Generic;
using MapFile.MapCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetCode
{
    public class StartGameDto
    {
        public string gameId;
        public string sessionIndex;
        public string udpIp;
        public int udpPort;
        [JsonConverter(typeof(StringEnumConverter))]
        public Map.MapEnum map;
        public List<AnotherPlayerInfoDto> players;
    }
}