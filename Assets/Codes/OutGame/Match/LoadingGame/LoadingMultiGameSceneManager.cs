using System;
using Codes.InGame;
using Codes.OutGame.PickCharacter;
using Codes.Util;
using Cysharp.Threading.Tasks;
using NetCode.ENetCode;
using NetTest;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Codes.OutGame.Match.LoadingGame
{
    public class LoadingMultiGameSceneManager : MonoBungleton<LoadingMultiGameSceneManager>
    {
        public event Action<string, float> OnUserProgressUpdated;
        private AsyncOperation inGameLoadOp;

        protected override void Initialize() { }

        private void Start()
        {
            // 시작하자마자 UI가 그릴 수 있도록 데이터 브로드캐스트!

            SubscribeNetworkEvents();
            ConnectAndAssignAsync().Forget();
        }

        private void SubscribeNetworkEvents()
        {
            if (EnetClient.Instance != null)
            {
                EnetClient.Instance.OnAssignSuccess += OnAssignSuccessHandler;
                EnetClient.Instance.OnPlayerProgressUpdated += HandleNetworkProgress;
                EnetClient.Instance.OnMapInitReceived += OnMapInitReceivedHandler;
                EnetClient.Instance.OnGeneratePlayerReceived += OnGeneratePlayerReceivedHandler;
            }
        }

        private async UniTaskVoid ConnectAndAssignAsync()
        {
            bool isConnected = await EnetClient.Instance.ConnectAsync(MatchMakeStatic.Instance.dedicatedIP, (ushort)MatchMakeStatic.Instance.dedicatedBasePort);
            if (isConnected)
            {
                AssignRequestDto assignReq = new AssignRequestDto {UserId = ClientStatic.Instance.authId, SessionId  = MatchMakeStatic.Instance.gameId, Key = MatchMakeStatic.Instance.userWebsocketKey };
                EnetClient.Instance.SendAssignPacket(assignReq);
            }
        }

        private void OnAssignSuccessHandler(AssignResponseDto dto)
        {
            InGameDataStatic.Instance.SetAssignData(dto.myPublicKey,dto.otherPlayers,ClientStatic.Instance.authId);

            LoadInGameSceneAsync().Forget();
        }

        private async UniTaskVoid LoadInGameSceneAsync()
        {
            inGameLoadOp = SceneManager.LoadSceneAsync(SceneEnum.TestScene.ToString(), LoadSceneMode.Single);
            inGameLoadOp.allowSceneActivation = false;

            while (inGameLoadOp.progress < 0.9f)
            {
                byte percent = (byte)(inGameLoadOp.progress * 100);
                
                // 내가 내 로딩 퍼센트를 갱신했음을 브로드캐스트 (UI가 듣고 갱신함)
                OnUserProgressUpdated?.Invoke(ClientStatic.Instance.authId, inGameLoadOp.progress);
                
                // 서버로도 전송
                EnetClient.Instance.SendProgressPacket(percent);
                
                await UniTask.Yield();
            }

            // 100% 완료
            OnUserProgressUpdated?.Invoke(ClientStatic.Instance.authId, 1f);
            EnetClient.Instance.SendProgressPacket(100);
        }

        private void HandleNetworkProgress(ProgressNotifyEventDto dto)
        {
            if (InGameDataStatic.Instance.keyToUserIdMap.TryGetValue(dto.publicKey, out string userId))
            {
                // 상대방의 로딩 퍼센트를 갱신했음을 브로드캐스트
                OnUserProgressUpdated?.Invoke(userId, dto.progressPercent / 100f); 
            }
        }

        private void OnMapInitReceivedHandler(MapInitDto dto)
        {
            InGameDataStatic.Instance.SetMapInitData(dto.objectNameMappings);
            if (inGameLoadOp != null)
                inGameLoadOp.allowSceneActivation = true;
        }

        private void OnGeneratePlayerReceivedHandler(GeneratePlayerDto dto)
        {
            InGameDataStatic.Instance.SetPlayerSpawnInfo(dto.players);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (EnetClient.Instance != null)
            {
                EnetClient.Instance.OnAssignSuccess -= OnAssignSuccessHandler;
                EnetClient.Instance.OnPlayerProgressUpdated -= HandleNetworkProgress;
                EnetClient.Instance.OnMapInitReceived -= OnMapInitReceivedHandler;
                EnetClient.Instance.OnGeneratePlayerReceived -= OnGeneratePlayerReceivedHandler;
            }
        }
    }
}