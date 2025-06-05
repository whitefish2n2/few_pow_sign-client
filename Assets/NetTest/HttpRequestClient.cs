using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetTest.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NetTest
{
    public class HttpRequestClient
    {
        private static readonly HttpClient HttpClient = new();
        
        public static HttpRequestClient Instance { get; } =  new();
        
        public async Task<bool> SignUp(Action<string> onSuccess, Action<ErrorResponse> onFail, Action onTimeOut, SignUpDto dto)
        {
            const string endPoint = "/auth/signup";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, 
                    NetTestStatic.instance.baseUrl + ":" + NetTestStatic.instance.serverPort+endPoint);
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));//타임아웃 시간 설정
                string json = JsonConvert.SerializeObject(dto);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await HttpClient.SendAsync(request,HttpCompletionOption.ResponseContentRead,  cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log(responseBody);
                    onSuccess.Invoke(responseBody);
                    return true;
                }
                else
                {
                    Debug.LogError(response.StatusCode);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                    if(errorResponse.message == null)
                        errorResponse.message = "Error ON SignIn";
                    
                    onFail.Invoke(errorResponse);
                    return false;
                }
            }
            catch (TaskCanceledException e)
            {
                onTimeOut.Invoke();
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                onFail.Invoke(ErrorResponse.NotDefined("SignUp"));
                return false;
            }
        }
        
        public async Task<bool> SignIn(Action<SignInResponseDto> onSuccess, Action<ErrorResponse> onFail,Action onTimeOut, SignInDto dto)
        {
            const string endPoint = "/auth/signin";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, 
                    NetTestStatic.instance.baseUrl + ":" + NetTestStatic.instance.serverPort+endPoint);
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));//타임아웃 시간 설정
                string json = JsonConvert.SerializeObject(dto);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await HttpClient.SendAsync(request,HttpCompletionOption.ResponseHeadersRead, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log(responseBody);
                    SignInResponseDto responseDto;
                    try
                    {
                        responseDto = JsonConvert.DeserializeObject<SignInResponseDto>(responseBody);
                        if (responseDto == null || string.IsNullOrEmpty(responseDto.Jwt) ||
                            string.IsNullOrEmpty(responseDto.RefreshToken))
                        {
                            throw new Exception("Json Parse Failed");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        onFail.Invoke(ErrorResponse.JsonParseFailed("signInJson"));
                        return false;
                    }
                    onSuccess.Invoke(responseDto);
                    Debug.LogWarning(onSuccess.Target + "에서 signin onsuccess 호출");
                    return true;
                }
                else
                {
                    Debug.LogError(response.StatusCode);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                    if(errorResponse.message == null)
                        errorResponse.message = "Error ON SignUp";
                    
                    onFail.Invoke(errorResponse);
                    return false;
                }
            }
            catch (TaskCanceledException e)
            {
                onTimeOut.Invoke();
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                onFail.Invoke(ErrorResponse.NotDefined("SignInNotDefined"));
                return false;
            }
        }
        
        public async Task<bool> GetInventoryItem()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> GetSkin()
        {
            throw new NotImplementedException();
        }
        
    }
}