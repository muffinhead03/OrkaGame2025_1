using UnityEngine;

public class LanguageCollector3_5 : MonoBehaviour
{
    // 화자명 테이블(에코/판/나르케) — 3_5 전용
    [TextArea] public string[] KoreanAbove3_5   = { "에코", "판", "나르케" };
    [TextArea] public string[] EnglishAbove3_5  = { "Echo", "Pan", "Narke" };
    [TextArea] public string[] JapaneseAbove3_5 = { "エコー", "パーン", "ナルケ" };
    [TextArea] public string[] ChineseAbove3_5  = { "艾可", "潘", "纳尔克" };
    [TextArea] public string[] KazaAbove3_5     = { "Эко", "Пан", "Нарыке" };

    // 대사
    public readonly string[] KoreanLines3_5 = {
        "헉 여기는?.... 내가 쓰러졌던 방문 앞이다!",
        "돌아왔구나....지금까지 본 것은 대체 뭐였을까?",
        "맞다! 이럴때가 아니라 음악 축제에 가야 해!",
        "아빠가 오고 있어...",
        "방 밖으로 나가기 두려워... 무슨 일이 생길지 모르겠어...",
        "하지만 나는 나갈 꺼야! ",
        "(아....평안하다....드디어 해방이다...)",
        "어...라...?",
        "허억 또 패니..ㄱ",
        "",
        "패..니ㄱ 패니...ㄱ.....판!",
        "................미안해",
        "............에코"
    };

    public readonly string[] EnglishLines3_5 = {
        "Ah… this is… the door where I fainted!",
        "I’m back… what was all that I saw until now?",
        "Oh, right! I shouldn’t be here… I have to go to the music festival!",
        "Dad is coming…",
        "I’m scared to leave the room… I don’t know what’s waiting for me…",
        "But I will leave!",
        "(Ah… such calmness… at last, freedom finds me…)",
        "H..u…….h?",
        "Hu..h A..h it’s …you ..ha..again…pa….ah..nic",
        "",
        "pa…n..ic..pan..ic…..PAN!",
        "………….Sorry",
        "............Echo"
    };

    public readonly string[] KazaLines3_5 = {
        "А осында? Мен есімнен таңғанда дəл осындай болғанмын!",
        "Қайтып келдім… Қазірге дейін көргер затым не?",
        "Дұрыс! Бағанағы не болса да мен музыка фестиваліне баруым керек!",
        "Əкем келе жатыр…",
        "Бөлме сыртына шығуға қорқамын… Не болып қалғанын бірмеймін…",
        "Бірақ мен шығуым керек!",
        "(А… тыныш… енді боспын…)",
        "О… А…?",
        "Қайтадан…",
        "",
        "Қане.. А!",
        "….. Кешір",
        "……….Эко"
    };

    public readonly string[] JapaneseLines3_5 = {
        "えっ、ここは…？ 私が倒れた部屋のドアの前だ！",
        "帰ってきちゃった… 今までのあれは一体何だったんだろう？",
        "あ！こうしてる場合じゃない。 早くお祭りに行かなきゃ！",
        "パパが来てる…",
        "部屋の外に出るのが怖い…。 何が起こるか分からない…",
        "それでも私は出る！",
        "(ああ…なんて静けさ…やっと自由だ…)",
        "あ…れ…？",
        "うっ、また…パニ…",
        "",
        "パ…ニ…ッ、パニ…ック……パーン！",
        "…………ごめん",
        "…………エコー"
    };

    public readonly string[] ChineseLines3_5 = {
        "啊……这里是？……我倒下的房门前……！",
        "回来了……刚刚的一切到底是什么？",
        "对了！不能再犹豫了，我得去音乐节！",
        "爸爸来了……",
        "我害怕走出这扇门……不知道会发生什么……",
        "但我还是要出去！",
        "（啊……好平静……终于自由了……）",
        "咦……？",
        "又要……发作了……",
        "",
        "恐……慌……发……作……潘！",
        "…………对不起",
        "…………艾可"
    };

    // ===== Helper =====
    public string[] GetLines()
    {
        switch (Normalize(LanguageManager.GetLanguage()))
        {
            case "korean":  return KoreanLines3_5;
            case "english": return EnglishLines3_5;
            case "japanese":return JapaneseLines3_5;
            case "chinese": return ChineseLines3_5;
            case "kazakh":  return KazaLines3_5;
            default:        return KoreanLines3_5;
        }
    }

    public string[] GetAboveTable()
    {
        switch (Normalize(LanguageManager.GetLanguage()))
        {
            case "korean":  return KoreanAbove3_5;
            case "english": return EnglishAbove3_5;
            case "japanese":return JapaneseAbove3_5;
            case "chinese": return ChineseAbove3_5;
            case "kazakh":  return KazaAbove3_5;
            default:        return KoreanAbove3_5;
        }
    }

    private string Normalize(string raw)
    {
        string s = (raw ?? "").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        // 카자흐어 표준키
        if (s.StartsWith("kk") || s.Contains("kazakh") || s.Contains("kaza")) return "kazakh";
        return s;
    }
}
