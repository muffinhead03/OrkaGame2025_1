using UnityEngine;

public class LanguageCollector3_1 : MonoBehaviour
{
    [Header("Aboveline / Speaker Names (per language)")]
    [TextArea] public string[] KoreanAbove1_2 = {"에코", "판"};
    [TextArea] public string[] EnglishAbove1_2 = {"Echo", "Pan"} ;
    [TextArea] public string[] JapaneseAbove1_2 = {"エコー", "パーン"};
    [TextArea] public string[] ChineseAbove1_2 = {"艾可","潘"};
    [TextArea] public string[] KazaAbove1_2 = {"Эко", "Пан"};

    // �� ��� �迭
    public readonly string[] KoreanLines3_1 = {
        "헉... 방금 뭐였지...?",
        "일어났나, 에코?",
        "당신은...?",
        "아! 쓰러질 때마다 나타난 건 당신이죠...?",
        "당신은 누구죠? ",
        "(아까 나르케가 누군가의 선택으로 이 세상에 올 수 있다고 했지...)",
        "저를 이 세계로 부른 것은 당신이죠?",
        "잠시 진정하렴. 내 이름은 판. 이 세상을 관리하는 자이지.",
        "그리고 미리 말해두지만 나는 이 세상의 유지를 위한 관리만을 하지, ,",
        "이 세상에 누구를 들일지를 선택할 권한은 없단다.",
        "그럼 누가 저를 이 세계로 부른 거죠?",
        "글쎄. 그건 답하기는 어렵구나, 에코.",
        "(왜 답하기 어렵다는 거지... 그것보다도) 저는 집에 돌아가고 싶어요",
        "오랫동안 기다려온 마을의 음악 축제에 가고 싶어요",
        "그곳에서 노래를 부르는 것까지는 어렵겠지만…",
        "가서 노래를 듣고, 즐기고 싶어요!",
        "이번이 마지막일지도 몰라요... 도와주세요, 제발...!",
        "............그래. 그것이 에코가 바라는 일이라면…",
        "(뭐지? 방금 목소리 뭔가 쓸쓸한 듯한...)",
        "일단 알겠다. 집에 돌아가려면 이 장치를 풀어야 된단다."
    };
    public readonly string[] EnglishLines3_1 = {
        "Huh what was that just now…?",
        "You’re finally awake, Echo.",
        "Y-You are…?",
        "Ah! It’s you who appears every time I faint, right?",
        "Who are you?",
        "(Narke said earlier that someone can come into this world by someone’s choice…)",
        "You’re the one who called me to this world, right?",
        "Calm yourself for a moment. My name is Pan — the one who governs this world.",
        "And let me make this clear, I merely maintain the order of this world, ",
        "and hold no authority over who may enter this world.",
"Then, who brought me here?", 
        "Well now… That is a difficult question to answer, Echo.",
        "(I don’t know why it’s so difficult to answer…More than that,) I just want to go home",
        "I really want to go to the music festival in town that I’ve been waiting for a long time",
        "It might be hard for me… to sing in the festival…",
        "but, I want to go… listen to the music, and enjoy it",
        "This might be the last time… please, help me…!",
        "……….I see. If that is what you desire, Echo…",
        "(What was that? The voice just now seemed… lonely…)",
        "Understood. To make your way back home,  you must first unlock this device.",
    };
    public readonly string[] KazaLines3_1 = {
        "А… Жаңағы не…?",
        "Ояндыңба, Эко?",
        "Сен…?",
        "А! Есімнен танып қалғанда пайда болған сен емес пе…?",
        "Сен кімсің?",
        "(Бағана Нарыке біреудің таңдауы бойынша осы əлемге келуге болады деп айтып еді ғой…)",
        "Мені осы əлемге шақырған сен емес пе?",
        " Біраз сабыр сақташы. Менің атым Пан. Осы əлемнің басқарышысымын.",
        " Сондай-ақ алдын ала айтқандай мен осы əлемнің сақтығы үшін басқарамын, ",
        "кім кіретінін таңдауды басқармаймын.",
        "Онда мені бұл əлемге кім шақырды?",
        "Білмедім. Жауап беру қиын-ақ, Эко.",
        " (Неге жауап беру қиын екен?… Одан сайын) Мен үйге қайтқым келеді ",
        " Көптен бері күткен ұнататын музыка фестиваліне барғым келеді",
        "Онда əн айтудың өзі қиындап барғаны болмаса…",
        " Барып, əн тыңдап, рахаттанғым келеді!",
        "Бұл соңғы фестиваль ма жоқ па білмеймін… Көмектесіңізші, өтінемін…! ",
        "………….Жақсы. Егер бұл Эконың қалағаны болса…",
        " (Не? Жаңа дауысы жалғызсырағандай естілгендей…)",
        "Мен ұқтым. Үйге қайту үшін мына құрылғыны үрлеуің керек."

    };

    public readonly string[] JapaneseLines3_1 = {
        "えっ…、今のあれは一体…?",
        "目が覚めたのか? エコー。",
        "あなたは…?",
        "あっ！アタシが倒れる度に現れた方ですよね…?",
        "あなたは一体誰?",
        "(さっき確かにナルケが、アタシは誰かに選ばれてこの世界に呼ばれてきたんだって…。)",
        "アタシをこの世界に呼んだのはあなたですね？",
        "しばらく落ち着けよ。 私の名はパーン。 この世界の管理者だ。",
        "そしてあらかじめ言っておくけど、私はただこの世界を維持するために管理だけをする。",
        "誰をこの世に入れるかを選ぶ権限はない。",
        "ならアタシをこの世界に呼んだのは?",
        "さあ? それは答えにくいかも、エコー。",
        "(なんでにくいんだろう? …いや、それよりも。) アタシ、帰りたいです。",
        "とても長い間待っていた、村の音楽祭に参加したいです。",
        "そこで歌うことまでは難しいと思うけど···.",
        "行って歌を聴いて、感じて、満喫したいです!",
        "今回がアタシの最後の機会かもしれません。 どうか手を貸してください...!",
        "……そうか、間違いなくエコーがそれを望むのなら。",
        "(あれ? 今なんだかちょっと寂しいような···。)",
        "分かった。 帰りたければ、まずはこの装置から解いてみなさい。"
    };

    public readonly string[] ChineseLines3_1 = {
        "啊……刚刚那是……？",
        "醒了吗，艾可？",
        "你是……？",
        "啊！每次我昏倒时出现的就是你，对吧？ ",
        "你到底是谁？ ",
        "（刚才纳尔克说过，只有被某人选中才能来到这个世界……）",
        "是你把我带来这个世界的吧？",
        "先冷静一下。我的名字是潘，是这个世界的管理者。",
        "不过我先说明，我只是负责维持这个世界的秩序，",
        "并没有决定谁能进入这里的权力。",
        "那……是谁把我带来的？",
        "这个嘛，很难说啊，艾可。 ",
        "（为什么说“很难说”……不管了）我想回家 ",
        "我想去我等了很久的村庄音乐节",
        "虽然可能不能在那里唱歌……",
        "但我想听歌，想去享受那个时光！",
        "这可能是我最后的机会了……求你了，拜托……！",
        "……好吧。如果这是你真正的愿望…… ",
        "（刚才的语气……好像有点寂寞……）",
        "总之，我知道了。你想回家，就必须先解开这个装置。"

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
                return KoreanLines3_1;
            case "english":
                return EnglishLines3_1;
            case "kazahustan":
            case "kaza":
                return KazaLines3_1;
            case "japanese":
                return JapaneseLines3_1;
            case "chinese":
                return ChineseLines3_1;
            default:
                Debug.LogWarning($"[LanguageCollector3_1] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_1;
        }
    }

}
