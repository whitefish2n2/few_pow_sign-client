using System;
using Codes;
using Codes.OutGame.PickCharacter;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using UnityEngine;

public class CharacterPickSceneUIManager : MonoBungleton<CharacterPickInterface>
{
    protected override void Initialize()
    { }

    public void Start()
    {
        CharacterPickSceneManager.Instance.SomeonePickCharacter += SelectCharacter;
    }

    public void InitPlayerElement()
    {
        foreach (var o in MatchMakeStatic.Instance.playerConstructor)
        {
            //o.
        }
    }

    public void SelectCharacter(CharacterPickNotifyDto dto)
    {
        
    }

    
}
