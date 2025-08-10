using UnityEngine;

public class LanguageCollector1_3 : MonoBehaviour
{
    // 0: Korean, 1: English, 2: Kazakh, 3: Chinese, 4: Japanese (요청 순서 고정)
    public readonly string[] speaker1_3 = { "에코", "Echo", "Эко", "艾可", "エコ" };

    // 라인들은 반드시 같은 개수로 유지(여기선 2줄)
    public readonly string[] KoreanLines1_3  = { "됐다! 이제 나갈 수 있겠어", "어...라?" };
    public readonly string[] EnglishLines1_3 = { "It worked! Now I can get out", "H..uh…?" };
    public readonly string[] KazakhLines1_3  = { "Ашылды! Енді сыртқа шыға аламын", "О… А?" };
    public readonly string[] ChineseLines1_3 = { "成功了！现在可以出去了！", "咦……？" };
    public readonly string[] JapaneseLines1_3= { "やった！ これで出られる", "あ…れ？" };

    /// <summary>언어 → 고정 인덱스(0~4)</summary>
    public int GetLanguageIndex()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLowerInvariant();
        switch (lang)
        {
            case "korean":  return 0;
            case "english": return 1;
            case "kazakh":  return 2;
            case "chinese": return 3;
            case "japanese":return 4;
            default:        return 1; // 알 수 없으면 영어
        }
    }

    /// <summary>현재 언어의 스피커 표시 이름</summary>
    public string GetSpeakerName()
    {
        int idx = Mathf.Clamp(GetLanguageIndex(), 0, speaker1_3.Length - 1);
        string name = speaker1_3[idx];
        if (string.IsNullOrEmpty(name)) name = speaker1_3[1]; // 비어있으면 영어로
        return name;
    }

    /// <summary>현재 언어의 대사 배열(2줄)</summary>
    public string[] GetLines()
    {
        switch (GetLanguageIndex())
        {
            case 0: return KoreanLines1_3;
            case 1: return EnglishLines1_3;
            case 2: return KazakhLines1_3;
            case 3: return ChineseLines1_3;
            case 4: return JapaneseLines1_3;
            default:return EnglishLines1_3;
        }
    }
}