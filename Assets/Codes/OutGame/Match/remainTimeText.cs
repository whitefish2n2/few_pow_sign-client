using System;
using Codes.OutGame;
using TMPro;
using UnityEngine;

public class remainTimeText : MonoBehaviour
{
    public TextMeshProUGUI text;

    
    private void Update()
    {
        if (PickFlowStatic.Instance != null)
        {
            text.text = PickFlowStatic.Instance.GetRemainingTime().ToString();
        }
    }
}
