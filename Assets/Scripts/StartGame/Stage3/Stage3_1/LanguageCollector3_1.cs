using UnityEngine;

public class LanguageCollector3_1 : MonoBehaviour
{
    // 언어별 대사 배열
    public readonly string[] KoreanLines3_1 = {
        "헉... 방금 뭐였지...?",
        "일어났나, 에코?",
        "당신은...?",
        "아! 쓰러질 때마다 나타난 건 당신이죠...?",
        "당신은 누구죠?",
        "(아까 나르케가 누군가의 선택으로 이 세상에 올 수 있다고 했지...)",
        "저를 이 세계로 부른 것은 당신이죠?",
        "잠시 진정하렴. 내 이름은 판. 이 세상을 관리하는 자이지.",
        "그리고 미리 말해두지만 나는 이 세상의 유지를 위한 관리만을 하지,",
        "이 세상에 누구를 들일지를 선택할 권한은 없단다.",
        "그럼 누가 저를 이 세계로 부른 거죠?",
        "글쎄. 그건 답하기는 어렵구나, 에코.",
        "(왜 답하기 어렵다는 거지... 그것보다도) 저는 집에 돌아가고 싶어요",
        "오랫동안 기다려온 마을의 음악 축제에 가고 싶어요",
        "그곳에서 노래를 부르는 것까지는 어렵겠지만...",
        "가서 노래를 듣고, 즐기고 싶어요!",
        "이번이 마지막일지도 몰라요... 도와주세요, 제발...!",
        "............그래. 그것이 에코가 바라는 일이라면...",
        "(뭐지? 방금 목소리 뭔가 쓸쓸한 듯한...)",
        "일단 알겠다. 집에 돌아가려면 이 장치를 풀어야 된단다."
    };
    public readonly string[] EnglishLines3_1 = {

    };
    public readonly string[] KazaLines3_1 = {

    };

    public readonly string[] JapaneseLines3_1 = {

    };

    public readonly string[] ChineseLines3_1 = {

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
                return KoreanLines3_1;
            case "english":
                return EnglishLines3_1;
            case "kazahustan":
            case "kaza":
                return KazaLines3_1;
            case "japanese":
                return JapaneseLines3_1;
            case "chinese":
                return ChineseLines3_1;
            default:
                Debug.LogWarning($"[LanguageCollector3_1] Unknown language '{lang}', using Korean as fallback.");
                return KoreanLines3_1;
        }
    }

}
