using Newtonsoft.Json;

namespace NetTest.Dto
{
    public struct SignInDto
    {
        [JsonProperty("id")]
        public string ID;
        
        [JsonProperty("password")]
        public string Password;
    }
}