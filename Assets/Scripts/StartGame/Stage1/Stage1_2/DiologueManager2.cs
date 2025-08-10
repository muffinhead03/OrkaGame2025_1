using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManagerStage1_2 : MonoBehaviour
{
    [Header("언어 오브젝트 (언어별 컨테이너)")]
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

    [Header("배경/캐릭터 (원하면 비워도 됨)")]
    public GameObject BlackImage;
    public Image Real_bg_Image;
    public Sprite Real_bg_Sprite;
    public GameObject Echo_Default;
    public GameObject Father_Default;

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip mainBGM;

    [Header("효과음 (인덱스 5, 8, 9, 11 유지)")]
    public AudioClip roughlyHittingWall; // idx 5
    public AudioClip kickEcho;           // idx 8
    public AudioClip echoBreathingSmall; // idx 9
    public AudioClip doorSlam;           // idx 11 (문쾅)
    public AudioClip lockingSound;       // idx 11 이후 락킹

    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    [Header("대사 스크립트")]
    public LanguageCollector1_2 languageCollector;

    // 선택된 언어의 라인들
    private string[] aboveLines;
    private string[] storyLines;

    private int index;
    private Coroutine typingCoroutine;

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

        // BGM 스타트 (Stage1_1 스타일)
        if (bgmSource != null && mainBGM != null)
        {
            bgmSource.clip = mainBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            Debug.LogError("LanguageCollector1_2가 할당되지 않았습니다.");
            aboveLines = new[] { "" };
            storyLines = new[] { "" };
            return;
        }

        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean":
                aboveLines = languageCollector.KoreanAbove1_2;
                storyLines = languageCollector.KoreanLines1_2;
                break;
            case "english":
                aboveLines = languageCollector.EnglishAbove1_2;
                storyLines = languageCollector.EnglishLines1_2;
                break;
            case "japanese":
                aboveLines = languageCollector.JapaneseAbove1_2;
                storyLines = languageCollector.JapaneseLines1_2;
                break;
            case "chinese":
                aboveLines = languageCollector.ChineseAbove1_2;
                storyLines = languageCollector.ChineseLines1_2;
                break;
            case "kazahustan":
            case "kazakhstan":
            case "kaza":
            case "kazakh":
                aboveLines = languageCollector.KazaAbove1_2;
                storyLines = languageCollector.KazaLines1_2;
                break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                aboveLines = languageCollector.KoreanAbove1_2;
                storyLines = languageCollector.KoreanLines1_2;
                break;
        }

        // 길이 가드
        if (aboveLines == null || storyLines == null || aboveLines.Length != storyLines.Length)
        {
            Debug.LogWarning("Aboveline/Storyline 배열이 비었거나 길이가 다릅니다. 임시로 최소값에 맞춰 사용합니다.");
            int min = Mathf.Min(aboveLines?.Length ?? 0, storyLines?.Length ?? 0);
            if (min <= 0) { aboveLines = new[] { "" }; storyLines = new[] { "" }; }
            else
            {
                var tmpA = new string[min];
                var tmpS = new string[min];
                for (int i = 0; i < min; i++) { tmpA[i] = aboveLines[i]; tmpS[i] = storyLines[i]; }
                aboveLines = tmpA; storyLines = tmpS;
            }
        }
    }

    private IEnumerator ShowLineSequence()
    {
        yield return UpdateVisuals(index);

        // 여기서 화자(above) 먼저 세팅
        if (aboveText != null && index < aboveLines.Length)
            aboveText.text = aboveLines[index];

        // 스토리 타이핑
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        string line = (index < storyLines.Length) ? storyLines[index] : "";
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);
    }

    private IEnumerator UpdateVisuals(int idx)
    {
        // Stage1_1 스타일: 배경/캐릭터/사운드 기본 초기화
        if (BlackImage != null) BlackImage.SetActive(false);
        if (Real_bg_Image != null)
        {
            Real_bg_Image.gameObject.SetActive(false);
            var c = Real_bg_Image.color; c.a = 1f; Real_bg_Image.color = c;
        }
        if (Echo_Default != null) Echo_Default.SetActive(false);
        if (Father_Default != null) Father_Default.SetActive(false);

        // 인덱스별 SFX 연출(요청대로 유지: 5,8,9,11)
        // ※ 인덱스는 0부터 시작하므로, 사용하던 스펙 그대로라면
        //    5,8,9,11이 실제로 존재하도록 배열 길이를 확보하세요.
        if (sfxSource != null)
        {
            if (idx == 5 && roughlyHittingWall != null) sfxSource.PlayOneShot(roughlyHittingWall);
            if (idx == 8 && kickEcho != null)           sfxSource.PlayOneShot(kickEcho);
            if (idx == 9 && echoBreathingSmall != null) sfxSource.PlayOneShot(echoBreathingSmall);
            if (idx == 11)
            {
                if (doorSlam != null) sfxSource.PlayOneShot(doorSlam);
                if (lockingSound != null)
                {
                    // 문쾅 이후 살짝 쉬고 락킹
                    yield return new WaitForSeconds(0.2f);
                    sfxSource.PlayOneShot(lockingSound);
                }
            }
        }

        // 간단한 배경/캐릭터 토글 (원하면 자유롭게 커스터마이즈)
        if (idx == 0)
        {
            if (BlackImage != null) BlackImage.SetActive(true);
        }
        else
        {
            if (Real_bg_Image != null)
            {
                Real_bg_Image.gameObject.SetActive(true);
                Real_bg_Image.sprite = Real_bg_Sprite;
            }

            // 예시: 짝수면 Echo, 홀수면 Father 보여주기 (원래 규칙이 있으면 그 규칙으로 교체)
            if (Echo_Default != null) Echo_Default.SetActive(idx % 2 == 0);
            if (Father_Default != null) Father_Default.SetActive(idx % 2 == 1);
        }

        yield break;
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null) yield break;

        storyText.text = "";
        if (typingSpeed <= 0f)
        {
            storyText.text = fullText;
            yield break;
        }

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
        if (storyLines == null || index >= storyLines.Length)
        {
            SceneManager.LoadScene("SlidingPuzzle"); // 다음 씬명 유지/변경 가능
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

        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        RectTransform above = Korean_Above, story = Korean_Story;

        switch (lang)
        {
            case "english":  above = English_Above;  story = English_Story;  break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese":  above = Chinese_Above;  story = Chinese_Story;  break;
            case "kazahustan":
            case "kazakhstan":
            case "kaza":
            case "kazakh":
                above = Kaza_Above; story = Kaza_Story; break;
            // default: Korean
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            // 현재 활성화된 언어 블록의 TMP 재바인딩
            aboveText = above.GetComponentInChildren<TextMeshProUGUI>(true);
            storyText = story.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        LoadLinesForCurrentLanguage();
        SetupLanguageUI();
        StopAllCoroutines();
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
