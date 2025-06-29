using UnityEngine;

public class LanguageCollector3_4 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines3_4 = {
        "뭐?",
        "이거 놔요!",
        "왜왜왜왜왜왜ㅗ애ㅗ애어ㅐ어ㅐㅓ애ㅓ어ㅐ왜왜왜왜ㅗ애ㅗ왜왜ㅗ애ㅙㅗㅗ애ㅙㅗ애ㅗ왜ㅙㅙㅙ",
        "(무서워...하지만)",
        "이곳은 아름답고 완벽해요! 당신이 말한 대로 이곳에서는 제가 원하는 것들을 다 이룰 수 있겠죠.",
        "하지만 이 세상은 제가 속한 곳이 아니에요.",
        "이곳에서 아무리 자유를 누려도, 그것은 결코 진짜 자유가 될 수 없어요.",
        "진짜 자유는 제가 처한 현실을 외면하지 않고 마주 봐야 찾을 수 있어요.",
        "아무리 현실이 괴롭고 앞이 보이지 않더라도, 그럼에도 저는 진실을 향해 가겠어요....!"
    };
    public readonly string[] EnglishLines3_4 = {

    };
    public readonly string[] KazaLines3_4 = {

    };

    public readonly string[] JapaneseLines3_4 = {

    };

    public readonly string[] ChineseLines3_4 = {

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
                return KoreanLines3_4;
            case "english":
                return EnglishLines3_4;
            case "kazahustan":
            case "kaza":
                return KazaLines3_4;
            case "japanese":
                return JapaneseLines3_4;
            case "chinese":
                return ChineseLines3_4;
            default:
                Debug.LogWarning($"[LanguageCollector3_4] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_4;
        }
    }

}
