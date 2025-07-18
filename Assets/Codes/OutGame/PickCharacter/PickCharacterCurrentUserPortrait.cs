using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class PickCharacterCurrentUserPortrait : PickPagePlayerPortraitElement
{
    

    public void SetCharacterKey(string key)
    {
        nameLocal.TableEntryReference = key;
        nameLocal.RefreshString();
    }
}
