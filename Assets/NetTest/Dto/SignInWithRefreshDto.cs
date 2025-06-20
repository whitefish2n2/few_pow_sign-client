using Codes.FileIO;
using Newtonsoft.Json;

namespace NetTest.Dto
{
    public struct SignInWithRefreshDto
    {
        [JsonProperty("refreshToken")] public string RefreshToken;

        public SignInWithRefreshDto(string token)
        {
            this.RefreshToken = token;
        }
    }
}