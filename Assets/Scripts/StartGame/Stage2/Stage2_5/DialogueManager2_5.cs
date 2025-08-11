using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class DialogueManager2_5 : MonoBehaviour
{
    
    [Header("언어 오브젝트")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("기본 위치값")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    [Header("UI 요소")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource windSource;

    [Header("배경 및 캐릭터")]
    public Image backgroundImage;
    public Sprite backGroundSprite;
    public GameObject Narke_3Obj;

    [Header("대사 스크립트")]
    public LanguageCollector2_5 languageCollector;

    [Header("다음 씬 설정")]
    public string nextSceneName = "CardGameThirdStage";  // ← 인스펙터에서 변경 가능

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 타이핑 스킵을 위한 상태
    private bool isTyping = false;
    private string currentFullLine = "";

    // === 언어 헬퍼 ===
    private string CurrentLanguage => NormalizeLang(LanguageManager.GetLanguage());
    private string NormalizeLang(string raw)
    {
        string s = (raw ?? "korean").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        // ✅ 표준키로 통일
        if (s.StartsWith("kk") || s.Contains("kazakh") || s.Contains("kaza") || s.Contains("kazah"))
            return "kazakh";
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
        SetupLanguageUI();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        if (windSource != null)
        {
            windSource.loop = true;
            if (!windSource.isPlaying) windSource.Play();
        }

        LoadLinesForCurrentLanguage();

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            Debug.LogError("[2_5] languageCollector가 비어 있습니다.");
            lines = new string[0];
            return;
        }

        switch (CurrentLanguage)
        {
            case "korean":   lines = languageCollector.KoreanLines2_5;  break;
            case "english":  lines = languageCollector.EnglishLines2_5; break;
            case "japanese": lines = languageCollector.JapaneseLines2_5;break;
            case "chinese":  lines = languageCollector.ChineseLines2_5; break;
            case "kazakh":   lines = languageCollector.KazaLines2_5;    break; // ✅
            default:
                Debug.LogWarning($"[2_5] Unknown lang='{CurrentLanguage}', fallback to Korean.");
                lines = languageCollector.KoreanLines2_5;
                break;
        }


        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[2_5] 선택된 언어 대사가 비어 있습니다.");
            lines = new[] { " " };
        }
    }

    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError("[2_5] 유효하지 않은 대사 인덱스.");
            yield break;
        }

        UpdateCharacterFace(index);

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        currentFullLine = lines[index] ?? "";
        Debug.Log($"[2_5] lang={CurrentLanguage}, idx={index}, text={currentFullLine}");
        typingCoroutine = StartCoroutine(TypeText(currentFullLine));
        yield return typingCoroutine;

        if (nextButton != null) nextButton.gameObject.SetActive(true);
    }

    private void UpdateCharacterFace(int idx)
    {
        if (Narke_3Obj != null) Narke_3Obj.SetActive(true);

        if (windSource != null && !windSource.isPlaying)
            windSource.Play();

        if (aboveText != null) aboveText.text = GetSpeakerNameNarke();
    }

    private string GetSpeakerNameNarke()
    {
        if (languageCollector == null) return "Narke";
        switch (CurrentLanguage)
        {
            case "korean":   return SafeName(languageCollector.KoreanAbove2_5,   2, "나르케");
            case "english":  return SafeName(languageCollector.EnglishAbove2_5,  2, "Narke");
            case "japanese": return SafeName(languageCollector.JapaneseAbove2_5, 2, "ナルケ");
            case "chinese":  return SafeName(languageCollector.ChineseAbove2_5,  2, "纳尔克");
            case "kazakh":   return SafeName(languageCollector.KazaAbove2_5,     2, "Нарыке"); // ✅
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
        if (storyText == null) yield break;

        isTyping = true;
        storyText.text = string.Empty;

        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void OnNext()
    {
        // 1) 타이핑 중이면 먼저 스킵
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            storyText.text = currentFullLine;
            isTyping = false;
            nextButton?.gameObject.SetActive(true);
            return;
        }

        // 2) 다음 줄로
        nextButton?.gameObject.SetActive(false);
        index++;

        // 3) 마지막 줄 이후면 다음 씬 로드
        if (index >= (lines?.Length ?? 0))
        {
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else
                Debug.LogError("[2_5] nextSceneName 이 비어 있습니다.");
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

        RectTransform above = Korean_Above, story = Korean_Story;
        switch (CurrentLanguage)
        {
            case "english":  above = English_Above;  story = English_Story;  break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese":  above = Chinese_Above;  story = Chinese_Story;  break;
            case "kazakh":   above = Kaza_Above;     story = Kaza_Story;     break; // ✅
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
            if (newAbove != null) aboveText = newAbove; else Debug.LogWarning("[2_5] aboveText 바인딩 실패");
            if (newStory != null) storyText = newStory; else Debug.LogWarning("[2_5] storyText 바인딩 실패");

            if (aboveText != null) aboveText.text = GetSpeakerNameNarke();
        }
        else
        {
            Debug.LogWarning("[2_5] 언어 컨테이너가 설정되지 않았습니다.");
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        StopAllCoroutines();
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Update()
    {
        if (windSource != null && !windSource.isPlaying)
            windSource.Play();
    }
}
