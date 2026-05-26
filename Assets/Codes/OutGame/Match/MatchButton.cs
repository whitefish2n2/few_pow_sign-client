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
        private OutGameMatchController cachedController;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            
            if (OutGameMatchController.IsInitialized)
            {
                cachedController = OutGameMatchController.Instance;
                OutGameMatchController.Instance.OnMatchMakingStart += BeMatchMaking;
                OutGameMatchController.Instance.OnMatchCanceledAction += MatchCancel;
                OutGameMatchController.Instance.OnMatchFoundAction += OnMatchFound;
            }
            
        }

        bool isMatching = false;
        public void Click()
        {
            if (!isMatching)
            {
                if (OutGameMatchController.IsInitialized)
                    OutGameMatchController.Instance.Match();
            }
            else
            {
                OutGameMatchController.Instance.Cancel();
            }
            
        }

        private void BeMatchMaking()
        {
            currentCoroutine = StartCoroutine(Matching());
            isMatching = true;
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
            
            if (cachedController != null)
            {
                cachedController.OnMatchMakingStart -= BeMatchMaking;
                cachedController.OnMatchCanceledAction -= MatchCancel;
                cachedController.OnMatchFoundAction -= OnMatchFound;
            }
            
        }
    }
}
