using System;
using System.Collections;
using NetCode;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace Codes.OutGame.Match
{
    public class MatchButton : MonoBehaviour
    { 
        [SerializeField] private Animator animator;
        private Button button;
        private Coroutine currentCoroutine;
        private TextMeshProUGUI buttonText;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            
            if (MatchingUIManager.IsInitialized)
            {
                MatchingUIManager.Instance.OnMatchingStart += BeMatching;
                MatchingUIManager.Instance.OnMatchCanceledAction += MatchCancel;
                MatchingUIManager.Instance.OnMatchFoundAction += OnMatchFound;
            }
            
        }

        bool isMatching = false;
        public void Click()
        {
            if (!isMatching)
            {
                if (MatchingUIManager.IsInitialized)
                    MatchingUIManager.Instance.Match();
                isMatching = true;
            }
            else
            {
                MatchingUIManager.Instance.Cancel();
            }
            
        }

        private void BeMatching()
        {
            currentCoroutine = StartCoroutine(Matching());
        }

        private int second = 0;
        private IEnumerator Matching()
        {
            second = 0;
            while (true)
            {
                buttonText.text = $"{(second/60):D2}:{(second%60):D2}";
                second++;
                yield return new WaitForSecondsRealtime(1);
            }
        }

        private void MatchCancel()
        {
            if(currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            buttonText.text = "Match";
            isMatching = false;
        }

        private void OnMatchFound()
        {
            if(currentCoroutine!=null) StopCoroutine(currentCoroutine);
        }
        private void OnDestroy()
        {
            if (MatchingUIManager.Instance)
            {
                MatchingUIManager.Instance.OnMatchingStart -= BeMatching;
                MatchingUIManager.Instance.OnMatchCanceledAction -= MatchCancel;
                MatchingUIManager.Instance.OnMatchFoundAction -= OnMatchFound;
            }
            
        }
    }
}
