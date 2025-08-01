using UnityEngine;
using System;

public static class LanguageManager
{
    private static string currentLanguage;

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
                // ❗ SystemLanguage.Kazakh는 존재하지 않으므로 아래처럼 처리
                /*
                case SystemLanguage.Kazakh: // ⚠️ Unity에 존재하지 않음
                    currentLanguage = "kazakh";
                    break;
                */
                default:
                    // PlayerPrefs 등을 통해 카자흐스탄어 감지 시
                    string userLang = PlayerPrefs.GetString("user_language", "").ToLower();
                    if (userLang == "kazakh" || userLang == "kk")
                        currentLanguage = "kazakh";
                    else
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
            OnLanguageChanged?.Invoke(currentLanguage);
        }
    }

    public static string GetLanguage()
    {
        return currentLanguage;
    }
}