using UnityEngine;

public class LanguageCollector2_5 : MonoBehaviour
{
    [TextArea] public string[] KoreanAbove2_5 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove2_5 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove2_5 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove2_5 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove2_5 = {"Эко", "Пан", "Нарыке"};

    // �� ��� �迭
    public readonly string[] KoreanLines2_5 = {
        "뭐?",
        "너는 여기 있어야만 해"
    };
    public readonly string[] EnglishLines2_5 = {
    "WHAT?",
    "YOU MUST STAY HERE",
};
    public readonly string[] KazaLines2_5 = {
        "Не?",
        "Сен осында болу керексің"
    };

    public readonly string[] JapaneseLines2_5 = {
        "は?",
        "キミはここにいなければならない"
    };

    public readonly string[] ChineseLines2_5 = {
        "什么？",
        "你必须待在这里"
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
                return KoreanLines2_5;
            case "english":
                return EnglishLines2_5;
            case "kazahustan":
            case "kaza":
                return KazaLines2_5;
            case "japanese":
                return JapaneseLines2_5;
            case "chinese":
                return ChineseLines2_5;
            default:
                Debug.LogWarning($"[LanguageCollector2_5] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines2_5;
        }
    }

}
