using System;
using MapFile.MapCode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPickSceneManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private Image mapImage;

    private void Start()
    {
        if (MatchMakeStatic.IsInitialized)
        {
            mapNameText.text = "NOW LOADING | " + MatchMakeStatic.Instance.map;
            if(Map.GetMapImage(MatchMakeStatic.Instance.map))
                mapImage.sprite = Map.GetMapImage(MatchMakeStatic.Instance.map);
        }
    }
}