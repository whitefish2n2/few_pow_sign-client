using System.Threading.Tasks;
using Codes.FileIO;
using Codes.Util;
using Cysharp.Threading.Tasks;
using NetTest;
using NetTest.Dto;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Codes.OutGame.LoginUi
{
    public class LoginUiManager : MonoBungleton<LoginUiManager>
    {
        [SerializeField] SignInForm signInForm;
        [SerializeField]SignUpForm signUpForm;
        //Log in

        protected override void Initialize()
        {
            if (signInForm == null || signUpForm == null)
            {
                Debug.LogError("LoginUiManager: signInForm, signUpForm Is Null");
            }

            TryAutoLogin();
        }

        private async UniTask TryAutoLogin()
        {
            //todo: auto login 여부 확인
            var token = TokenIO.LoadToken();
            if (token == null) return;
            await RequestClient.Instance.SignInWithRefreshToken(OnSigninComplete,
                (e) => {Debug.LogError(e.code+" : "+e.message);AlertManager.Instance.AlertRetryableError(e); },
                () => { },
                new SignInWithRefreshDto(token.refreshToken));
        }
        private void OnSigninComplete(ApiResponse<SignInResponseDto> response)
        {
            TokenHolder.instance.SetToken(response.data.Jwt,response.data.RefreshToken);
            SceneManager.LoadScene("Scenes/OutgameSkeleton");
        }

        private void OnSigninFailed(ErrorResponse errorResponse)
        {
            if (errorResponse.code is (int)ExceptionCode.PlayerNotFoundException or (int)ExceptionCode.LoginNotRegisteredIdException)
            {
                signInForm.InvalidId();
            }
            else if (errorResponse.code is (int)ExceptionCode.LoginPasswordNotMatchedException)
            {
                signInForm.InvalidPassword();
            }
            else
            {
                AlertManager.Instance.AlertRetryableError(errorResponse);
            }
        }

        public SignInDto tempSignInInfo;
        private void OnSigninTimeout()
        {
            AlertManager.Instance.AlertRetryableError(ErrorResponse.ServerTimeout, async () => { await SignIn(tempSignInInfo);} );
        }
        public async UniTask SignIn(SignInDto signInDto)
        {
            tempSignInInfo = signInDto;
            var task = RequestClient.Instance.SignIn(OnSigninComplete,OnSigninFailed,OnSigninTimeout, signInDto);
            await UIEventManager.Instance.SpinningInAsync(0.1f, task, 0.1f);
        }
        
        //Sign Up
        public SignUpDto tempSignUpInfo;
        private void OnSignUpComplete(ApiResponse<SignInResponseDto> response)
        {
            TokenHolder.instance.SetToken(response.data.Jwt,response.data.RefreshToken);
            SceneManager.LoadScene("Scenes/OutgameSkeleton");
        }
        private void OnSignUpFailed(ErrorResponse errorResponse)
        {
            if (errorResponse.code is (int)ExceptionCode.AlreadyExistsIdException)
            {
                signUpForm.AlreadyExistsId();
            }
            else
            {
                AlertManager.Instance.AlertRetryableError(errorResponse);
            }
        }

        private void OnSignUpTimeout()
        {
            AlertManager.Instance.AlertRetryableError(ErrorResponse.ServerTimeout, async () => { await SignUp(tempSignUpInfo);} );
        }
        public async UniTask SignUp(SignUpDto signUpDto)
        {
            tempSignUpInfo = signUpDto;
            var task = RequestClient.Instance.SignUp(OnSignUpComplete, OnSignUpFailed, OnSignUpTimeout, signUpDto);
            await UIEventManager.Instance.SpinningInAsync(0.1f, task, 0.1f);
            return;
        }

        [SerializeField] private GameObject signInCanvas;
        [SerializeField] private GameObject signUpCanvas;
        public void ChangeToSignInWindow()
        {
            signInCanvas.SetActive(true);
            signUpCanvas.SetActive(false);
        }

        public void ChangeToSignUpWindow()
        {
            signInCanvas.SetActive(false);
            signUpCanvas.SetActive(true);
        }
    
    }
}
