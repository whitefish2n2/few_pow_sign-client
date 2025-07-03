using System.Collections.Generic;
using MapFile.MapCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetCode
{
    public class MatchFoundDto
    {
        public string gameId;
        public string sessionVerifyKey;
        public string sessionIndex;
        public string url;
        [JsonConverter(typeof(StringEnumConverter))]
        public Map.MapEnum map;
        public List<NewPlayerDto> players;
    }
}