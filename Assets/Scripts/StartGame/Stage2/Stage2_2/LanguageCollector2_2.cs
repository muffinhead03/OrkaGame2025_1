using UnityEngine;

public class LanguageCollector2_2 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines2_2 = {
        "흠흠~♪",
        "(고양이...?) 저기...",
        "음? 자네는?",
        "(뭐지...? 왜 이 아이를 보면 애달픈 마음이...)",
        "자네는 다른 세상에서 온 사람이구나",
        "헉, 어떻게 알았어?",
        "이곳에 사는 사람들은 이 세상이 무너지는 일이 생기기 전까지는 근심 따위는 없는, 사랑이 넘치는 표정만 짓고 있거든",
        "마치 귀여운 나처럼 말이야",
        "여기는 대체 어디야...?",
        "여기는 아르카디아",
        "사랑만이 존재하는 평화로운 낙원이지",
        "혹시... 여기는 천국이야?",
        "천국? 음... 천국과 비슷한가",
        "여기는 누군가의 선택을 받아야 들어올 수 있는 장소야",
        "대체 누구의 선택을...?",
        "그런 복잡한 건 됐고! 자기소개부터 하자",
        "나는 나르키소스! 보시다시피 귀엽고 깜찍한 고양이야",
        "줄여서 나르케라고 불러줘~ 자네는?",
        "나는 에코... 음, 너랑 달리 나는 귀엽거나 그러지는 않은... 그냥 평범한 사람이야",
        "흐음~ 내가 보기에는 자네도 충분히 사랑스러운데~",
        "자 이리 와서 물가에 비친 아름다운 나와 자네의 모습을 보렴",
        "이...게 나? 내가 이렇게 생겼었나?",
        "정말 살아있는 인간 같아... 아니 너무 인간 같아서 오히려 인형 같아",
        "히히 그나저나 자네는 이제 뭐할 거야?",
        "나는... 내 집으로 돌아가고 싶어",
        "아빠가 내가 사라진 것을 아시면 난리가 날 거야",
        "그리고 오늘이 내가 무척 기다리던 마을에서 음악 축제하는 날이어서 돌아가야겠어",
        "어떻게 이곳에 오게 되었는지 기억 안 나?",
        "응... 평소처럼 갑자기 숨이 막혀 쓰러졌는데 눈을 떠보니 여기였어",
        "그럼 아직 시간은 있으니까 같이 카드 게임이라도 하면서 천천히 기억해 봐",
        "(그래... 아직 시간은 좀 있으니까... 왜인지 모르겠지만 좀 더 이 아이와 있고 싶기도 하고) 응...! 좋아",

    };
    public readonly string[] EnglishLines2_2 = {

    };
    public readonly string[] KazaLines2_2 = {

    };

    public readonly string[] JapaneseLines2_2 = {

    };

    public readonly string[] ChineseLines2_2 = {

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
                return KoreanLines2_2;
            case "english":
                return EnglishLines2_2;
            case "kazahustan":
            case "kaza":
                return KazaLines2_2;
            case "japanese":
                return JapaneseLines2_2;
            case "chinese":
                return ChineseLines2_2;
            default:
                Debug.LogWarning($"[LanguageCollector2_2] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_2;
        }
    }

}