using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codes.OutGame.Match.LoadingGame
{
    public class LoadingUserElement : MonoBehaviour
    {
        [SerializeField] private Image portrait;
        [SerializeField] private Image loadingBar;
        [SerializeField] private TextMeshProUGUI userName;

        public void SetPlayer(Sprite portraitSprite, string playerName)
        {
            portrait.sprite = portraitSprite;
            userName.text = playerName;
        }

        public void SetProgress(float progress)
        {
            loadingBar.fillAmount = progress;
        }
    }
}
