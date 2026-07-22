using System;
using System.Collections.Generic;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Codes.OutGame.PickCharacter
{
    public class CharacterPickSceneUIManager : MonoBungleton<CharacterPickSceneUIManager>
    {
        protected override void Initialize()
        { }

        [SerializeField] private RectTransform[] anotherPlayerPortraitPositionRects;
        [SerializeField] private GameObject anotherPlayerPortraitPrefab;
        [SerializeField] private GameObject currentPlayerPortraitInstance;
        [SerializeField] private LockInButton lockInButton; 
        private Dictionary<string, PickPagePlayerPortraitElement> playerPortraitElements = new();//key:userId, value: P.P.P.P.E

        public void Start()
        {
            CharacterPickSceneManager.Instance.SomeonePickCharacterTemporary += HoverCharacter;
            CharacterPickSceneManager.Instance.SomeonePickCharacter += LockInCharacter;
            InitPlayerElement();
        }

        

    
        public void InitPlayerElement()
        {
            var idx = 0;
            if (anotherPlayerPortraitPositionRects.Length == 0) return;
            foreach (var o in MatchMakeStatic.Instance.playerConstructor)
            {
                if (MatchMakeStatic.Instance.isCurrentPlayer(o))
                {
                    var ppppe = currentPlayerPortraitInstance.GetComponent<PickPagePlayerPortraitElement>();
                    ppppe.SetUserName(o.name);
                    playerPortraitElements.Add(o.id,ppppe);
                }
                else
                {
                
                    var obj = Instantiate(anotherPlayerPortraitPrefab, anotherPlayerPortraitPositionRects[(idx++)%anotherPlayerPortraitPositionRects.Length]);
                    var ppppe = obj.GetComponent<PickPagePlayerPortraitElement>();
                    playerPortraitElements.Add(o.id, ppppe);
                }
            }
        }

        public void HoverCharacter(CharacterPickNotifyDto dto)
        {
            _ = SelectCharacterAsAsync(dto);
        }
        public async UniTaskVoid SelectCharacterAsAsync(CharacterPickNotifyDto dto)
        {
            try
            {
                if (!playerPortraitElements.TryGetValue(dto.playerId, out var element))
                {
                    Debug.LogWarning($"No player portrait found for playerId: {dto.playerId}");
                    return;
                }

                element.SetCharacterKey(dto.characterId);
                var sprite = await ReusableSpriteHolder.Instance.GetCharacterPortraitSprite(dto.characterId);
                element.SetCharacterImage(sprite);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPick] Error while selecting character: {ex}");
            }
        }

        void LockInCharacter(CharacterPickNotifyDto dto)
        {
            _ = LockInCharacterAsAsync(dto);
        }
        public async UniTaskVoid LockInCharacterAsAsync(CharacterPickNotifyDto dto)
        {
            try
            {
                if (!playerPortraitElements.TryGetValue(dto.playerId, out var element))
                {
                    Debug.LogWarning($"No player portrait found for playerId: {dto.playerId}");
                    return;
                }

                if (ClientStatic.Instance.authId == dto.playerId)
                {
                    lockInButton.Disappear();
                }

                element.SetCharacterKey(dto.characterId);
                var sprite = await ReusableSpriteHolder.Instance.GetCharacterPortraitSprite(dto.characterId);
                element.SetCharacterImage(sprite);
                element.LockIn();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPick] Error while selecting character: {ex}");
            }
        }
        
        protected override void OnDestroy()
        {
            if (CharacterPickSceneManager.IsInitialized)
            {
                CharacterPickSceneManager.Instance.SomeonePickCharacterTemporary -= HoverCharacter;
                CharacterPickSceneManager.Instance.SomeonePickCharacter -= LockInCharacter;
            }
            
            base.OnDestroy();
        }

    
    }
}
