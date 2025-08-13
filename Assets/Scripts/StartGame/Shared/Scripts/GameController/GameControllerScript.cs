using UnityEngine;
using UnityEngine.UI;   // Button, CanvasGroup
using System.Globalization;

public class GameControllerScript : MonoBehaviour
{
    [Header("패널들")]
    public GameObject FirstPanel;
    public GameObject SettingPanel;

    [Header("다음 진행 타깃(대화 컨트롤러 등)")]
    [Tooltip("Next를 수행할 컴포넌트(예: DialogueController). SendMessage로 메서드 호출")]
    public MonoBehaviour dialogueControllerToPause;
    [Tooltip("대상 컴포넌트에 있는 '다음 진행' 메서드명")]
    public string nextMethodName = "Next";

    [Header("Next 버튼 (여기에 버튼 드래그)")]
    [SerializeField] private Button nextButton;
    private CanvasGroup nextButtonCg; // 클릭 차단용(시각 변화 없음)

    [Header("언어별 텍스트 박스 오브젝트 (RectTransform)")]
    public RectTransform Korean_AboveLine;
    public RectTransform Korean_Story;
    public RectTransform English_Above;
    public RectTransform English_Story;
    public RectTransform Japanese_Above;
    public RectTransform Japanese_Story;
    public RectTransform Chinese_Above;
    public RectTransform Chinese_Story;
    public RectTransform Kaza_Above;
    public RectTransform Kaza_Story;

    [Header("중앙 판정 허용 오차(픽셀)")]
    [SerializeField] private float centerTolerance = 0.5f;

    private string previousNormLang = "";

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;

        // 버튼 리스너 연결
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextGuardedClick);

            // CanvasGroup 확보(없으면 추가) → 레이캐스트만 제어
            nextButtonCg = nextButton.GetComponent<CanvasGroup>();
            if (nextButtonCg == null) nextButtonCg = nextButton.gameObject.AddComponent<CanvasGroup>();
            nextButtonCg.interactable = true;        // 시각/상태 유지
            nextButtonCg.ignoreParentGroups = false; // 상위 CG 영향 받음
            nextButtonCg.blocksRaycasts = true;      // 기본: 클릭 허용
        }
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;

        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextGuardedClick);
    }

    private void Start()
    {
        ApplyLanguageNow();
    }

    private void Update()
    {
        // 언어 문자열 바뀔 때만 적용
        string norm = NormalizeLang(LanguageManager.GetLanguage());
        if (norm != previousNormLang)
            ApplyLanguage(norm);

        // ✅ 패널이 중앙에서 활성일 때만 버튼의 레이캐스트를 차단(시각 변화 없음)
        if (nextButtonCg != null)
        {
            bool block = IsPanelBlockingNext();
            nextButtonCg.blocksRaycasts = !block;
        }
    }

    private void OnLanguageChanged(string _raw)
    {
        ApplyLanguageNow();
    }

    // ===== 언어 적용 =====
    private void ApplyLanguageNow()
    {
        string norm = NormalizeLang(LanguageManager.GetLanguage());
        ApplyLanguage(norm);
    }

    private void ApplyLanguage(string normLang)
    {
        DisableAllLanguageTexts();

        switch (normLang)
        {
            case "korean":   ActivateLanguage(Korean_AboveLine,   Korean_Story);   break;
            case "english":  ActivateLanguage(English_Above,      English_Story);  break;
            case "japanese": ActivateLanguage(Japanese_Above,     Japanese_Story); break;
            case "chinese":  ActivateLanguage(Chinese_Above,      Chinese_Story);  break;
            case "kazakh":   ActivateLanguage(Kaza_Above,         Kaza_Story);     break;
            default:
                Debug.LogWarning($"[GameControllerScript] Unknown lang '{normLang}', fallback=korean");
                ActivateLanguage(Korean_AboveLine, Korean_Story);
                break;
        }

        previousNormLang = normLang;
    }

    // ko, ko-KR, 한국어 등 → "korean"으로 통일
    private string NormalizeLang(string raw)
    {
        string s = raw ?? "";
        s = s.Trim();

        try
        {
            var two = new CultureInfo(s).TwoLetterISOLanguageName.ToLowerInvariant();
            s = two; // ko, en, ja, zh, kk ...
        }
        catch
        {
            s = s.ToLowerInvariant();
        }

        if (s == "ko") return "korean";
        if (s == "en") return "english";
        if (s == "ja") return "japanese";
        if (s == "zh") return "chinese";
        if (s == "kk") return "kazakh";

        var lower = (raw ?? "").ToLowerInvariant();
        if (lower.Contains("korean") || lower.Contains("한국")) return "korean";
        if (lower.Contains("english") || lower.Contains("영어")) return "english";
        if (lower.Contains("japanese") || lower.Contains("일본")) return "japanese";
        if (lower.Contains("chinese") || lower.Contains("중국")) return "chinese";
        if (lower.Contains("kazakh") || lower.Contains("kaza")) return "kazakh";

        return "korean"; // 안전 기본값
    }

    private void ActivateLanguage(RectTransform above, RectTransform story)
    {
        // ✅ 위치(anchoredPosition) 제어 제거: 다른 코드가 담당
        if (above != null)
        {
            above.gameObject.SetActive(true);
            EnableTypewriterSafely(above);
        }

        if (story != null)
        {
            story.gameObject.SetActive(true);
            EnableTypewriterSafely(story);
        }
    }

    private void DisableAllLanguageTexts()
    {
        RectTransform[] all = {
            Korean_AboveLine, Korean_Story,
            English_Above, English_Story,
            Japanese_Above, Japanese_Story,
            Chinese_Above, Chinese_Story,
            Kaza_Above, Kaza_Story
        };

        foreach (var r in all)
            if (r != null) r.gameObject.SetActive(false);
    }

    // 텍스트를 지우지 않도록 '리셋' 금지. 그냥 켜기만.
    private void EnableTypewriterSafely(RectTransform obj)
    {
        var typewriter = obj.GetComponent<TypewriterEffect>();
        if (typewriter != null)
            typewriter.enabled = true;
    }

    // ====== Next 버튼 가드 ======
    private bool IsPanelAtCenterAndActive(GameObject panel)
    {
        if (panel == null) return false;
        if (!panel.activeInHierarchy) return false;
        Vector3 lp = panel.transform.localPosition;
        return lp.sqrMagnitude <= centerTolerance * centerTolerance; // (0,0,0) 근처
    }

    private bool IsPanelBlockingNext()
    {
        return IsPanelAtCenterAndActive(FirstPanel) || IsPanelAtCenterAndActive(SettingPanel);
    }

    private void OnNextGuardedClick()
    {
        // 레이캐스트 차단으로 원천 봉쇄되지만, 혹시 다른 경로(키보드 등) 대비로 한 번 더 가드
        if (IsPanelBlockingNext())
        {
            Debug.Log("[GameControllerScript] Next blocked: panel open at center.");
            return;
        }

        if (dialogueControllerToPause != null && !string.IsNullOrEmpty(nextMethodName))
            dialogueControllerToPause.SendMessage(nextMethodName, SendMessageOptions.DontRequireReceiver);
    }

    // ====== 예: 다른 UI에서도 호출 가능 ======
    public void CarrotButtonClicked()
    {
        if (FirstPanel == null)
        {
            Debug.LogError("❌ FirstPanel is NULL!");
            return;
        }

        FirstPanel.SetActive(true);
        FirstPanel.transform.localPosition = Vector3.zero;

        if (SettingPanel != null)
            SettingPanel.SetActive(false);
    }

    public void OnLanguageDropdownChanged(int index)
    {
        string selectedLang = index switch
        {
            0 => "korean",
            1 => "english",
            2 => "japanese",
            3 => "chinese",
            4 => "kazakh",
            _ => "english"
        };
        LanguageManager.SetLanguage(selectedLang);
        // LanguageManager.OnLanguageChanged 이벤트에서 ApplyLanguageNow 호출됨
    }
}
