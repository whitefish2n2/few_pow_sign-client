using Codes.FileIO;
using UnityEngine;

namespace NetTest
{
    public class TokenHolder
    {
        public static readonly TokenHolder instance = new();

        private string jwt = null;
        private string refreshToken = null;
        private bool isLoaded = false;

        public void InitFromDisk()
        {
            if (isLoaded) return;

            if (TryGetToken(out var data))
            {
                SetToken(data.jwt, data.refreshToken);
                isLoaded = true;
                Debug.Log("[TokenHolder] 토큰 로드 성공");
            }
            else
            {
                Debug.LogWarning("[TokenHolder] 토큰 로드 실패");
            }
        }

        public string GetJwt()
        {
            if (!isLoaded) InitFromDisk();
            return jwt;
        }

        public string GetRefreshToken()
        {
            if (!isLoaded) InitFromDisk();
            return refreshToken;
        }

        public void SetToken(string newJwt, string newRefresh)
        {
            jwt = newJwt;
            refreshToken = newRefresh;
            isLoaded = true;

            TokenIO.SaveToken(newJwt,newRefresh);
        }

        public void Clear()
        {
            jwt = null;
            refreshToken = null;
            isLoaded = false;
            TokenIO.DeleteToken();
        }

        private bool TryGetToken(out AuthTokenData data)
        {
            data = TokenIO.LoadToken();

            if (data == null) return false;
            if (string.IsNullOrWhiteSpace(data.jwt)) return false;
            if (string.IsNullOrWhiteSpace(data.refreshToken)) return false;
            return true;
        }
    }
}