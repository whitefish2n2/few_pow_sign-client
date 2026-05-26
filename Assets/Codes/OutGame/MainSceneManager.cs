using System;
using Codes.OutGame.LoginUi.Dto;
using Codes.Util;
using NetTest;
using UnityEngine;

namespace Codes.OutGame
{
    public class MainSceneManager:MonoBungleton<MainSceneManager>
    {
        protected override void Initialize()
        { }

        private void Start()
        {
            _ = RequestClient.Instance.GetPlayerPrivateInfo(OnGetPlayerPrivateInfo,OnFailGetUserInfo,OnTimeOutUserInfo);
        }

        void OnGetPlayerPrivateInfo(ApiResponse<PlayerPrivateInformationDto> dto)
        {
            Debug.Log("GetPleyerPrivateInfo Success!!");
            ClientStatic.Instance.authId = dto.data.userId;
            ClientStatic.Instance.authName = dto.data.userName;
            ClientStatic.Instance.accountCreatedAt = dto.data.createdAt;
        }

        void OnFailGetUserInfo(ErrorResponse errorResponse)
        {
            //todo:
        }

        void OnTimeOutUserInfo()
        {
            //todo:
        }
    }
}