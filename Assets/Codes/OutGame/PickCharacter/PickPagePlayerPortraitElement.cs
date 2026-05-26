using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

// P.P.P.P.E
/// <summary>
/// 픽 화면에서 플레이어(자신 제외)들 초상화 코드
/// nameLocal에서 이래저래 바뀌면 이러쿵저러쿵 번역되는걸로 가요
/// </summary>
public class PickPagePlayerPortraitElement : MonoBehaviour
{ 
    [SerializeField] protected TextMeshProUGUI nameField;
    [SerializeField] protected TextMeshProUGUI characterNameField;
    [SerializeField] protected Image portraitImage;
    [SerializeField] protected Outline outline;
    public LocalizedString nameLocal;

    private void OnEnable()
    {
        nameLocal.StringChanged += UpdateCharacterName;
    }

    private void OnDisable()
    {
        nameLocal.StringChanged -= UpdateCharacterName;
    }
    private void UpdateCharacterName(string value)
    {
        if (characterNameField != null)
            characterNameField.text = value;
    }
    public void SetUserName(string name)
    {
        nameField.text = name;
    }
    
    public void SetCharacterKey(string key)
    {
        if (!key.StartsWith("local_character_name_"))
        {
            key =  "local_character_name_" + key;
        }
        nameLocal.TableEntryReference = key;
        nameLocal.RefreshString();
    }

    public void SetCharacterImage(Sprite portrait)
    {
        portraitImage.sprite = portrait;
    }

    public void LockIn()
    {
        portraitImage.color = Color.white;
    }
}
