using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class DialogueManager2_4 : MonoBehaviour
{
    [Header("Early End (optional)")]
    [SerializeField] private bool endAfterFirstNext = false;     // 첫 줄 다음버튼에서 바로 엔딩
    [SerializeField] private bool earlyEndBreathOnly = true;     // 플루트 스킵하고 숨소리만 재생
    [SerializeField] private bool loadSceneAfterEarlyEnd = true; // 검은 화면 후 씬 전환 여부
    [SerializeField] private float earlyFadeDelay = 0.2f;        // 숨소리 시작 전에 잠깐 딜레이
    [SerializeField] private float earlyFadeDuration = 0f;       // 조기엔딩 페이드(0이면 즉시 전환)

    [Header("Ending / Scene")]
    [SerializeField] private string endingSceneName = "Credits"; // 크레딧 씬 이름
    [SerializeField] private bool setPlayerPrefsIndex = false;   // 인덱스 저장 여부(선택)
    [SerializeField] private string playerPrefsKey = "StartFromIndex";
    [SerializeField] private int playerPrefsIndexValue = 4;

    [Header("마지막 줄 엔딩 옵션")]
    [SerializeField] private bool forceBreathOnlyAtEnd = false;  // 마지막에서도 숨소리만 재생하고 바로 전환
    [SerializeField] private float endFadeDuration = 2.0f;       // 마지막 줄 일반 엔딩용 페이드

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
    public Image backgroundImage;
    public Sprite backGroundSprite;
    public GameObject Narke_2Obj;   // 말풍선 캐릭터(나르케)
    public Image endingImage;

    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource fluteSource;
    public AudioSource breathSource;

    [Header("대사 소스")]
    public LanguageCollector2_4 languageCollector;

    [Header("Next 버튼(선택사항)")]
    public GameObject nextButton;   // 있으면 클릭 시 OnNextClicked 연결

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 실행 가드
    private bool soundSequenceStarted = false;

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

        // Next 버튼 연결
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

        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    // ===== 대사 로드 (언어별 명시 선택) =====
    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            Debug.LogError("[DialogueManager2_4] languageCollector가 비어있습니다.");
            lines = new string[0];
            return;
        }

        string lang = CurrentLanguage;
        switch (lang)
        {
            case "korean":   lines = languageCollector.KoreanLines2_4;   break;
            case "english":  lines = languageCollector.EnglishLines2_4;  break;
            case "japanese": lines = languageCollector.JapaneseLines2_4; break;
            case "chinese":  lines = languageCollector.ChineseLines2_4;  break;
            case "kazakh":   lines = languageCollector.KazaLines2_4;     break;
            default:
                Debug.LogWarning($"[DialogueManager2_4] Unknown lang '{lang}', fallback=Korean");
                lines = languageCollector.KoreanLines2_4;
                break;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager2_4] 선택 언어 대사가 비어있습니다. 인스펙터에서 입력해 주세요.");
            lines = new[] { " " };
        }

        Debug_LogLine("LOAD", -1, $"loaded={lines.Length}");
    }

    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError("[DialogueManager2_4] 유효하지 않은 대사 인덱스.");
            yield break;
        }

        // 화자명(나르케) 언어별 표시
        if (aboveText != null)
            aboveText.text = GetSpeakerNameNarke();

        // 타이핑
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
        // ⬇️ 첫 줄에서 바로 엔딩을 원할 때
        if (endAfterFirstNext && index == 0 && !soundSequenceStarted)
        {
            if (nextButton != null) nextButton.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            storyText?.SetText(lines?[index] ?? string.Empty); // 현재 줄은 완성해서 보여주기(선택)
            StartCoroutine(EarlyEndSequence());
            return;
        }

        if (nextButton != null) nextButton.SetActive(false);

        index++;
        if (index < (lines?.Length ?? 0))
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            StartCoroutine(TypeText(lines[index]));
        }
        else
        {
            // 마지막 줄에서 엔딩 트리거
            if (forceBreathOnlyAtEnd)
                StartCoroutine(BreathOnlyThenCredits());
            else
                StartEndingIfNeeded();
        }
    }

    // ===== 사운드 + 엔딩 페이드 (한 번만) =====
    private void StartEndingIfNeeded()
    {
        if (soundSequenceStarted) return;
        soundSequenceStarted = true;
        StartCoroutine(PlaySoundSequenceAndFade());
    }

    private IEnumerator PlaySoundSequenceAndFade()
    {
        // 1) 플루트 재생
        if (fluteSource != null)
        {
            fluteSource.Play();
            yield return new WaitWhile(() => fluteSource.isPlaying);
        }

        // 2) 숨소리
        if (breathSource != null) breathSource.Play();

        // 3) 잠깐 대기 후 엔딩 이미지 페이드 인
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeInEndingImage(endFadeDuration));

        // 4) 씬 전환
        LoadEndingScene();
    }

    private IEnumerator BreathOnlyThenCredits()
    {
        if (soundSequenceStarted) yield break;
        soundSequenceStarted = true;

        if (breathSource != null)
        {
            breathSource.Play();
            yield return new WaitWhile(() => breathSource.isPlaying);
        }

        // 숨소리만 듣고 바로(혹은 페이드 후) 전환하고 싶다면 earlyFadeDuration 사용
        if (earlyFadeDuration > 0f)
            yield return FadeToBlack(earlyFadeDuration);

        LoadEndingScene();
    }

    private IEnumerator EarlyEndSequence()
    {
        soundSequenceStarted = true; // 중복 방지

        // (선택) 플루트 재생
        if (!earlyEndBreathOnly && fluteSource != null)
        {
            fluteSource.Play();
            yield return new WaitWhile(() => fluteSource.isPlaying);
        }

        yield return new WaitForSeconds(earlyFadeDelay);

        // 숨소리 재생
        if (breathSource != null)
        {
            breathSource.Play();
            yield return new WaitWhile(() => breathSource.isPlaying);
        }

        // 검은 화면 페이드(0이면 건너뜀)
        if (earlyFadeDuration > 0f)
            yield return FadeToBlack(earlyFadeDuration);

        if (loadSceneAfterEarlyEnd)
            LoadEndingScene();
    }

    private void LoadEndingScene()
    {
        if (setPlayerPrefsIndex)
            PlayerPrefs.SetInt(playerPrefsKey, playerPrefsIndexValue);

        if (!string.IsNullOrEmpty(endingSceneName))
            SceneManager.LoadScene(endingSceneName);
        else
            Debug.LogError("[DialogueManager2_4] endingSceneName 이 비어있습니다.");
    }

    private IEnumerator FadeInEndingImage(float fadeDuration = 2.0f)
    {
        if (endingImage == null || fadeDuration <= 0f) yield break;

        Color color = endingImage.color;
        color.a = 0f;
        endingImage.color = color;
        endingImage.gameObject.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            endingImage.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator FadeToBlack(float duration = 2f)
    {
        if (endingImage == null || duration <= 0f) yield break;

        // 순수 블랙 오버레이로 사용
        endingImage.sprite = null; // 스프라이트 제거(단색)
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
            if (newAbove != null) aboveText = newAbove; else Debug.LogWarning("[2_4] aboveText 바인딩 실패");
            if (newStory != null) storyText = newStory; else Debug.LogWarning("[2_4] storyText 바인딩 실패");

            if (aboveText != null) aboveText.text = GetSpeakerNameNarke();
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        // 언어 바뀌면: UI 재바인딩 → 대사 재로드 → 인덱스 초기화 → 시퀀스 재시작
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        StopAllCoroutines();
        index = 0;

        // 언어 바뀌면 사운드 시퀀스 가드 해제
        soundSequenceStarted = false;

        StartCoroutine(ShowLineSequence());
    }

    // ===== 화자명(나르케) 언어별 =====
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
