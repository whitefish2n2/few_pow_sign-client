using System;
using System.Collections.Generic;
using System.Timers;
using Codes.InGame;
using Codes.OutGame.Match;
using Codes.OutGame.PickCharacter.Dto;
using MapFile.MapCode;
using NetCode;
using Plugins;
using UnityEngine;

namespace Codes.OutGame
{
    public class PickFlowStatic:MonoSingleton<PickFlowStatic>
    {
        Dictionary<string, AnotherPlayerInfoDto> teamPlayerInfos;
        public long pickEndTime;

        public int GetRemainingTime()
        {
            long currentMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int remainingSeconds = (int)((pickEndTime - currentMillis) / 1000);
            return Math.Max(0, remainingSeconds);
        }
        protected override void Initialize()
        {
            teamPlayerInfos = new Dictionary<string, AnotherPlayerInfoDto>();
        }

        protected override void Start()
        {
            base.Start();
            OutGameWsManager.Instance.OnMatchFound += OnMatchFound;
            OutGameWsManager.Instance.OnCharacterPickTemporaryNotify += OnHoverCharacterSomeone;
            OutGameWsManager.Instance.OnCharacterPickNotify += OnPickCharacterSomeone;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (OutGameWsManager.Instance != null)
            {
                OutGameWsManager.Instance.OnMatchFound -= OnMatchFound;
                OutGameWsManager.Instance.OnCharacterPickTemporaryNotify -= OnHoverCharacterSomeone;
                OutGameWsManager.Instance.OnCharacterPickNotify -= OnPickCharacterSomeone;
            }
            
        }

        public void PrepareToNewMatch()
        {
            teamPlayerInfos.Clear();
        }
        
        public void OnMatchFound(MatchFoundDto dto)
        {
            teamPlayerInfos.Clear();
            foreach (var p in dto.teamPlayers)
            {
                teamPlayerInfos.Add(p.id,p);
            }

            pickEndTime = dto.pickEndTime;
        }

        public void OnHoverCharacterSomeone(CharacterPickNotifyDto dto)
        {
            teamPlayerInfos[dto.playerId].characterId = dto.characterId;
        }

        public void OnPickCharacterSomeone(CharacterPickNotifyDto dto)
        {
            Debug.Log(dto.playerId + " Lock In to:  " + dto.characterId);
            teamPlayerInfos[dto.playerId].characterId = dto.characterId;
            teamPlayerInfos[dto.playerId].isLockedIn = true;
        }

        public string GetCurrentPlayerCharacter()
        {
            return teamPlayerInfos[ClientStatic.Instance.authId].characterId;
        }
    }
}