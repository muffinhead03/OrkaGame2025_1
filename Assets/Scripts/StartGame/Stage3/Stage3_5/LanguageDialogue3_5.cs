using UnityEngine;

public class LanguageCollector3_5 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines3_5 = {
        "헉, 여기는?... 내가 쓰러졌던 방문 앞이다!",
        "돌아왔구나... 지금까지 본 것은 대체 뭐였을까?",
        "맞다! 이럴 때가 아니라 음악 축제에 가야 해!",
        "아빠가 오고 있어...",
        "방 밖으로 나가기 두려워... 무슨 일이 생길지 모르겠어...",
        "하지만 나는 나갈 거야!",
        "(아... 평안하다... 드디어 해방이다...)",
        "어...라...?",
        "허억 또 패니...ㄱ",
        "패..니ㄱ, 패니...ㄱ.....판!",
        ".................미안해",
        "............에코"
    };
    public readonly string[] EnglishLines3_5 = {

    };
    public readonly string[] KazaLines3_5 = {

    };

    public readonly string[] JapaneseLines3_5 = {

    };

    public readonly string[] ChineseLines3_5 = {

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
                return KoreanLines3_5;
            case "english":
                return EnglishLines3_5;
            case "kazahustan":
            case "kaza":
                return KazaLines3_5;
            case "japanese":
                return JapaneseLines3_5;
            case "chinese":
                return ChineseLines3_5;
            default:
                Debug.LogWarning($"[LanguageCollector3_5] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_5;
        }
    }

}
