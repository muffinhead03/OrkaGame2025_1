using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class DialogueManagerStage2_1 : MonoBehaviour
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
    public TextMeshProUGUI aboveText;    // 이름 레이블 (언어별 패널의 TMP로 재바인딩)
    public TextMeshProUGUI storyText;    // 대사 텍스트 (언어별 패널의 TMP로 재바인딩)
    public Button nextButton;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource ArcadiaBGMSource;
    public AudioSource birdSound;
    public AudioSource grassSound;

    [Header("표정 오브젝트")]
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_defaultObj;
    public GameObject Eco_surprisedObj;
    public GameObject Eco_readyObj;

    [Header("배경 이미지(선택)")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 스크립트")]
    public LanguageCollector2_1 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

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
        // 1) 언어별 패널 활성화 + TMP 재바인딩
        SetupLanguageUI();
        if (nextButton != null) nextButton.transform.SetAsLastSibling();

        // 2) 배경 세팅(선택)
        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        // 3) 이름(AboveLine) 언어별로 표기
        SetSpeakerNameEcho();

        // 4) 대사 배열 로드
        LoadLinesForCurrentLanguage();

        // 5) 첫 줄부터 시작
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    // ====== 화자명(에코) 언어별 적용 ======
    private void SetSpeakerNameEcho()
    {
        if (aboveText == null || languageCollector == null) return;

        string name = CurrentLanguage switch
        {
            "korean"   => SafeName(languageCollector.KoreanAbove2_1,   0, "에코"),
            "english"  => SafeName(languageCollector.EnglishAbove2_1,  0, "Echo"),
            "japanese" => SafeName(languageCollector.JapaneseAbove2_1, 0, "エコー"),
            "chinese"  => SafeName(languageCollector.ChineseAbove2_1,  0, "艾可"),
            "kaza"     => SafeName(languageCollector.KazaAbove2_1,     0, "Эко"),
            _          => "Echo"
        };
        aboveText.text = name;
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
            Debug.LogWarning("[DialogueStage2_1] languageCollector가 비어있습니다.");
            lines = new[] { " " };
            return;
        }

        switch (CurrentLanguage)
        {
            case "korean":   lines = languageCollector.KoreanLines2_1; break;
            case "english":  lines = languageCollector.EnglishLines2_1; break;
            case "japanese": lines = languageCollector.JapaneseLines2_1; break;
            case "chinese":  lines = languageCollector.ChineseLines2_1; break;
            case "kaza":     lines = languageCollector.KazaLines2_1; break;
            default:
                Debug.LogWarning($"[DialogueStage2_1] Unknown language '{CurrentLanguage}', default to Korean.");
                lines = languageCollector.KoreanLines2_1;
                break;
        }

        if (lines == null || lines.Length == 0)
            lines = new[] { " " };
    }

    // ===== 한 줄 표시 시퀀스 =====
    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
            yield break;

        // 표정 연출
        UpdateCharacterFace(index);

        // 살짝 텀
        yield return new WaitForSeconds(0.5f);

        // 타이핑
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        string line = lines[index] ?? "";
        Debug_LogLine("BEGIN", index, line);   // 디버깅: 어떤 언어/몇 번 줄인지 출력
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;

        // Next 표시
        nextButton?.gameObject.SetActive(true);
    }

    private void UpdateCharacterFace(int idx)
    {
        Eco_eyeclosedObj?.SetActive(false);
        Eco_defaultObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);

        switch (idx)
        {
            case 0:
            case 2:
            case 8:
                Eco_eyeclosedObj?.SetActive(true); break;

            case 1:
            case 5:
            case 7:
                Eco_defaultObj?.SetActive(true); break;

            case 3:
            case 4:
            case 9:
                Eco_surprisedObj?.SetActive(true); break;

            case 6:
                Eco_readyObj?.SetActive(true); break;

            default:
                Eco_defaultObj?.SetActive(true); break;
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
        Debug_LogLine("END", index, fullText); // 디버깅: 라인 타이핑 완료
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= (lines?.Length ?? 0))
        {
            SceneManager.LoadScene("Stage2_2");
            return;
        }
        StartCoroutine(ShowLineSequence());
    }

    // ===== 언어별 패널 활성화 + TMP 재바인딩 =====
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

            // 언어별 TMP 다시 물기(영어가 안 뜨는 문제 방지)
            var newAbove = FindTMP(above);
            var newStory = FindTMP(story);
            if (newAbove != null) aboveText = newAbove;
            if (newStory != null) storyText = newStory;
        }

        // 화자명 다시 세팅
        SetSpeakerNameEcho();
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
        // 패널/텍스트 재바인딩 → 대사 재로드 → 현재 인덱스에서 재시작
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        // 인덱스 범위 보정
        if (index >= (lines?.Length ?? 0)) index = Mathf.Max(0, (lines?.Length ?? 1) - 1);

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    private void Update()
    {
        // BGM
        if (index >= 0 && index <= 8)
        {
            if (ArcadiaBGMSource != null && !ArcadiaBGMSource.isPlaying)
            {
                ArcadiaBGMSource.loop = true;
                ArcadiaBGMSource.Play();
            }
        }
        else
        {
            if (ArcadiaBGMSource != null && ArcadiaBGMSource.isPlaying)
                ArcadiaBGMSource.Stop();
        }

        // 새소리
        if (index == 0)
        {
            if (birdSound != null && !birdSound.isPlaying)
            {
                birdSound.loop = true;
                birdSound.Play();
            }
        }
        else
        {
            if (birdSound != null && birdSound.isPlaying)
                birdSound.Stop();
        }

        // 풀밟는 소리
        if (index >= 4 && index <= 7)
        {
            if (grassSound != null && !grassSound.isPlaying)
            {
                grassSound.loop = true;
                grassSound.Play();
            }
        }
        else
        {
            if (grassSound != null && grassSound.isPlaying)
                grassSound.Stop();
        }
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    // ===== 디버깅 로그 =====
    private void Debug_LogLine(string phase, int idx, string text)
    {
        Debug.Log($"[Stage2_1:{phase}] lang={CurrentLanguage}, idx={idx}, text={(text ?? "").Replace('\n',' ')}");
    }
}
