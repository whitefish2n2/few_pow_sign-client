using System;
using System.Collections.Generic;
using Codes;
using Codes.OutGame.PickCharacter;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using UnityEngine;

public class CharacterPickSceneUIManager : MonoBungleton<CharacterPickInterface>
{
    protected override void Initialize()
    { }

    [SerializeField] private RectTransform[] anotherPlayerPortraitPositionRects;
    [SerializeField] private GameObject anotherPlayerPortraitPrefab;
    [SerializeField] private GameObject currentPlayerPortraitInstance;
    private Dictionary<string, PickPagePlayerPortraitElement> playerPortraitElements;//key:userId, value: P.P.P.P.E

    public void Start()
    {
        CharacterPickSceneManager.Instance.SomeonePickCharacter += SelectCharacter;
        InitPlayerElement();
    }

    
    public void InitPlayerElement()
    {
        var idx = 0;
        foreach (var o in MatchMakeStatic.Instance.playerConstructor)
        {
            if (MatchMakeStatic.Instance.isCurrentPlayer(o))
            {
                var ppppe = currentPlayerPortraitInstance?.GetComponent<PickPagePlayerPortraitElement>();
                ppppe!.SetUserName(o.Name);
                playerPortraitElements.Add(o.Id,ppppe);
            }
            else
            {
                if (anotherPlayerPortraitPositionRects.Length == 0) break;
                var obj = Instantiate(anotherPlayerPortraitPrefab, anotherPlayerPortraitPositionRects[(idx++)%anotherPlayerPortraitPositionRects.Length]);
                var ppppe = obj.GetComponent<PickPagePlayerPortraitElement>();
                playerPortraitElements.Add(o.Id, ppppe);
            }
                
        }
    }

    public void SelectCharacter(CharacterPickNotifyDto dto)
    {
        
    }

    
}
