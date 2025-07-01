using System.Collections.Generic;
using Map;
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
        public MapEnum map;
        public List<NewPlayerDto> players;
    }
}