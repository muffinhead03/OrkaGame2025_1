using UnityEngine;

public class LanguageCollector2_3 : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] KoreanLines2_3 = {
        "내가 이겼다!",
        "어라, 방금 무슨 소리 들리지 않았어?",
        "무슨 소리? 잘못 들은 거겠지~ 나는 못 들었는걸",
        "그것보다도 다음 판 시작하자",
        "(하긴 이런 평화로운 곳에 그런 소리가...) 그래, 좋아!"
    };
    public readonly string[] EnglishLines2_3 = {

    };
    public readonly string[] KazaLines2_3 = {

    };

    public readonly string[] JapaneseLines2_3 = {

    };

    public readonly string[] ChineseLines2_3 = {

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
                return KoreanLines2_3;
            case "english":
                return EnglishLines2_3;
            case "kazahustan":
            case "kaza":
                return KazaLines2_3;
            case "japanese":
                return JapaneseLines2_3;
            case "chinese":
                return ChineseLines2_3;
            default:
                Debug.LogWarning($"[LanguageCollector2_3] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_3;
        }
    }

}
