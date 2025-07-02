using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Codes.FileIO
{
    public class TokenIO
    {
        private static string tokenFilePath =>
            Path.Combine(Application.persistentDataPath, "auth_token.env");
        public static void SaveToken(string newJwt, string refresh)
        {
            AuthTokenData tokenData = new AuthTokenData
            {
                jwt = newJwt,
                refreshToken = refresh,
            };
            
            string json = JsonConvert.SerializeObject(tokenData);
            File.WriteAllText(tokenFilePath, json);
            Debug.Log("New Jwt:" + newJwt);
            Debug.Log($"[TOKEN] 저장 완료: {tokenFilePath}");
        }
        
        public static AuthTokenData LoadToken()
        {
            if (!File.Exists(tokenFilePath))
            {
                Debug.LogWarning("[TOKEN] 저장된 토큰 파일 없음.");
                return null;
            }

            string json = File.ReadAllText(tokenFilePath);
            AuthTokenData tokenData = JsonConvert.DeserializeObject<AuthTokenData>(json);
            return tokenData;
        }

        public static void DeleteToken()
        {
            try
            {
                if (File.Exists(tokenFilePath))
                {
                    File.Delete(tokenFilePath);
                    Debug.Log($"[TokenIO] 토큰 파일 삭제됨: {tokenFilePath}");
                }
                else
                {
                    Debug.LogWarning($"[TokenIO] 삭제할 토큰 파일 없음: {tokenFilePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TokenIO] 토큰 파일 삭제 실패: {e.Message}");
            }
        }


    }
}