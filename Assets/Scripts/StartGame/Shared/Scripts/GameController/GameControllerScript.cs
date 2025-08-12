using UnityEngine;
using System.Globalization;

public class GameControllerScript : MonoBehaviour
{
    [Header("패널들")]
    public GameObject FirstPanel;
    public GameObject SettingPanel;

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

    [Header("타이핑 제어 (더 이상 껐다 켜지 않음)")]
    public MonoBehaviour dialogueControllerToPause;

    [Header("텍스트 위치")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    private string previousNormLang = "";

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Start()
    {
        // 씬 시작 시 한 번 강제 적용
        ApplyLanguageNow();
    }

    private void Update()
    {
        // 패널 열림 판정 (대사 스크립트/타이핑은 건드리지 않음)
        // bool firstOpen = FirstPanel != null && FirstPanel.activeSelf && FirstPanel.transform.localPosition == Vector3.zero;

        // 언어 문자열 바뀔 때만 적용
        string norm = NormalizeLang(LanguageManager.GetLanguage());
        if (norm != previousNormLang)
        {
            ApplyLanguage(norm);
        }
    }

    private void OnLanguageChanged(string _raw)
    {
        // 드롭다운/외부 변경에도 즉시 반응
        ApplyLanguageNow();
    }

    // ===== 핵심: 항상 정규화 → 모두 끄고 → 해당 언어만 켜기 =====
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
            case "korean":
                ActivateLanguage(Korean_AboveLine, Korean_Story); break;
            case "english":
                ActivateLanguage(English_Above, English_Story); break;
            case "japanese":
                ActivateLanguage(Japanese_Above, Japanese_Story); break;
            case "chinese":
                ActivateLanguage(Chinese_Above, Chinese_Story); break;
            case "kazakh":
                ActivateLanguage(Kaza_Above, Kaza_Story); break;
            default:
                Debug.LogWarning($"[GameControllerScript] Unknown lang '{normLang}', fallback=korean");
                ActivateLanguage(Korean_AboveLine, Korean_Story);
                break;
        }

        previousNormLang = normLang;
        Debug.Log($"[GameControllerScript] lang applied: {normLang}");
    }

    // ko, ko-KR, 한국어 등 → "korean"으로 통일
    private string NormalizeLang(string raw)
    {
        string s = raw ?? "";
        s = s.Trim();

        // CultureInfo가 먹히면 두 자리 코드로 축약
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
        if (above != null)
        {
            above.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            EnableTypewriterSafely(above);
        }

        if (story != null)
        {
            story.gameObject.SetActive(true);
            story.anchoredPosition = StoPo;
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

    // ❗ 텍스트를 지우지 않도록 '리셋' 금지. 그냥 켜기만 함.
    private void EnableTypewriterSafely(RectTransform obj)
    {
        var typewriter = obj.GetComponent<TypewriterEffect>();
        if (typewriter != null)
        {
            // typewriter.StopTyping();  // ← 금지: 내용 리셋 위험
            typewriter.enabled = true;   // 켜기만 (이미 켜져 있으면 그대로)
        }
    }

    public void CarrotButtonClicked()
    {
        Debug.Log("🐰 Carrot button clicked!");

        if (FirstPanel == null)
        {
            Debug.LogError("❌ FirstPanel is NULL!");
            return;
        }

        FirstPanel.SetActive(true);
        FirstPanel.transform.localPosition = Vector3.zero;

        if (SettingPanel != null)
            SettingPanel.SetActive(false);

        // ❌ 패널 열릴 때 언어를 다시 적용하지 않음 (타이핑/텍스트 깜빡임 방지)
        // ApplyLanguageNow();
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
        // SetLanguage가 끝나면 OnLanguageChanged에서 ApplyLanguageNow()가 호출됨
    }
}
