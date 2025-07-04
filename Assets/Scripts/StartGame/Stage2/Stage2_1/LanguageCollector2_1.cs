using UnityEngine;

public class LanguageCollector2_1 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines2_1 = {
        "으음... ",
        "갑자기 숨이 막혀서... 아... 또 쓰러졌나보다...",
        "요즘 따라 숨이 더 자주 막히는 것 같아",
        "잠만, 여기는 어디야...!",
        "이 옷도 대체...! 난 분명 내 방에서 쓰러졌을 텐데...",
        "음... 주변을 둘러봐도 여기가 어딘지 전혀 모르겠어",
        "마치 이 세상에 존재하지 않을 것 같은 아름다운 장소야",
        "...이런 상상하기 싫지만",
        "결국 난 죽어서 천국에 온 건가?",
        "어라? 저 물가에 누군가가 있어"
    };
    public readonly string[] EnglishLines2_1 = {
        "k","k","k","k","k","k","k","k","k","k"
    };
    public readonly string[] KazaLines2_1 = {
        "k","k","k","k","k","k","k","k","k","k"
    };

    public readonly string[] JapaneseLines2_1 = {
        "k","k","k","k","k","k","k","k","k","k"
    };

    public readonly string[] ChineseLines2_1 = {
        "k","k","k","k","k","k","k","k","k","k"
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
                return KoreanLines2_1;
            case "english":
                return EnglishLines2_1;
            case "kazahustan":
            case "kaza":
                return KazaLines2_1;
            case "japanese":
                return JapaneseLines2_1;
            case "chinese":
                return ChineseLines2_1;
            default:
                Debug.LogWarning($"[LanguageCollector2_1] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_1;
        }
    }

}
