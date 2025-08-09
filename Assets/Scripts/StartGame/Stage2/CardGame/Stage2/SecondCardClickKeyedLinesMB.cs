using System;
using UnityEngine;

/// <summary>
/// SecondCard 전용: 언어별로 6줄(클릭 0~5) 대사를 직접 입력하는 MB.
/// SecondCardClickLinesDB에서 e.keyedLines.GetLine(lang, idx)로 사용됩니다.
/// </summary>
public class SecondCardClickKeyedLinesMB : MonoBehaviour
{
    private const int LINE_COUNT = 6;

    [Header("언어별 대사 (클릭 인덱스 0~5)")]
    [TextArea] public string[] koreanLines   = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] japaneseLines = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] englishLines  = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] chineseLines  = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] kazakhLines   = new string[LINE_COUNT] { "", "", "", "", "", "" };

    [Header("고정 인덱스 옵션(클릭해도 안 변함)")]
    [Tooltip("true면 clickIndex를 무시하고 fixedIndex만 사용합니다. -1이면 자막 없음 처리.")]
    public bool useFixedIndex = true;

    [Tooltip("자막 인덱스 고정값(-1=자막 없음, 0~5=해당 줄)")]
    [Range(-1, 5)]
    public int fixedIndex = -1;

    [Header("기타 옵션")]
    [Tooltip("요청한 언어가 없을 때 한국어 → 영어 → 첫 비어있지 않은 배열 순으로 대체")]
    public bool useFallback = true;

    [Tooltip("요청 인덱스를 0~(LINE_COUNT-1)로 자동 클램프(고정 모드 OFF일 때만 의미)")]
    public bool clampClickIndex = true;

    /// <summary>
    /// LanguageManager.GetLanguage()가 주는 코드와 idx로 1줄 반환
    /// </summary>
    public string GetLine(string languageCode, int clickIndex)
    {
        var arr = GetArrayByLanguageCode(languageCode);
        if (arr == null && useFallback) arr = GetFallbackArray();
        if (arr == null) return string.Empty;

        int idx;
        if (useFixedIndex)
        {
            // 고정 모드: -1이면 자막 없음, 0~5만 허용
            if (fixedIndex < 0) return string.Empty;
            idx = Mathf.Clamp(fixedIndex, 0, LINE_COUNT - 1);
        }
        else
        {
            // 기존 모드: 전달된 clickIndex 사용
            idx = clampClickIndex ? Mathf.Clamp(clickIndex, 0, LINE_COUNT - 1) : clickIndex;
            if (idx < 0 || idx >= arr.Length) return string.Empty;
        }

        return arr[idx] ?? string.Empty;
    }

    public string[] GetLines(string languageCode)
    {
        var arr = GetArrayByLanguageCode(languageCode);
        if (arr == null && useFallback) arr = GetFallbackArray();
        return ToFixedLength(arr, LINE_COUNT);
    }

    private string[] GetArrayByLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return null;
        var lang = languageCode.Trim().ToLowerInvariant();

        if (lang == "ko" || lang == "kr" || lang.StartsWith("ko-")) return SafeArray(koreanLines);
        if (lang == "ja" || lang.StartsWith("ja-"))                 return SafeArray(japaneseLines);
        if (lang == "en" || lang.StartsWith("en-"))                 return SafeArray(englishLines);
        if (lang == "zh" || lang.StartsWith("zh-") || lang == "cn" || lang == "zh-cn" || lang == "zh-tw")
            return SafeArray(chineseLines);
        if (lang == "kk" || lang == "kz" || lang.StartsWith("kk-")) return SafeArray(kazakhLines);

        return null;
    }

    private string[] GetFallbackArray()
    {
        if (HasAny(koreanLines))  return SafeArray(koreanLines);
        if (HasAny(englishLines)) return SafeArray(englishLines);
        if (HasAny(japaneseLines))return SafeArray(japaneseLines);
        if (HasAny(chineseLines)) return SafeArray(chineseLines);
        if (HasAny(kazakhLines))  return SafeArray(kazakhLines);
        return null;
    }

    private bool HasAny(string[] arr)
    {
        if (arr == null) return false;
        foreach (var s in arr) if (!string.IsNullOrEmpty(s)) return true;
        return false;
    }

    private string[] SafeArray(string[] arr)
    {
        if (arr == null) return new string[LINE_COUNT];
        if (arr.Length == LINE_COUNT) return arr;

        var fixedArr = new string[LINE_COUNT];
        for (int i = 0; i < Mathf.Min(LINE_COUNT, arr.Length); i++)
            fixedArr[i] = arr[i] ?? string.Empty;
        return fixedArr;
    }

    private string[] ToFixedLength(string[] arr, int len)
    {
        if (arr == null) return new string[len];
        if (arr.Length == len) return arr;

        var fixedArr = new string[len];
        for (int i = 0; i < Mathf.Min(len, arr.Length); i++)
            fixedArr[i] = arr[i] ?? string.Empty;
        return fixedArr;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        koreanLines   = SafeArray(koreanLines);
        japaneseLines = SafeArray(japaneseLines);
        englishLines  = SafeArray(englishLines);
        chineseLines  = SafeArray(chineseLines);
        kazakhLines   = SafeArray(kazakhLines);
    }
#endif
}
