using UnityEngine;

public class LanguageCollector3_4 : MonoBehaviour
{
    [TextArea] public string[] KoreanAbove1_2 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove1_2 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove1_2 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove1_2 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove1_2 = {"Эко", "Пан", "Нарыке"};

    // �� ��� �迭
    public readonly string[] KoreanLines3_4 = {
        "뭐?",
        "이거 놔요!",
        "왜왜왜왜왜왜ㅗ애ㅗ애어ㅐ어ㅐㅓ애ㅓ어ㅐ왜왜왜왜ㅗ애ㅗ왜왜ㅗ애ㅙㅗㅗ애ㅙㅗ애ㅗ왜ㅙㅙㅙ",
        "(무서워...하지만)",
        "이곳은 아름답고 완벽해요! 당신이 말한대로 이곳에서는 제가 원하는 것들을 다 이룰 수 있겠죠.",
        "하지만 이 세상은 제가 속한 곳이 아니에요.",
        "이곳에서 아무리 자유를 누려도, 그것은 결코 진짜 자유가 될 수 없어요",
        "진짜 자유는 제가 처한 현실을 외면하지 않고 마주 봐야 찾을 수 있어요",
        "아무리 현실이 괴롭고 앞이 보이지 않더라도, 그럼에도 저는 진실을 향해 가겠어요....!"
    };
    public readonly string[] EnglishLines3_4 = {
        "What?",
        "Let me go!",
        "WHYHYWHYWHYWHHHHYWHYHWYHWHHHHHHHHHHHHWHHHHHWYYYYYYYYWHWYWYWYYYYYYYYYYYYWHY",
        "(I’m scared… but I have to…)",
        "This place is beautiful and perfect… Just like you said, I guess I can achieve everything I want in this world",
        "But this world,  it’s not really where I belong.",
        "No matter how free I feel here,  it can never be true freedom",
        "True freedom can only be found when I face the reality I’m in, without turning away",
        "No matter how painful reality is, no matter how lost I feel… I’ll still keep moving toward the truth…!"
    };
    public readonly string[] KazaLines3_4 = {
        "Не?",
        " Жібер!",
        "Неге неге неге неге негеее",
        " (Қорқынышты… бірақ)",
        " Бұл жер əдемі жəне мінсіз! Сендер айтқандай мен қалаған заттың барлығына қол жеткізуге болады.",
        "Бірақ бұл мен тиесілі жер емес.",
        " Бұл жерде қалай да бостандық алсам да, бұнда ешқашан шын бос бола алмаймын. ",
        " Шын бостандықты өз шын өміріңнен қашпасаң ғана таба аласын",
        "Шын өмір ауырпашыл болып көрінсе де, мен шындық жағында боламын…! "

    };

    public readonly string[] JapaneseLines3_4 = {
        "は?",
        "離して!",
        "どうしtどうしてddどうしてどうしiiてどうsしてどうしてeどうsしてどdうしてどuuuうしてどうしてどうしiて",
        "(怖い…でも…！)",
        "ここは完璧です！あなたの言うとおり、ここではアタシが望むことを何でも叶えることができるでしょう",
        "でも、アタシはここに属していません。",
        "ここでアタシが得るのは偽の自由だけです。",
        "真の自由とは、アタシが置かれている現実を無視せずに向き合わなければ見つけられません。",
        "いくら現実が苦しくて前が見えなくなっても、それでもアタシは真実に向かって行きます…!"
    };

    public readonly string[] ChineseLines3_4 = {
        "什么！？",
        "放开我！",
        "为什么为什么为什么为什么为什么！！为！什！么！！",
        "（好可怕……但我不能退缩） ",
        "这个世界的确很美，也如你们所说能让我实现所有愿望……",
        "但这并不是我真正属于的地方。 ",
        "就算在这里能享受所谓的自由，那也不是真正的自由。",
        "真实的自由，是勇敢面对我所处的现实，而不是逃避它。",
        "哪怕现实再痛苦，再黑暗……我也要走向真相……！"
    };

    /// <summary>
    /// ���� ������ �� ���� �ش� ��� �迭�� ��ȯ�մϴ�.
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
