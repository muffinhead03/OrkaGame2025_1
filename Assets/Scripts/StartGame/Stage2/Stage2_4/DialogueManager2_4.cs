using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class DialogueManager2_4 : MonoBehaviour
{
    // ====== Ending / Scene (엔딩 연출용) ======
    [Header("Ending / Scene")]
    [SerializeField] private float endingFadeDelay = 3f;      // 숨소리 시작 후 몇 초 뒤에 이미지 페이드인 시작
    [SerializeField] private float endingFadeDuration = 2f;   // 엔딩 이미지가 서서히 뜨는 시간
    [SerializeField] private string endingSceneName = "Stage2_3_1"; // ← 바로 다음 씬으로 수정
    [SerializeField] private bool setPlayerPrefsIndex = false;
    [SerializeField] private string playerPrefsKey = "StartFromIndex";
    [SerializeField] private int playerPrefsIndexValue = 4;

    // ====== Early End (옵션) ======
    [Header("Early End (optional)")]
    [SerializeField] private bool endAfterFirstNext = false;
    [SerializeField] private bool earlyEndBreathOnly = true;
    [SerializeField] private bool loadSceneAfterEarlyEnd = true;
    [SerializeField] private float earlyFadeDelay = 0.2f;
    [SerializeField] private float earlyFadeDuration = 0f;

    // ====== 언어별 컨테이너 ======
    [Header("언어별 컨테이너")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("기본 위치값")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    // ====== UI & 타이핑 ======
    [Header("UI")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Image backgroundImage;
    public Sprite backGroundSprite;
    public GameObject Narke_2Obj;
    public Image endingImage;

    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    // ====== 오디오 ======
    [Header("오디오")]
    public AudioSource fluteSource;
    public AudioSource breathSource;

    // ====== 데이터 ======
    [Header("대사 소스")]
    public LanguageCollector2_4 languageCollector;

    [Header("Next 버튼(선택사항)")]
    public GameObject nextButton;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 상태/가드
    private bool soundSequenceStarted = false;
    private bool introFlutePlayed = false;

    // ===== 언어 헬퍼 =====
    private string CurrentLanguage => NormalizeLang(LanguageManager.GetLanguage());
    private string NormalizeLang(string raw)
    {
        string s = (raw ?? "korean").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        if (s.StartsWith("kk") || s.Contains("kazakh") || s.Contains("kaza") || s.Contains("kazah"))
            return "kazakh";
        return "korean";
    }

    private void Awake()
    {
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;

        var btn = nextButton ? nextButton.GetComponent<Button>() : null;
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnNextClicked);
            nextButton.SetActive(false);
        }
        else if (nextButton != null)
        {
            Debug.LogWarning("[DialogueManager2_4] nextButton에 Button 컴포넌트가 없습니다. (선택 사항)");
        }
    }

    private void Start()
    {
        SetupLanguageUI();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        if (Narke_2Obj != null) Narke_2Obj.SetActive(true);

        LoadLinesForCurrentLanguage();
        index = 0;

        if (!introFlutePlayed && fluteSource != null && !fluteSource.isPlaying)
        {
            fluteSource.Play();
            introFlutePlayed = true;
        }

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
            Debug.LogError("[DialogueManager2_4] languageCollector가 비어있습니다.");
            lines = new string[0];
            return;
        }

        switch (CurrentLanguage)
        {
            case "korean":   lines = languageCollector.KoreanLines2_4;   break;
            case "english":  lines = languageCollector.EnglishLines2_4;  break;
            case "japanese": lines = languageCollector.JapaneseLines2_4; break;
            case "chinese":  lines = languageCollector.ChineseLines2_4;  break;
            case "kazakh":   lines = languageCollector.KazaLines2_4;     break;
            default:
                Debug.LogWarning("[DialogueManager2_4] Unknown lang, fallback Korean");
                lines = languageCollector.KoreanLines2_4;
                break;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager2_4] 선택 언어 대사가 비어있습니다.");
            lines = new[] { " " };
        }

        Debug_LogLine("LOAD", -1, $"loaded={lines.Length}");
    }

    // ===== 표시/타이핑 =====
    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError("[DialogueManager2_4] 유효하지 않은 대사 인덱스.");
            yield break;
        }

        if (aboveText != null) aboveText.text = GetSpeakerNameNarke();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        string line = lines[index] ?? "";
        Debug_LogLine("BEGIN", index, line);
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;

        if (nextButton != null) nextButton.SetActive(true);
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

        if (nextButton != null) nextButton.SetActive(true);
    }

    // ===== Next =====
    public void OnNextClicked()
    {
        if (nextButton != null) nextButton.SetActive(false);

        if (endAfterFirstNext && index == 0 && !soundSequenceStarted)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            storyText?.SetText(lines?[index] ?? string.Empty);
            StartCoroutine(EarlyEndSequence());
            return;
        }

        index++;
        if (index < (lines?.Length ?? 0))
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            StartCoroutine(TypeText(lines[index]));
        }
        else
        {
            // 마지막 줄에서: 숨소리 재생 + (지연) 이미지 페이드인 → 즉시 다음 씬
            StartCoroutine(BreathThenShowEndingImage());
        }
    }

    // ===== 숨소리 → (지연) → 엔딩 이미지 페이드인 → 바로 다음 씬 =====
    private IEnumerator BreathThenShowEndingImage()
    {
        if (soundSequenceStarted) yield break;
        soundSequenceStarted = true;

        if (fluteSource != null && fluteSource.isPlaying)
            fluteSource.Stop();

        if (breathSource != null && !breathSource.isPlaying)
            breathSource.Play();

        // 지연 후 페이드인 실행 및 완료까지 대기
        yield return StartCoroutine(FadeInEndingImageAfterDelay(endingFadeDelay, endingFadeDuration));

        // 필요하면 숨소리 종료까지 기다리기
        // if (breathSource != null && !breathSource.loop)
        //     yield return new WaitWhile(() => breathSource.isPlaying);

        // 비디오 없이 바로 씬 이동
        LoadEndingScene();
    }

    private IEnumerator FadeInEndingImageAfterDelay(float delay, float duration)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        yield return FadeInEndingImage(duration);
    }

    // ===== 조기 엔딩(옵션) =====
    private IEnumerator EarlyEndSequence()
    {
        soundSequenceStarted = true;

        if (!earlyEndBreathOnly && fluteSource != null)
        {
            fluteSource.Play();
            if (!fluteSource.loop) yield return new WaitWhile(() => fluteSource.isPlaying);
        }

        if (earlyFadeDelay > 0f) yield return new WaitForSeconds(earlyFadeDelay);

        if (breathSource != null)
        {
            breathSource.Play();
            if (!breathSource.loop) yield return new WaitWhile(() => breathSource.isPlaying);
        }

        if (earlyFadeDuration > 0f) yield return FadeToBlack(earlyFadeDuration);

        if (loadSceneAfterEarlyEnd) LoadEndingScene();
    }

    // ===== 공통 유틸 =====
    private void LoadEndingScene()
    {
        if (setPlayerPrefsIndex)
            PlayerPrefs.SetInt(playerPrefsKey, playerPrefsIndexValue);

        if (!string.IsNullOrEmpty(endingSceneName))
            SceneManager.LoadScene(endingSceneName);
        else
            Debug.LogError("[DialogueManager2_4] endingSceneName 이 비어있습니다.");
    }

    private IEnumerator FadeInEndingImage(float fadeDuration)
    {
        if (endingImage == null || fadeDuration <= 0f) yield break;

        Color start = endingImage.color;
        start.a = 0f;
        endingImage.color = start;
        endingImage.gameObject.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            endingImage.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (endingImage == null || duration <= 0f) yield break;

        endingImage.sprite = null;
        var c = Color.black; c.a = 0f;
        endingImage.color = c;
        endingImage.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            endingImage.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }
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
            case "kazakh":   above = Kaza_Above;     story = Kaza_Story;     break;
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            var newAbove = above.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
            var newStory = story.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
            if (newAbove != null) aboveText = newAbove; else Debug.LogWarning("[2_4] aboveText 바인딩 실패");
            if (newStory != null) storyText = newStory; else Debug.LogWarning("[2_4] storyText 바인딩 실패");

            if (aboveText != null) aboveText.text = GetSpeakerNameNarke();
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        StopAllCoroutines();
        index = 0;
        soundSequenceStarted = false;

        StartCoroutine(ShowLineSequence());
    }

    // ===== 화자명 =====
    private string GetSpeakerNameNarke()
    {
        if (languageCollector == null) return "Narke";
        switch (CurrentLanguage)
        {
            case "korean":   return SafeName(languageCollector.KoreanAbove2_4,   2, "나르케");
            case "english":  return SafeName(languageCollector.EnglishAbove2_4,  2, "Narke");
            case "japanese": return SafeName(languageCollector.JapaneseAbove2_4, 2, "ナルケ");
            case "chinese":  return SafeName(languageCollector.ChineseAbove2_4,  2, "纳尔克");
            case "kazakh":   return SafeName(languageCollector.KazaAbove2_4,     2, "Нарыке");
            default:         return "Narke";
        }
    }

    private string SafeName(string[] arr, int idx, string fallback)
    {
        if (arr != null && arr.Length > idx && !string.IsNullOrEmpty(arr[idx])) return arr[idx];
        return fallback;
    }

    // ===== 디버깅 =====
    private void Debug_LogLine(string phase, int idx, string text)
    {
        Debug.Log($"[2_4:{phase}] lang={CurrentLanguage}, idx={idx}, text={(text ?? "").Replace('\n',' ')}");
    }
}
