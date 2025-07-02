using System;
using DG.Tweening;
using UnityEngine;

namespace Codes.OutGame.Match
{
    public class MatchFoundPanel:MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            MatchingUIManager.Instance.OnMatchFoundAction += OnMatchFound;
            MatchingUIManager.Instance.OnMatchCanceledAction += OnMatchCanceled;
            gameObject.SetActive(false);
        }

        private void OnMatchFound()
        {
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, 0.3f);
        }

        private void OnMatchCanceled()
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                canvasGroup.alpha = 0;
            }
        }

        private void OnDestroy()
        {
            if(!MatchingUIManager.IsInitialized) return;
            MatchingUIManager.Instance.OnMatchFoundAction -= OnMatchFound;
            MatchingUIManager.Instance.OnMatchCanceledAction -= OnMatchCanceled;
        }
        
    }
}