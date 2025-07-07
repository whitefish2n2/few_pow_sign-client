using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickCharacterElement : MonoBehaviour
{
    public string characterId;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Button pickButton;
    [SerializeField] private RectTransform pickButtonRect;
    [SerializeField] private bool isClickable;

    private void Awake()
    {
        pickButtonRect = gameObject.GetComponent<RectTransform>();
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

    public void CLick()
    {
        
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
        if(isClickable)
            pickButtonRect.DOScale(Vector3.one*1.2f, 0.2f);
    }

    public void OnMouseExit()
    {
        pickButtonRect.DOScale(Vector3.one, 0.2f);
    }
    
}
