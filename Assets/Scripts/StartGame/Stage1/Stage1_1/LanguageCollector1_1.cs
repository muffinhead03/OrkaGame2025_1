using UnityEngine;

public class LanguageCollector1_1 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines1_1 = {
        "(으음...아침...?)",
        "오늘은 며칠이지?",
        "(이 소리는?...아 오늘이 마을에서 음악 축제를 하는 날이구나)",
        "(꼭 가야만 해 오늘이 아니면 안돼)"
    };

    public readonly string[] EnglishLines1_1 = {
        "Hmm... is it morning?",
        "What day is it today?",
        "(This sound?... Today is the day for music festival)",
        "I have to go today"
    };

    public readonly string[] KazaLines1_1 = {
        "k", "k", "k", "kk"
    };

    public readonly string[] JapaneseLines1_1 = {
        "l", "l", "l", "l"
    };

    public readonly string[] ChineseLines1_1 = {
        "a", "a", "a", "a"
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
                return KoreanLines1_1;
            case "english":
                return EnglishLines1_1;
            case "kazahustan":
            case "kaza":
                return KazaLines1_1;
            case "japanese":
                return JapaneseLines1_1;
            case "chinese":
                return ChineseLines1_1;
            default:
                Debug.LogWarning($"[LanguageCollector1_1] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines1_1;
        }
    }

}