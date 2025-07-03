using System;
using System.Threading.Tasks;
using Codes.OutGame.LoginUi;
using NetTest.Dto;
using UnityEngine;

public class SignUpForm : MonoBehaviour
{
    [SerializeField] private TmpInputFieldInterface id;
    [SerializeField] private TmpInputFieldInterface username;
    [SerializeField] private TmpInputFieldInterface password;
    [SerializeField] private TmpInputFieldInterface passwordConfirm;

    public async void Apply()
    {
        try
        {
            SignUpDto signDto = new SignUpDto();
            signDto.ID = id.tmpInputField.text;
            signDto.Name = username.tmpInputField.text;
            signDto.Password = password.tmpInputField.text;
            await LoginUiManager.Instance.SignUp(signDto);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void CheckPasswordConfirm()
    {
        if (password.tmpInputField.text != passwordConfirm.tmpInputField.text)
        {
            InvalidConfirm();
        }
    }

    public void CheckValidName()
    {
        if (username.tmpInputField.text.Contains("fuck"))
        {
            InvalidUsername();
        }
        //todo:닉네임 검열
    }

    public void CheckValidId()
    {
        //todo:아이디 검열
    }

    public void CheckValidPassword()
    {
        //todo:패스워드 규칙 정의
    }

    public void InvalidId()
    {
        id.WrongInput("Wrong Input.");
    }
    public void AlreadyExistsId()
    {
        id.WrongInput("Id already exists");
    }

    public void InvalidUsername()
    {
        username.WrongInput("Invalid Username.");
    }

    public void InvalidPassword()
    {
        password.WrongInput("Invalid Password.");
    }
    
    public void InvalidConfirm()
    {
        passwordConfirm.WrongInput("Password Is Not Matched");
    }
}
