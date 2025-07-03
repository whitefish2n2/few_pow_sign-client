using System;
using NetTest;
using NetTest.Dto;
using UnityEngine;
using UnityEngine.Rendering;

namespace Codes.OutGame.LoginUi
{
    public class SignInForm : MonoBehaviour
    {
        
        [SerializeField] private TmpInputFieldInterface id;
        [SerializeField] private TmpInputFieldInterface password;

        public async void Apply()
        {
            try
            {
                SignInDto signDto = new SignInDto();
                signDto.ID = id.tmpInputField.text;
                signDto.Password = password.tmpInputField.text; ;
                await LoginUiManager.Instance.SignIn(signDto);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void InvalidId()
        {
            id.WrongInput("User Not Found.");
        }

        public void InvalidPassword()
        {
            password.WrongInput("Wrong Password.");
        }
    }
}
