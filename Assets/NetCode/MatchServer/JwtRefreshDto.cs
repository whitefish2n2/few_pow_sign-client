using Newtonsoft.Json;

namespace NetCode
{
    public class JwtRefreshDto
    {
        public JwtRefreshDto(string jwt, string refreshToken){this.Jwt = jwt;this.RefreshToken = refreshToken;}
        
        [JsonProperty("jwt")]
        public string Jwt;
        [JsonProperty("refreshToken")]
        public string RefreshToken;
    }
}