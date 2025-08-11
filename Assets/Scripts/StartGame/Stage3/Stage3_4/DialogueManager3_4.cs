using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 맨 위 using들 사이에 추가


public class DialogueManager3_4_backup : MonoBehaviour
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
    public Image fadeImage;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("배경 이미지")]
    public Image Arcadia_bg;
    public Image arcadia_red;
    public Image real_bg;
    public GameObject blackImageObj;

    [Header("캐릭터")]
    public GameObject Pan_defaultObj;
    public GameObject Pan_3Obj;
    public GameObject Eco_readyObj;
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_smiledObj;
    public GameObject Eco_tearObj;

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource glitchSound;
    public AudioSource footSound;
    public AudioSource fluteSound;

    [Header("대사 소스")]
    public LanguageCollector3_4 languageCollector;

    // Debug 옵션
    [Header("Debug")]
    [SerializeField] private bool logDebug = true;
    [SerializeField] private bool logFullLine = false;
    [SerializeField] private int previewChars = 80;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "Stage3_5";
    private bool sceneLoading = false;
    private void LoadNextScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }
    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // ===== 언어 헬퍼 =====
    private string currentLangKey = "korean";
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

    private void Start()
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        if (blackImageObj != null)
            blackImageObj.SetActive(false);

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        currentLangKey = CurrentLanguage;

        // collector에서 직접 가져와도 되지만 기존 패턴 유지
        switch (currentLangKey)
        {
            case "korean":   lines = languageCollector.KoreanLines3_4; break;
            case "english":  lines = languageCollector.EnglishLines3_4; break;
            case "japanese": lines = languageCollector.JapaneseLines3_4; break;
            case "chinese":  lines = languageCollector.ChineseLines3_4; break;
            case "kaza":     lines = languageCollector.KazaLines3_4; break;
            default:
                lines = languageCollector.KoreanLines3_4;
                currentLangKey = "korean";
                Debug.LogWarning("[D3_4] Unknown language, default to Korean.");
                break;
        }

        Debug_LogLangSelected(lines?.Length ?? 0);
    }

    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError($"[D3_4] Invalid line index {index} (len={lines?.Length ?? 0})");
            yield break;
        }

        // 표시 직전 TMP 안전 바인딩
        EnsureTMPBound();

        UpdateVisuals(index);
        UpdateAboveText(index);

        // BGM 시작 타이밍 유지
        if (index == 2 && bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        Debug_LogLine("BEGIN", index, lines[index]);

        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        Debug_LogLine("END", index, lines[index]);

        nextButton?.gameObject.SetActive(true);

        // 인덱스별 사운드/연출
        switch (index)
        {
            case 0:
                glitchSound?.Play();
                break;

            case 1:
                footSound?.Play();
                break;

            case 8:
                fluteSound?.Play();
                if (blackImageObj != null && fadeImage != null)
                    StartCoroutine(FadeToBlack(4f));
                break;
        }
    }

    private void UpdateVisuals(int idx)
    {
        if (Arcadia_bg != null)  Arcadia_bg.enabled  = (idx == 0 || idx == 1);
        if (arcadia_red != null) arcadia_red.enabled = (idx >= 2 && idx <= 7);
        if (real_bg != null)     real_bg.enabled     = (idx == 8);

        Pan_defaultObj?.SetActive(false);
        Pan_3Obj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_eyeclosedObj?.SetActive(false);
        Eco_smiledObj?.SetActive(false);
        Eco_tearObj?.SetActive(false);

        switch (idx)
        {
            case 0: Pan_defaultObj?.SetActive(true); break;
            case 1:
            case 3:
            case 5:
            case 6: Eco_readyObj?.SetActive(true); break;
            case 2: Pan_3Obj?.SetActive(true); break;
            case 4: Eco_eyeclosedObj?.SetActive(true); break;
            case 7: Eco_smiledObj?.SetActive(true); break;
            case 8: Eco_tearObj?.SetActive(true); break;
        }
    }

    // === 화자명: 기존 연출 기준 (0,2는 Pan / 나머지는 Echo) + 언어별 이름 적용 ===
    private void UpdateAboveText(int idx)
    {
        if (aboveText == null) return;

        bool isPan = (idx == 0 || idx == 2);
        aboveText.text = isPan ? GetSpeakerNamePan() : GetSpeakerNameEcho();
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null)
        {
            Debug.LogError("[D3_4] storyText가 바인딩되지 않았습니다.");
            yield break;
        }
        storyText.text = "";
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (blackImageObj != null)
            blackImageObj.SetActive(true);

        if (fadeImage == null) yield break;

        Color baseColor = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        // ✅ 페이드 완료 후에도 씬 이동 (중복 로딩은 sceneLoading으로 가드)
        LoadNextScene();
    }


    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length)
        {
            // ✅ 대사 모두 끝난 순간 바로 다음 씬
            LoadNextScene();
            return;
        }
        StartCoroutine(ShowLineSequence());
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
        currentLangKey = lang;

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

            // 활성 언어의 TMP 재바인딩
            aboveText = FindTMP(above);
            storyText = FindTMP(story);

            Debug_LogBind("Above", aboveText);
            Debug_LogBind("Story", storyText);

            if (aboveText == null || storyText == null)
                Debug.LogWarning("[D3_4] Active language TMP not found. Check children TextMeshProUGUI.");
        }
    }

    private void EnsureTMPBound()
    {
        if (aboveText != null && storyText != null &&
            aboveText.gameObject.activeInHierarchy && storyText.gameObject.activeInHierarchy)
            return;

        RectTransform a, s;
        GetLangRoots(CurrentLanguage, out a, out s);
        if (a != null) aboveText = FindTMP(a);
        if (s != null) storyText = FindTMP(s);

        Debug_LogBind("Ensure.Above", aboveText);
        Debug_LogBind("Ensure.Story", storyText);
    }

    private void GetLangRoots(string lang, out RectTransform above, out RectTransform story)
    {
        switch (lang)
        {
            case "english":   above = English_Above;   story = English_Story;   return;
            case "japanese":  above = Japanese_Above;  story = Japanese_Story;  return;
            case "chinese":   above = Chinese_Above;   story = Chinese_Story;   return;
            case "kaza":      above = Kaza_Above;      story = Kaza_Story;      return;
            default:          above = Korean_Above;    story = Korean_Story;    return;
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

        if (index >= (lines?.Length ?? 0)) index = 0;

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    // === 화자명(언어별) ===
    private string GetSpeakerNameEcho()
    {
        switch (CurrentLanguage)
        {
            case "korean":   return SafeName(languageCollector?.KoreanAbove1_2, 0, "에코");
            case "english":  return SafeName(languageCollector?.EnglishAbove1_2, 0, "Echo");
            case "japanese": return SafeName(languageCollector?.JapaneseAbove1_2, 0, "エコー");
            case "chinese":  return SafeName(languageCollector?.ChineseAbove1_2, 0, "艾可");
            case "kaza":     return SafeName(languageCollector?.KazaAbove1_2,    0, "Эко");
            default:         return "Echo";
        }
    }
    private string GetSpeakerNamePan()
    {
        switch (CurrentLanguage)
        {
            case "korean":   return SafeName(languageCollector?.KoreanAbove1_2, 1, "판");
            case "english":  return SafeName(languageCollector?.EnglishAbove1_2,1, "Pan");
            case "japanese": return SafeName(languageCollector?.JapaneseAbove1_2,1, "パーン");
            case "chinese":  return SafeName(languageCollector?.ChineseAbove1_2, 1, "潘");
            case "kaza":     return SafeName(languageCollector?.KazaAbove1_2,    1, "Пан");
            default:         return "Pan";
        }
    }
    private string SafeName(string[] arr, int idx, string fallback)
    {
        if (arr != null && arr.Length > idx && !string.IsNullOrEmpty(arr[idx])) return arr[idx];
        return fallback;
    }

    // ===== Debug helpers =====
    private void Debug_LogLangSelected(int totalLines)
    {
        if (!logDebug) return;
        Debug.Log($"[D3_4][LANG] selected={currentLangKey}, lines={totalLines}");
    }
    private void Debug_LogLine(string phase, int idx, string line)
    {
        if (!logDebug) return;
        string text = line ?? "";
        if (!logFullLine && text.Length > previewChars)
            text = text.Substring(0, previewChars) + "...";
        Debug.Log($"[D3_4][LINE {phase}] lang={currentLangKey}, idx={idx}, text=\"{text}\"");
    }
    private void Debug_LogBind(string which, TextMeshProUGUI tmp)
    {
        if (!logDebug) return;
        string name = tmp ? tmp.gameObject.name : "NULL";
        Debug.Log($"[D3_4][BIND] {which} -> {name}");
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
