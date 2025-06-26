using UnityEngine;
using System;

public static class LanguageManager
{
    private static string currentLanguage;

    // ✅ 언어 변경 이벤트
    public static event Action<string> OnLanguageChanged;

    public static void Initialize()
    {
        if (string.IsNullOrEmpty(currentLanguage))
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    currentLanguage = "korean";
                    break;
                case SystemLanguage.Japanese:
                    currentLanguage = "japanese";
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    currentLanguage = "chinese";
                    break;
                default:
                    currentLanguage = "english";
                    break;
            }
        }
    }

    public static void SetLanguage(string lang)
    {
        lang = lang.Trim().ToLower();

        if (currentLanguage != lang)
        {
            currentLanguage = lang;
            OnLanguageChanged?.Invoke(currentLanguage); // ✅ 변경 통보
        }
    }

    public static string GetLanguage()
    {
        return currentLanguage;
    }
}