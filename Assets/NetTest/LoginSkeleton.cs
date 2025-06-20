using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NetTest.Dto;
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

        public void LoginSuccess(ApiResponse<SignInResponseDto> response)
        {
            try
            {
                NetTestStatic.instance.jwt = response.data.Jwt;
                NetTestStatic.instance.refreshToken = response.data.RefreshToken;
                ModalManager.instance.Alert(("Login Success"));
            }
            catch (Exception ex)
            {
                Debug.LogError("LoginSuccess Error: " + ex.Message);
                ModalManager.instance.Alert("로그인 응답 처리 중 오류 발생");
            }
            
        }

        public void LoginFail(ErrorResponse response)
        {
            Debug.LogError(response.code);
            Debug.LogError(response.message);
            ModalManager.instance.Alert(response.message);
        }

        public void LoginTimeout()
        {
            ModalManager.instance.Alert("요청 시간이 초과되었습니다.");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task TrySignInAsync()
        {
            RequestClient requestClient = new RequestClient();
            SignInDto signindto = new SignInDto();
            signindto.ID = NetTestStatic.instance.authId;
            signindto.Password = NetTestStatic.instance.authPassword;
            await requestClient.SignIn(LoginSuccess,LoginFail,LoginTimeout,signindto);
            //딴 로직 처리할거 있으면 여기 넣으셈
        }


    }
}
#endif