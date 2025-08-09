using UnityEngine;

public class LanguageCollector2_5 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines2_5 = {
        "뭐?",
        "너는 여기 있어야만 해",
    };
    public readonly string[] EnglishLines2_5 = {
    "WHAT?",
    "YOU MUST STAY HERE",
};
    public readonly string[] KazaLines2_5 = {

    };

    public readonly string[] JapaneseLines2_5 = {

    };

    public readonly string[] ChineseLines2_5 = {

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
                return KoreanLines2_5;
            case "english":
                return EnglishLines2_5;
            case "kazahustan":
            case "kaza":
                return KazaLines2_5;
            case "japanese":
                return JapaneseLines2_5;
            case "chinese":
                return ChineseLines2_5;
            default:
                Debug.LogWarning($"[LanguageCollector2_5] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_5;
        }
    }

}
