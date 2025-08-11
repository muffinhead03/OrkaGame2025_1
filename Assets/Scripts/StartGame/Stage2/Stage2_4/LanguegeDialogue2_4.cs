using UnityEngine;

public class LanguageCollector2_4 : MonoBehaviour
{    [TextArea] public string[] KoreanAbove2_4 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove2_4 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove2_4 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove2_4 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove2_4 = {"Эко", "Пан", "Нарыке"};

    // �� ��� �迭
    public readonly string[] KoreanLines2_4 = {
        "히히 좋아~"
    };
    public readonly string[] EnglishLines2_4 = {
        "Hehe, Alright~"

    };
    public readonly string[] KazaLines2_4 = {
        "хихи жақсы~ "

    };

    public readonly string[] JapaneseLines2_4 = {
        "ふふ、いいね〜"

    };

    public readonly string[] ChineseLines2_4 = {
        "嘻嘻，好啊~"

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
                return KoreanLines2_4;
            case "english":
                return EnglishLines2_4;
            case "kazahustan":
            case "kaza":
                return KazaLines2_4;
            case "japanese":
                return JapaneseLines2_4;
            case "chinese":
                return ChineseLines2_4;
            default:
                Debug.LogWarning($"[LanguageCollector2_4] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_4;
        }
    }

}
