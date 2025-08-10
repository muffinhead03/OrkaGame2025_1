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

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // === 언어 헬퍼 ===
    private string CurrentLanguage => NormalizeLang(LanguageManager.GetLanguage());

    private string NormalizeLang(string raw)
    {
        string s = (raw ?? "korean").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        if (s.StartsWith("ka") || s.Contains("kaza") || s.Contains("kazah")) return "kaza"; // kazakh 변형 포함
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

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);
    }

    // ===== 화자명: 언어별 AboveLine[0/1] 사용 =====
    private void UpdateCharacterFace(int idx)
    {
        // 표정 초기화
        Eco_eyeclosedObj?.SetActive(false);
        Eco_defaultObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_smiledObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Pan_defaultObj?.SetActive(false);
        Pan_4eyeclosedObj?.SetActive(false);

        // 기존 연출 유지
        switch (idx)
        {
            case 0:
                Eco_surprisedObj?.SetActive(true);
                break;

            case 1:
            case 7:
            case 8:
            case 11:
            case 19:
                Pan_defaultObj?.SetActive(true);
                break;

            case 9:
            case 17:
                Pan_4eyeclosedObj?.SetActive(true);
                break;

            case 2:
            case 3:
            case 6:
            case 12:
            case 13:
            case 15:
                Eco_readyObj?.SetActive(true);
                break;

            case 4:
            case 18:
                Eco_defaultObj?.SetActive(true);
                break;

            case 5:
            case 10:
            case 16:
                Eco_eyeclosedObj?.SetActive(true);
                break;

            case 14:
                Eco_smiledObj?.SetActive(true);
                break;
        }

        bool isPan = IsPanLine(idx);
        string speakerName = GetSpeakerName(isPan);
        if (aboveText != null)
            aboveText.text = speakerName;
    }

    // 이 인덱스가 판의 대사인가?
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
                return false; // 나머지는 에코
        }
    }

    // 현재 언어의 화자명: [0]=에코, [1]=판
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

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null) yield break;
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
        {
            SceneManager.LoadScene("Stage3_2");
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

            // 언어별 TMP 재바인딩 (영어 미출력 이슈 해결)
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
        // 언어 바뀌면 먼저 UI 패널 교체/재바인딩 후 라인 로드
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
