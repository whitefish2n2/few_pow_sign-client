using System;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if UNITY_EDITOR
namespace NetTest
{
    public class SigninSkeleton : MonoBehaviour
    {
        private readonly HttpClient client = new HttpClient();
        [SerializeField] private string endpoint;
        public async void Click()
        {
            try
            {
                var body = new {
                    id = NetTestStatic.instance.authId,
                    password = NetTestStatic.instance.authPassword,
                    name = NetTestStatic.instance.authName
                };
                string json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(NetTestStatic.instance.baseUrl+":"+NetTestStatic.instance.serverPort+endpoint,content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Debug.Log(responseBody);
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Debug.LogError(response.StatusCode);
                    JObject jObject = JObject.Parse(responseBody);
                    var message = jObject["message"]?.ToString();
                    Debug.LogError(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
#endif