using System;
using Codes.InGame;
using Codes.Util;
using NetCode;
using NetTest;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

namespace Codes.OutGame.Match
{
    /// <summary>
    /// Bungleton On OutGame(Menu Scene)
    /// 메인 메뉴에서 매치 관련 로직 관리하는 오브젝트
    /// event 체이닝으로 매치 관련 이벤트 반응 가능
    /// </summary>
    public class OutGameMatchController : MonoBungleton<OutGameMatchController>
    {
        protected override void Initialize() { }
        
        public event Action OnMatchFoundAction;
        public event Action OnMatchCanceledAction;
        public event Action OnMatchMakingStart;

        public event Action OnMatchTimout;

        private void Start()
        {
            OutGameWsManager.Instance.PrepareToNewMatch();
            MatchMakeStatic.Instance.PrepareToNewMatch();
            InGameLogicStatic.Instance.PrepareToNewMatch();
            PickFlowStatic.Instance.PrepareToNewMatch();
            
            //이벤트 구독
            
            DeSubscribeEvents();
            OutGameWsManager.Instance.OnMatchMakingStarted += OnMatchMakingStarted;
            OutGameWsManager.Instance.OnMatchCanceled += OnMatchCancelled;
            OutGameWsManager.Instance.OnMatchFound += OnMatchFound;
            OutGameWsManager.Instance.OnTimeout += OnTimeout;
            OutGameWsManager.Instance.OnForcedLogout += GoToLoginSceneEvent;
            OutGameWsManager.Instance.OnConnectionFatalError += GoToLoginSceneEvent;
        }

        private void DeSubscribeEvents()
        {
            if (OutGameWsManager.TryGetInstance(out var wsManager))
            {
                wsManager.OnMatchMakingStarted -= OnMatchMakingStarted;
                wsManager.OnMatchCanceled -= OnMatchCancelled;
                wsManager.OnMatchFound -= OnMatchFound;
                wsManager.OnTimeout -= OnTimeout;
                wsManager.OnForcedLogout -= GoToLoginSceneEvent;
                wsManager.OnConnectionFatalError -= GoToLoginSceneEvent;
            }
        }
        private void OnMatchFound(MatchFoundDto startGameDto)
        {
            if (this == null) return;
            
            OnMatchFoundAction?.Invoke();
        }

        private void OnMatchCancelled()
        {
            if (this == null) return;
            
            OnMatchCanceledAction?.Invoke();
        }

        private void OnMatchMakingStarted()
        {
            if (this == null) return;
            Debug.Log("MatchMaking Start On MatchingUIManager");
            OnMatchMakingStart?.Invoke();
        }

        private void OnTimeout()
        {
            if (this == null) return;
            
            ClientMonoStatic.Instance.HandleCriticalOrShouldLoginError(ErrorResponse.ServerTimeout);
            OnMatchTimout?.Invoke();
        }

        private void GoToLoginSceneEvent(string message)
        {
            ClientMonoStatic.Instance.HandleCriticalOrShouldLoginError(new ErrorResponse(500, message));
        }
        
        public void Match()
        {
            _ = OutGameWsManager.Instance.Match(15);
        }

        public void Cancel()
        { 
            _ = OutGameWsManager.Instance.Cancel();
        }
        
        public void LogOut(){
            TokenHolder.instance.Clear();  
            SceneManager.LoadScene("LoginUi");
        }
        protected override void OnDestroy()
        {
            DeSubscribeEvents();
            
            base.OnDestroy();
        }
    }
}
