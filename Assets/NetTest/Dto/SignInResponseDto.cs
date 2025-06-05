using Newtonsoft.Json;

namespace NetTest.Dto
{
    
    public class SignInResponseDto
    {
        [JsonProperty("jwt")]
        public string Jwt;
        [JsonProperty("refreshToken")]
        public string RefreshToken;
    }
}