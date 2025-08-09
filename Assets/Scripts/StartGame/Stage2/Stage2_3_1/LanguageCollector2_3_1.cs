using UnityEngine;

public class LanguageCollector2_3_1 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines2_3_1 = {
        "뭔가 이상해! 자꾸 어디선가 누군가의 비명소리가 들려와",
        "별거 아니라니깐~",
        "아냐, 한번 소리가 나는 곳으로 가봐야겠어",
        "무슨 소리니, 에코~ 이상하게 굴고 있네",
        "자, 계속 게임 하자"
    };
    public readonly string[] EnglishLines2_3_1 = {
    "Something feels wrong! I keep hearing someone screaming from somewhere",
    "It's nothing, seriously~",
    "No, I should go check where that sound came from",
    "What are you talking about, Echo~ You're acting kinda weird",
    "Come on, let's keep the game going"
};
    public readonly string[] KazaLines2_3_1 = {

    };

    public readonly string[] JapaneseLines2_3_1 = {

    };

    public readonly string[] ChineseLines2_3_1 = {

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
                return KoreanLines2_3_1;
            case "english":
                return EnglishLines2_3_1;
            case "kazahustan":
            case "kaza":
                return KazaLines2_3_1;
            case "japanese":
                return JapaneseLines2_3_1;
            case "chinese":
                return ChineseLines2_3_1;
            default:
                Debug.LogWarning($"[LanguageCollector2_3_1] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_3_1;
        }
    }

}
