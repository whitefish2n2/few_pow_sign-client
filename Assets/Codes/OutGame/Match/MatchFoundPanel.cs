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
            OutGameMatchController.Instance.OnMatchFoundAction += OnMatchFound;
            OutGameMatchController.Instance.OnMatchCanceledAction += OnMatchCanceled;
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
            if(!OutGameMatchController.IsInitialized) return;
            OutGameMatchController.Instance.OnMatchFoundAction -= OnMatchFound;
            OutGameMatchController.Instance.OnMatchCanceledAction -= OnMatchCanceled;
        }
        
    }
}