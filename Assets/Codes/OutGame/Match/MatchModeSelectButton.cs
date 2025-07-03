using Codes.OutGame.Match;
using NetTest;
using UnityEngine;

public class MatchModeSelectButton : MonoBehaviour
{
    [SerializeField] private GameMode currentGameMode;
    public void Click()
    {
        if(OutGameMatchController.IsInitialized)
            MatchingWsManager.Instance.ChangeGameMode(currentGameMode);
    }
}
