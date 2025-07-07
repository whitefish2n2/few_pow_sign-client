using System.Collections.Generic;
using System.Linq;
using Codes.Util;
using UnityEngine;

public class CharacterPickInterface : MonoBungleton<CharacterPickInterface>
{
    private Dictionary<string, PickCharacterElement> characters = new Dictionary<string, PickCharacterElement>();

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

    public void SelectElement(string characterId)
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
    public void DisSelectElement(string characterId)
    {
        if (characters.TryGetValue(characterId, out PickCharacterElement element))
        {
            element.BeClickable();
        }
    }
}
