using System;
using NetCode;
using UnityEngine;

namespace Codes.OutGame.Match
{
    /// <summary>
    /// outgame Scene(메인 씬)에 있는 Match Found되었을떄 ClientStatic에 값 옮겨주는 친구
    /// </summary>
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
        
    }
}
