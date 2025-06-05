using NetTest;
using NetTest.Dto;
using UnityEngine;

namespace Codes.OutGame.LoginUi
{
    public class SignInForm : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private string password;

        public void Apply()
        {
            SignInDto signDto = new SignInDto();
            signDto.ID = id;
            signDto.Password = password;
            //HttpRequestClient.Instance.SignIn();
        }
    }
}
