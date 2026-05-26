using Codes.OutGame;
using Codes.OutGame.PickCharacter;
using Codes.OutGame.PickCharacter.Dto;
using UnityEngine;

public class LockInButton : MonoBehaviour
{
    public void Click()
    {
        CharacterPickInterface.Instance.LockInCharacter(PickFlowStatic.Instance.GetCurrentPlayerCharacter());
    }

    public void Disappear()
    {
        gameObject.SetActive(false);
    }
}
