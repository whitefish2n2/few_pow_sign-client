using System.Threading.Tasks;
using NetTest;
using NetTest.Dto;
using UnityEngine;

namespace Codes.OutGame.LoginUi
{
    public class LoginUiManager : MonoBehaviour
    {
        public static LoginUiManager instance;

        private void Awake()
        {
            instance = this;
        }


        private void LoginComplete(SignInResponseDto response)
        {
            
        }

        private void LoginFailed(ErrorResponse errorResponse)
        {
            
        }

        private void LoginTimeout()
        {
            
        }
        public void Login(SignInDto signInDto)
        {
            var r = HttpRequestClient.Instance.SignIn(LoginComplete,LoginFailed,LoginTimeout, signInDto);
        }
    
        private void OnDestroy()
        {
            instance = null;
        }
    
    }
}
