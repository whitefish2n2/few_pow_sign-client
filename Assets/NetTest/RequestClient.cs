using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using NativeWebSocket;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codes;
using Codes.FileIO;
using Codes.Util;
using NetCode;
using NetTest.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.PackageManager;
using UnityEngine;

namespace NetTest
{
    public class RequestClient
    {
        private static readonly HttpClient HttpClient = new();
        
        public static RequestClient Instance { get; } =  new();
        
        
        public Action<ErrorResponse> OnTokenExpired;
        public Action OnServerTimeout;

        public async Task SignInWithRefreshToken(Action<ApiResponse<SignInResponseDto>> onSuccess, Action<ErrorResponse> onFail,Action onTimeOut, SignInWithRefreshDto dto)
        {
            const string endPoint = "/auth/signin-with-refresh";
            var url = new UrlBuilder(ClientStatic.Instance.GetFullUrl()+endPoint).Build();
            await HandlePostRequest(url,onSuccess,onFail,onTimeOut,dto,"SignInWithRefreshToken",20);
        }
        
        
        public async Task SignUp(Action<ApiResponse<SignInResponseDto>> onSuccess, Action<ErrorResponse> onFail, Action onTimeOut, SignUpDto dto)
        {
            const string endPoint = "/auth/signup";
            var url = (new UrlBuilder(ClientStatic.Instance.GetFullUrl() + endPoint)).Build();
            await HandlePostRequest(url,onSuccess,onFail,onTimeOut, dto,"SignUp",20);
        }
        
        public async Task SignIn(Action<ApiResponse<SignInResponseDto>> onSuccess, Action<ErrorResponse> onFail,Action onTimeOut, SignInDto dto)
        {
            const string endPoint = "/auth/signin";
            var url = new UrlBuilder(ClientStatic.Instance.MatchServerBaseUrl).SetPort(ClientStatic.Instance.MatchServerPort).SetEndPoint(endPoint).Build();
            await HandlePostRequest(url,onSuccess,onFail,onTimeOut,dto,"SignIn",15);
        }

        public async Task<bool> ValidateToken(string jwt)
        {
            var tcs = new TaskCompletionSource<bool>();

            const string endPoint = "/auth/validate-token";
            var url = new UrlBuilder(ClientStatic.Instance.GetFullUrl()).SetEndPoint(endPoint).Build();

            await HandleGetRequest<ApiResponse<bool>>(
                url,
                res => tcs.TrySetResult(true),
                err => tcs.TrySetResult(false),
                () => tcs.TrySetResult(false),
                "Validate Token"
            );

            return await tcs.Task;
        }
        
        private async Task RefreshJwt(JwtRefreshDto dto, Action<ApiResponse<string>> onSuccess, Action<ErrorResponse> onFail, Action onTimeOut)
        {
            const string endPoint = "/auth/refresh";
            var url = new UrlBuilder(ClientStatic.Instance.MatchServerBaseUrl).SetPort(ClientStatic.Instance.MatchServerPort).SetEndPoint(endPoint).Build();
            await HandlePostRequest<ApiResponse<string>>(url, onSuccess,onFail,onTimeOut,dto,"RefreshJwt",15);
        }
        public async Task<bool> RefreshJwt()
        {
            var tcs = new TaskCompletionSource<bool>();
            var dto = new JwtRefreshDto(TokenHolder.instance.GetJwt(),TokenHolder.instance.GetRefreshToken());
            const string endPoint = "/auth/refresh";
            var url = new UrlBuilder(ClientStatic.Instance.MatchServerBaseUrl).SetPort(ClientStatic.Instance.MatchServerPort).SetEndPoint(endPoint).Build();
            await HandlePostRequest<ApiResponse<string>>(url, 
                (v)=>
                {
                    var newJwt = v.data;
                    if (!string.IsNullOrEmpty(newJwt))
                    {
                        tcs.TrySetResult(true);
                        Debug.LogWarning("JWT Refreshed:" + newJwt);
                        TokenHolder.instance.SetToken(newJwt,TokenHolder.instance.GetRefreshToken());
                    }
                    else
                    {
                        OnTokenExpired(ErrorResponse.TokenExpired);
                        tcs.TrySetResult(false);
                    }
                },
                (e) =>
                {
                    OnTokenExpired(e);
                    tcs.TrySetResult(false);
                },
                () =>
                {
                    tcs.TrySetResult(false);
                },
                dto,
                "RefreshJwt",
                15);
            return await tcs.Task;
        }
        private async Task<bool> RefreshJwtInternal()
        {
            var dto = new JwtRefreshDto(TokenHolder.instance.GetJwt(), TokenHolder.instance.GetRefreshToken());
            string newJwt = null;
            await RefreshJwt(dto,(v)=>
            {
                newJwt = v.data;
                if (newJwt != null)
                {
                     TokenHolder.instance.SetToken(newJwt,TokenHolder.instance.GetRefreshToken());
                }
                else OnTokenExpired(ErrorResponse.TokenExpired);
            },(v)=>
            {
                Debug.LogError("Error "+v.code+" "+v.message);
                OnTokenExpired.Invoke(v);
            }, OnServerTimeout);
                return newJwt != null;
        }
        
        
        /// <summary>
        /// Matching 웹소켓 서버 url로 만들어진 웹소켓 객체를 반환해요.
        /// </summary>
        /// <param name="gameModeIndex">게임 모드에요</param>
        /// <returns></returns>
        public WebSocket GetMatchWebsocket()
        {
            const string endPoint = "/match-wait";

            string url = new UrlBuilder(ClientStatic.Instance.MatchWebsocketBaseUrl)
                .SetPort(ClientStatic.Instance.MatchServerPort)
                .SetEndPoint(endPoint)
                .AddParam("token", TokenHolder.instance.GetJwt())
                .Build();// $"{ClientStatic.Instance.MatchWebsocketBaseUrl}/match-wait?token={TokenHolder.instance.GetJwt()}&gameMode={gameModeIndex}";
            return new NativeWebSocket.WebSocket(url);
        }
        public WebSocket GetMatchWebsocket(string token)
        {
            const string endPoint = "/match-wait";
            string url = new UrlBuilder(ClientStatic.Instance.MatchWebsocketBaseUrl)
                .SetPort(ClientStatic.Instance.MatchServerPort)
                .SetEndPoint(endPoint)
                .Build();// $"{ClientStatic.Instance.MatchWebsocketBaseUrl}/match-wait";
            
            return new NativeWebSocket.WebSocket(url, new Dictionary<string, string>{{"Authorization", "Bearer " + token}});
        }
        
        public async Task GetInventoryItem()
        {
            throw new NotImplementedException();
        }

        public async Task GetSkin()
        {
            throw new NotImplementedException();
        }
        

        public async Task HandlePostRequest<T>(string url,
            Action<T> onSuccess
            , Action<ErrorResponse> onFail
            , Action onTimeOut,
            object sendData,
            string indicator,
            int timeOutTime = 15
            )
        {
            try
            {
                var response = await Post(url, sendData,timeOutTime);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"[{indicator}] Post Response: {responseBody}");
                if (response.IsSuccessStatusCode)
                {
                    T res = JsonConvert.DeserializeObject<T>(responseBody);
                    onSuccess.Invoke(res);
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                    if (errorResponse != null)
                    {
                        
                        //Jwt Refresh 시도 후 재요청
                        Debug.LogError($"[{indicator}] Error:{response.StatusCode}");
                        if (errorResponse.code == (int)ExceptionCode.InvalidJwtException && ClientStatic.Instance.jwt != null && ClientStatic.Instance.refreshToken != null)
                        {
                            await RefreshJwtInternal();
                        }
                    }
                    else
                    {
                        errorResponse = ErrorResponse.NotDefined(indicator);
                        Debug.LogError($"[{indicator}] Error-Error Response Parse Failed. Http Status Code:{response.StatusCode}");
                    }
                    
                    onFail.Invoke(errorResponse);
                }
            }
            catch (TaskCanceledException e)
            {
                onTimeOut.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                onFail.Invoke(ErrorResponse.NotDefined(indicator));
            }
        }
        public static async Task HandleGetRequest<T>(string url,
            Action<T> onSuccess
            , Action<ErrorResponse> onFail
            , Action onTimeOut,
            string indicator,
            int timeOutTime = 15
        )
        {
            try
            {
                var response = await Get(url,timeOutTime);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"[{indicator}] Get Response: {responseBody}");
                if (response.IsSuccessStatusCode)
                {
                    T res = JsonConvert.DeserializeObject<T>(responseBody);
                    onSuccess.Invoke(res);
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                    if (errorResponse != null)
                    {
                        Debug.LogError($"[{indicator}] Error:{response.StatusCode}");
                    }
                    else
                    {
                        errorResponse = ErrorResponse.NotDefined(indicator);
                        Debug.LogError($"[{indicator}] Error-Error Response Parse Failed. Http Status Code:{response.StatusCode}");
                    }
                    
                    onFail.Invoke(errorResponse);
                }
            }
            catch (TaskCanceledException e)
            {
                onTimeOut.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                onFail.Invoke(ErrorResponse.NotDefined(indicator));
            }
        }

        
        private static async Task<HttpResponseMessage> Post(string url, object data, int timeoutTime = 15)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "Bearer " + TokenHolder.instance.GetJwt());
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutTime));
            string json = JsonConvert.SerializeObject(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await HttpClient.SendAsync(request,HttpCompletionOption.ResponseHeadersRead, cts.Token);
        }
        
        public static async Task<HttpResponseMessage> Get(string url, int timeoutTime = 15)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "Bearer " + TokenHolder.instance.GetJwt());
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutTime));
            return await HttpClient.SendAsync(request,HttpCompletionOption.ResponseHeadersRead, cts.Token);
        }
        
    }
}