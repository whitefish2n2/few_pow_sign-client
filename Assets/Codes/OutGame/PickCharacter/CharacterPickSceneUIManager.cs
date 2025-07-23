using System;
using System.Collections.Generic;
using Codes;
using Codes.OutGame.PickCharacter;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterPickSceneUIManager : MonoBungleton<CharacterPickInterface>
{
    protected override void Initialize()
    { }

    [SerializeField] private RectTransform[] anotherPlayerPortraitPositionRects;
    [SerializeField] private GameObject anotherPlayerPortraitPrefab;
    [SerializeField] private GameObject currentPlayerPortraitInstance;
    private Dictionary<string, PickPagePlayerPortraitElement> playerPortraitElements = new();//key:userId, value: P.P.P.P.E

    public void Start()
    {
        CharacterPickSceneManager.Instance.SomeonePickCharacter += SelectCharacter;
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
                ppppe.SetUserName(o.Name);
                playerPortraitElements.Add(o.Id,ppppe);
            }
            else
            {
                
                var obj = Instantiate(anotherPlayerPortraitPrefab, anotherPlayerPortraitPositionRects[(idx++)%anotherPlayerPortraitPositionRects.Length]);
                var ppppe = obj.GetComponent<PickPagePlayerPortraitElement>();
                playerPortraitElements.Add(o.Id, ppppe);
            }
        }
    }

    public void SelectCharacter(CharacterPickNotifyDto dto)
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
            var sprite = await ReusableSpriteHolder.Instance.GetSprite(dto.characterId);
            element.SetCharacterImage(sprite);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CharacterPick] Error while selecting character: {ex}");
        }
    }

    protected override void OnDestroy()
    {
        if (CharacterPickSceneManager.Instance != null)
            CharacterPickSceneManager.Instance.SomeonePickCharacter -= SelectCharacter;
        base.OnDestroy();
    }

    
}
