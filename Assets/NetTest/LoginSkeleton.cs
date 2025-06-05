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

        public void LoginSuccess(SignInResponseDto response)
        {
            try
            {
                NetTestStatic.instance.jwt = response.Jwt;
                NetTestStatic.instance.refreshToken = response.RefreshToken;
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
        public async Task<bool> TrySignInAsync()
        {
            HttpRequestClient requestClient = new HttpRequestClient();
            SignInDto signindto = new SignInDto();
            signindto.ID = NetTestStatic.instance.authId;
            signindto.Password = NetTestStatic.instance.authPassword;
            var req = requestClient.SignIn(LoginSuccess,LoginFail,LoginTimeout,signindto);
            //딴 로직 처리할거 있으면 여기 넣으셈
            return await req;
        }

        public IEnumerator Request()
        {

            var req = TrySignInAsync();
            while (!req.IsCompleted)
            {
                //대기 애니메이션같은거 넣을수 있고
                yield return null;
            }

            if (req.Result)//Result 는 트랜젝션 성공 여부 반환- 아 그러면 안되는데 ㅅㅂ 정보 담은 dto 줘야지 Task<SignInResponseDto>이렇게 하면 되나
            {
                //성공 처리
            }
            else
            {
                //실패 처리
            }
            
        }
    }
}
#endif