using TMPro;
using UnityEngine;

namespace Codes.InGame
{
    public class InGameHudUIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI myTeamScoreText;
        [SerializeField] private TextMeshProUGUI enemyTeamScoreText;

        private void Update()
        {
            if (!InGameLogicStatic.IsInitialized) return;

            phaseText.text = InGameLogicStatic.Instance.GetCurrentPhaseText();
            hpText.text = InGameLogicStatic.Instance.GetMyHpText();
            ammoText.text = InGameLogicStatic.Instance.GetMyAmmoText();
            myTeamScoreText.text = InGameLogicStatic.Instance.GetMyTeamScore().ToString();
            enemyTeamScoreText.text = InGameLogicStatic.Instance.GetBestEnemyTeamScore().ToString();
        }
    }
}
