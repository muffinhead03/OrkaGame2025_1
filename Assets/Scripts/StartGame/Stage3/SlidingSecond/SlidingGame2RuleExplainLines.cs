// 파일명: SlidingGame2RuleExplainLines.cs
using UnityEngine;

[DisallowMultipleComponent]
public class SlidingGame2RuleExplainLines : MonoBehaviour
{
    [TextArea] public string[] koreanLines  = new string[7];
    [TextArea] public string[] englishLines = new string[7];
    [TextArea] public string[] japaneseLines= new string[7];
    [TextArea] public string[] chineseLines = new string[7];
    [TextArea] public string[] kazakhLines  = new string[7];

    public string[] GetLinesFor(string langStandardKey)
    {
        switch (langStandardKey)
        {
            case "korean":   return koreanLines;
            case "english":  return englishLines;
            case "japanese": return japaneseLines;
            case "chinese":  return chineseLines;
            case "kazakh":   return kazakhLines;
            default:         return englishLines;
        }
    }
}