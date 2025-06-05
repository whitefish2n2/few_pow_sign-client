using System;
using NetTest;
using TMPro;
using UnityEngine;

namespace Codes.OutGame.Modal
{
    public class RetryableErrorModal : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI errorCodeUGUI;
        [SerializeField] private TextMeshProUGUI errorBodyUGUI;
        private CanvasGroup modalCanvasGroup;

        private Action retryAction;
        public void Alert(ErrorResponse response, Action onRetry)
        {
            gameObject.SetActive(true);
            errorCodeUGUI.text = response.code.ToString();
            errorBodyUGUI.text = response.message;
            retryAction = onRetry;
        }
        public void ClickClose()
        {
            gameObject.SetActive(false);
            retryAction = null;
        }

        public void ClickRetry()
        {
            gameObject.SetActive(false);
            if (retryAction != null)
            {
                retryAction.Invoke();
            }
            retryAction = null;
        }
    
    }
}
