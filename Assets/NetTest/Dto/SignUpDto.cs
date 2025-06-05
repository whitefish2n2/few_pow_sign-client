using Newtonsoft.Json;

namespace NetTest.Dto
{
    public class SignUpDto
    {
        [JsonProperty("id")]
        public string ID = NetTestStatic.instance.authId;
        
        [JsonProperty("password")]
        public string Password = NetTestStatic.instance.authPassword;
        
        [JsonProperty("name")]
        public string Name = NetTestStatic.instance.authName;
    }
}