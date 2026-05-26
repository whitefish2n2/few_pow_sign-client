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
    public class LoadingMultiGameSceneManager : MonoBungleton<LoadingMultiGameSceneManager>
    {
        public GameObject playerElement;
        [SerializeField] private GameObject leftPlayerHolder;
        [SerializeField] private GameObject rightPlayerHolder;
        [SerializeField] private TextMeshProUGUI mapName;
        [SerializeField] private Image mapImage;

        Dictionary<string, LoadingUserElement> userElements = new Dictionary<string, LoadingUserElement>();


        void Start()
        {
            _ = LoadPlayerElement(MatchMakeStatic.Instance.playerConstructor);
        }
        public async UniTaskVoid LoadPlayerElement(List<AnotherPlayerInfoDto> dto)
        {
            foreach (var p in dto)
            {
                var newElement = Instantiate(playerElement, leftPlayerHolder.transform, true);//dto에 team도 받자
                var elementComponent = newElement.GetComponent<LoadingUserElement>();
                userElements.Add(p.id, elementComponent);
                Sprite sprite = await ReusableSpriteHolder.Instance.GetCharacterPortraitSprite(p.characterId);
                elementComponent.SetPlayer(sprite,p.name);//todo: PlayerDto 손봐서 유저 아이콘같은 정보도ㅗ받게하죠, 아니면 플레이어 캐릭터 사진을 어디 매니저를 만들ㅇ서 관리하던가
            }
        }

        public void SetProgress(string playerId, float progress)
        {
            userElements[playerId].SetProgress(progress);
        }

        public void SetMapInfo(string newMapName, Sprite newMapImage)
        {
            this.mapName.text = newMapName;
            this.mapImage.sprite = newMapImage;
        }

        protected override void Initialize()
        { }
    }
}
