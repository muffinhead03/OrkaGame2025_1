using UnityEngine;

public class LanguageCollector3_2 : MonoBehaviour
{
    [TextArea] public string[] KoreanAbove1_2 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove1_2 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove1_2 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove1_2 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove1_2 = {"Эко", "Пан", "Нарыке"};

    // �� ��� �迭
    public readonly string[] KoreanLines3_2 = {
        "해냈다! 이 열쇠를 가지고 가면...!",
        "잠깐!",
        "읏",
        "정말 여기를 떠날 것인가? ",
        "이곳에서는 에코가 원하는 것을 다 이룰 수 있단다. 바깥에서 못 부르던 노래도 이곳에서는 마음껏 부를 수 있어. 음악도 이곳에서는 원하는 만큼 들어도 돼.",
        "에코가 그토록 원하던 자유를 여기서는 누릴 수 있어.",
        "그럼에도 이곳을 떠날 것인가?",
        "잠깐, 이 팔 좀 놓고...",
        "미안. 나도 모르게....",
        "이곳은 정말 아름답고 마음에 들지만... 그치만...",
        "무엇이 마음에 안 든다는 거지? 이곳에는 에코를 매번 때리고 가두는 ‘아빠’라는 작자도 없어.",
        "맞아 에코~ 여기는 너의 소중한 것들을 없애버리는 아빠 따위 없어",
        "여기서는 네가 바라는 거 다 이룰 수 있어 ",
        "내가 그렇게 해줄게~",
        "나르케......",
        "자 이리 오렴. 밖은 너에게 위험하단다. 너는 여기 있어야만 해!"
    };
    public readonly string[] EnglishLines3_2 = {
        "I did it…! With this key, I can…!",
        "Hold on!",
        "Uh—",
        "Are you sure you’re going to leave this place?",
        "In this place, Echo, all that you long for can be fulfilled. The songs you could not sing outside—you are free to sing them here whenever you want. And the music—feel free to listen to it as much as you want.",
        "Here, Echo, you may enjoy the freedom you’ve so deeply longed for.",
        "And yet, do you still want to leave this world?",
        "Wait… let go of my arm ",
        "My apologies. It wasn’t intentional.",
        "This world is so beautiful and I really love it, but… still…",
        "What is it that displeases you? There is no one here like that man called ‘Father’ who would beat and imprison you over and over..",
        "That’s right, Echo~ Here, there’s no ‘dad’ to take away the things you hold, my dear",
        "Here, anything you desire can come true",
        "I’ll make sure of that~",
        "Narke…..",
        "Now, come here.  The outside world is dangerous for you. YOU MUST STAY HERE!"
    };
    public readonly string[] KazaLines3_2 = {
        "Істедім! Бұл кілтті бірге алып кетсем…!",
        "Тоқта! ",
        "А",
        "Шынымен де осы жерден кетпексің бе?",
        "Бұл жерде Эко қалаған заттың барлығына қол жеткізуге болады. Сыртта айта алмайтын өлеңді де осында жүрегің қалауынша айта аласың. Музыканы да осында қалағандай тыңдауыңа болады.",
        "Эко қалаған бостандықты осында сезіне аласың.",
        "Сонда да кетпексің бе?",
        "Қолыңды мында қойшы…",
        "Кешір. Мен де білмедім…",
        "Бұл жер шынымен де əдемі жəне маған ұнаса да… Сонда да…",
        "Саған көңіліңе не жақпады? Бұл жерде Эконы алып кете беретін “Əке” деп айтатын адам да жоқ.",
        " Дұрыс айтады Эко~ Бұнда саған қымбат заттарды жоятын əкең сияқтылар жоқ ",
        "Бұнда сен қалаған заттың барлығына қол жеткізуге болады",
        "Мен солай істер едім~",
        "Нарыке….",
        "Мында кел. Сыртта сен үшін қауіпті. Сен тек осында болуың керек!"

    };

    public readonly string[] JapaneseLines3_2 = {
        "やった! もうこの鍵があれば…!",
        "お待ちなさい！",
        "うっ!",
        "本気でここを離れるのか? ",
        "ここではエコーが望むすべてを成し遂げることができる。外では歌えなかった歌も思う存分歌える。素敵な演奏を飽きるほど聴いてもいい。",
        "どうして分かってくれないのか? エコーがあれほど望んでいた自由がここにある。",
        "それでもここを離れるのか？",
        "ちょっと、まずこの腕を置いて…。",
        "すまない。私も知らないうちについ。",
        "この世界はとても美しい。 でも······",
        "何が気に入らないのか？ ここにはエコーを何度も殴って閉じ込める「パパ」というモノもいない。",
        "そうだよ、エコー~！ ここにはキミの大切なものを消し去るパパなんかいないのよ。",
        "ここではキミのすべての願いが叶うのさ！",
        "ボクがそうしてあげる。",
        "ナルケ……。",
        "さあ、おいでなさい。外は君に危険すぎる。エコーの居場所はここじゃなきゃいけないんだ！"
    };

    public readonly string[] ChineseLines3_2 = {
        "成功了！拿着这把钥匙就能……！ ",
        "等一下！",
        " 呃！",
        "你真的要离开这里吗？",
        "在这里，你想要的一切都能实现。在外面唱不了的歌，在这里你可以尽情歌唱。音乐也可以随你听个够。 ",
        "你渴望的自由，在这里都能拥有。",
        "即便如此，你还要离开？",
        "先……请你放开我的手……",
        "抱歉，我不是故意的…… ",
        "这里真的很美，我也很喜欢……但是…… ",
        "是哪里让你不满意？在这里可没有那个每天打你、关你在房间里的“爸爸”。",
        "没错，艾可~这里没有那个把你珍贵的东西都夺走的爸爸~ ",
        "在这里你想要什么，我都能给你 ",
        "我会让你实现所有愿望~",
        "纳尔克…… ",
        " 来吧，外面太危险了。你必须留在这里！"
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

