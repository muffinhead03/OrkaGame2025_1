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

    [Header("배경/캐릭터")]
    public GameObject BlackImage;
    public Image Real_bg_Image;
    public Sprite Real_bg_Sprite;

    [Space(6)]
    public GameObject Echo_Root;
    public Image Echo_Image;
    public GameObject Father_Root;
    public Image Father_Image;
    public Sprite Dad_Sprite;

    [Header("에코 스프라이트들")]
    public Sprite real_echo_default;
    public Sprite real_echo_2;
    public Sprite real_echo_5;
    public Sprite real_echo_6;
    public Sprite real_echo_7;

    [Header("오디오 소스 (모두 AudioSource)")]
    public AudioSource bgmSource;        // loop = true (인스펙터에서)
    public AudioSource knockingSource;   // 시작 노크
    public AudioSource roughOpenSource;  // 거칠게 여는 소리
    public AudioSource kickSource;       // 9번 끝
    public AudioSource fearBreathSource; // 10번 시작과 동시에
    public AudioSource doorSlamSource;   // 12번 끝
    public AudioSource lockingSource;    // 12번: 문쾅 종료 후
    public AudioSource wallHitSource;    // ✅ 6번 끝: 벽쾅 (NEW)
    
    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    [Header("대사 스크립트")]
    public LanguageCollector1_2 languageCollector;

    // 언어별: Above=화자명(17칸), Story=대사(17칸)
    private string[] aboveLines;
    private string[] storyLines;

    private int index; // 0-based
    private Coroutine typingCoroutine;
    private bool isAutoAdvanceScheduled;

    // ---------- Debug helpers ----------
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
            // 필요하면 중복 재생 방지
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
        // 12번: 문쾅 끝나면 잠금 소리(모두 개별 AudioSource)
        yield return PlaySource(doorSlamSource, "Door Slam", true);
        yield return PlaySource(lockingSource, "Door Lock", false);
    }
    // -----------------------------------

// --- (2) Awake: 첫 프레임부터 아빠 비활성화(필요시 에코도 OFF) ---
    private void Awake()
    {
        // 초기 강제 상태(아빠 OFF)
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

        // 시작 연출: knock 즉시, 0.5초 후 rough open
        StartCoroutine(PlaySource(knockingSource, "Knocking", false));
        yield return PlaySource(roughOpenSource, "Rough Open", false, 0.5f);

        // 1번 대사 시작
        yield return ShowCurrentLine();
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

    // 언어 코드 노멀라이즈
    string raw = LanguageManager.GetLanguage();
    string lang = (raw ?? "").Trim().ToLower();
    if (lang.StartsWith("en")) lang = "english";
    else if (lang.StartsWith("ko") || lang.Contains("korean")) lang = "korean";
    else if (lang.StartsWith("ja") || lang.Contains("japanese")) lang = "japanese";
    else if (lang.StartsWith("zh") || lang.Contains("chinese") || lang == "cn") lang = "chinese";
    else if (lang.StartsWith("kk") || lang.Contains("kazakh") || lang.Contains("kazakhstan") || lang.Contains("kaza")) lang = "kaza";

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
        case "kaza":
            aboveLines = languageCollector.KazaAbove1_2;
            storyLines = languageCollector.KazaLines1_2;
            break;
        default:
            Debug.LogWarning($"Unknown language '{raw}'(norm='{lang}'), default to Korean.");
            aboveLines = languageCollector.KoreanAbove1_2;
            storyLines = languageCollector.KoreanLines1_2;
            break;
    }

    // ✅ 더 이상 공통 최소값으로 "자르지" 않음!
    // Story는 원본 길이 유지, Above는 부족해도 그대로 두고 표시 시 보간(다른 함수에서 처리)
    if (storyLines == null || storyLines.Length == 0)
    {
        Debug.LogWarning("StoryLines가 비어있습니다. 빈 라인 하나로 대체합니다.");
        storyLines = new[] { "" };
    }

    if (aboveLines == null)
    {
        Debug.LogWarning("AboveLines가 null입니다. 표시 시 기본 이름을 사용합니다.");
        aboveLines = new string[0];
    }

    Debug.Log($"[LoadLines] lang='{raw}' -> '{lang}', aboveLen={aboveLines.Length}, storyLen={storyLines.Length}");
}


    private bool IsDadLine(int n1Based)
    {
        // 아빠: 2,4,5,7,11,12 (에코: 1,3,6,8,9,10,13,14,15,16,17)
        switch (n1Based)
        {
            case 2:
            case 4:
            case 5:
            case 7:
            case 11:
            case 12:
                return true;
            default:
                return false;
        }
    }

    private IEnumerator ShowCurrentLine()
    {
        int n = index + 1;

        // 줄 시작 시점에 스피커 상태 보정
        SetSpeakerActive(IsDadLine(n));
        yield return UpdateVisuals(index);

        // 🔈 위쪽 화자명: 현재 언어의 Above 배열에서 "해당 인덱스" 사용
        if (aboveText != null)
        {
            string speaker = GetSpeakerNameForIndex(n);
            aboveText.text = speaker;
            LogEvt($"Speaker -> {speaker}");
        }


        // 스토리 타이핑
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        string line = (index < storyLines.Length) ? storyLines[index] : "";

        SetNextActive(false);
        isAutoAdvanceScheduled = false;

        // 10번: 타이핑 시작과 동시에 숨소리
        if (n == 10) StartCoroutine(PlaySource(fearBreathSource, "Echo Fear Breath", false));

        LogEvt($"Type START\n{line}");
        typingCoroutine = StartCoroutine(TypeText(line));
        yield return typingCoroutine;
        LogEvt("Type END");

        // 🔔 6번: 타이핑 끝나자마자 벽쾅
        if (n == 6) StartCoroutine(PlaySource(wallHitSource, "Wall Hit", false));

        // 9번: 타이핑 끝나자마자 발차기
        if (n == 9) StartCoroutine(PlaySource(kickSource, "Kick", false));

        // 12번: 타이핑 끝나자마자 DoorSlam -> (완료 후) Locking
        if (n == 12) StartCoroutine(DoorThenLockSequence());

        yield return new WaitForSeconds(0.5f);
        SetNextActive(true);

        // 10번: 4초 뒤 자동 진행
        if (n == 10 && !isAutoAdvanceScheduled)
        {
            LogEvt("Auto-advance scheduled in 4s");
            StartCoroutine(AutoAdvanceAfterSeconds(4f, index));
        }
    }
    private string GetSpeakerNameForIndex(int n1Based)
    {
        int i = n1Based - 1;

        // 1) 배열에 해당 인덱스가 있고 값이 있으면 그대로 사용
        if (aboveLines != null && i >= 0 && i < aboveLines.Length && !string.IsNullOrEmpty(aboveLines[i]))
            return aboveLines[i];

        // 2) 위 배열이 "2칸(에코/아빠)" 스타일이면 규칙으로 보간
        if (aboveLines != null && aboveLines.Length >= 2)
            return IsDadLine(n1Based) ? aboveLines[1] : aboveLines[0];

        // 3) 최종 폴백
        return IsDadLine(n1Based) ? "Dad" : "Echo";
    }



    private IEnumerator AutoAdvanceAfterSeconds(float seconds, int indexSnapshot)
    {
        isAutoAdvanceScheduled = true;
        yield return new WaitForSeconds(seconds);
        if (index == indexSnapshot)
        {
            LogEvt("Auto-advance triggered");
            OnNext();
        }
    }

    private IEnumerator UpdateVisuals(int idx0)
    {
        int n = idx0 + 1;
        bool isDadLine = IsDadLine(n);
        // 스피커 상태 재보정(혹시 다른 곳에서 변경됐어도 복구)
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

            if (sp != null) { Echo_Image.sprite = sp; LogEvt($"Echo Sprite -> {sp.name}"); }
        }

        // BGM: 13번부터
        if (bgmSource != null)
        {
            if (n >= 13)
            {
                if (!bgmSource.isPlaying)
                {
                    // 인스펙터에서 loop=true, clip 지정 권장
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
        if (Echo_Root   != null) Echo_Root.SetActive(!dadOn);
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
        }

        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;

            aboveText = above.GetComponentInChildren<TextMeshProUGUI>(true);
            storyText = story.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

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
