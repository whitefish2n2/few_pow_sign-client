using Codes.OutGame.Match.LoadingGame;
using Codes.Util;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Codes.OutGame.GameEnd
{
    public class GameEndUIManager : MonoBehaviour
    {
        public GameObject playerElement;   // LoadingUserElement 프리팹(로딩 씬이랑 동일 프리팹 재사용)
        [SerializeField] private Transform winningTeamHolder;
        [SerializeField] private TextMeshProUGUI winTeamText;

        private void Start()
        {
            winTeamText.text = $"Team {MatchMakeStatic.Instance.winningTeam} WIN!";
            LoadWinningTeamElements().Forget();
        }

        private async UniTaskVoid LoadWinningTeamElements()
        {
            byte winningTeam = MatchMakeStatic.Instance.winningTeam;

            foreach (var p in MatchMakeStatic.Instance.playerConstructor)
            {
                if (p.team != winningTeam) continue;

                var newElement = Instantiate(playerElement, winningTeamHolder, true);
                var elementComponent = newElement.GetComponent<LoadingUserElement>();

                Sprite sprite = await ReusableSpriteHolder.Instance.GetCharacterPortraitSprite(p.characterId);
                elementComponent.SetPlayer(sprite, p.name);
            }
        }

        // 버튼 OnClick에 연결
        public void OnClickBackToMain()
        {
            SceneLoadingManager.Instance.LoadSceneAsync(SceneEnum.OutgameSkeleton, null);
        }
    }
}
