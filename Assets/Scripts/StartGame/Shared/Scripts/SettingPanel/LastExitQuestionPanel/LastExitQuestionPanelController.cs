using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class LastExitQuestionPanelController : MonoBehaviour
{
    [Header("패널 루트 (끄고 싶은 오브젝트)")]
    [SerializeField] private GameObject panelRoot; // ← LastExitQuestionPanel 오브젝트 드래그

    [Header("Text Boxes by Language (자식 오브젝트 할당)")]
    [SerializeField] private GameObject koreanBox;    // "Korean"
    [SerializeField] private GameObject englishBox;   // "English"
    [SerializeField] private GameObject chineseBox;   // "Chinese"
    [SerializeField] private GameObject japaneseBox;  // "Japanese"
    [SerializeField] private GameObject kazakhBox;    // "Kaza" 혹은 "Kazakh"

    [Header("Options")]
    [SerializeField] private bool autoFindChildrenByName = true; 
    [SerializeField] private string mainMenuSceneName = "mainMenuPanel"; 

    [Header("Events")]
    public UnityEvent onClosed;

    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject; // 기본값은 자기 자신
        if (autoFindChildrenByName) AutoFind();
    }

    private void OnEnable()
    {
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += HandleLanguageChanged;
        ApplyLanguage(LanguageManager.GetLanguage());
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(string lang)
    {
        if (LanguageManager.IsLanguageSwitching) return;
        ApplyLanguage(lang);
    }

    private void ApplyLanguage(string lang)
    {
        string key = string.IsNullOrEmpty(lang) ? "english" : lang.ToLowerInvariant();

        SetActive(koreanBox,   false);
        SetActive(englishBox,  false);
        SetActive(chineseBox,  false);
        SetActive(japaneseBox, false);
        SetActive(kazakhBox,   false);

        switch (key)
        {
            case "korean":   SetActive(koreanBox, true);   break;
            case "japanese": SetActive(japaneseBox, true); break;
            case "chinese":  SetActive(chineseBox, true);  break;
            case "kazakh":   SetActive(kazakhBox, true);   break;
            case "english":
            default:         SetActive(englishBox, true);  break;
        }
    }

    private void SetActive(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }

    private void AutoFind()
    {
        Transform root = panelRoot != null ? panelRoot.transform : transform;

        if (!koreanBox)   koreanBox   = root.Find("Korean")?.gameObject;
        if (!englishBox)  englishBox  = root.Find("English")?.gameObject;
        if (!chineseBox)  chineseBox  = root.Find("Chinese")?.gameObject;
        if (!japaneseBox) japaneseBox = root.Find("Japanese")?.gameObject;

        if (!kazakhBox)
        {
            var t = root.Find("Kaza") ?? root.Find("Kazakh");
            if (t) kazakhBox = t.gameObject;
        }
    }

    // ── 버튼 연결용 ─────────────────────────────────────────────────────────────
    public void OnTurnOffButtonClicked()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        else gameObject.SetActive(false);

        onClosed?.Invoke();
    }

    public void OnExitButtonClicked()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogError("[LastExitQuestionPanelController] mainMenuSceneName이 비었습니다.");
    }
}
