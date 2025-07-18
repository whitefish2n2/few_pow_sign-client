using System;
using System.Collections;
using Plugins;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageConfigManager : MonoSingleton<LanguageConfigManager>
{
    public event Action OnLocalizationChanged;
    private bool isChanging;

    protected override void Initialize()
    { }

    public bool SetLanguage(string code)
    {
        if (isChanging) return false;
        isChanging = true;
        StartCoroutine(SetLocaleCoroutine(code));
        return true;
    }
    private IEnumerator SetLocaleCoroutine(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = GetLocaleFromCode(code);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
    private Locale GetLocaleFromCode(string code)
    {
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == code)
                return locale;
        }
        Debug.LogWarning($"Locale code |{code}| not found.");
        return null;
    }
}
