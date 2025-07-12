using System.Collections.Generic;
using System.Linq;
using Codes.OutGame.Match;
using Codes.OutGame.PickCharacter;
using Codes.Util;
using UnityEngine;

/// <summary>
/// Select 버튼이랑 캐릭터 선택버튼 인터페이스
/// </summary>
public class CharacterPickInterface : MonoBungleton<CharacterPickInterface>
{
    private Dictionary<string, PickCharacterElement> characters = new Dictionary<string, PickCharacterElement>();

    private SelectButton selectButton;
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

    public void RegisterSelectButton(SelectButton b)
    {
        selectButton = b;
    }

    public void BeSelectedElement(string characterId)
    {
        if (characters.TryGetValue(characterId, out PickCharacterElement element))
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

    public void SelectCharacterFromPlayer(string characterId)
    {
        currentSelected = characterId;
        if (currentSelected == currentWatching)
        {
            selectButton.BeUnClickable();
        }
    }

    public void SelectCharacterTemporaryFromPlayer(string characterId)
    {
        currentWatching = characterId;
        if (currentSelected == currentWatching)
        {
            selectButton.BeUnClickable();
        }
    }
    public void ClickCharacter(string characterId)
    {
        _ = CharacterPickSceneManager.Instance.ClickCharacter(characterId);
    }

    public void SelectButton()
    {
        if (currentSelected != "")
        {
            
        }
        else return;
    }
}
