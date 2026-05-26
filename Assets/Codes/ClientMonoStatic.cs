using System;
using NetTest;
using Plugins;
using UnityEngine;

public class ClientMonoStatic : MonoSingleton<ClientMonoStatic>
{

    protected override void Initialize()
    {
        RequestClient.Instance.OnTokenExpired = TokenExpired;
        RequestClient.Instance.OnServerTimeout = () => {AlertManager.Instance.Alert("서버 통신에 실패했습니다.");};
    }

    
    /// <summary>
    /// 토큰이 만료되었을때 로그인으로 보내는 로직
    /// </summary>
    /// <param name="errorResponse"></param>
    public void TokenExpired(ErrorResponse errorResponse)
    {
        ClientMonoStatic.Instance.HandleCriticalOrShouldLoginError(ErrorResponse.TokenExpired);
    }


    private void CheckCriticalSingleton()
    {
        if (SceneLoadingManager.Instance == null || AlertManager.Instance == null)
        {
            Debug.LogError("Critical Singleton Missing(Scene Load Manager or Alert Manager)! Game will quit.");
            Application.Quit();
            return; // 안전빵
        }
    }

    /// <summary>
    /// 다시 로그인해야하거나(토큰 만료) 복구 불가능한 에러가 발생했을떄 로그인화면으로 보내버리는 친구
    /// </summary>
    /// <param name="errorResponse"></param>
    public void HandleCriticalOrShouldLoginError(ErrorResponse errorResponse)
    {
        try
        {
            CheckCriticalSingleton();
            SceneLoadingManager.Instance.LoadSceneWithLoadingScene(
                SceneEnum.LoginUi,
                SceneEnum.Loading,
                () => { },
                null,
                async () => { AlertManager.Instance.AlertError(errorResponse); }
            );
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
