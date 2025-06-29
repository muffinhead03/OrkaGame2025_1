using UnityEngine;

public class LanguageCollector3_3 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines3_3 = {
        "좋아!",
        "우린 영원히 함께야, 에코!",

    };
    public readonly string[] EnglishLines3_3 = {

    };
    public readonly string[] KazaLines3_3 = {

    };

    public readonly string[] JapaneseLines3_3 = {

    };

    public readonly string[] ChineseLines3_3 = {

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
                return KoreanLines3_3;
            case "english":
                return EnglishLines3_3;
            case "kazahustan":
            case "kaza":
                return KazaLines3_3;
            case "japanese":
                return JapaneseLines3_3;
            case "chinese":
                return ChineseLines3_3;
            default:
                Debug.LogWarning($"[LanguageCollector3_3] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_3;
        }
    }

}
