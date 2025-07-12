using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Codes.OutGame.Match
{
    public class SelectButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                // 선택 해제
                EventSystem.current.SetSelectedGameObject(null);
            });
        }

        private void Start()
        {
            CharacterPickInterface.Instance.RegisterSelectButton(this);
        }

        public void Click()
        {
            CharacterPickInterface.Instance.SelectButton();
        }

        public void BeUnClickable()
        {
            button.interactable = false;
        }

        public void BeClickable()
        {
            button.interactable = true;
        }
    }
}