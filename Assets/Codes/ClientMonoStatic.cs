using System;
using NetTest;
using Plugins;
using UnityEngine;

public class ClientMonoStatic : MonoSingleton<ClientMonoStatic>
{

    protected override void Initialize()
    {
        RequestClient.Instance.OnTokenExpired = HandleJwtExpired;
        RequestClient.Instance.OnServerTimeout = () => {AlertManager.Instance.Alert("서버 통신에 실패했습니다.");};
    }

    
    private async void HandleJwtExpired(ErrorResponse errorResponse)
    {
        try
        {
            //Jwt, Refresh Token 만료 시 콜백 설정
            if (SceneLoadingManager.Instance == null || AlertManager.Instance == null)
            {
                Debug.LogError("Critical Singleton Missing(Scene Load Manager or Alert Manager)! Game will quit.");
                Application.Quit();
                return; // 안전빵
            }
            await SceneLoadingManager.Instance.LoadSceneWithLoadingSceneAsync(
                SceneEnum.Sign,
                () => { },
                null,
                async () => { AlertManager.Instance.AlertRetryableError(errorResponse); }
            );
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
