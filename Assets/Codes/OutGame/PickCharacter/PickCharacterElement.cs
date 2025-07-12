using System;
using Codes.OutGame.PickCharacter;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class PickCharacterElement : MonoBehaviour
{
    public string characterId;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Button pickButton;
    [SerializeField] private RectTransform pickButtonRect;
    [SerializeField] private bool isClickable = true;

    public LocalizedString localizedName;

    private void Awake()
    {
        localizedName.RefreshString();
        pickButtonRect = gameObject.GetComponent<RectTransform>();
        localizedName.StringChanged += value => {characterName.text = value;
            
        };
    }

    private void Start()
    {
        if(CharacterPickInterface.IsInitialized)
            CharacterPickInterface.Instance.RegisterPickCharacter(characterId,this);
    }

    public void ChangeCharacterName(string name)
    {
        characterName.text = name;
    }
    public void ChangeCharacterPortrait(Sprite sprite)
    {
        characterPortrait.sprite = sprite;
    }

    public void Click()
    {
        CharacterPickInterface.Instance.ClickCharacter(characterId);
    }

    public void BeUnClickable()
    {
        isClickable = false;
        //todo: 대충 버튼 모습 바꾸는 로 직 이 에 요
    }
    public void BeClickable()
    {
        isClickable = true;
        //todo: 대충 버튼 모습 바꾸는 로 직 이 에 요
    }
    public void OnMouseOver()
    {
        if (isClickable)
        {
            pickButtonRect.DOScale(Vector3.one*1.2f, 0.2f);
            //Cursor.SetCursor(,); todo
        }
            
    }

    public void OnMouseExit()
    {
        pickButtonRect.DOScale(Vector3.one, 0.2f);
    }
    
}
