using UnityEngine;

public class FirstCardLanguageScript : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines = {
        "자자~~ 지금부터 카드게임 시작할게~",
        "룰은 간단해!",
        "오른쪽에 내가 섞은 카드 무더기가 있어",
        "그걸 마우스로 잡아서 왼쪽 테이블에 놓은 뒤",
        "카드 안의 그림이 자연스럽게 하나의 스토리가 되도록",
        "에코가 화면 밑 카드함에  배열하면 끝!",
        "끝나면 화면 위의 당근을 눌러줘~~"
    };

    public readonly string[] EnglishLines = {
        "I don't know where this is...",
        "Why am I alone in a place like this?",
        "Something must be wrong.",
        "I should look around.",
        "I think I hear something over there.",
        "Maybe I'll figure it out if I get closer.",
        "Okay, let's go carefully."
    };

    public readonly string[] KazaLines = {
        "Бұл жердің қайда екенін білмеймін...",
        "Неге мен осындай жерде жалғызбын?",
        "Бір нәрсе дұрыс емес сияқты.",
        "Маңа-айналаны қарап шығуым керек.",
        "Ана жақтан бір дыбыс естілгендей.",
        "Жақындасам, мүмкін не болып жатқанын түсінермін.",
        "Жарайды, абайлап барайын."
    };

    public readonly string[] JapaneseLines = {
        "ここがどこかわからない……。",
        "どうしてこんな場所に一人でいるんだろう？",
        "きっと何かがおかしい。",
        "周囲を調べてみよう。",
        "あっちから音が聞こえる気がする。",
        "近づけば何かわかるかもしれない。",
        "よし、慎重に行こう。"
    };

    public readonly string[] ChineseLines = {
        "我不知道这里是哪里……",
        "为什么我会独自在这个地方？",
        "肯定发生了什么不对劲的事情。",
        "我得看看周围的情况。",
        "好像从那边传来什么声音。",
        "靠近一点或许能知道发生了什么。",
        "好，慢慢靠近看看。"
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
                return KoreanLines;
            case "english":
                return EnglishLines;
            case "kazahustan":
            case "kaza":
                return KazaLines;
            case "japanese":
                return JapaneseLines;
            case "chinese":
                return ChineseLines;
            default:
                Debug.LogWarning($"[FirstCardLanguageScript] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines;
        }
    }
}
