using Codes.OutGame.Match;
using NetTest;
using UnityEngine;

public class MatchModeSelectButton : MonoBehaviour
{
    [SerializeField] private GameMode currentGameMode;
    public void Click()
    {
        if(MatchingUIManager.IsInitialized)
            MatchingWsManager.Instance.ChangeGameMode(currentGameMode);
    }
}
