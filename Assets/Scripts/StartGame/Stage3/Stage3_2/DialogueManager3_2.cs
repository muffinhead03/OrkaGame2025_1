using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DialogueManager3_2 : MonoBehaviour
{
    private int currentCase = 0;

    // ===== Debug 옵션 =====
    [Header("Debug")]
    [SerializeField] private bool logDebug = true;
    [SerializeField] private bool logFullLine = false;
    [SerializeField] private int previewChars = 80;
    private string currentLangKey = "korean";

    public void ChangeCase(int caseNumber)
    {
        currentCase = caseNumber;
        index = caseNumber;

        Debug.Log($"[Dialogue3_2] 케이스 {caseNumber}로 이동");

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Stage3_2")
        {
            // 필요 시 외부에서 ChangeCase 호출해 시작 지점 바꿔주세요.
            // 여기서는 자동 점프 안 함
        }
    }

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

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource kwangSound;

    [Header("표정 오브젝트")]
    public GameObject Eco_smiledObj;
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_readyObj;
    public GameObject Eco_surprisedObj;

    public GameObject Pan_defaultObj;
    public GameObject Pan_4eyeclosedObj;

    public GameObject Narke_defaultObj;
    public GameObject Narke_2Obj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사/이름 소스")]
    public LanguageCollector3_2 languageCollector;

    [Header("언어별 전용 오브젝트 루트들")]
    public GameObject[] koreanObjects;
    public GameObject[] englishObjects;
    public GameObject[] japaneseObjects;
    public GameObject[] chineseObjects;
    public GameObject[] kazaObjects;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;
    private bool kwangPlayed = false;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SetupLanguageUI();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        LoadLinesForCurrentLanguage();

        // 기본 0부터 시작
        index = Mathf.Clamp(index, 0, (lines != null && lines.Length > 0) ? lines.Length - 1 : 0);
        currentCase = index;

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("[DialogueManager3_2] 대사가 없습니다.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        currentLangKey = CurrentLanguage;

        switch (currentLangKey)
        {
            case "korean":   lines = languageCollector.KoreanLines3_2; break;
            case "english":  lines = languageCollector.EnglishLines3_2; break;
            case "japanese": lines = languageCollector.JapaneseLines3_2; break;
            case "chinese":  lines = languageCollector.ChineseLines3_2; break;
            case "kaza":     lines = languageCollector.KazaLines3_2; break;
            default:
                Debug.LogWarning($"Unknown language '{currentLangKey}', default to Korean.");
                lines = languageCollector.KoreanLines3_2;
                currentLangKey = "korean";
                break;
        }

        Debug_LogLangSelected(lines?.Length ?? 0);
    }

    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError($"[ShowLineSequence] 유효하지 않은 대사 인덱스 index={index}, len={lines?.Length ?? 0}");
            yield break;
        }

        // 활성 TMP 바인딩 보증 (언어 패널 전환/비활성화 대비)
        EnsureTMPBound();

        // 효과음/배경음
        if (index == 1 && !kwangPlayed)
        {
            kwangSound?.Play();
            kwangPlayed = true;

            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.Stop();
        }
        else if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 표정 및 화자명 처리
        UpdateCharacterFace(index);

        // 언어별 전용 UI 초기화
        DisableAllLanguageObjects();

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // 디버그: 라인 시작 로그
        Debug_LogLine("BEGIN", index, lines[index]);

        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        // 디버그: 라인 완료 로그
        Debug_LogLine("END", index, lines[index]);

        nextButton?.gameObject.SetActive(true);

        // 마지막 줄이면 언어별 선택지 UI 활성화 및 버튼 바인딩
        if (index == lines.Length - 1)
        {
            string lang = CurrentLanguage;
            GameObject[] targetObjects = null;

            switch (lang)
            {
                case "korean":  targetObjects = koreanObjects;  break;
                case "english": targetObjects = englishObjects; break;
                case "japanese":targetObjects = japaneseObjects;break;
                case "chinese": targetObjects = chineseObjects; break;
                case "kaza":    targetObjects = kazaObjects;    break;
            }

            if (targetObjects != null)
            {
                foreach (var parentObj in targetObjects)
                {
                    if (parentObj == null) continue;

                    if (!parentObj.name.ToLower().Trim().StartsWith("2"))
                        parentObj.SetActive(true);

                    Button[] buttons = parentObj.GetComponentsInChildren<Button>(true);
                    foreach (var btn in buttons)
                    {
                        string btnName = btn.gameObject.name.ToLower().Trim();
                        Debug.Log($"[선택지 버튼 탐색] {btnName}");

                        if (btnName.StartsWith("ss"))
                        {
                            btn.onClick.RemoveAllListeners();
                            if (btnName.Contains("2_1"))
                                btn.onClick.AddListener(() => LoadScene("Stage3_3"));
                            else if (btnName.Contains("2_2"))
                                btn.onClick.AddListener(() => LoadScene("Stage3_4"));
                        }
                    }
                }
            }
        }
    }

    private void UpdateCharacterFace(int idx)
    {
        Eco_smiledObj?.SetActive(false);
        Eco_eyeclosedObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Pan_defaultObj?.SetActive(false);
        Pan_4eyeclosedObj?.SetActive(false);
        Narke_defaultObj?.SetActive(false);
        Narke_2Obj?.SetActive(false);

        switch (idx)
        {
            case 0: Eco_smiledObj?.SetActive(true); break;
            case 1:
            case 3:
            case 4:
            case 6:
            case 8:
            case 10:
            case 15: Pan_defaultObj?.SetActive(true); break;
            case 5: Pan_4eyeclosedObj?.SetActive(true); break;
            case 2:
            case 9: Eco_eyeclosedObj?.SetActive(true); break;
            case 7: Eco_readyObj?.SetActive(true); break;
            case 11:
            case 12: Narke_defaultObj?.SetActive(true); break;
            case 13: Narke_2Obj?.SetActive(true); break;
            case 14: Eco_surprisedObj?.SetActive(true); break;
        }

        bool isNarke = ((Narke_defaultObj != null && Narke_defaultObj.activeSelf) ||
                        (Narke_2Obj != null && Narke_2Obj.activeSelf));
        bool isPan   = ((Pan_defaultObj != null && Pan_defaultObj.activeSelf) ||
                        (Pan_4eyeclosedObj != null && Pan_4eyeclosedObj.activeSelf));
        bool isEcho  = !isNarke && !isPan;

        if (aboveText != null)
        {
            if (isNarke)      aboveText.text = GetSpeakerNameNarke();
            else if (isPan)   aboveText.text = GetSpeakerNamePan();
            else              aboveText.text = GetSpeakerNameEcho();
        }
    }

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
    private string GetSpeakerNameNarke()
    {
        switch (CurrentLanguage)
        {
            case "korean":   return SafeName(languageCollector?.KoreanAbove1_2, 2, "나르케");
            case "english":  return SafeName(languageCollector?.EnglishAbove1_2, 2, "Narke");
            case "japanese": return SafeName(languageCollector?.JapaneseAbove1_2, 2, "ナルケ");
            case "chinese":  return SafeName(languageCollector?.ChineseAbove1_2, 2, "纳尔克");
            case "kaza":     return SafeName(languageCollector?.KazaAbove1_2,    2, "Нарыке");
            default:         return "Narke";
        }
    }
    private string SafeName(string[] arr, int idx, string fallback)
    {
        if (arr != null && arr.Length > idx && !string.IsNullOrEmpty(arr[idx])) return arr[idx];
        return fallback;
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null)
        {
            Debug.LogError("[Dialogue3_2] storyText가 바인딩되지 않았습니다.");
            yield break;
        }
        storyText.text = string.Empty;
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length)
            return;
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

            // 언어별 TMP 재바인딩
            aboveText = FindTMP(above);
            storyText = FindTMP(story);

            Debug_LogBind("Above", aboveText);
            Debug_LogBind("Story", storyText);

            if (aboveText == null || storyText == null)
                Debug.LogWarning("[DialogueManager3_2] Active language TMP not found. Check children TextMeshProUGUI.");
        }
    }

    // 활성 언어 패널에서 TMP 재탐색 (안전망)
    private void EnsureTMPBound()
    {
        if (aboveText != null && storyText != null && aboveText.gameObject.activeInHierarchy && storyText.gameObject.activeInHierarchy)
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

        if (index >= lines.Length) index = 0;

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());

        UpdateLanguageSpecificObjects(CurrentLanguage);
    }

    private void UpdateLanguageSpecificObjects(string lang)
    {
        DisableAllLanguageObjects();

        GameObject[] target = null;
        switch (NormalizeLang(lang))
        {
            case "korean":  target = koreanObjects;  break;
            case "english": target = englishObjects; break;
            case "japanese":target = japaneseObjects;break;
            case "chinese": target = chineseObjects; break;
            case "kaza":    target = kazaObjects;    break;
        }

        if (target != null)
        {
            foreach (var obj in target)
                if (obj != null) obj.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (bgmSource != null && !bgmSource.isPlaying && index != 1)
        {
            bgmSource.Play();
        }
    }

    private void DisableAllLanguageObjects()
    {
        foreach (var obj in koreanObjects)  if (obj != null) obj.SetActive(false);
        foreach (var obj in englishObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in japaneseObjects)if (obj != null) obj.SetActive(false);
        foreach (var obj in chineseObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in kazaObjects)    if (obj != null) obj.SetActive(false);
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ===== Debug helpers =====
    private void Debug_LogLangSelected(int totalLines)
    {
        if (!logDebug) return;
        Debug.Log($"[D3_2][LANG] selected={currentLangKey}, lines={totalLines}");
    }

    private void Debug_LogLine(string phase, int idx, string line)
    {
        if (!logDebug) return;
        string text = line ?? "";
        if (!logFullLine && text.Length > previewChars)
            text = text.Substring(0, previewChars) + "...";
        Debug.Log($"[D3_2][LINE {phase}] lang={currentLangKey}, idx={idx}, text=\"{text}\"");
    }

    private void Debug_LogBind(string which, TextMeshProUGUI tmp)
    {
        if (!logDebug) return;
        string name = tmp ? tmp.gameObject.name : "NULL";
        Debug.Log($"[D3_2][BIND] {which} -> {name}");
    }
}
