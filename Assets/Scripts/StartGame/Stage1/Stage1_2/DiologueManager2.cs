using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManagerStage1_2 : MonoBehaviour
{
    [Header("언어 오브젝트 (언어별 컨테이너)")] public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("기본 위치값")] public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    [Header("UI 요소")] public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;

    [Header("배경/캐릭터")] public GameObject BlackImage;
    public Image Real_bg_Image;
    public Sprite Real_bg_Sprite;

    [Space(6)] public GameObject Echo_Root;
    public Image Echo_Image;
    public GameObject Father_Root;
    public Image Father_Image;
    public Sprite Dad_Sprite;

    [Header("에코 스프라이트들")] public Sprite real_echo_default;
    public Sprite real_echo_2;
    public Sprite real_echo_5;
    public Sprite real_echo_6;
    public Sprite real_echo_7;

    [Header("오디오 소스 (모두 AudioSource)")] public AudioSource bgmSource;
    public AudioSource knockingSource;
    public AudioSource roughOpenSource;
    public AudioSource kickSource;
    public AudioSource fearBreathSource;
    public AudioSource doorSlamSource;
    public AudioSource lockingSource;
    public AudioSource wallHitSource;

    [Header("타이핑")] public float typingSpeed = 0.04f;

    [Header("대사 스크립트")] public LanguageCollector1_2 languageCollector;

    private string[] aboveLines;
    private string[] storyLines;

    private int index; // 0-based
    private Coroutine typingCoroutine;
    private bool isAutoAdvanceScheduled;

    private void LogEvt(string msg) => Debug.Log($"[Stage1_2] (line {index + 1}) {msg}");

    private void SetNextActive(bool on)
    {
        if (!nextButton) return;
        nextButton.gameObject.SetActive(on);
        LogEvt($"NEXT BUTTON => {(on ? "ENABLED" : "DISABLED")}");
    }

    private IEnumerator PlaySource(AudioSource src, string label, bool waitForEnd = false, float delay = 0f)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (src != null)
        {
            if (src.isPlaying) src.Stop();
            src.Play();
            string clipName = (src.clip != null) ? src.clip.name : "None";
            LogEvt($"SFX ▶ {label} (source: {src.name}, clip: {clipName})");
            if (waitForEnd && src.clip != null)
                yield return new WaitForSeconds(src.clip.length);
        }
        else
        {
            LogEvt($"SFX ✖ {label} (source null)");
        }
    }

    private IEnumerator DoorThenLockSequence()
    {
        yield return PlaySource(doorSlamSource, "Door Slam", true);
        yield return PlaySource(lockingSource, "Door Lock", false);
    }

    private void Awake()
    {
        SetSpeakerActive(false);

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
        index = 0;
        StartCoroutine(OpeningThenFirstLine());
    }

    private IEnumerator OpeningThenFirstLine()
    {
        if (BlackImage != null) BlackImage.SetActive(true);
        if (Real_bg_Image != null) Real_bg_Image.gameObject.SetActive(false);
        if (Echo_Root != null) Echo_Root.SetActive(false);
        if (Father_Root != null) Father_Root.SetActive(false);

        StartCoroutine(PlaySource(knockingSource, "Knocking", false));
        yield return PlaySource(roughOpenSource, "Rough Open", false, 0.5f);

        yield return ShowCurrentLine();
    }

    // 표준키만 사용 (korean/english/japanese/chinese/kazakh)
    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            Debug.LogError("LanguageCollector1_2가 할당되지 않았습니다.");
            aboveLines = System.Array.Empty<string>();
            storyLines = new[] { "" };
            return;
        }

        string lang = (LanguageManager.GetLanguage() ?? "").Trim().ToLowerInvariant();
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
            case "kazakh":
                aboveLines = languageCollector.KazaAbove1_2;
                storyLines = languageCollector.KazaLines1_2;
                break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', fallback to Korean.");
                aboveLines = languageCollector.KoreanAbove1_2;
                storyLines = languageCollector.KoreanLines1_2;
                break;
        }

        if (storyLines == null || storyLines.Length == 0) storyLines = new[] { "" };
        if (aboveLines == null) aboveLines = System.Array.Empty<string>();

        Debug.Log($"[LoadLines] lang='{lang}', aboveLen={aboveLines.Length}, storyLen={storyLines.Length}");
    }

    private bool IsDadLine(int n1Based)
    {
        switch (n1Based)
        {
            case 2: case 4: case 5: case 7: case 11: case 12:
                return true;
            default:
                return false;
        }
    }

    private IEnumerator ShowCurrentLine()
    {
        int n = index + 1;

        SetSpeakerActive(IsDadLine(n));
        yield return UpdateVisuals(index);

        if (aboveText != null)
        {
            string speaker = GetSpeakerNameForIndex(n);
            aboveText.text = speaker;
            LogEvt($"Speaker -> {speaker}");
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        string line = (index < storyLines.Length) ? storyLines[index] : "";

        SetNextActive(false);
        isAutoAdvanceScheduled = false;

        if (n == 10) StartCoroutine(PlaySource(fearBreathSource, "Echo Fear Breath", false));

        LogEvt($"Type START\n{line}");
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;
        LogEvt("Type END");

        if (n == 6) StartCoroutine(PlaySource(wallHitSource, "Wall Hit", false));
        if (n == 9) StartCoroutine(PlaySource(kickSource, "Kick", false));
        if (n == 12) StartCoroutine(DoorThenLockSequence());

        yield return new WaitForSeconds(0.5f);
        SetNextActive(true);

        if (n == 10 && !isAutoAdvanceScheduled)
        {
            LogEvt("Auto-advance scheduled in 4s");
            StartCoroutine(AutoAdvanceAfterSeconds(4f, index));
        }
    }

    private string GetSpeakerNameForIndex(int n1Based)
    {
        bool isDad = IsDadLine(n1Based);
        int i = n1Based - 1;

        if (aboveLines != null && aboveLines.Length == 2)
        {
            string name = isDad ? aboveLines[1] : aboveLines[0];
            if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
            return isDad ? "Dad" : "Echo";
        }

        if (aboveLines != null && i >= 0 && i < aboveLines.Length)
        {
            string name = aboveLines[i];
            if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
            return isDad ? "Dad" : "Echo";
        }

        return isDad ? "Dad" : "Echo";
    }

    private IEnumerator AutoAdvanceAfterSeconds(float seconds, int indexSnapshot)
    {
        isAutoAdvanceScheduled = true;
        yield return new WaitForSeconds(seconds);
        if (index == indexSnapshot) OnNext();
    }

    private IEnumerator UpdateVisuals(int idx0)
    {
        int n = idx0 + 1;
        bool isDadLine = IsDadLine(n);
        SetSpeakerActive(isDadLine);

        if (BlackImage != null) BlackImage.SetActive(false);
        if (Real_bg_Image != null)
        {
            Real_bg_Image.gameObject.SetActive(true);
            Real_bg_Image.sprite = Real_bg_Sprite;
            var c = Real_bg_Image.color; c.a = 1f; Real_bg_Image.color = c;
        }

        if (Father_Root != null) Father_Root.SetActive(isDadLine);
        if (Echo_Root != null) Echo_Root.SetActive(!isDadLine);
        LogEvt(isDadLine ? "Character: DAD ON / ECHO OFF" : "Character: ECHO ON / DAD OFF");

        if (isDadLine)
        {
            if (Father_Image != null && Dad_Sprite != null)
                Father_Image.sprite = Dad_Sprite;
        }
        else if (Echo_Image != null)
        {
            Sprite sp = null;
            if (n == 1 || n == 10) sp = real_echo_6;
            else if (n == 3 || n == 6 || n == 16) sp = real_echo_default;
            else if (n == 8 || n == 13 || n == 15) sp = real_echo_5;
            else if (n == 9) sp = real_echo_2;
            else if (n == 14) sp = real_echo_7;

            if (sp != null)
            {
                Echo_Image.sprite = sp;
                LogEvt($"Echo Sprite -> {sp.name}");
            }
        }

        if (bgmSource != null)
        {
            if (n >= 13)
            {
                if (!bgmSource.isPlaying)
                {
                    if (bgmSource.clip == null)
                        LogEvt("BGM WARNING: clip not assigned.");
                    bgmSource.loop = true;
                    bgmSource.Play();
                    LogEvt($"BGM ▶ Start (source: {bgmSource.name}, clip: {(bgmSource.clip ? bgmSource.clip.name : "None")})");
                }
            }
            else
            {
                if (bgmSource.isPlaying)
                {
                    bgmSource.Stop();
                    LogEvt("BGM ■ Stop");
                }
            }
        }

        yield break;
    }

    private void SetSpeakerActive(bool dadOn)
    {
        if (Father_Root != null) Father_Root.SetActive(dadOn);
        if (Echo_Root != null) Echo_Root.SetActive(!dadOn);
        LogEvt(dadOn ? "Character: DAD ON / ECHO OFF" : "Character: ECHO ON / DAD OFF");
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
        SetNextActive(false);
        index++;
        LogEvt("NEXT pressed -> move to next line");

        if (storyLines == null || index >= storyLines.Length)
        {
            LogEvt("Reached end of script. Load next scene.");
            SceneManager.LoadScene("SlidingPuzzle");
            return;
        }

        StartCoroutine(ShowCurrentLine());
    }

    private void SetupLanguageUI()
    {
        var all = new[]
        {
            Korean_Above, Korean_Story,
            English_Above, English_Story,
            Japanese_Above, Japanese_Story,
            Chinese_Above, Chinese_Story,
            Kaza_Above, Kaza_Story
        };
        foreach (var rt in all) rt?.gameObject.SetActive(false);

        string lang = (LanguageManager.GetLanguage() ?? "").Trim().ToLowerInvariant();
        RectTransform above = Korean_Above, story = Korean_Story;

        switch (lang)
        {
            case "english":  above = English_Above;  story = English_Story;  break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese":  above = Chinese_Above;  story = Chinese_Story;  break;
            case "kazakh":   above = Kaza_Above;     story = Kaza_Story;     break;
            // default: Korean
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            var aboveTf = above.Find("AboveText");
            var storyTf = story.Find("StoryText");

            aboveText = (aboveTf != null)
                ? aboveTf.GetComponent<TextMeshProUGUI>()
                : above.GetComponentInChildren<TextMeshProUGUI>(true);

            storyText = (storyTf != null)
                ? storyTf.GetComponent<TextMeshProUGUI>()
                : story.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        // ✅ 명시적 null 체크로 컴파일 오류 방지
        Debug.Log($"[Stage1_2] aboveText={(aboveText != null ? aboveText.name : "NULL")}, " +
                  $"storyText={(storyText != null ? storyText.name : "NULL")}");
    } // ← 누락됐던 닫는 중괄호

    private void OnLanguageChanged(string newLang)
    {
        LogEvt($"Language changed -> {newLang}");
        LoadLinesForCurrentLanguage();
        SetupLanguageUI();
        StopAllCoroutines();
        index = 0;
        StartCoroutine(OpeningThenFirstLine());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
