using UnityEngine;

public class LanguageCollector3_2 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines3_2 = {
        "해냈다! 이 열쇠를 가지고 가면...!",
        "잠깐!",
        "읏",
        "정말 여기를 떠날 것인가?",
        "이곳에서는 에코가 원하는 것을 다 이룰 수 있단다. 바깥에서 못 부르던 노래도 이곳에서는 마음껏 부를 수 있어. 음악도 이곳에서는 원하는 만큼 들어도 돼.",
        "에코가 그토록 원하던 자유를 여기서는 누릴 수 있어.",
        "그럼에도 이곳을 떠날 것인가?",
        "잠깐, 이 팔 좀 놓고...",
        "미안. 나도 모르게....",
        "이곳은 정말 아름답고 마음에 들지만... 그치만...",
        "무엇이 마음에 안 든다는 거지? 이곳에는 에코를 매번 때리고 가두는 ‘아빠’라는 작자도 없어.",
        "맞아, 에코~ 여기는 너의 소중한 것들을 없애버리는 아빠 따위 없어",
        "여기서는 네가 바라는 거 다 이룰 수 있어",
        "내가 그렇게 해줄게~",
        "나르케......",
        "자 이리 오렴. 밖은 너에게 위험하단다. 너는 여기 있어야만 해!"
    };
    public readonly string[] EnglishLines3_2 = {

    };
    public readonly string[] KazaLines3_2 = {

    };

    public readonly string[] JapaneseLines3_2 = {

    };

    public readonly string[] ChineseLines3_2 = {

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
                return KoreanLines3_2;
            case "english":
                return EnglishLines3_2;
            case "kazahustan":
            case "kaza":
                return KazaLines3_2;
            case "japanese":
                return JapaneseLines3_2;
            case "chinese":
                return ChineseLines3_2;
            default:
                Debug.LogWarning($"[LanguageCollector3_2] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_2;
        }
    }

}

