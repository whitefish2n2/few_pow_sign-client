using NetTest;
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
        [SerializeField] private TextMeshProUGUI pingText;
        [SerializeField] private TextMeshProUGUI packetLossText;
        [SerializeField] private TextMeshProUGUI fpsText;

        private const float StatsRefreshInterval = 0.5f;   // 핑/유실률/fps는 매프레임 안 갱신, 0.5초마다
        private float statsTimer;
        private int frameCountSinceRefresh;

        private void Update()
        {
            if (!InGameLogicStatic.IsInitialized) return;

            phaseText.text = InGameLogicStatic.Instance.GetCurrentPhaseText();
            hpText.text = InGameLogicStatic.Instance.GetMyHpText();
            ammoText.text = InGameLogicStatic.Instance.GetMyAmmoText();
            myTeamScoreText.text = InGameLogicStatic.Instance.GetMyTeamScore().ToString();
            enemyTeamScoreText.text = InGameLogicStatic.Instance.GetBestEnemyTeamScore().ToString();

            frameCountSinceRefresh++;
            statsTimer += Time.unscaledDeltaTime;
            if (statsTimer >= StatsRefreshInterval)
            {
                if (EnetClient.IsInitialized)
                {
                    pingText.text = $"RTT:{EnetClient.Instance.GetPingMs()}ms";
                    packetLossText.text = $"loss:{EnetClient.Instance.GetPacketLossPercent():F1}%";
                }
                fpsText.text = $"fps:{frameCountSinceRefresh / statsTimer:F0}";

                statsTimer = 0f;
                frameCountSinceRefresh = 0;
            }
        }
    }
}
