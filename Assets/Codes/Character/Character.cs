using Codes.Character;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Identity")]
    public string myCharacterId;
    private CharacterData masterData;

    void Start()
    {
        InitCharacter();
    }

    public void InitCharacter()
    {
        // 매니저가 로딩될 때까지 기다리는 방어 로직이 필요할 수 있습니다.
        if (!CharacterInfoLoader.Instance.IsLoaded)
        {
            Debug.LogWarning("아직 마스터 데이터가 로드되지 않았습니다!");
            return;
        }

        // 1. 매니저에게 내 ID를 주고 스탯 원본을 받아옴
        masterData = CharacterInfoLoader.Instance.GetCharacterData(myCharacterId);
    }
}