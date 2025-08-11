using UnityEngine;

public class LanguageCollector2_3 : MonoBehaviour
{
    [TextArea] public string[] KoreanAbove2_2 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove2_2 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove2_2 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove2_2 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove2_2 = {"Эко", "Пан", "Нарыке"};

    // 기본 한글 대사 (필요 시 인스펙터에서 수정 가능)
    [TextArea(2, 5)]
    public string[] KoreanLines2_3 = {
        "내가 이겼다!",
        "어라, 방금 무슨 소리 들리지 않았어?",
        "무슨 소리? 잘못 들은 거겠지~ 나는 못 들었는걸",
        "그것보다도 다음 판 시작하자",
        "(하긴 이런 평화로운 곳에 그런 소리가...) 그래, 좋아!"
    };

    // 기본 영어 대사 (원하면 인스펙터에서 수정)
    [TextArea(2, 5)]
    public string[] EnglishLines2_3 = {
        "I did it...!",
        "Wait... did you hear that noise?",
        "What? That can't be right ~ I didn't hear a thing",
        "Never mind that, let's just start the next round",
        "(Well... there's no way a place this peaceful would have such a noise like that...) Okay...!"
    };

    // 다른 언어는 인스펙터에서 채우도록 공개
    [TextArea(2, 5)] public string[] JapaneseLines2_3 =
    {
        "やった! アタシの勝ちよ！",
        "あれ？今、何か音が聞こえなかった？",
        "なに言ってるんだい？聞き間違いだろう〜ボクは聞こえてないよ",
        "それよりさ、次のゲームを始めよう",
        "(まあ、こんな平和な場所でそんな音がするなんて……) いいよ！"
    };
    [TextArea(2, 5)] public string[] ChineseLines2_3 =
    {
        "我赢了！",
        "咦？刚才是不是听到了什么声音？",
        "什么声音？你听错了吧~我可没听见",
        "比起那个，快开始下一局吧",
        "（也是，在这样和平的地方，不可能有什么怪声吧……）好吧！"
    };
    [TextArea(2, 5)] public string[] KazaLines2_3 =
    {
        "Мен жеңдім!",
        "Жаңа бір дыбыс естімедің бе?",
        "Қандай дыбыс? Дұрыс естімедім~ Мен ести алмайтын сияқтымын",
        "Бола берсін, келесі кезеңді бастайық",
        "(Осындай бейбіт жерде бұндай дыбыстар…) Жарайды!"
    };

    // 언어 문자열 정규화
    private static string Normalize(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return "korean";
        lang = lang.Trim().ToLower();

        if (lang.StartsWith("en")) return "english";
        if (lang.StartsWith("ko")) return "korean";
        if (lang.StartsWith("ja")) return "japanese";
        if (lang.StartsWith("zh") || lang.Contains("chinese")) return "chinese";
        if (lang.StartsWith("kk") || lang.Contains("kaza") || lang == "kazahustan") return "kaza";

        return "korean";
    }

    public string[] GetLines()
    {
        string lang = Normalize(LanguageManager.GetLanguage());

        switch (lang)
        {
            case "english":  return Safe(EnglishLines2_3, KoreanLines2_3);
            case "japanese": return Safe(JapaneseLines2_3, KoreanLines2_3);
            case "chinese":  return Safe(ChineseLines2_3, KoreanLines2_3);
            case "kaza":     return Safe(KazaLines2_3, KoreanLines2_3);
            case "korean":
            default:         return Safe(KoreanLines2_3, KoreanLines2_3);
        }
    }

    // 널/빈 배열일 때 안전하게 폴백 제공
    private static string[] Safe(string[] primary, string[] fallback)
    {
        if (primary != null && primary.Length > 0) return primary;
        if (fallback != null && fallback.Length > 0) return fallback;
        return new string[0];
    }
}
