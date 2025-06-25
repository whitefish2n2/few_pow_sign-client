using System;
using Codes.Util;
using NetTest;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

namespace Codes.OutGame.Match
{
    /// <summary>
    /// Bungleton On OutGame(Menu Scene)
    /// </summary>
    public class MatchingUIManager : MonoBungleton<MatchingUIManager>
    {
        protected override void Initialize() { }
        
        public event Action OnMatchFoundAction;
        public event Action OnMatchCanceledAction;
        public event Action OnMatchingStart;

        public event Action OnMatchTimout;

        private void Start()
        {
            //이벤트 구독
            MatchingWsManager.Instance.PrepareToNewMatch();
            MatchingWsManager.Instance.OnMatchCanceled += OnMatchCancelled;
            MatchingWsManager.Instance.OnMatchStarted += OnMatchFound;
            MatchingWsManager.Instance.OnTimeout += OnTimeout;
        }

        private void OnMatchFound()
        {
            OnMatchFoundAction?.Invoke();
        }

        private void OnMatchCancelled()
        {
            OnMatchCanceledAction?.Invoke();
        }

        private void OnMatchStart()
        {
            OnMatchingStart?.Invoke();
        }

        private void OnTimeout()
        {
            ClientMonoStatic.Instance.HandleCriticalOrShouldLoginError(ErrorResponse.ServerTimeout);
            OnMatchTimout?.Invoke();
        }
        
        public void Match()
        {
            OnMatchStart();
            _ = MatchingWsManager.Instance.Match(15);
        }

        public void Cancel()
        { 
            MatchingWsManager.Instance.Cancel();
        }
        protected override void OnDestroy()
        {
            MatchingWsManager.Instance. OnMatchCanceled -= OnMatchCancelled;
            MatchingWsManager.Instance.OnMatchStarted -= OnMatchFound;
            base.OnDestroy();
        }
    }
}
