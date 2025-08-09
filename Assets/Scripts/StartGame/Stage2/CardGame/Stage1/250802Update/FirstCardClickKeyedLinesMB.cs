using UnityEngine;

public class FirstCardClickKeyedLinesMB : MonoBehaviour
{
    [Header("Korean (k1~k6)")]
    [TextArea] public string k1="k1"; [TextArea] public string k2="k2"; [TextArea] public string k3="k3";
    [TextArea] public string k4="k4"; [TextArea] public string k5="k5"; [TextArea] public string k6="k6";

    [Header("Japanese (j1~j6)")]
    [TextArea] public string j1="j1"; [TextArea] public string j2="j2"; [TextArea] public string j3="j3";
    [TextArea] public string j4="j4"; [TextArea] public string j5="j5"; [TextArea] public string j6="j6";

    [Header("English (e1~e6)")]
    [TextArea] public string e1="e1"; [TextArea] public string e2="e2"; [TextArea] public string e3="e3";
    [TextArea] public string e4="e4"; [TextArea] public string e5="e5"; [TextArea] public string e6="e6";

    [Header("Chinese (c1~c6)")]
    [TextArea] public string c1="c1"; [TextArea] public string c2="c2"; [TextArea] public string c3="c3";
    [TextArea] public string c4="c4"; [TextArea] public string c5="c5"; [TextArea] public string c6="c6";

    [Header("Kazakh (ka1~ka6)")]
    [TextArea] public string ka1="ka1"; [TextArea] public string ka2="ka2"; [TextArea] public string ka3="ka3";
    [TextArea] public string ka4="ka4"; [TextArea] public string ka5="ka5"; [TextArea] public string ka6="ka6";

    // ✅ LanguageManager와 바로 연동하는 편의 함수
    public string GetLine(int index)
    {
        return GetLine(LanguageManager.GetLanguage(), index);
    }

    public string GetLine(string langKey, int index)
    {
        int n = Mathf.Clamp(index + 1, 1, 6);
        string norm = Normalize(langKey);

        string line =
            norm == "k"  ? GetK(n)  :
            norm == "j"  ? GetJ(n)  :
            norm == "e"  ? GetE(n)  :
            norm == "c"  ? GetC(n)  :
            norm == "ka" ? GetKa(n) : GetE(n);

        // ✅ 빈 문자열이면 영어 폴백
        if (string.IsNullOrEmpty(line))
        {
            line = GetE(n);
            if (string.IsNullOrEmpty(line)) line = $"[{norm}{n}]";
        }

        Debug.Log($"[CardData] GetLine lang='{langKey}'(norm='{norm}') index={index}(n={n}) → '{line}'");
        return line;
    }

    static string Normalize(string lang)
    {
        if (string.IsNullOrEmpty(lang)) return "e";
        lang = lang.Trim().ToLowerInvariant();

        if (lang.StartsWith("ko") || lang == "korean" || lang == "k") return "k";
        if (lang.StartsWith("ja") || lang == "japanese" || lang == "j") return "j";
        if (lang.StartsWith("en") || lang == "english" || lang == "e") return "e";
        if (lang.StartsWith("zh") || lang.Contains("chinese") || lang == "c"
            || lang.StartsWith("zh-hans") || lang.StartsWith("zh-hant")) return "c";
        if (lang.StartsWith("ka") || lang == "kazakh" || lang == "kazakhstan" || lang == "kazahustan") return "ka";

        return "e";
    }

    string GetK(int n){ switch(n){case 1:return k1;case 2:return k2;case 3:return k3;case 4:return k4;case 5:return k5;default:return k6;} }
    string GetJ(int n){ switch(n){case 1:return j1;case 2:return j2;case 3:return j3;case 4:return j4;case 5:return j5;default:return j6;} }
    string GetE(int n){ switch(n){case 1:return e1;case 2:return e2;case 3:return e3;case 4:return e4;case 5:return e5;default:return e6;} }
    string GetC(int n){ switch(n){case 1:return c1;case 2:return c2;case 3:return c3;case 4:return c4;case 5:return c5;default:return c6;} }
    string GetKa(int n){ switch(n){case 1:return ka1;case 2:return ka2;case 3:return ka3;case 4:return ka4;case 5:return ka5;default:return ka6;} }
}
