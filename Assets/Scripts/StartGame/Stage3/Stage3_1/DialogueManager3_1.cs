using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager3_1 : MonoBehaviour
{
    [Header("언어별 RectTransform")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("기본 위치값")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    [Header("UI")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("BGM")]
    public AudioSource bgmSource;

    [Header("표정 오브젝트")]
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_defaultObj;
    public GameObject Eco_readyObj;
    public GameObject Eco_smiledObj;
    public GameObject Eco_surprisedObj;
    public GameObject Pan_defaultObj;
    public GameObject Pan_4eyeclosedObj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사/이름 소스")]
    public LanguageCollector3_1 languageCollector;

    [Header("진행 차단 패널(활성 + 원점일 때 차단)")]
    [SerializeField] private RectTransform FirstPanel;
    [SerializeField] private RectTransform SettingPanel;
    [SerializeField] private bool requireActiveForBlock = true;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    private bool isTyping = false;       // 현재 줄 타이핑 중?
    private bool isFullyShown = false;   // 현재 줄이 완전히 출력됨?
    private bool blocked = false;        // 패널로 인해 진행이 차단 중?

    // === 언어 헬퍼 ===
    private string CurrentLanguage => NormalizeLang(LanguageManager.GetLanguage());
    private string NormalizeLang(string raw)
    {
        string s = (raw ?? "korean").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        if (s.StartsWith("ka") || s.Contains("kaza") || s.Contains("kazah")) return "kaza";
        return s;
    }

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNext);
            nextButton.gameObject.SetActive(false);
        }
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private IEnumerator Start()
    {
        SetupLanguageUI();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        LoadLinesForCurrentLanguage();
        index = 0;
        yield return StartCoroutine(ShowLineSequence());
    }

    private void Update()
    {
        bool nowBlocked = IsBlockingOpen();
        if (nowBlocked && !blocked)
        {
            // 막히는 순간: 현재 줄을 즉시 완성(인덱스 유지)
            ForceCompleteCurrentLine();
        }
        blocked = nowBlocked;
    }

    // ===== 패널 차단 판정 =====
    private bool IsRectAtOrigin(RectTransform rt)
    {
        if (rt == null) return false;
        if (requireActiveForBlock && !rt.gameObject.activeInHierarchy) return false;
        const float eps = 0.01f;
        Vector2 ap = rt.anchoredPosition;
        Vector3 lp = rt.localPosition;
        bool apZero = Mathf.Abs(ap.x) <= eps && Mathf.Abs(ap.y) <= eps;
        bool lpZero = Mathf.Abs(lp.x) <= eps && Mathf.Abs(lp.y) <= eps && Mathf.Abs(lp.z) <= eps;
        return apZero || lpZero;
    }
    private bool IsBlockingOpen() => IsRectAtOrigin(FirstPanel) || IsRectAtOrigin(SettingPanel);

    private void LoadLinesForCurrentLanguage()
    {
        string lang = CurrentLanguage;
        switch (lang)
        {
            case "korean":   lines = languageCollector.KoreanLines3_1; break;
            case "english":  lines = languageCollector.EnglishLines3_1; break;
            case "japanese": lines = languageCollector.JapaneseLines3_1; break;
            case "chinese":  lines = languageCollector.ChineseLines3_1; break;
            case "kaza":     lines = languageCollector.KazaLines3_1; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines3_1;
                break;
        }
        if (lines == null || lines.Length == 0) lines = new[] { "" };
    }

    private IEnumerator ShowLineSequence()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        UpdateCharacterFace(index);
        yield return new WaitForSeconds(0.5f);

        // 새 줄 시작 전에 버튼 숨김
        nextButton?.gameObject.SetActive(false);

        // 타이핑 시작
        StartTyping(lines[index]);

        // 타이핑이 끝날 때까지 대기(중간에 ForceComplete될 수 있음)
        while (isTyping) yield return null;

        // 줄이 완성되면 Next 노출
        nextButton?.gameObject.SetActive(true);
        nextButton.interactable = true;
    }

    private void StartTyping(string text)
    {
        StopTyping();
        typingCoroutine = StartCoroutine(TypeText(text ?? ""));
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    private void ForceCompleteCurrentLine()
    {
        if (lines == null || index < 0 || index >= lines.Length) return;

        StopTyping();
        if (storyText != null) storyText.text = lines[index]; // 현재 줄 즉시 완성
        isFullyShown = true;
        nextButton?.gameObject.SetActive(true);
        if (nextButton != null) nextButton.interactable = true;
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        isFullyShown = false;

        if (storyText != null) storyText.text = string.Empty;

        foreach (char c in fullText)
        {
            // 중간에 막히면 즉시 완성 로직으로 넘겨서 종료
            if (IsBlockingOpen())
            {
                break;
            }
            if (storyText != null) storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 막혔든 끝났든 최종 정리
        if (storyText != null) storyText.text = fullText;
        isTyping = false;
        isFullyShown = true;
        typingCoroutine = null;
    }

    // Next 버튼
    private void OnNext()
    {
        // 1) 막힌 상태면 진행 금지. (타이핑 중이라면 강제 완성만)
        if (IsBlockingOpen())
        {
            if (isTyping) ForceCompleteCurrentLine();
            return; // 인덱스 유지
        }

        // 2) 타이핑 중이면 먼저 완성하고 종료(인덱스 유지)
        if (isTyping)
        {
            ForceCompleteCurrentLine();
            return;
        }

        // 3) 이미 완성된 상태 → 다음 줄로
        nextButton?.gameObject.SetActive(false);
        index++;

        if (lines == null || index >= lines.Length)
        {
            SceneManager.LoadScene("SlidingGameSecond");
            return;
        }

        StartCoroutine(ShowLineSequence());
    }

    // ===== 화자/표정 =====
    private void UpdateCharacterFace(int idx)
    {
        Eco_eyeclosedObj?.SetActive(false);
        Eco_defaultObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_smiledObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Pan_defaultObj?.SetActive(false);
        Pan_4eyeclosedObj?.SetActive(false);

        switch (idx)
        {
            case 0:  Eco_surprisedObj?.SetActive(true); break;
            case 1:
            case 7:
            case 8:
            case 11:
            case 19: Pan_defaultObj?.SetActive(true); break;
            case 9:
            case 17: Pan_4eyeclosedObj?.SetActive(true); break;
            case 2:
            case 3:
            case 6:
            case 12:
            case 13:
            case 15: Eco_readyObj?.SetActive(true); break;
            case 4:
            case 18: Eco_defaultObj?.SetActive(true); break;
            case 5:
            case 10:
            case 16: Eco_eyeclosedObj?.SetActive(true); break;
            case 14: Eco_smiledObj?.SetActive(true); break;
        }

        bool isPan = IsPanLine(idx);
        string speakerName = GetSpeakerName(isPan);
        if (aboveText != null)
            aboveText.text = speakerName;
    }

    private bool IsPanLine(int idx)
    {
        switch (idx)
        {
            case 1:
            case 7:
            case 8:
            case 9:
            case 11:
            case 17:
            case 19:
                return true;
            default:
                return false;
        }
    }

    private string GetSpeakerName(bool isPan)
    {
        if (languageCollector == null)
            return isPan ? "Pan" : "Echo";

        string[] names;
        switch (CurrentLanguage)
        {
            case "korean":   names = languageCollector.KoreanAbove1_2;   break;
            case "english":  names = languageCollector.EnglishAbove1_2;  break;
            case "japanese": names = languageCollector.JapaneseAbove1_2; break;
            case "chinese":  names = languageCollector.ChineseAbove1_2;  break;
            case "kaza":     names = languageCollector.KazaAbove1_2;     break;
            default:         names = languageCollector.EnglishAbove1_2;  break;
        }
        if (names == null || names.Length < 2)
            return isPan ? "Pan" : "Echo";

        return isPan ? (names[1] ?? "Pan") : (names[0] ?? "Echo");
    }

    private void SetupLanguageUI()
    {
        var all = new[] {
            Korean_Above, Korean_Story,
            English_Above, English_Story,
            Japanese_Above, Japanese_Story,
            Chinese_Above, Chinese_Story,
            Kaza_Above, Kaza_Story
        };
        foreach (var rt in all) rt?.gameObject.SetActive(false);

        string lang = CurrentLanguage;
        RectTransform above = Korean_Above, story = Korean_Story;
        switch (lang)
        {
            case "english":   above = English_Above;   story = English_Story;   break;
            case "japanese":  above = Japanese_Above;  story = Japanese_Story;  break;
            case "chinese":   above = Chinese_Above;   story = Chinese_Story;   break;
            case "kaza":      above = Kaza_Above;      story = Kaza_Story;      break;
            // default: korean
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            aboveText = FindTMP(above);
            storyText = FindTMP(story);

            if (aboveText == null || storyText == null)
                Debug.LogWarning("[DialogueManager3_1] Active language TMP not found. Check children TextMeshProUGUI.");
        }
    }

    private TextMeshProUGUI FindTMP(RectTransform root)
    {
        if (root == null) return null;
        var tmp = root.GetComponent<TextMeshProUGUI>();
        if (tmp != null) return tmp;
        return root.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnLanguageChanged(string newLang)
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        // 현재 인덱스 유지 + 현재 줄 재표시(막혀있으면 강제완성)
        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
        if (IsBlockingOpen())
            ForceCompleteCurrentLine();
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
