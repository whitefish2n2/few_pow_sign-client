using Newtonsoft.Json;

namespace NetCode
{
    public class NewPlayerDto
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("key")]
        public string Key;
    }
}