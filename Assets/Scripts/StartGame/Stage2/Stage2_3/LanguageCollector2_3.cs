using UnityEngine;

public class LanguageCollector2_3 : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] KoreanLines2_3 = {
<<<<<<< HEAD
        "³»°¡ ÀÌ°å´Ù!",
        "¾î¶ó, ¹æ±İ ¹«½¼ ¼Ò¸® µé¸®Áö ¾Ê¾Ò¾î?",
        "¹«½¼ ¼Ò¸®? Àß¸ø µéÀº °Å°ÚÁö~ ³ª´Â ¸ø µé¾ú´Â°É",
        "±×°Íº¸´Ùµµ ´ÙÀ½ ÆÇ ½ÃÀÛÇÏÀÚ",
        "(ÇÏ±ä ÀÌ·± ÆòÈ­·Î¿î °÷¿¡ ±×·± ¼Ò¸®°¡...) ±×·¡, ÁÁ¾Æ!"
    };
    public readonly string[] EnglishLines2_3 = {
    "I did it...!",
    "Wait... did you hear that noise?",
    "What? That can't be right - I didn't hear a thing",
    "Never mind that, let's just start the next round",
    "(Well... there's no way a place this peaceful would have such a noise like that...) Okay...!"
};

    public readonly string[] KazaLines2_3 = {

=======
        "ë‚´ê°€ ì´ê²¼ë‹¤!",
        "ì–´ë¼, ë°©ê¸ˆ ë¬´ìŠ¨ ì†Œë¦¬ ë“¤ë¦¬ì§€ ì•Šì•˜ì–´?",
        "ë¬´ìŠ¨ ì†Œë¦¬? ì˜ëª» ë“¤ì€ ê±°ê² ì§€~ ë‚˜ëŠ” ëª» ë“¤ì—ˆëŠ”ê±¸",
        "ê·¸ê²ƒë³´ë‹¤ë„ ë‹¤ìŒ íŒ ì‹œì‘í•˜ì",
        "(í•˜ê¸´ ì´ëŸ° í‰í™”ë¡œìš´ ê³³ì— ê·¸ëŸ° ì†Œë¦¬ê°€...) ê·¸ë˜, ì¢‹ì•„!"
>>>>>>> 58898e4f1273b97d2ada7bf00e039897dc67cb45
    };

    // â¬‡â¬‡â¬‡ readonly ì œê±° + ì¸ìŠ¤í™í„°ì— ë³´ì´ê²Œ ë§Œë“¤ê¸°
    [TextArea(2, 5)] public string[] EnglishLines2_3;
    [TextArea(2, 5)] public string[] KazaLines2_3;
    [TextArea(2, 5)] public string[] JapaneseLines2_3;
    [TextArea(2, 5)] public string[] ChineseLines2_3;

    // ì–¸ì–´ ë¬¸ìì—´ì„ ê¹”ë”í•˜ê²Œ ì •ê·œí™”
    private static string Normalize(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return "korean";
        lang = lang.Trim().ToLower();

        if (lang.StartsWith("en")) return "english";
        if (lang.StartsWith("ko")) return "korean";
        if (lang.StartsWith("ja")) return "japanese";
        if (lang.StartsWith("zh") || lang.Contains("chinese")) return "chinese";
        if (lang.StartsWith("kk") || lang.Contains("kaza")) return "kaza"; // ì¹´ìí

        // ê¸°ì¡´ì— ì“°ë˜ ì˜¤íƒ€ ëŒ€ì‘
        if (lang == "kazahustan") return "kaza";

        return lang;
    }

    public string[] GetLines()
    {
        string lang = Normalize(LanguageManager.GetLanguage());
        switch (lang)
        {
            case "korean":   return KoreanLines2_3;
            case "english":  return EnglishLines2_3;
            case "japanese": return JapaneseLines2_3;
            case "chinese":  return ChineseLines2_3;
            case "kaza":     return KazaLines2_3;
            default:
                Debug.LogWarning($"[LanguageCollector2_3] Unknown language '{lang}', fallback to Korean.");
                return KoreanLines2_3;
        }
    }
}