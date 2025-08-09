using System;
using UnityEngine;

/// <summary>
/// SecondCard 전용: 언어별로 6줄(클릭 0~5) 대사를 직접 입력하는 MB.
/// SecondCardClickLinesDB에서 e.keyedLines.GetLine(lang, idx)로 사용됩니다.
/// </summary>
public class SecondCardClickKeyedLinesMB : MonoBehaviour
{
    // 필요시 여기만 7로 바꾸면 모든 배열 길이가 따라갑니다.
    private const int LINE_COUNT = 6;

    [Header("언어별 대사 (클릭 인덱스 0~5)")]
    [TextArea] public string[] koreanLines     = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] japaneseLines   = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] englishLines    = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] chineseLines    = new string[LINE_COUNT] { "", "", "", "", "", "" };
    [TextArea] public string[] kazakhLines     = new string[LINE_COUNT] { "", "", "", "", "", "" }; // Kazakhstan → Kazakh(kk)

    [Header("옵션")]
    [Tooltip("요청한 언어가 없을 때 한국어 → 영어 → 첫 비어있지 않은 배열 순으로 대체")]
    public bool useFallback = true;

    [Tooltip("요청 인덱스를 0~(LINE_COUNT-1)로 자동 클램프")]
    public bool clampClickIndex = true;

    /// <summary>
    /// LanguageManager.GetLanguage()가 주는 코드와 idx로 1줄 반환
    /// </summary>
    public string GetLine(string languageCode, int clickIndex)
    {
        var arr = GetArrayByLanguageCode(languageCode);

        if (arr == null && useFallback)
            arr = GetFallbackArray();

        if (arr == null) return string.Empty;

        int idx = clampClickIndex
            ? Mathf.Clamp(clickIndex, 0, LINE_COUNT - 1)
            : clickIndex;

        if (idx < 0 || idx >= arr.Length) return string.Empty;

        return arr[idx] ?? string.Empty;
    }

    /// <summary>
    /// 해당 언어의 전체 줄(길이 LINE_COUNT 보장)을 반환
    /// </summary>
    public string[] GetLines(string languageCode)
    {
        var arr = GetArrayByLanguageCode(languageCode);
        if (arr == null && useFallback)
            arr = GetFallbackArray();

        return ToFixedLength(arr, LINE_COUNT);
    }

    private string[] GetArrayByLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return null;

        // 소문자/트림 정규화
        var lang = languageCode.Trim().ToLowerInvariant();

        // 한국어
        if (lang == "ko" || lang == "kr" || lang.StartsWith("ko-"))
            return SafeArray(koreanLines);

        // 일본어
        if (lang == "ja" || lang.StartsWith("ja-"))
            return SafeArray(japaneseLines);

        // 영어
        if (lang == "en" || lang.StartsWith("en-"))
            return SafeArray(englishLines);

        // 중국어(간/번 통합)
        if (lang == "zh" || lang.StartsWith("zh-") || lang == "cn" || lang == "zh-cn" || lang == "zh-tw")
            return SafeArray(chineseLines);

        // 카자흐어(kk가 표준, 때때로 kz로 들어오는 경우도 매핑)
        if (lang == "kk" || lang == "kz" || lang.StartsWith("kk-"))
            return SafeArray(kazakhLines);

        return null;
    }

    private string[] GetFallbackArray()
    {
        // 선호 순서: 한국어 → 영어 → 그다음 비어있지 않은 첫 배열
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
        // 방어적으로 길이 보정
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
        // 에디터에서 배열 길이 자동 보정
        koreanLines   = SafeArray(koreanLines);
        japaneseLines = SafeArray(japaneseLines);
        englishLines  = SafeArray(englishLines);
        chineseLines  = SafeArray(chineseLines);
        kazakhLines   = SafeArray(kazakhLines);
    }
#endif
}
