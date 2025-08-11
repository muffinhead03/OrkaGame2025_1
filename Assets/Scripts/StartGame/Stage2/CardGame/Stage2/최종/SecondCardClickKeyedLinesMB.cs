using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Cards/Second Card Click Keyed Lines MB")]
public class SecondCardClickKeyedLinesMB : MonoBehaviour
{
    public const int LINE_COUNT = 6;

    [Header("Korean (index 0~5)")]
    [TextArea] public string[] korean   = new string[LINE_COUNT];

    [Header("Japanese (index 0~5)")]
    [TextArea] public string[] japanese = new string[LINE_COUNT];

    [Header("English (index 0~5)")]
    [TextArea] public string[] english  = new string[LINE_COUNT];

    [Header("Chinese (index 0~5)")]
    [TextArea] public string[] chinese  = new string[LINE_COUNT];

    [Header("Kazakh (index 0~5)")]
    [TextArea] public string[] kazakh   = new string[LINE_COUNT];

    [Header("Options")]
    [Tooltip("요청 언어 줄이 비어있으면 영어 → 첫 비어있지 않은 언어 순으로 폴백")]
    public bool useFallback = true;

    // --- Public API ---

    /// <summary>LanguageManager와 바로 연동</summary>
    public string GetLine(int index)
    {
        return GetLine(LanguageManager.GetLanguage(), index);
    }

    /// <summary>언어키와 0~5 인덱스로 한 줄 반환</summary>
    public string GetLine(string langKey, int index)
    {
        int idx = Mathf.Clamp(index, 0, LINE_COUNT - 1);

        var arr = GetArrayByLanguage(langKey);
        if (arr == null && useFallback) arr = GetFallbackArray();

        if (arr == null) return string.Empty;

        string line = (idx < arr.Length && arr[idx] != null) ? arr[idx] : string.Empty;

        // 마지막 안전장치: 비어있으면 영어 폴백 → 그래도 없으면 토큰
        if (string.IsNullOrWhiteSpace(line))
        {
            var en = Safe(english);
            if (idx < en.Length && !string.IsNullOrWhiteSpace(en[idx]))
                line = en[idx];
            else
                line = $"[{Canon(langKey)}{idx + 1}]";
        }

        return line;
    }

    // --- Internals ---

    private static string Canon(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) return "e";
        var s = lang.Trim().ToLowerInvariant();

        // 한국어
        if (s == "ko" || s.StartsWith("ko-") || s == "korean" || s == "k") return "k";
        // 일본어
        if (s == "ja" || s.StartsWith("ja-") || s == "japanese" || s == "j") return "j";
        // 영어
        if (s == "en" || s.StartsWith("en-") || s == "english" || s == "e") return "e";
        // 중국어
        if (s == "zh" || s.StartsWith("zh-") || s == "cn" || s == "chinese" || s == "c"
            || s == "zh-hans" || s == "zh-hant") return "c";
        // 카자흐 (ka/kk/kz 모두 허용)
        if (s == "ka" || s == "kk" || s == "kz" || s.StartsWith("ka-") || s.StartsWith("kk-")
            || s == "kazakh" || s == "kazakhstan" || s == "kazahustan") return "ka";

        return "e";
    }

    private string[] GetArrayByLanguage(string languageCode)
    {
        switch (Canon(languageCode))
        {
            case "k":  return Safe(korean);
            case "j":  return Safe(japanese);
            case "e":  return Safe(english);
            case "c":  return Safe(chinese);
            case "ka": return Safe(kazakh);
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
        for (int i = 0; i < arr.Length; i++)
            if (!string.IsNullOrWhiteSpace(arr[i])) return true;
        return false;
    }

    private static string[] Safe(string[] src)
    {
        var dst = new string[LINE_COUNT];
        if (src != null)
        {
            int n = Mathf.Min(LINE_COUNT, src.Length);
            for (int i = 0; i < n; i++)
                dst[i] = src[i] ?? string.Empty;
        }
        return dst;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 배열 길이/널 보정
        korean   = Safe(korean);
        japanese = Safe(japanese);
        english  = Safe(english);
        chinese  = Safe(chinese);
        kazakh   = Safe(kazakh);
    }

    [ContextMenu("DBG/Preview Current Language")]
    private void DBG_Preview()
    {
        string lang = LanguageManager.GetLanguage();
        string canon = Canon(lang);
        var arr = GetArrayByLanguage(lang) ?? new string[LINE_COUNT];
        Debug.Log($"[SecondCardLinesMB] lang='{lang}' canon='{canon}' → " +
                  $"0:'{arr[0]}', 1:'{arr[1]}', 2:'{arr[2]}', 3:'{arr[3]}', 4:'{arr[4]}', 5:'{arr[5]}'");
    }
#endif
}
