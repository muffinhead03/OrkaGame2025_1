using UnityEngine;

public class LanguageCollector2_3_1 : MonoBehaviour
{
    [TextArea] public string[] KoreanAbove2_3_1 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove2_3_1 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove2_3_1 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove2_3_1 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove2_3_1 = {"Эко", "Пан", "Нарыке"};

    // �� ��� �迭
    public readonly string[] KoreanLines2_3_1 = {
        "뭔가 이상해! 자꾸 어디선가 누군가의 비명소리가 들려와",
        "별거 아니라니깐~",
        "아냐 한번 소리가 나는 곳으로 가봐야겠어",
        "무슨 소리니 에코~ 이상하게 굴고 있네",
        "자 계속 게임 하자"
    };
    public readonly string[] EnglishLines2_3_1 = {
    "Something feels wrong! I keep hearing someone screaming from somewhere",
    "It's nothing, seriously~",
    "No, I should go check where that sound came from",
    "What are you talking about, Echo~ You're acting kinda weird",
    "Come on, let's keep the game going"
};
    public readonly string[] KazaLines2_3_1 = {
        "Біртүрлі! Қайта қайта бір жақтан біреудің айғайлаған дауысын естимін",
        "Ой, түк емес~",
        "Жо-жоқ, дауыс шығып жатқан жаққа барып көрейік",
        "Қандай дыбыс Эко~",
        "Ары қарай ойнай берейік"
    };

    public readonly string[] JapaneseLines2_3_1 = {
        "なんだかおかしいのよ! どこかで誰かの悲鳴がずっと聞こえてくるの",
        "別にたいしたことないんだってば〜気にすんなよ！",
        "ううん、一度、音がするところに行ってみなきゃ",
        "何言ってるんだい、エコー〜変だぞ？?",
        "さあ、ゲームを続けよう"
    };

    public readonly string[] ChineseLines2_3_1 = {

        "有点奇怪……总觉得好像听到了什么人在尖叫…… ",
        "都说了没什么啦~",
        "不，我得去看看声音传来的方向 ",
        " 你在说什么啊艾可~怎么怪里怪气的",
        "来继续玩游戏吧"
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
