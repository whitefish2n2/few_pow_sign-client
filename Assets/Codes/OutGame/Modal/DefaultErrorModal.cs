using System;
using NetTest;
using TMPro;
using UnityEngine;

namespace Codes.OutGame.Modal
{
    public class DefaultErrorModal:MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI errorCodeUGUI;
        [SerializeField] private TextMeshProUGUI errorBodyUGUI;
        private CanvasGroup modalCanvasGroup;
        public event Action OnClickClose;
        public void Alert(ErrorResponse response)
        {
            gameObject.SetActive(true);
            errorCodeUGUI.text = response.code.ToString();
            errorBodyUGUI.text = response.message;
        }
        public void ClickClose()
        {
            OnClickClose?.Invoke();
            gameObject.SetActive(false);
        }

        public void ClickQuitGame()
        {
            Application.Quit();
        }
    }
}