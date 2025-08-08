using UnityEngine;

public class LanguageCollector1_2 : MonoBehaviour
{
    // 언어별 대사 배열 (빈 배열로 변경)
    public readonly string[] KoreanLines1_2 = {
        "아빠...",
        "야 아침이다!",
        "오늘만 혹시 밖에 나가도 될까요...?",
        "무슨 소리니? 너는 여기 있어야만 해",
        "밖은 너에게 위험하단다",
        "오늘 밖에서 음악 축제를 하는 것 같은데...2년에 1번 열리는 축제라 오늘이 아니면 그",
        "야!!! 음악 소리 하지 말랬지 너네 엄마도 음악하다가 그 꼴 났는데",
        "(무서워 하지만 오늘이 아니면...) 이번에 못가면 다시는 갈 기회가 없을 것 같아요",
        "부탁이에요 제발 저는 곧... 이 세상에 없을 것 같다고요",
        "허..억..",
        "그딴 소리 내가 하지 말랬지!",
        "넌 평생 앞으로도 영원히 여기에 있는거야!",
        "(............., ........, .............) 하아....하아..이제 좀 괜찮아진 것 같아....",
        "(아빠는 저렇게 말씀하셨지만...오늘이 아니면 안돼) 문 잠금장치를 살펴보자",
        "안에서 무언가 쑤셔 넣으면 잠금장치가 열릴 것 같은데...",
        "아 내 머리핀으로 하면 열릴 것 같아",
        "한번 해보자...!"
    };
    public readonly string[] EnglishLines1_2 = { };
    public readonly string[] KazaLines1_2 = { };
    public readonly string[] JapaneseLines1_2 = { };
    public readonly string[] ChineseLines1_2 = { };

    /// <summary>
    /// 현재 설정된 언어에 따라 해당 대사 배열을 반환합니다.
    /// </summary>
    public string[] GetLines()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();

        switch (lang)
        {
            case "korean":
                return KoreanLines1_2;
            case "english":
                return EnglishLines1_2;
            case "kazahustan":
            case "kaza":
                return KazaLines1_2;
            case "japanese":
                return JapaneseLines1_2;
            case "chinese":
                return ChineseLines1_2;
            default:
                Debug.LogWarning($"[LanguageCollector1_2] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines1_2;
        }
    }
}
