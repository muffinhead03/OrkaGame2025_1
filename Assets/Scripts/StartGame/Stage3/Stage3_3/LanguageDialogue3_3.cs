using UnityEngine;

public class LanguageCollector3_3 : MonoBehaviour
{    [TextArea] public string[] KoreanAbove1_2 = {"에코", "판", "나르케"};
    [TextArea] public string[] EnglishAbove1_2 = {"Echo", "Pan", "Narke"} ;
    [TextArea] public string[] JapaneseAbove1_2 = {"エコー", "パーン","ナルケ" };
    [TextArea] public string[] ChineseAbove1_2 = {"艾可","潘","纳尔克"};
    [TextArea] public string[] KazaAbove1_2 = {"Эко", "Пан", "Нарыке"};

    // �� ��� �迭
    public readonly string[] KoreanLines3_3 = {
        "좋아! ",
        "우린 영원히 함께야, 에코!",

    };
    public readonly string[] EnglishLines3_3 = {
"Excellent!", "We shall be together forever, Echo!"
    };
    public readonly string[] KazaLines3_3 = {
" Жақсы?","Біз өмір бойы бірге боламыз, Эко!"
    };

    public readonly string[] JapaneseLines3_3 = {
    "うれしい!","ボクたちはこれからもずっと一緒だよ、エコー！"
    };

    public readonly string[] ChineseLines3_3 = {
        "太好了！ ","我们永远在一起，艾可！"
        
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
                return KoreanLines3_3;
            case "english":
                return EnglishLines3_3;
            case "kazahustan":
            case "kaza":
                return KazaLines3_3;
            case "japanese":
                return JapaneseLines3_3;
            case "chinese":
                return ChineseLines3_3;
            default:
                Debug.LogWarning($"[LanguageCollector3_3] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_3;
        }
    }

}
