using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class DialogueManager2_3 : MonoBehaviour
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

    [Header("UI 참조")]
    public TextMeshProUGUI aboveText;   // 언어 컨테이너 안의 TMP로 재바인딩됨
    public TextMeshProUGUI storyText;   // 언어 컨테이너 안의 TMP로 재바인딩됨
    public Button nextButton;
    public Image fadeImage;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("사운드")]
    public AudioSource bgmSource;
    public AudioSource ScreamingSound;

    [Header("표정 오브젝트")]
    public GameObject Narke_2Obj;
    public GameObject Narke_defaultObj;
    public GameObject Eco_surprisedObj;
    public GameObject Eco_smiledObj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 수집기")]
    public LanguageCollector2_3 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 효과음 1회 재생 가드
    private bool screamedOnce = false;

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
        if (nextButton != null) nextButton.transform.SetAsLastSibling();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        LoadLinesForCurrentLanguage();

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("[DialogueManager2_3] 선택된 언어 대사가 없습니다.");
            return;
        }

        index = 0;

        // 페이드 이미지 초기화
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            var c = fadeImage.color; c.a = 0f; fadeImage.color = c;
        }

        StartCoroutine(ShowLineSequence());
    }

    // ====== 이름(AboveLine) 언어별로 꺼내기 ======
    private string GetNameEcho()
    {
        if (languageCollector == null) return "Echo";
        return CurrentLanguage switch
        {
            "korean"   => SafeName(languageCollector.KoreanAbove2_2,   0, "에코"),
            "english"  => SafeName(languageCollector.EnglishAbove2_2,  0, "Echo"),
            "japanese" => SafeName(languageCollector.JapaneseAbove2_2, 0, "エコー"),
            "chinese"  => SafeName(languageCollector.ChineseAbove2_2,  0, "艾可"),
            "kaza"     => SafeName(languageCollector.KazaAbove2_2,     0, "Эко"),
            _          => "Echo"
        };
    }
    private string GetNameNarke()
    {
        if (languageCollector == null) return "Narke";
        return CurrentLanguage switch
        {
            "korean"   => SafeName(languageCollector.KoreanAbove2_2,   2, "나르케"),
            "english"  => SafeName(languageCollector.EnglishAbove2_2,  2, "Narke"),
            "japanese" => SafeName(languageCollector.JapaneseAbove2_2, 2, "ナルケ"),
            "chinese"  => SafeName(languageCollector.ChineseAbove2_2,  2, "纳尔克"),
            "kaza"     => SafeName(languageCollector.KazaAbove2_2,     2, "Нарыке"),
            _          => "Narke"
        };
    }
    private string SafeName(string[] arr, int idx, string fallback)
    {
        if (arr != null && arr.Length > idx && !string.IsNullOrEmpty(arr[idx])) return arr[idx];
        return fallback;
    }

    // ===== 대사 로드 =====
    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            Debug.LogError("[DialogueManager2_3] languageCollector가 비어있습니다.");
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

        // 화자명: 인덱스 기반(2,3은 나르케, 나머지는 에코)
        if (aboveText != null)
            aboveText.text = IsNarkeIndex(index) ? GetNameNarke() : GetNameEcho();

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        string line = lines[index] ?? "";
        Debug_LogLine("BEGIN", index, line);
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;

        // 마지막 줄 → 페이드 아웃 후 다음 씬
        if (index == lines.Length - 1)
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(FadeOutAndLoadScene("CardgameSecondStage"));
        }
        else
        {
            nextButton?.gameObject.SetActive(true);
        }
    }

    private bool IsNarkeIndex(int idx) => (idx == 2 || idx == 3);

    private void UpdateCharacterFace(int idx)
    {
        if (Narke_2Obj)        Narke_2Obj.SetActive(false);
        if (Narke_defaultObj)  Narke_defaultObj.SetActive(false);
        if (Eco_surprisedObj)  Eco_surprisedObj.SetActive(false);
        if (Eco_smiledObj)     Eco_smiledObj.SetActive(false);

        switch (idx)
        {
            case 0:
                if (Eco_smiledObj) Eco_smiledObj.SetActive(true);
                if (!screamedOnce) { ScreamingSound?.Play(); screamedOnce = true; }
                if (bgmSource != null && !bgmSource.isPlaying) { bgmSource.loop = true; bgmSource.Play(); }
                break;

            case 1:
                if (Eco_surprisedObj) Eco_surprisedObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
                break;

            case 2:
                if (Narke_defaultObj) Narke_defaultObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
                break;

            case 3:
                if (Narke_2Obj) Narke_2Obj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
                break;

            case 4:
                if (Eco_smiledObj) Eco_smiledObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
                break;

            default:
                if (Eco_smiledObj) Eco_smiledObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
                break;
        }
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
        Debug_LogLine("END", index, fullText);
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length)
        {
            StartCoroutine(FadeOutAndLoadScene("CardgameSecondStage"));
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
            case "kaza":     above = Kaza_Above;     story = Kaza_Story;     break;
            // default: korean
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            // 활성화된 컨테이너 안의 TMP를 안전하게 재바인딩
            var newAbove = above.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
            var newStory = story.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
            if (newAbove != null) aboveText = newAbove; else Debug.LogWarning("[DialogueManager2_3] aboveText 바인딩 실패");
            if (newStory != null) storyText = newStory; else Debug.LogWarning("[DialogueManager2_3] storyText 바인딩 실패");
        }
        else
        {
            Debug.LogError("[DialogueManager2_3] 언어 UI 컨테이너가 설정되지 않았습니다.");
        }

        // 언어 바뀌면 화자명도 즉시 갱신
        if (aboveText != null)
            aboveText.text = IsNarkeIndex(index) ? GetNameNarke() : GetNameEcho();
    }

    private void OnLanguageChanged(string newLang)
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        screamedOnce = (index > 0); // 0에서만 비명 SFX, 언어 바꿨다고 또 안 나오게
        StopAllCoroutines();
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float duration = 1f, elapsed = 0f;
            Color c = fadeImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
                fadeImage.color = c;
                yield return null;
            }
            c.a = 1f; fadeImage.color = c;
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    // ===== 디버깅 로그 =====
    private void Debug_LogLine(string phase, int idx, string text)
    {
        Debug.Log($"[Stage2_3:{phase}] lang={CurrentLanguage}, idx={idx}, text={(text ?? "").Replace('\n',' ')}");
    }
}
