using System;
using Codes.InGame;
using NetCode;
using UnityEngine;

namespace Codes.OutGame.Match
{
    
    public class MatchInformationSaver : MonoBehaviour
    {
        void Start()
        {
            MatchingWsManager.Instance.OnMatchFound += HandleAndSaveMatchFound;
        }

        private void HandleAndSaveMatchFound(MatchFoundDto dto)
        {
            ClientStatic.Instance.dedicatedBaseUrl = dto.url;
            ClientStatic.Instance.sessionIndex = Convert.ToUInt16(dto.sessionIndex);//todo 검증
            ClientStatic.Instance.sessionKey = dto.sessionVerifyKey;
        }

        void OnDestroy()
        {
            MatchingWsManager.Instance.OnMatchFound -= HandleAndSaveMatchFound;
        }
        
    }
}
