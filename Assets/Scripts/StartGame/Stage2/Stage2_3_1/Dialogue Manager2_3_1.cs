using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;

public class DialogueManager2_3_1 : MonoBehaviour
{
    [Header("언어별 컨테이너")]
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

    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource glitchSound;

    [Header("표정 오브젝트")]
    public GameObject Eco_readyObj;
    public GameObject Narke_2Obj;

    [Header("배경")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 소스")]
    public LanguageCollector2_3_1 languageCollector;

    [Header("언어별 선택지 루트들")]
    public GameObject[] koreanObjects;
    public GameObject[] englishObjects;
    public GameObject[] japaneseObjects;
    public GameObject[] chineseObjects;
    public GameObject[] kazaObjects;

    [Header("선택지 노출 인덱스 (1-based)")]
    [Tooltip("예: 5 → 다섯 번째 대사가 타이핑 끝난 직후 선택지 표시")]
    public int choiceAppearIndexOneBased = 5;

    // 내부
    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;
    private Coroutine glitchCoroutine;

    // ===== 언어 헬퍼 =====
    private string CurrentLanguage => NormalizeLang(LanguageManager.GetLanguage());
    private string NormalizeLang(string raw)
    {
        string s = (raw ?? "korean").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        if (s.StartsWith("ka") || s.Contains("kaza") || s.Contains("kazah")) return "kaza";
        return "korean";
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
        DisableAllLanguageObjects(); // 시작에 전부 숨김
        SetupLanguageUI();
        if (nextButton != null) nextButton.transform.SetAsLastSibling();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        LoadLinesForCurrentLanguage();
        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("[DialogueManager2_3_1] 선택된 언어 대사가 없습니다.");
            return;
        }

        if (PlayerPrefs.HasKey("StartFromIndex"))
        {
            index = PlayerPrefs.GetInt("StartFromIndex");
            PlayerPrefs.DeleteKey("StartFromIndex");
        }
        else index = 0;

        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    // ===== 대사 로드 =====
    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            Debug.LogError("[DialogueManager2_3_1] languageCollector가 비었습니다.");
            lines = new string[0];
            return;
        }
        lines = languageCollector.GetLines();
        if (lines == null || lines.Length == 0)
            lines = new[] { " " };
    }

    // ===== 한 줄 표시 시퀀스 =====
    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError("[ShowLineSequence] 유효하지 않은 대사 인덱스.");
            yield break;
        }

        UpdateCharacterFace(index);

        // 화자명 언어별 표시
        if (aboveText != null)
            aboveText.text = IsNarkeSpeaking() ? GetNameNarke() : GetNameEcho();

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        string line = lines[index] ?? "";
        Debug_LogLine("BEGIN", index, line);
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;

        // 타이핑이 끝난 '지금 index'가 노출 대상인지 (1-based 지정)
        int targetIdx = Mathf.Clamp(Mathf.Max(1, choiceAppearIndexOneBased) - 1, 0, lines.Length - 1);

        if (index == targetIdx)
        {
            nextButton?.gameObject.SetActive(false);
            ShowChoicesForCurrentLanguage(); // 언어별 선택지 활성화
        }
        else
        {
            nextButton?.gameObject.SetActive(true);
        }
    }

    // ===== 다음 줄 =====
    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length) return; // 마지막엔 선택지가 뜸
        StartCoroutine(ShowLineSequence());
    }

    // ===== 표정 & 사운드 =====
    private void UpdateCharacterFace(int idx)
    {
        Eco_readyObj?.SetActive(false);
        Narke_2Obj?.SetActive(false);

        switch (idx)
        {
            case 0:
            case 2:
                Eco_readyObj?.SetActive(true);
                break;
            case 1:
            case 3:
            case 4:
                Narke_2Obj?.SetActive(true);
                break;
        }

        // 글리치 사운드 루프
        if (idx >= 0 && idx <= 2)
        {
            if (glitchCoroutine == null)
                glitchCoroutine = StartCoroutine(GlitchLoop());
        }
        else
        {
            if (glitchCoroutine != null)
            {
                StopCoroutine(glitchCoroutine);
                glitchCoroutine = null;
            }
            if (glitchSound != null && glitchSound.isPlaying)
                glitchSound.Stop();
        }
    }

    private IEnumerator GlitchLoop()
    {
        while (true)
        {
            if (glitchSound != null && !glitchSound.isPlaying)
                glitchSound.Play();
            yield return new WaitForSeconds(1.5f);
        }
    }

    // ===== 타자 효과 =====
    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null) yield break;
        storyText.text = string.Empty;
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        Debug_LogLine("END", index, fullText);
    }

    // ===== 언어별 컨테이너 & TMP 재바인딩 =====
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

        RectTransform above = Korean_Above, story = Korean_Story;
        switch (CurrentLanguage)
        {
            case "english":  above = English_Above;  story = English_Story;  break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese":  above = Chinese_Above;  story = Chinese_Story;  break;
            case "kaza":     above = Kaza_Above;     story = Kaza_Story;     break;
            // default: korean
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            var newAbove = above.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
            var newStory = story.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
            if (newAbove != null) aboveText = newAbove; else Debug.LogWarning("[2_3_1] aboveText 바인딩 실패");
            if (newStory != null) storyText = newStory; else Debug.LogWarning("[2_3_1] storyText 바인딩 실패");
        }

        // 즉시 화자명 갱신
        if (aboveText != null)
            aboveText.text = IsNarkeSpeaking() ? GetNameNarke() : GetNameEcho();
    }

    private void OnLanguageChanged(string newLang)
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        StopAllCoroutines();
        index = 0; // 언어 바뀌면 처음부터
        StartCoroutine(ShowLineSequence());

        UpdateLanguageSpecificObjects(CurrentLanguage);
    }

    // ===== 언어별 선택지 표시 =====
    private void ShowChoicesForCurrentLanguage()
    {
        string lang = CurrentLanguage;
        GameObject[] targets = null;
        switch (lang)
        {
            case "korean":  targets = koreanObjects;  break;
            case "english": targets = englishObjects; break;
            case "japanese":targets = japaneseObjects;break;
            case "chinese": targets = chineseObjects; break;
            case "kaza":    targets = kazaObjects;    break;
        }
        if (targets == null) return;

        foreach (var parent in targets)
        {
            if (parent == null) continue;

            // "ss"로 시작하는 루트는 건너뜀(버튼 자체를 찾는 건 아래에서)
            if (!parent.name.ToLower().Trim().StartsWith("ss"))
                parent.SetActive(true);

            // 자식 버튼 바인딩
            var buttons = parent.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string n = btn.gameObject.name.ToLower().Trim();
                if (!n.StartsWith("ss")) continue;

                btn.onClick.RemoveAllListeners();
                if (n.Contains("4_1"))
                {
                    btn.onClick.AddListener(() => LoadScene("Stage2_4"));
                }
                else if (n.Contains("4_2"))
                {
                    btn.onClick.AddListener(() => LoadScene("Stage2_5"));
                }
            }
        }
    }

    // ===== 언어별 추가 오브젝트 갱신(언어 변경 시) =====
    private void UpdateLanguageSpecificObjects(string langRaw)
    {
        DisableAllLanguageObjects();

        GameObject[] target = null;
        switch (NormalizeLang(langRaw))
        {
            case "korean":  target = koreanObjects;  break;
            case "english": target = englishObjects; break;
            case "japanese":target = japaneseObjects;break;
            case "chinese": target = chineseObjects; break;
            case "kaza":    target = kazaObjects;    break;
        }
        if (target != null)
            foreach (var obj in target)
                if (obj != null) obj.SetActive(true);
    }

    private void DisableAllLanguageObjects()
    {
        foreach (var obj in koreanObjects)  if (obj != null) obj.SetActive(false);
        foreach (var obj in englishObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in japaneseObjects)if (obj != null) obj.SetActive(false);
        foreach (var obj in chineseObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in kazaObjects)    if (obj != null) obj.SetActive(false);
    }

    private void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    // ===== 화자 판정 & 이름 =====
    private bool IsNarkeSpeaking()
    {
        // 현재 설정: 1,3,4에서 Narke_2 활성 → 나르케, 그 외 에코
        return (Narke_2Obj && Narke_2Obj.activeSelf) && !(Eco_readyObj && Eco_readyObj.activeSelf);
    }

    private string GetNameEcho()
    {
        if (languageCollector == null) return "Echo";
        return CurrentLanguage switch
        {
            "korean"   => SafeName(languageCollector.KoreanAbove2_3_1,   0, "에코"),
            "english"  => SafeName(languageCollector.EnglishAbove2_3_1,  0, "Echo"),
            "japanese" => SafeName(languageCollector.JapaneseAbove2_3_1, 0, "エコー"),
            "chinese"  => SafeName(languageCollector.ChineseAbove2_3_1,  0, "艾可"),
            "kaza"     => SafeName(languageCollector.KazaAbove2_3_1,     0, "Эко"),
            _          => "Echo"
        };
    }

    private string GetNameNarke()
    {
        if (languageCollector == null) return "Narke";
        return CurrentLanguage switch
        {
            "korean"   => SafeName(languageCollector.KoreanAbove2_3_1,   2, "나르케"),
            "english"  => SafeName(languageCollector.EnglishAbove2_3_1,  2, "Narke"),
            "japanese" => SafeName(languageCollector.JapaneseAbove2_3_1, 2, "ナルケ"),
            "chinese"  => SafeName(languageCollector.ChineseAbove2_3_1,  2, "纳尔克"),
            "kaza"     => SafeName(languageCollector.KazaAbove2_3_1,     2, "Нарыке"),
            _          => "Narke"
        };
    }

    private string SafeName(string[] arr, int idx, string fallback)
    {
        if (arr != null && arr.Length > idx && !string.IsNullOrEmpty(arr[idx])) return arr[idx];
        return fallback;
    }

    // ===== 디버깅 =====
    private void Debug_LogLine(string phase, int idx, string text)
    {
        Debug.Log($"[2_3_1:{phase}] lang={CurrentLanguage}, idx={idx}, text={(text??"").Replace('\n',' ')}");
    }
}
