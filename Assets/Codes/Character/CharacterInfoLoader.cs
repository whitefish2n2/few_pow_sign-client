using System.Collections.Generic;
using Codes.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Plugins; // (주인님의 MonoSingleton 경로)

public class CharacterInfoLoader : MonoSingleton<CharacterInfoLoader>
{
    // ID로 즉시 검색하기 위한 딕셔너리
    private Dictionary<string, CharacterData> characterDict = new Dictionary<string, CharacterData>();
    private Dictionary<int, CharacterData> idDict = new Dictionary<int, CharacterData>();
    public string CurrentDataVersion { get; private set; }
    public bool IsLoaded { get; private set; } = false;

    protected override void Initialize()
    {
        // 게임 시작 시 비동기로 데이터 로드 시작
        LoadDataAsync().Forget();
    }

    private async UniTaskVoid LoadDataAsync()
    {
        // 💡 [중요] 유니티 에디터에서 해당 JSON 파일의 Addressable 주소를 이 이름으로 맞춰주세요!
        string address = "Assets/CharacterData"; 

        try
        {
            // JSON 파일을 텍스트 에셋으로 로드
            TextAsset jsonAsset = await Addressables.LoadAssetAsync<TextAsset>(address);
            
            // 유니티 기본 JsonUtility를 이용해 파싱
            CharacterDatabase db = JsonUtility.FromJson<CharacterDatabase>(jsonAsset.text);
            CurrentDataVersion = db.dataVersion;

            // 딕셔너리에 싹 다 집어넣기
            idDict.Clear();
            foreach (var charData in db.characterList)
            {
                characterDict[charData.characterId] = charData;
                idDict[charData.id] = charData;
            }

            IsLoaded = true;
            Debug.Log($"✅ [데이터 로드 성공] 버전: {CurrentDataVersion} | 총 {characterDict.Count}명의 캐릭터 로드 완료!");
            
            // 다 쓴 TextAsset은 메모리에서 해제 (데이터는 딕셔너리에 남음)
            Addressables.Release(jsonAsset); 
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [데이터 로드 실패] 어드레서블 주소가 맞는지 확인하세요. Error: {e.Message}");
        }
    }

    // 다른 스크립트에서 캐릭터 정보를 요청할 때 쓰는 메서드
    public CharacterData GetCharacterData(string id)
    {
        if (characterDict.TryGetValue(id, out CharacterData data))
        {
            return data; // 데이터 던져주기
        }
        
        Debug.LogError($"⚠️ 존재하지 않는 캐릭터 ID입니다: {id}");
        return null;
    }

    public List<CharacterData> GetCharacterDataList()
    {
        return new List<CharacterData>(characterDict.Values);
    }
    
    public CharacterData GetById(int id)
    {
        if (idDict.TryGetValue(id, out var data)) return data;
        Debug.LogError($"존재하지 않는 캐릭터 숫자 id: {id}");
        return null;
    }
    public int CharacterIdToId(string characterId)
    {
        var d = GetCharacterData(characterId);
        return d != null ? d.id : -1;
    }
    public string IdToCharacterId(int id)
    {
        var d = GetById(id);
        return d != null ? d.characterId : string.Empty;
    }
}