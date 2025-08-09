using UnityEngine;

public class LanguageCollector2_3 : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] KoreanLines2_3 = {
        "내가 이겼다!",
        "어라, 방금 무슨 소리 들리지 않았어?",
        "무슨 소리? 잘못 들은 거겠지~ 나는 못 들었는걸",
        "그것보다도 다음 판 시작하자",
        "(하긴 이런 평화로운 곳에 그런 소리가...) 그래, 좋아!"
    };

    // ⬇⬇⬇ readonly 제거 + 인스펙터에 보이게 만들기
    [TextArea(2, 5)] public string[] EnglishLines2_3;
    [TextArea(2, 5)] public string[] KazaLines2_3;
    [TextArea(2, 5)] public string[] JapaneseLines2_3;
    [TextArea(2, 5)] public string[] ChineseLines2_3;

    // 언어 문자열을 깔끔하게 정규화
    private static string Normalize(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return "korean";
        lang = lang.Trim().ToLower();

        if (lang.StartsWith("en")) return "english";
        if (lang.StartsWith("ko")) return "korean";
        if (lang.StartsWith("ja")) return "japanese";
        if (lang.StartsWith("zh") || lang.Contains("chinese")) return "chinese";
        if (lang.StartsWith("kk") || lang.Contains("kaza")) return "kaza"; // 카자흐

        // 기존에 쓰던 오타 대응
        if (lang == "kazahustan") return "kaza";

        return lang;
    }

    public string[] GetLines()
    {
        string lang = Normalize(LanguageManager.GetLanguage());
        switch (lang)
        {
            case "korean":   return KoreanLines2_3;
            case "english":  return EnglishLines2_3;
            case "japanese": return JapaneseLines2_3;
            case "chinese":  return ChineseLines2_3;
            case "kaza":     return KazaLines2_3;
            default:
                Debug.LogWarning($"[LanguageCollector2_3] Unknown language '{lang}', fallback to Korean.");
                return KoreanLines2_3;
        }
    }
}