using UnityEngine;

public class LanguageCollector2_2 : MonoBehaviour
{
    [TextArea] public string[] KoreanAbove2_2 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove2_2 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove2_2 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove2_2 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove2_2 = {"Эко", "Пан", "Нарыке"};
    // �� ��� �迭
    public readonly string[] KoreanLines2_2 = {
        "흠흠~\u266a",
        "(고양이...?) 저기... ",
        "음? 자네는?",
        " (뭐지...? 왜 이 아이를 보면 애달픈 마음이...)",
        "자네는 다른 세상에서 온 사람이구나",
        "헉 어떻게 알았어?",
        "이곳에 사는 사람들은 이 세상이 무너지는 일이 생기기 전까지는 근심 따 위는 없는, 사랑이 넘치는 표정만 짓고 있거든",
        " 마치 귀여운 나처럼 말이야",
        "여기는 대체 어디야...?",
        "여기는 아르카디아",
        "사랑만이 존재하는 평화로운 낙원이지",
        " 혹시... 여기는 천국이야?",
        "천국? 음... 천국과 비슷한가",
        "여기는 누군가의 선택을 받아야 들어올 수 있는 장소야",
        "대체 누구의 선택을...?",
        "그런 복잡한 건 됐고! 자기소개부터 하자",
        "나는 나르키소스! 보시다시피 귀엽고 깜찍한 고양이야",
        "줄여서 나르케라고 불러줘~ 자네는?",
        "나는 에코...음 너랑 달리 나는 귀엽거나 그러지는 않은...그냥 평범한 사람이야",
        "흐음~ 내가 보기에는 자네도 충분히 사랑스러운데~ ",
        " 자 이리 와서 물가에 비친 아름다운 나와 자네의 모습을 보렴",
        "이...게 나? 내가 이렇게 생겼었나?",
        "정말 살아있는 인간 같아... 아니 너무 인간 같아서 오히려 인형 같아",
        "히히 그나저나 자네는 이제 뭐할 거야?",
        "나는... 내 집으로 돌아가고 싶어",
        "아빠가 내가 사라진 것을 아시면 난리가 날거야",
        "그리고 오늘이 내가 무척 기다리던 마을에서 음악 축제하는 날이어서 돌아가야 겠어",
        "어떻게 이곳에 오게 되었는지 기억 안 나?",
        "응...평소처럼 갑자기 숨이 막혀 쓰러졌는데 눈을 떠보니 여기였어",
        "그럼 아직 시간은 있으니까 같이 카드 게임이라도 하면서 천천히 기억해 봐",
        "(그래...아직 시간은 좀 있으니까...왜인지 모르겠지만 좀 더 이 아이와 있고 싶기도 하고) 응...! 좋아 "

    };
    public readonly string[] EnglishLines2_2 = {
    "Hmm hmm~\u266a",
    "(A cat...?) Excuse me...",
    "Hmm? And who might you be, then?",
    "(What is it...? Why does looking at him make me feel so nostalgic...?)",
    "You're someone who's wandered here from another world",
    "How did you figure that out?",
    "The folks living here, they wear nothing but faces full of love, no worries in sight---at least until this world starts falling apart",
    "Just like me, the adorable one",
    "Where is this place...?",
    "This place? It's Arcadia",
    "A peaceful paradise where only love exists",
    "Am I... in heaven...?",
    "Heaven? Hmm... sounds a lot like it",
    "Here, only those chosen by someone can step through",
    "Who made that choice...?",
    "Enough with the fancy talk! Let me introduce myself",
    "I'm Narcissus! As you can see, I'm one adorable, charming little cat",
    "Just call me Narke~ And you are?",
    "I'm Echo... Um, I'm not really cute or anything like you... I'm just... an ordinary person",
    "Hmm~ I gotta say, you're pretty lovely in your own way~",
    "Come here and take a look at the beautiful reflection of you and me by the water",
    "Is this... really me?  Did I really look like this?",
    "It... really looks like a living person... No, it's so human that it almost feels like a doll instead",
    "Hehe, so... what's your next move?",
    "I just... want to go back to my home",
    "If Dad finds out I'm gone, he's going to be really angry",
    "And today's the day of the music festival in my town that I've been really looking forward to, so I should go back",
    "You really don't remember how you got here?",
    "Yeah... like usual, I suddenly couldn't breathe and passed out... and when I woke up, I was here",
    "Then there's still time --- why don't we play some cards together and slowly remind you of things?",
    "(Right... there's still a bit of time... I'm not sure why, but I want to be with him a little more) Yeah...! That sounds good"
};


    public readonly string[] KazaLines2_2 = {
        "М-м~\u266a",
        "(мысық…?) Анау жақта… ",
        "хм? Сен ше?",
        " (не екен…? неге мына баланы көрсем жүрегім ауырады?)",
        "Сен басқа əлемнен келген адамсың ғой",
        " О, қайдан білесің? ",
        " Осында тұрып жатқан адамдар, бұл əлемнің қирала бастағанына əлі күнге дейін мəн бермейді, жай ғана жақсы көретіндей түр істейды",
        " мен сияқты сүйкімді",
        "Мен қайдамын…?",
        "Аркадиядасың",
        "тек махаббат бар бейбіт жерде",
        "Бұл жəннат па…?",
        " Жəннат? хм, соған ұқсас",
        "Бұл біреуге таңдау мүмкіндікі түскенде ғана кіре алатын жер",
        "Кімнің таңдауы…? ",
        "Бұл енді біраз қиындау! Алдымен өзімді таныстырайын",
        " Мен Нарциссуспын! Өзің көріп тұрғандай сүйкімді мысықпын",
        " Қысқаша Нарыке деп атауыңа болады~ Сен ше?",
        "Мен Эко… хм сен сияқты сүйкімді емеспін… жай кəдімгі адаммын",
        "Хыммм~ Меніңше сен де жеткілікті сүйкімді сияқтысың~",
        "Бері кел, судағы екеуміздің əдемі түрімізді қара",
        " Бұл… А? Мен осылай пайда болдым ба?",
        "Біраз көп өмір көрген адам сияқты… Адам болғандықтан қуыршаққа ұқсаймын ғой деймін",
        "хихи, енді не істейсің?",
        "Мен… мен үйіме қайтқым келеді",
        "Əкем кетіп қалғанымды білсе, есі шығады",
        "Жəне бүгін қатты күткен ауылда музыка фестиваль күні, сол үшін қайту керекпін",
        "Мында қалай келгенің есіңде ме?",
        " Ия… Əдеттегідей демім тарылып, құлап қалғаннан кейін көзімді ашсам осында болдым ",
        "Онда əлі уақытың келмесе бірге карта ойынын ойнайық, сенің есіңе ақырындап түсіріп көр",
        "(Онда… əлі уақыт бар болса… неге екенін білмеймін біраз мына баламен бірге бола тұрғым \nкеледі) Ия…! Жақсы "

    };

    public readonly string[] JapaneseLines2_2 = {
        "ふふん~\u266a",
        "(猫…？) あの…… ",
        "ん？キミは……見ない顔だねぇ？",
        "(何だろう……？どうしてこの子を見ると切ない気持ちになるのよ)",
        "なるほど、キミは別の世界から来た人なんだね",
        "えっ、どうしてわかったの?",
        "ここに住んでる人たちはね、世界が崩れ落ちるその時まで、悩みなんて一つもないんだ。愛に満ちた顔だけをしてるんだから",
        "まるで可愛いボクみたいにさ",
        "ここは一体どこなの……？",
        "ここはアルカディア",
        "愛だけが存在する、平和な楽園ってわけさ",
        "もしかして……ここは天国...?",
        "天国？ん〜…天国みたいなものかな",
        "ここはね、誰かに選ばれないと入れない場所なんだ",
        "いったい、誰の選択を...?",
        "そんな面倒な話はいいから！まずは自己紹介から始めよう",
        "ボクはナルキッソス! 見ての通り、ボクは可愛くてキュートなネコさ",
        "まあ、ナルケって呼んでくれ〜キミは？",
        "アタシはエコー……ううん、あなたとは違って、可愛いとかじゃなくて……ただ普通の人なの",
        "ふむ〜ボクから見れば、キミも十分に愛らしいけどね〜",
        " さあ、こっちに来て、水辺に映る美しいボクとキミの姿を見て",
        "こ、これがアタシ？アタシって、こんな顔してたっけ……？",
        "本当に生きている人みたい……でも、あんまり人間っぽすぎて、逆に人形みたい",
        "ふふっ、ところでキミはこれから何をするつもり？",
        "アタシは……自分の家に帰りたい",
        "パパがアタシがいなくなったって知ったら、大変なことになるのよ",
        "それに、今日はアタシがずっと待ってた村の音楽祭の日だから、帰らなきゃ…",
        "どうやってここに来たのか、覚えてないのかい？",
        "うん……いつもみたいに急に息が苦しくなって倒れちゃって、目を覚ましたらここにいたの",
        "じゃあ、まだ時間はあるから、一緒にカードゲームでもしながらゆっくり思い出してみようよ",
        "(そう……まだ時間は少しあるし……なんだか理由はわからないけど、この子ともう少し一緒にいたい) うん……いいよ!"

    };

    public readonly string[] ChineseLines2_2 = {
        "哼哼~\u266a",
        "（猫……？）那个…… ",
        "嗯？你是？",
        "（怎么回事……看到这个孩子就莫名觉得心酸……）",
        "你是从另一个世界来的吧。",
        " 啊？你怎么知道的？",
        "住在这里的人啊，除非这个世界崩塌了，否则脸上总是充满爱，没有任何忧愁。",
        "就像可爱的我一样~",
        "这里……到底是哪里？",
        "这里是阿卡迪亚",
        "是个只有爱存在的和平天堂",
        "这里……是天堂吗？",
        "天堂？嗯……差不多吧",
        "这里是只有被某人选中才能进入的地方",
        "被谁选中……？",
        "哎呀别想那么复杂！先来做个自我介绍吧 ",
        "我是那耳喀索斯！如你所见，是只可爱又迷人的猫咪~",
        "简称纳尔克~你呢？",
        "我是艾可……嗯，不像你那么可爱……只是个普通人 ",
        "哼哼~在我看来你也足够可爱啦~",
        "来，看看水里的倒影，看看美丽的我和你的模样~",
        "这……是我？我原来长这样？",
        "看起来真的像是活人……不，太像人类了，反而像是个娃娃",
        "嘻嘻，说起来你接下来想做什么？",
        "我……我想回家 ",
        "如果爸爸发现我不见了，一定会大发雷霆 ",
        "而且今天是我期待已久的音乐节，我必须得回去",
        "你不记得自己是怎么来到这里的吗？",
        "嗯……和平时一样，突然喘不过气来晕倒了，醒来就在这里了 ",
        "那既然还有时间，不如一起玩会儿卡牌游戏，慢慢回忆一下吧",
        "（是啊……反正还有点时间……不知为何，我也想多待在这个孩子身边) 嗯……好啊！"

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