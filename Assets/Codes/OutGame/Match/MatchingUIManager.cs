using System;
using Codes.Util;
using NetCode;
using NetTest;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

namespace Codes.OutGame.Match
{
    /// <summary>
    /// Bungleton On OutGame(Menu Scene)
    /// </summary>
    public class OutGameMatchController : MonoBungleton<OutGameMatchController>
    {
        protected override void Initialize() { }
        
        public event Action OnMatchFoundAction;
        public event Action OnMatchCanceledAction;
        public event Action OnMatchingStart;

        public event Action OnMatchTimout;

        private void Start()
        {
            MatchingWsManager.Instance.PrepareToNewMatch();
            MatchMakeStatic.Instance.PrepareToNewMatch();
            
            //이벤트 구독
            MatchingWsManager.Instance.OnMatchCanceled += OnMatchCancelled;
            MatchingWsManager.Instance.OnMatchFound += OnMatchFound;
            MatchingWsManager.Instance.OnTimeout += OnTimeout;
        }

        private void OnMatchFound(MatchFoundDto matchFoundDto)
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
            if (MatchingWsManager.IsInitialized)
            {
                MatchingWsManager.Instance.OnMatchCanceled -= OnMatchCancelled;
                MatchingWsManager.Instance.OnMatchFound -= OnMatchFound;
            }
            
            base.OnDestroy();
        }
    }
}
