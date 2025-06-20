using System;
using Codes.Util;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

namespace Codes.OutGame.Match
{
    public class MatchingUIManager : MonoBungleton<MatchingUIManager>
    {
        protected override void Initialize() { }

        [SerializeField] private GameObject matchButton;

        private void Start()
        {
            matchButton.GetComponent<Button>();
            //이벤트 구독
            MatchingManager.Instance.PrepareToNewMatch();
            MatchingManager.Instance.OnMatchCanceled += OnMatchCancelled;
        }

        private void OnMatchFound()
        {
            
        }

        private void OnMatchCancelled()
        {
            AlertManager.Instance.Alert("Match cancelled");
        }
        
        public void Match()
        {
            _ = MatchingManager.Instance.Match(15);
        }

        protected override void OnDestroy()
        {
            //이벤트 구독 취소
            base.OnDestroy();
        }

        
    }
}
