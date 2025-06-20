using Newtonsoft.Json;

namespace NetTest.Dto
{
    public class SignUpDto
    {
        [JsonProperty("id")]
        public string ID;
        
        [JsonProperty("password")]
        public string Password;

        [JsonProperty("name")] public string Name;
    }
}