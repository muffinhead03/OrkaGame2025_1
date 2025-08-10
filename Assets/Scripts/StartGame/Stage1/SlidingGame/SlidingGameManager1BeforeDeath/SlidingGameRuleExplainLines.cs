using UnityEngine;

public class SlidingGameRuleExplainLines : MonoBehaviour
{
    [TextArea] public string[] koreanLines     = new string[7] { "", "", "", "", "", "", "" };
    [TextArea] public string[] englishLines    = new string[7] { "", "", "", "", "", "", "" };
    [TextArea] public string[] japaneseLines   = new string[7] { "", "", "", "", "", "", "" };
    [TextArea] public string[] chineseLines    = new string[7] { "", "", "", "", "", "", "" };
    [TextArea] public string[] kazakhLines     = new string[7] { "", "", "", "", "", "", "" };

    /// <summary>LanguageManager 표준키(korean/english/japanese/chinese/kazakh)로 언어 배열 반환</summary>
    public string[] GetLinesFor(string langStandardKey)
    {
        switch (langStandardKey)
        {
            case "korean":   return koreanLines;
            case "english":  return englishLines;
            case "japanese": return japaneseLines;
            case "chinese":  return chineseLines;
            case "kazakh":   return kazakhLines;
            default:         return englishLines; // 모르면 영어
        }
    }
}