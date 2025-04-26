using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
namespace NetTest
{
    public class LoginSkeleton : MonoBehaviour
    {
        [SerializeField] private string endpoint;
        private readonly HttpClient client = new();

        public async void Click()
        {
            try
            {
                var body = new
                {
                    id = NetTestStatic.instance.authId,
                    password = NetTestStatic.instance.authPassword
                };
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(
                    NetTestStatic.instance.baseUrl + ":" + NetTestStatic.instance.serverPort + endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Debug.Log(responseBody);
                    var jObject = JObject.Parse(responseBody);
                    NetTestStatic.instance.jwt = jObject["jwt"]?.ToString();
                    NetTestStatic.instance.refreshToken = jObject["refreshToken"]?.ToString();
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Debug.LogError(response.StatusCode);
                    var jObject = JObject.Parse(responseBody);
                    var message = jObject["message"]?.ToString();
                    Debug.LogError(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
        }
    }
}
#endif