using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public static string[] SupportedLanguages = { "en", "ja", "kk-KZ", "ko" };

    public static void SetLanguage(string localeCode)
    {
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString("language", localeCode);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning($"Locale '{localeCode}' not found.");
        }
    }

    public static IEnumerator ApplySavedLanguage()
    {
        yield return LocalizationSettings.InitializationOperation;

        string savedCode = PlayerPrefs.GetString("language", "en"); // 기본 영어
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(savedCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
}