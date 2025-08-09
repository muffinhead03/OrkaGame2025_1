using UnityEngine;

public class FirstCardClickLines : MonoBehaviour
{
    [TextArea] public string[] korean   = new string[6];
    [TextArea] public string[] japanese = new string[6];
    [TextArea] public string[] english  = new string[6];
    [TextArea] public string[] chinese  = new string[6];
    [TextArea] public string[] kazakh   = new string[6]; // kazakhstan

    public string GetLine(string lang, int index)
    {
        string Safe(string[] arr)
        {
            if (arr == null || index < 0 || index >= arr.Length) return "";
            return arr[index];
        }

        switch (lang)
        {
            case "korean":      return string.IsNullOrEmpty(Safe(korean))   ? Safe(english) : Safe(korean);
            case "japanese":    return string.IsNullOrEmpty(Safe(japanese)) ? Safe(english) : Safe(japanese);
            case "chinese":     return string.IsNullOrEmpty(Safe(chinese))  ? Safe(english) : Safe(chinese);
            case "kazakh":
            case "kazakhstan":  return string.IsNullOrEmpty(Safe(kazakh))   ? Safe(english) : Safe(kazakh);
            default:            return Safe(english);
        }
    }
}