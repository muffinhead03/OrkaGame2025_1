using UnityEngine;
using TMPro;

public class FirstCardClickManager : MonoBehaviour
{
    [Header("CardClick 루트 (초기 비활성 추천)")]
    [SerializeField] private GameObject cardClickRoot;

    [Header("언어별 컨테이너 (각 컨테이너 안에 TMP 1개 + 해당 언어 폰트 에셋 지정)")]
    [SerializeField] private GameObject koreanClick;
    [SerializeField] private GameObject japaneseClick;
    [SerializeField] private GameObject englishClick;
    [SerializeField] private GameObject chineseClick;
    [SerializeField] private GameObject kazakhClick;

    [Header("대사 데이터 (MB)")]
    [SerializeField] private FirstCardClickKeyedLinesMB data;

    private string currentLang;              // normalized: korean/japanese/english/chinese/kazakh
    private int lastShownIndex = -1;
    private TextMeshProUGUI activeTMP;       // 현재 언어 컨테이너 안의 TMP
    private bool uiLocked = true;            // 대화 끝날 때 풀기

    void Awake()
    {
        var raw = LanguageManager.GetLanguage();
        currentLang = NormalizeLang(raw);

        if (cardClickRoot)
        {
            Debug.Log($"[DEBUG][CardClick] Awake: 초기 상태 activeSelf={cardClickRoot.activeSelf}, inHierarchy={cardClickRoot.activeInHierarchy}, path={GetPath(cardClickRoot.transform)}");
            cardClickRoot.SetActive(false);
            Debug.Log("[DEBUG][CardClick] Awake: cardClickRoot 강제 비활성화");
        }
        else
        {
            Debug.LogError("[DEBUG][CardClick] cardClickRoot가 NULL. 인스펙터에 루트를 할당하세요.");
        }
    }

    void OnEnable()  { LanguageManager.OnLanguageChanged += OnLangChanged; }
    void OnDisable() { LanguageManager.OnLanguageChanged -= OnLangChanged; }

    private void OnLangChanged(string _)
    {
        currentLang = NormalizeLang(LanguageManager.GetLanguage());

        // 잠겨있으면 UI는 켜지지 않게, TMP만 재바인딩
        if (!uiLocked && cardClickRoot && cardClickRoot.activeSelf)
            ActivateUIForCurrentLanguage();
        else
            RebindTMPOnly();

        if (!uiLocked && lastShownIndex >= 0)
            ShowByCardIndex(lastShownIndex);
    }

    // 대화 끝에서 호출: UI 켜기 + 초기화
    public void UnlockAndShow()
    {
        uiLocked = false;
        Debug.Log("[DEBUG][CardClick] UnlockAndShow() → uiLocked=false, ActivateUIForCurrentLanguage 호출");
        ActivateUIForCurrentLanguage();

        if (activeTMP != null)
        {
            activeTMP.text = "";
            SetTMPColorBlack();
        }

        if (cardClickRoot != null)
            Debug.Log($"[DEBUG][CardClick] UnlockAndShow 결과: activeSelf={cardClickRoot.activeSelf}, inHierarchy={cardClickRoot.activeInHierarchy}, parentActiveSelf={cardClickRoot.transform.parent?.gameObject.activeSelf}");
    }

    public void ShowByCardIndex(int indexZeroBased)
    {
        if (uiLocked) return;          // 아직 대사 중이면 무시
        if (data == null) return;

        if (cardClickRoot && !cardClickRoot.activeSelf) cardClickRoot.SetActive(true);
        ActivateUIForCurrentLanguage();
        if (activeTMP == null) return;

        lastShownIndex = Mathf.Clamp(indexZeroBased, 0, 5);

        // 항상 data(MB)에서 현재 언어로 텍스트를 가져옴
        string text = data.GetLine(currentLang, lastShownIndex);
        if (string.IsNullOrEmpty(text))
        {
            // 영어 폴백 → 그래도 없으면 토큰
            text = data.GetLine("english", lastShownIndex);
            if (string.IsNullOrEmpty(text))
                text = $"[{currentLang}{lastShownIndex + 1}]";
        }

        activeTMP.gameObject.SetActive(true);
        SetTMPColorBlack();
        activeTMP.text = text;
        activeTMP.ForceMeshUpdate(true);
    }

    public void ActivateUIForCurrentLanguage()
    {
        if (uiLocked)
        {
            Debug.Log("[DEBUG][CardClick] ActivateUIForCurrentLanguage → uiLocked=true라서 리턴");
            return;
        }

        if (cardClickRoot && !cardClickRoot.activeSelf)
        {
            Debug.Log("[DEBUG][CardClick] cardClickRoot.SetActive(true) 시도");
            cardClickRoot.SetActive(true);
        }

        // 전부 끄고 현재 언어만 켜기
        if (koreanClick)   koreanClick.SetActive(false);
        if (japaneseClick) japaneseClick.SetActive(false);
        if (englishClick)  englishClick.SetActive(false);
        if (chineseClick)  chineseClick.SetActive(false);
        if (kazakhClick)   kazakhClick.SetActive(false);

        GameObject target = englishClick;
        switch (currentLang)
        {
            case "korean":   target = koreanClick;   break;
            case "japanese": target = japaneseClick; break;
            case "chinese":  target = chineseClick;  break;
            case "kazakh":   target = kazakhClick;   break;
        }

        if (target) target.SetActive(true);

        // TMP 재바인딩
        activeTMP = target ? target.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        if (activeTMP == null && cardClickRoot != null)
            activeTMP = cardClickRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        Debug.Log($"[DEBUG][CardClick] Activate 끝: lang={currentLang}, target={(target?target.name:"NULL")}, tmp={(activeTMP?activeTMP.name:"NULL")}, rootActive={cardClickRoot?.activeSelf}");
        
        // 부모 비활성 탐지
        if (cardClickRoot && IsAnyParentInactive(cardClickRoot, out var culprit))
            Debug.LogError($"[DEBUG][CardClick] 부모 중 비활성 존재: culprit={culprit.name}, path={GetPath(culprit)}");
    }
    bool IsAnyParentInactive(GameObject go, out Transform culprit)
    {
        culprit = null;
        var p = go?.transform;
        while (p != null)
        {
            if (!p.gameObject.activeSelf) { culprit = p; return true; }
            p = p.parent;
        }
        return false;
    }
    string GetPath(Transform t)
    {
        if (t == null) return "null";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
        while (t.parent != null) { t = t.parent; sb.Insert(0, t.parent.name + "/"); }
        return sb.ToString();
    }

    private void RebindTMPOnly()
    {
        // UI는 켜지지 않게 한 채 TMP만 다시 찾음
        GameObject[] candidates = { koreanClick, japaneseClick, chineseClick, kazakhClick, englishClick, cardClickRoot };
        activeTMP = null;
        foreach (var go in candidates)
        {
            if (go == null) continue;
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) { activeTMP = tmp; break; }
        }
    }

    private void SetTMPColorBlack()
    {
        if (activeTMP == null) return;
        var col = activeTMP.color;
        col.a = 1f; col.r = 0f; col.g = 0f; col.b = 0f;
        activeTMP.color = col;
    }

    // 다양한 표기를 하나로 통일
    private static string NormalizeLang(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "english";
        raw = raw.ToLowerInvariant();

        if (raw == "chinese" || raw == "zh" || raw.StartsWith("zh-") || raw.Contains("chinese"))
            return "chinese";

        if (raw.StartsWith("ko") || raw == "korean")                         return "korean";
        if (raw.StartsWith("ja") || raw == "japanese")                       return "japanese";
        if (raw.StartsWith("ka") || raw == "kazakh" || raw == "kazakhstan")  return "kazakh";
        if (raw.StartsWith("en") || raw == "english")                        return "english";

        return "english";
    }
}
