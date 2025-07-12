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
            //todo: 매치 정보 파일에 저장하고 재접속 준비
        }

        void OnDestroy()
        {
            MatchingWsManager.Instance.OnMatchFound -= HandleAndSaveMatchFound;
        }
        
    }
}
