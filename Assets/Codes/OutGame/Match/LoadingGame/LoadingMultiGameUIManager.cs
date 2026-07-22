using System.Collections.Generic;
using Codes.OutGame.PickCharacter;
using Codes.Util;
using Cysharp.Threading.Tasks;
using NetCode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codes.OutGame.Match.LoadingGame
{
    public class LoadingMultiGameUIManager : MonoBungleton<LoadingMultiGameUIManager>
    {
        public GameObject playerElement;
        [SerializeField] private GameObject leftPlayerHolder;
        [SerializeField] private GameObject rightPlayerHolder;
        [SerializeField] private TextMeshProUGUI mapName;
        [SerializeField] private Image mapImage;

        private Dictionary<string, LoadingUserElement> userElements = new Dictionary<string, LoadingUserElement>();

        protected override void Initialize() { }

        private void Start()
        {
            if (LoadingMultiGameSceneManager.IsInitialized)
            {
                LoadingMultiGameSceneManager.Instance.OnUserProgressUpdated += SetProgress;
            }
            HandleMatchDataReady(ref MatchMakeStatic.Instance.playerConstructor);
        }

        private void HandleMatchDataReady(ref List<AnotherPlayerInfoDto> dto)
        {
            LoadPlayerElement(dto).Forget();
            
        }

        private async UniTaskVoid LoadPlayerElement(List<AnotherPlayerInfoDto> dto)
        {
            foreach (var p in dto)
            {
                var newElement = Instantiate(playerElement, leftPlayerHolder.transform, true);
                var elementComponent = newElement.GetComponent<LoadingUserElement>();
                userElements.Add(p.id, elementComponent);
                
                Sprite sprite = await ReusableSpriteHolder.Instance.GetCharacterPortraitSprite(p.characterId);
                elementComponent.SetPlayer(sprite, p.name);
            }
        }

        private void SetProgress(string playerId, float progress)
        {
            if (userElements.ContainsKey(playerId))
            {
                userElements[playerId].SetProgress(progress);
            }
        }

        public void SetMapInfo(string newMapName, Sprite newMapImage)
        {
            this.mapName.text = newMapName;
            this.mapImage.sprite = newMapImage;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (LoadingMultiGameSceneManager.IsInitialized)
            {
                LoadingMultiGameSceneManager.Instance.OnUserProgressUpdated -= SetProgress;
            }
        }
    }
}