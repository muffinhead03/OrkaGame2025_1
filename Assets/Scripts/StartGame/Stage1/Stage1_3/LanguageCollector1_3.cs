using UnityEngine;

public class LanguageCollector1_3 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines1_3 = {
        "됐다! 이제 나갈 수 있겠어",
        "어...라?"
    };
    public readonly string[] EnglishLines1_3 = { };
    public readonly string[] KazaLines1_3 = { };
    public readonly string[] JapaneseLines1_3 = { };
    public readonly string[] ChineseLines1_3 = { };

    /// <summary>
    /// 현재 설정된 언어에 따라 해당 대사 배열을 반환합니다.
    /// </summary>
    public string[] GetLines()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();

        switch (lang)
        {
            case "korean":
                return KoreanLines1_3;
            case "english":
                return EnglishLines1_3;
            case "kazahustan":
            case "kaza":
                return KazaLines1_3;
            case "japanese":
                return JapaneseLines1_3;
            case "chinese":
                return ChineseLines1_3;
            default:
                Debug.LogWarning($"[LanguageCollector1_3] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines1_3;
        }
    }
}
