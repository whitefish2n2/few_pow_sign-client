//TokenHolder로 이관

using System;
using System.Threading.Tasks;
using NetTest;
/*
namespace NetCode
{
    public class JwtRefresher
    {
        public static JwtRefresher Instance { get; private set; } = new();


        private void OnFail(ErrorResponse response)
        {
            if (response.code is (int)ExceptionCode.InvalidTokenException or (int)ExceptionCode.InvalidJwtException)
            {
                SceneLoadingManager.Instance.LoadSceneWithLoadingScene(
                    SceneEnum.Sign,
                    () => { },
                    null,
                    () => { AlertManager.Instance.AlertRetryableError(ErrorResponse.TokenExpired); }
                );
            }
            else
            {
                AlertManager.Instance.AlertError(response);
            }
        }
        public async Task<string> Refresh(string oldToken, string refreshToken)
        {
            JwtRefreshDto dto = new JwtRefreshDto(oldToken, refreshToken);
            var tcs = new TaskCompletionSource<string>();
            string newJwt = null;
            await HttpRequestClient.Instance.RefreshJwt(
                dto,
                r => tcs.TrySetResult(r.data),
                OnFail,
                () =>
                {
                    AlertManager.Instance.AlertError(ErrorResponse.ServerTimeout);
                    tcs.TrySetCanceled();
                }
            );
            return await tcs.Task;
        }

    }
}*/