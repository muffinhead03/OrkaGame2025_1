using UnityEngine;

public class LanguageCollector2_4 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines2_4 = {
        "히히 좋아~ "
    };
    public readonly string[] EnglishLines2_4 = {
        "k"

    };
    public readonly string[] KazaLines2_4 = {
        "k"

    };

    public readonly string[] JapaneseLines2_4 = {
        "k"

    };

    public readonly string[] ChineseLines2_4 = {
        "k"

    };

    /// <summary>
    /// 현재 설정된 언어에 따라 해당 대사 배열을 반환합니다.
    /// </summary>
    public string[] GetLines()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();

        switch (lang)
        {
            case "korean":
                return KoreanLines2_4;
            case "english":
                return EnglishLines2_4;
            case "kazahustan":
            case "kaza":
                return KazaLines2_4;
            case "japanese":
                return JapaneseLines2_4;
            case "chinese":
                return ChineseLines2_4;
            default:
                Debug.LogWarning($"[LanguageCollector2_4] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_4;
        }
    }

}
