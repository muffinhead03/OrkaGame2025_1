using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager1_3 : MonoBehaviour
{
    [Header("언어 오브젝트 (언어별 컨테이너)")]
    public RectTransform Korean_Above,  Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above,  Chinese_Story;
    public RectTransform Kaza_Above,     Kaza_Story;

    [Header("기본 위치값 (컨테이너 앵커드 포지션)")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    [Header("UI 텍스트 (자동 바인딩됨)")]
    public TextMeshProUGUI aboveText; // SetupLanguageUI에서 현재 언어 컨테이너에서 찾아 바인딩
    public TextMeshProUGUI storyText; // SetupLanguageUI에서 현재 언어 컨테이너에서 찾아 바인딩
    public Button nextButton;

    [Header("초상/이미지")]
    public Image echoImage;            // UI Image
    public Sprite realEchoDefault;     // 첫 대사
    public Sprite realEcho4;           // 두 번째 대사

    [Header("페이드용 CanvasGroup (비활성 시작 가능)")]
    public CanvasGroup blackFadeImage; // 3초 블랙 페이드
    public CanvasGroup goatCanvas;     // goat22-2, 4초 페이드인

    [Header("오디오")]
    public AudioSource bgmAudioSource;
    public AudioSource doorAudioSource;    // 시작과 동시에
    public AudioSource fluteAudioSource;   // 2번째 대사 시작과 동시에
    public AudioSource glitchAudioSource;  // 블랙 페이드 후, 염소 뜨기 직전

    [Header("타이핑")]
    public float typingSpeed = 0.03f;

    [Header("대사 스크립트")]
    public LanguageCollector1_3 languageCollector;

    // 내부 상태
    private string[] lines;
    private int index = 0;
    private Coroutine typingCo;
    private Coroutine blackFadeCo;
    private Coroutine goatFadeCo;

    private void Awake()
    {
        // 버튼 연결
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNext);
            nextButton.gameObject.SetActive(false);
        }

        // 언어 매니저 초기화 및 변경 감지
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        SetupLanguageUI();          // 현재 언어 컨테이너 활성 + 텍스트 바인딩
        LoadLinesForCurrentLanguage();

        // 초기 이미지/오디오
        if (echoImage != null && realEchoDefault != null) echoImage.sprite = realEchoDefault;
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying) bgmAudioSource.Play();
        if (doorAudioSource != null) doorAudioSource.Play(); // 시작과 동시에

        // 페이드용 초기값 (비활성로 시작해도 OK)
        if (blackFadeImage != null) blackFadeImage.alpha = 0f;
        if (goatCanvas != null)     goatCanvas.alpha = 0f;

        // 첫 줄 출력
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    // ===== 언어별 컨테이너 활성 + 텍스트 바인딩 =====
    private void SetupLanguageUI()
    {
        // 모두 끔
        var all = new[] {
            Korean_Above, Korean_Story,
            English_Above, English_Story,
            Japanese_Above, Japanese_Story,
            Chinese_Above, Chinese_Story,
            Kaza_Above, Kaza_Story
        };
        foreach (var rt in all) if (rt) rt.gameObject.SetActive(false);

        // 현재 언어 선택
        string lang = LanguageManager.GetLanguage()?.Trim().ToLowerInvariant();
        RectTransform above = English_Above, story = English_Story; // 기본 영어

        switch (lang)
        {
            case "korean":  above = Korean_Above;  story = Korean_Story;  break;
            case "english": above = English_Above; story = English_Story; break;
            case "kazakh":  above = Kaza_Above;    story = Kaza_Story;    break;
            case "chinese": above = Chinese_Above; story = Chinese_Story; break;
            case "japanese":above = Japanese_Above;story = Japanese_Story;break;
        }

        // 활성 + 위치 세팅
        if (above) { above.gameObject.SetActive(true); above.anchoredPosition = AboPo; }
        if (story) { story.gameObject.SetActive(true); story.anchoredPosition = StoPo; }

        // 컨테이너 내부 TMP_Text 자동 바인딩
        aboveText = above ? above.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        storyText = story ? story.GetComponentInChildren<TextMeshProUGUI>(true) : null;
    }

    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null) languageCollector = FindObjectOfType<LanguageCollector1_3>();

        lines = (languageCollector != null)
            ? languageCollector.GetLines()
            : new[] { "Line1", "Line2" };

        // 스피커 이름
        if (aboveText != null && languageCollector != null)
            aboveText.text = languageCollector.GetSpeakerName();
    }

    // ===== 시퀀스 =====
    private IEnumerator ShowLineSequence()
    {
        // 타이핑 중지
        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }

        // 현재 줄 타이핑
        if (storyText != null)
        {
            typingCo = StartCoroutine(TypeText(lines[index]));
            yield return typingCo;
        }

        // 다음 버튼 표시
        if (nextButton != null) nextButton.gameObject.SetActive(true);
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null) yield break;

        storyText.text = "";
        fullText = fullText ?? "";
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnNext()
    {
        if (nextButton != null) nextButton.gameObject.SetActive(false);

        index++;

        // 두 번째 줄에 진입할 때 연출(초상 교체 + 플루트 + 블랙 페이드)
        if (index == 1)
        {
            if (echoImage != null && realEcho4 != null) echoImage.sprite = realEcho4;
            if (fluteAudioSource != null) fluteAudioSource.Play();

            // 비활성이어도 자동 활성화 + 클릭 막기 (원하면 blockRaycastsDuring=false로)
            if (blackFadeCo != null) { StopCoroutine(blackFadeCo); blackFadeCo = null; }
            blackFadeCo = StartCoroutine(FadeCanvas(blackFadeImage, 0f, 1f, 3f, true, false, true));
        }

        if (lines == null || index >= lines.Length)
        {
            // 블랙 페이드 완료 대기 후 글리치 + 염소 4초 페이드인 → 씬 전환
            StartCoroutine(EndSequenceAndLoad());
            return;
        }

        StartCoroutine(ShowLineSequence());
    }

    private IEnumerator EndSequenceAndLoad()
    {
        // 블랙 페이드가 아직 진행 중이면 대기(최대 3초)
        yield return new WaitForSeconds(3f);

        if (glitchAudioSource != null) glitchAudioSource.Play();

        if (goatFadeCo != null) { StopCoroutine(goatFadeCo); goatFadeCo = null; }
        goatFadeCo = StartCoroutine(FadeCanvas(goatCanvas, 0f, 1f, 4f, true, false, false));
        yield return goatFadeCo;

        SceneManager.LoadScene("Stage2_1");
    }

    // ===== 페이드 헬퍼: 비활성 시작도 지원 =====
    private IEnumerator FadeCanvas(
        CanvasGroup cg,
        float from, float to, float duration,
        bool activateBefore = true,
        bool deactivateAfter = false,
        bool blockRaycastsDuring = false)
    {
        if (cg == null) yield break;

        if (activateBefore && !cg.gameObject.activeSelf)
            cg.gameObject.SetActive(true);

        cg.blocksRaycasts = blockRaycastsDuring;
        cg.interactable   = blockRaycastsDuring;

        cg.alpha = from;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;

        if (deactivateAfter)
            cg.gameObject.SetActive(false);
    }

    // ===== 언어 변경 대응 =====
    private void OnLanguageChanged(string _)
    {
        LoadLinesForCurrentLanguage();
        SetupLanguageUI(); // 텍스트 레퍼런스 재바인딩
        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
        index = 0;
        StartCoroutine(ShowLineSequence());
    }
}
