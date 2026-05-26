using System;
using Codes.OutGame.Match;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using Cysharp.Threading.Tasks;
using NetCode;
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
            OutGameWsManager.Instance.OnCharacterPickNotify += OnCharacterPickNotify;
            OutGameWsManager.Instance.OnCharacterPickTemporaryNotify += OnTemporaryCharacterPickNotify;
            OutGameWsManager.Instance.OnStartGame += OnStartGame;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OutGameWsManager.Instance.OnCharacterPickNotify -= OnCharacterPickNotify;
            OutGameWsManager.Instance.OnCharacterPickTemporaryNotify -= OnTemporaryCharacterPickNotify;
            OutGameWsManager.Instance.OnStartGame -= OnStartGame;
        }

        public event Action<CharacterPickNotifyDto> SomeonePickCharacter;

        public void OnCharacterPickNotify(CharacterPickNotifyDto notifyDto)
        {
            SomeonePickCharacter?.Invoke(notifyDto);
        }
        
        public event Action<CharacterPickNotifyDto> SomeonePickCharacterTemporary;
        public void OnTemporaryCharacterPickNotify(CharacterPickNotifyDto notifyDto)
        {
            SomeonePickCharacterTemporary?.Invoke(notifyDto);
        }

        public event Action<StartGameDto> StartGame;
        
        public void OnStartGame(StartGameDto dto)
        {
            StartGame?.Invoke(dto);
        }
        public bool IsPlayerId(string userId)
        {
            return ClientStatic.Instance.authId == userId;
        }
        
        public async UniTask ClickCharacter(string characterId)
        {
            if(OutGameWsManager.Instance.IsConnected())
                await OutGameWsManager.Instance.ClickCharacter(characterId);
        }

        public async UniTask LockInCharacter(string characterId)
        {
            if (OutGameWsManager.Instance.IsConnected())
                await OutGameWsManager.Instance.LockInCharacter(characterId);
        }

    }
}
