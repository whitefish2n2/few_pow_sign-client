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
    [SerializeField] private TextMeshProUGUI nameField;
    [SerializeField] private TextMeshProUGUI characterNameField;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Outline outline;
    public LocalizedString nameLocal;

    private void Awake()
    {
        nameLocal.StringChanged += value => { characterNameField.text = value; };
    }

    public void Init(string name,string characterNameHolder, Sprite defaultPortrait)
    {
        nameField.text = name;
        characterNameField.text = characterNameHolder;
        
        portraitImage.sprite = defaultPortrait;
    }

    public void SetCharacterKey(string key)
    {
        nameLocal.TableEntryReference = key;
        nameLocal.RefreshString();
    }

    public void SetCharacterImage(Sprite portrait)
    {
        portraitImage.sprite = portrait;
    }
    public void Pick(){}
}
