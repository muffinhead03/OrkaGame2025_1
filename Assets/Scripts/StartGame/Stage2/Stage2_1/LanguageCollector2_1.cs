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
        "Um…",
        "I can’t breathe all of a sudden… Ah… looks like I… fainted again…",
        "I’ve been having trouble breathing more often lately",
        "W-Wait… Where am I…? ",
        "What’s with these clothes…? I’m sure I… I fainted in my room…",
        "Mm… even looking around, I still have no idea where I am",
        "It’s so beautiful, like somewhere that doesn’t even belong in this world",
        "...I don’t… really want to think about this, but",
        "maybe I really died and now I’m in heaven…?",
        "Oh, There’s someone by the water"
    };
    public readonly string[] KazaLines2_1 = {
        "k","k","k","k","k","k","k","k","k","k"
    };

    public readonly string[] JapaneseLines2_1 = {
        "k","k","k","k","k","k","k","k","k","k"
    };

    public readonly string[] ChineseLines2_1 = {
    "嗯... ",
    "突然喘不过气来...啊...又晕过去了吗...",
    "最近好像越来越常这样喘不过气来...",
    "等等，这是哪里...！",
    "这身衣服是怎么回事...！我明明是在自己房间晕倒的...",
    "嗯...看看四周，完全不知道这是什么地方.",
    "就像是个根本不属于现实世界的美丽地方.",
    "...虽然不想这么想",
    "难道...我死了，来到天堂了吗？",
    "咦？ 那水边好像有人..."
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
