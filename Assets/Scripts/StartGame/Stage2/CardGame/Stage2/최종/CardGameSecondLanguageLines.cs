using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Game Second Language Lines", fileName = "CardGameSecondLanguageLines")]
public class CardGameSecondLanguageLines : ScriptableObject
{
    private const int LINE_COUNT = 6;

    [Header("언어별 6줄 (인덱스 0~5)")]
    [TextArea] public string[] korean   = new string[LINE_COUNT];
    [TextArea] public string[] japanese = new string[LINE_COUNT];
    [TextArea] public string[] english  = new string[LINE_COUNT];
    [TextArea] public string[] chinese  = new string[LINE_COUNT];
    [TextArea] public string[] kazakh   = new string[LINE_COUNT];

    [Header("옵션")]
    [Tooltip("요청 언어가 비어있으면 영어 → 다른 언어 순으로 폴백")]
    public bool useFallback = true;

    /// <summary>언어코드와 0~5 인덱스로 한 줄 반환</summary>
    public string GetLine(string languageCode, int index)
    {
        int idx = Mathf.Clamp(index, 0, LINE_COUNT - 1);

        var arr = GetArrayByLanguage(languageCode);
        if (arr == null && useFallback) arr = GetFallbackArray();

        if (arr == null) return string.Empty;

        string line = (idx < arr.Length) ? (arr[idx] ?? string.Empty) : string.Empty;

        // 최종 안전장치: 비었으면 영어 → 그래도 없으면 빈 문자열
        if (string.IsNullOrWhiteSpace(line))
        {
            var en = Safe(english);
            if (idx < en.Length && !string.IsNullOrWhiteSpace(en[idx])) line = en[idx];
        }

        return line ?? string.Empty;
    }

    // === 내부 유틸 ===

    private static string Canon(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) return "en";
        var s = lang.Trim().ToLowerInvariant();

        if (s == "ko" || s.StartsWith("ko-") || s == "korean" || s == "k") return "ko";
        if (s == "ja" || s.StartsWith("ja-") || s == "japanese" || s == "j") return "ja";
        if (s == "en" || s.StartsWith("en-") || s == "english" || s == "e") return "en";
        if (s == "zh" || s.StartsWith("zh-") || s == "cn" || s == "chinese" || s == "c"
            || s == "zh-hans" || s == "zh-hant") return "zh";
        if (s == "ka" || s == "kk" || s == "kz" || s.StartsWith("ka-") || s.StartsWith("kk-")
            || s == "kazakh" || s == "kazakhstan" || s == "kazahustan") return "kk";

        return "en";
    }

    private string[] GetArrayByLanguage(string languageCode)
    {
        switch (Canon(languageCode))
        {
            case "ko": return Safe(korean);
            case "ja": return Safe(japanese);
            case "en": return Safe(english);
            case "zh": return Safe(chinese);
            case "kk": return Safe(kazakh);
            default:   return Safe(english);
        }
    }

    private string[] GetFallbackArray()
    {
        if (HasAny(korean))   return Safe(korean);
        if (HasAny(english))  return Safe(english);
        if (HasAny(japanese)) return Safe(japanese);
        if (HasAny(chinese))  return Safe(chinese);
        if (HasAny(kazakh))   return Safe(kazakh);
        return null;
    }

    private static bool HasAny(string[] arr)
    {
        if (arr == null) return false;
        foreach (var s in arr) if (!string.IsNullOrWhiteSpace(s)) return true;
        return false;
    }

    private static string[] Safe(string[] src)
    {
        var dst = new string[LINE_COUNT];
        if (src != null)
        {
            for (int i = 0; i < Mathf.Min(LINE_COUNT, src.Length); i++)
                dst[i] = src[i] ?? string.Empty;
        }
        return dst;
    }
}
