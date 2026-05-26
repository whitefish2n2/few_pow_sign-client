using System;
using System.Collections.Generic;
using System.Net.Http;
using NativeWebSocket;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codes;
using Codes.OutGame.LoginUi.Dto;
using Codes.Util;
using Cysharp.Threading.Tasks;
using NetCode;
using NetTest.Dto;
using Newtonsoft.Json;
using UnityEngine;

namespace NetTest
{
    public class RequestClient
    {
        private static readonly HttpClient HttpClient = new();
        
        public static RequestClient Instance { get; } =  new();
        
        
        public Action<ErrorResponse> OnTokenExpired;
        public Action OnServerTimeout;

        public async UniTask SignInWithRefreshToken(Action<ApiResponse<SignInResponseDto>> onSuccess, Action<ErrorResponse> onFail,Action onTimeOut, SignInWithRefreshDto dto)
        {
            const string endPoint = "/auth/signin-with-refresh";
            var url = new UrlBuilder(ClientStatic.Instance.GetFullUrl()+endPoint).Build();
            await HandlePostRequest(url,onSuccess,onFail,onTimeOut,dto,"SignInWithRefreshToken",20);
        }
        
        
        public async UniTask SignUp(Action<ApiResponse<SignInResponseDto>> onSuccess, Action<ErrorResponse> onFail, Action onTimeOut, SignUpDto dto)
        {
            const string endPoint = "/auth/signup";
            var url = (new UrlBuilder(ClientStatic.Instance.GetFullUrl() + endPoint)).Build();
            await HandlePostRequest(url,onSuccess,onFail,onTimeOut, dto,"SignUp",20);
        }
        
        public async UniTask SignIn(Action<ApiResponse<SignInResponseDto>> onSuccess, Action<ErrorResponse> onFail,Action onTimeOut, SignInDto dto)
        {
            ClientStatic.Instance.authId = dto.ID;
            const string endPoint = "/auth/signin";
            var url = new UrlBuilder(ClientStatic.Instance.MatchServerBaseUrl).SetPort(ClientStatic.Instance.MatchServerPort).SetEndPoint(endPoint).Build();
            await HandlePostRequest(url,onSuccess,onFail,onTimeOut,dto,"SignIn",15);
        }

        public async UniTask<bool> ValidateToken(string jwt)
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
        
        private async UniTask RefreshJwt(JwtRefreshDto dto, Action<ApiResponse<string>> onSuccess, Action<ErrorResponse> onFail, Action onTimeOut)
        {
            const string endPoint = "/auth/refresh";
            var url = new UrlBuilder(ClientStatic.Instance.MatchServerBaseUrl).SetPort(ClientStatic.Instance.MatchServerPort).SetEndPoint(endPoint).Build();
            await HandlePostRequest<ApiResponse<string>>(url, onSuccess,onFail,onTimeOut,dto,"RefreshJwt",15);
        }
        public async UniTask<bool> RefreshJwt()
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
        private async UniTask<bool> RefreshJwtInternal()
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
        
        
        public WebSocket GetMatchWebsocket(string token)
        {
            const string endPoint = "/match-wait";
            string url = new UrlBuilder(ClientStatic.Instance.MatchWebsocketBaseUrl)
                .SetPort(ClientStatic.Instance.MatchServerPort)
                .SetEndPoint(endPoint)
                .Build();// $"{ClientStatic.Instance.MatchWebsocketBaseUrl}/match-wait";
            
            return new NativeWebSocket.WebSocket(url, new Dictionary<string, string>{{"Authorization", "Bearer " + token}});
        }
        
        public async UniTask GetPlayerPrivateInfo(Action<ApiResponse<PlayerPrivateInformationDto>> onSuccess, Action<ErrorResponse> onFail,Action onTimeOut)
        {
            const string endPoint = "/user/getCurrentPlayerInformation";
            var url = new UrlBuilder(ClientStatic.Instance.MatchServerBaseUrl).SetPort(ClientStatic.Instance.MatchServerPort).SetEndPoint(endPoint).Build();
            var dto = "GetPlayerPrivateInfo";
            await HandleGetRequest(url,onSuccess,onFail,onTimeOut,dto,15);
        }
        public async UniTask GetInventoryItem()
        {
            throw new NotImplementedException();
        }

        public async UniTask GetSkin()
        {
            throw new NotImplementedException();
        }
        

        public async UniTask HandlePostRequest<T>(string url,
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
                
                var responseBody = await response.Content.ReadAsStringAsync().AsUniTask();
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
                        if (errorResponse.code == (int)ExceptionCode.InvalidJwtException && TokenHolder.instance.GetJwt() != null && TokenHolder.instance.GetRefreshToken() != null)
                        {
                            bool refreshSuccess = await RefreshJwtInternal();
                            if (refreshSuccess)
                            {
                                await HandlePostRequest<T>(url,onSuccess,onFail,onTimeOut,sendData,indicator,timeOutTime);
                                return;
                            }
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
        public async UniTask HandleGetRequest<T>(string url,
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
                
                var responseBody = await response.Content.ReadAsStringAsync().AsUniTask();
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
                        
                        //Jwt Refresh 시도 후 재요청
                        Debug.LogError($"[{indicator}] Error:{response.StatusCode}");
                        if (errorResponse.code == (int)ExceptionCode.InvalidJwtException && TokenHolder.instance.GetJwt() != null && TokenHolder.instance.GetRefreshToken() != null)
                        {
                            bool refreshSuccess = await RefreshJwtInternal();
                            if (refreshSuccess)
                            {
                                await HandleGetRequest<T>(url,onSuccess,onFail,onTimeOut,indicator,timeOutTime);
                                return;
                            }
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

        
        private static async UniTask<HttpResponseMessage> Post(string url, object data, int timeoutTime = 15)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "Bearer " + TokenHolder.instance.GetJwt());
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutTime));
            string json = JsonConvert.SerializeObject(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            
            return await HttpClient.SendAsync(request,HttpCompletionOption.ResponseHeadersRead, cts.Token).AsUniTask();
        }
        
        public static async UniTask<HttpResponseMessage> Get(string url, int timeoutTime = 15)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "Bearer " + TokenHolder.instance.GetJwt());
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutTime));
            return await HttpClient.SendAsync(request,HttpCompletionOption.ResponseHeadersRead, cts.Token).AsUniTask();
        }
        
    }
}