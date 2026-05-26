using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codes.Character;
using Codes.OutGame.Match;
using Codes.OutGame.PickCharacter;
using Codes.OutGame.PickCharacter.Dto;
using Codes.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Select 버튼이랑 캐릭터 선택버튼 인터페이스
/// </summary>
public class CharacterPickInterface : MonoBungleton<CharacterPickInterface>
{
    [SerializeField] private GameObject atkBox;
    [SerializeField] private GameObject defBox;
    [SerializeField] private GameObject supBox;
    [SerializeField] private GameObject elementPrefab;
    private Dictionary<string, PickCharacterElement> characters = new Dictionary<string, PickCharacterElement>();

    private LockInButton lockInButton;
    private string currentSelected = "";//현재 선택된 캐릭터 id
    private string currentWatching = "";//지금 보는 캐릭터 id
    protected override void Initialize()
    { }

    /// <summary>
    /// PickCharacterElement에서 Start()에 자신을 등록하는 함수(addressable 호?환)
    /// </summary>
    /// <param name="characterId"></param>
    /// <param name="element"></param>
    public void RegisterPickCharacter(string characterId, PickCharacterElement element)
    {
        characters.Add(characterId, element);
    }

    private async void Start()
    {
        CharacterPickSceneManager.Instance.SomeonePickCharacter += BeSelectedElement;
        var characterList =CharacterInfoLoader.Instance.GetCharacterDataList();
        foreach (var character in characterList)
        {
            CharacterRole role = CharacterRoleExtensions.ToRoleEnum(character.role);
            if (CharacterRole.attack == role)
            {
                var obj = Instantiate(elementPrefab, atkBox.transform);
                var comp = obj.GetComponent<PickCharacterElement>();
                comp.SetCharacterKey(character.characterId);
                _ = SetElementSprite(comp,character.characterId);
            }
            if (CharacterRole.defense == role)
            {
                var obj = Instantiate(elementPrefab, defBox.transform);
                var comp = obj.GetComponent<PickCharacterElement>();
                comp.SetCharacterKey(character.characterId);
                _ = SetElementSprite(comp,character.characterId);
            }
            if (CharacterRole.support == role)
            {
                var obj = Instantiate(elementPrefab, supBox.transform);
                var comp = obj.GetComponent<PickCharacterElement>();
                comp.SetCharacterKey(character.characterId);
                _ = SetElementSprite(comp,character.characterId);
            }
        }
    }

    async UniTaskVoid SetElementSprite(PickCharacterElement element,  string characterId)
    {
        var sprite = await ReusableSpriteHolder.Instance.GetCharacterPortraitSprite(characterId);
        element.ChangeCharacterPortrait(sprite);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CharacterPickSceneManager.Instance.SomeonePickCharacter -= BeSelectedElement;
    }

    public void BeSelectedElement(CharacterPickNotifyDto dto)
    {
        if (characters.TryGetValue(dto.characterId, out PickCharacterElement element))
        {
            element.BeUnClickable();
        }
        else
        {
            //todo: 대충 소켓 끊고 메인으로 보내버리기
        }
    }
    public void BeSelectableElement(string characterId)
    {
        if (characters.TryGetValue(characterId, out PickCharacterElement element))
        {
            element.BeClickable();
        }
    }
    
    public void ClickCharacter(string characterId)
    {
        _ = CharacterPickSceneManager.Instance.ClickCharacter(characterId);
    }

    public void LockInCharacter(string characterId)
    {
        _ = CharacterPickSceneManager.Instance.LockInCharacter(characterId);
    }
    
}
