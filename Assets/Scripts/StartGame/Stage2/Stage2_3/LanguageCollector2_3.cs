using UnityEngine;

public class LanguageCollector2_3 : MonoBehaviour
{
    // 기본 한글 대사 (필요 시 인스펙터에서 수정 가능)
    [TextArea(2, 5)]
    public string[] KoreanLines2_3 = {
        "내가 이겼다!",
        "어라, 방금 무슨 소리 들리지 않았어?",
        "무슨 소리? 잘못 들은 거겠지~ 나는 못 들었는걸",
        "그것보다도 다음 판 시작하자",
        "(하긴 이런 평화로운 곳에 그런 소리가...) 그래, 좋아!"
    };

    // 기본 영어 대사 (원하면 인스펙터에서 수정)
    [TextArea(2, 5)]
    public string[] EnglishLines2_3 = {
        "I did it...!",
        "Wait... did you hear that noise?",
        "What? That can't be right - I didn't hear a thing",
        "Never mind that, let's just start the next round",
        "(Well... there's no way a place this peaceful would have such a noise like that...) Okay...!"
    };

    // 다른 언어는 인스펙터에서 채우도록 공개
    [TextArea(2, 5)] public string[] JapaneseLines2_3;
    [TextArea(2, 5)] public string[] ChineseLines2_3;
    [TextArea(2, 5)] public string[] KazaLines2_3;

    // 언어 문자열 정규화
    private static string Normalize(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return "korean";
        lang = lang.Trim().ToLower();

        if (lang.StartsWith("en")) return "english";
        if (lang.StartsWith("ko")) return "korean";
        if (lang.StartsWith("ja")) return "japanese";
        if (lang.StartsWith("zh") || lang.Contains("chinese")) return "chinese";
        if (lang.StartsWith("kk") || lang.Contains("kaza") || lang == "kazahustan") return "kaza";

        return "korean";
    }

    public string[] GetLines()
    {
        string lang = Normalize(LanguageManager.GetLanguage());

        switch (lang)
        {
            case "english":  return Safe(EnglishLines2_3, KoreanLines2_3);
            case "japanese": return Safe(JapaneseLines2_3, KoreanLines2_3);
            case "chinese":  return Safe(ChineseLines2_3, KoreanLines2_3);
            case "kaza":     return Safe(KazaLines2_3, KoreanLines2_3);
            case "korean":
            default:         return Safe(KoreanLines2_3, KoreanLines2_3);
        }
    }

    // 널/빈 배열일 때 안전하게 폴백 제공
    private static string[] Safe(string[] primary, string[] fallback)
    {
        if (primary != null && primary.Length > 0) return primary;
        if (fallback != null && fallback.Length > 0) return fallback;
        return new string[0];
    }
}
