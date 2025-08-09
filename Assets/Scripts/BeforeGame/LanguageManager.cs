using UnityEngine;
using System;

public static class LanguageManager
{
    // PlayerPrefs 키
    private const string PrefKey = "user_language";

    // 내부에선 항상 소문자 표준키만 사용
    // "korean", "english", "chinese", "japanese", "kazakh"
    private static string currentLanguage;

    // 세팅 패널에서 언어 바꾸는 동안 구독자들이 무시하도록 쓸 수 있는 플래그
    public static bool IsLanguageSwitching { get; set; } = false;

    public static event Action<string> OnLanguageChanged;

    // 런타임 시작 시 한 번 보장
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RuntimeInit()
    {
        currentLanguage = null; // 도중 재컴파일 대비 리셋
    }

    /// <summary>외부에서 안 불러도 안전: 필요 시 자동 초기화</summary>
    public static void Initialize()
    {
        if (!string.IsNullOrEmpty(currentLanguage)) return;

        // 1) PlayerPrefs 우선
        var saved = PlayerPrefs.GetString(PrefKey, string.Empty);
        if (!string.IsNullOrEmpty(saved))
        {
            currentLanguage = Normalize(saved);
            if (string.IsNullOrEmpty(currentLanguage)) currentLanguage = "english";
            return;
        }

        // 2) 시스템 언어 추론
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:   currentLanguage = "korean";   break;
            case SystemLanguage.Japanese: currentLanguage = "japanese"; break;

            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                currentLanguage = "chinese"; break;

            // Unity에 Kazakh 없음 → 지역/사용자 설정 추론
            default:
                // 사용자 설정에서 가져오기 (kk, kazakh 등)
                string userLang = PlayerPrefs.GetString(PrefKey, "").ToLowerInvariant();
                if (userLang == "kazakh" || userLang == "kk")
                    currentLanguage = "kazakh";
                else
                    currentLanguage = "english";
                break;
        }

        // 기본값 저장
        PlayerPrefs.SetString(PrefKey, currentLanguage);
        PlayerPrefs.Save();
    }

    /// <summary>언어 설정(대소문자/다양 표기 허용). 이벤트 발생.</summary>
    public static void SetLanguage(string lang)
    {
        Initialize();

        string norm = Normalize(lang);
        if (string.IsNullOrEmpty(norm)) norm = "english";

        if (currentLanguage == norm) return;

        currentLanguage = norm;

        // 저장
        PlayerPrefs.SetString(PrefKey, currentLanguage);
        PlayerPrefs.Save();

        // 알림
        OnLanguageChanged?.Invoke(currentLanguage);
    }

    /// <summary>현재 언어(표준키) 얻기. 자동 초기화.</summary>
    public static string GetLanguage()
    {
        Initialize();
        return currentLanguage;
    }

    /// <summary>다양한 표기를 표준키로 정규화</summary>
    private static string Normalize(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return "english";

        lang = lang.Trim().ToLowerInvariant();

        // 오타/변형들 보정
        if (lang == "kazahustan" || lang == "kazahstan" || lang == "kazakhstan" || lang == "kazakh" || lang == "kk")
            return "kazakh";

        if (lang == "ko" || lang.StartsWith("ko-") || lang == "korean")
            return "korean";

        if (lang == "ja" || lang.StartsWith("ja-") || lang == "japanese")
            return "japanese";

        // 중국어 다양한 입력 허용
        if (lang == "zh" || lang.StartsWith("zh-") || lang.Contains("chinese") || lang == "chinese" ||
            lang == "zh_cn" || lang == "zh-hans" || lang == "zh-hant")
            return "chinese";

        if (lang == "en" || lang.StartsWith("en-") || lang == "english")
            return "english";

        // 마지막 기본값
        return "english";
    }
}
