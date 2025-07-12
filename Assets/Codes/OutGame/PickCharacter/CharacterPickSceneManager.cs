using System;
using Codes.OutGame.Match;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using Cysharp.Threading.Tasks;
using NetTest;
using UnityEngine;

namespace Codes.OutGame.PickCharacter
{
    public class CharacterPickSceneManager : MonoBungleton<CharacterPickSceneManager>
    {
        protected override void Initialize()
        { }

        private void Start()
        {
            //캐릭터 선택|현재 픽 현황 조회 등 이벤트 구독
            MatchingWsManager.Instance.OnCharacterPickNotify += OnCharacterPickNotify;
            MatchingWsManager.Instance.OnCharacterPickTemporaryNotify += OnTemporaryCharacterPickNotify;
        }

        public event Action<CharacterPickNotifyDto> SomeonePickCharacter;
        public event Action<CharacterPickNotifyDto> UserPickCharacter;

        public void OnCharacterPickNotify(CharacterPickNotifyDto notifyDto)
        {
            if (IsPlayerId(notifyDto.playerId))
                UserPickCharacter?.Invoke(notifyDto);
            else
                SomeonePickCharacter?.Invoke(notifyDto);
        }

        public event Action<CharacterPickNotifyDto> SomeonePickCharacterTemporary;
        public event Action<CharacterPickNotifyDto> UserPickCharacterTemporary;
        public void OnTemporaryCharacterPickNotify(CharacterPickNotifyDto notifyDto)
        {
            if (IsPlayerId(notifyDto.playerId))
                UserPickCharacterTemporary?.Invoke(notifyDto);
            else
                SomeonePickCharacterTemporary?.Invoke(notifyDto);
        }

        public bool IsPlayerId(string userId)
        {
            return ClientStatic.Instance.authId == userId;
        }
        
        public async UniTask ClickCharacter(string characterId)
        {
            await MatchingWsManager.Instance.ClickCharacter(MatchMakeStatic.Instance.gameId,MatchMakeStatic.Instance.userWebsocketKey, characterId);
        }

    }
}
