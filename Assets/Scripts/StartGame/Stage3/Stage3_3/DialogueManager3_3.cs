using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager3_3 : MonoBehaviour
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
    public VideoPlayer endingVideoPlayer;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("배경/연출 오브젝트")]
    public Image backgroundImage;
    public GameObject blackImageObj;
    public GameObject Pan_4eyeclosedObj;
    public GameObject Pan_2Obj;

    public GameObject CarrotButton;
    public GameObject DialogueImage;
    public GameObject FirstPanel;
    public GameObject SettingPanel;
    public GameObject blackCoverPanel;

    [Header("오디오")]
    public AudioSource bgmSource;

    [Header("대사 소스")]
    public LanguageCollector3_3 languageCollector;

    // ===== Debug 옵션 =====
    [Header("Debug")]
    [SerializeField] private bool logDebug = true;
    [SerializeField] private bool logFullLine = false;
    [SerializeField] private int previewChars = 80;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 점프 스케일 연출
    private bool isJumpScaling = false;
    private float jumpTimer = 0f;
    private float jumpDuration = 0.3f;
    private Vector3 pan2TargetScale = Vector3.one;

    // === 언어 헬퍼 ===
    private string currentLangKey = "korean";
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

        LoadLinesForCurrentLanguage();

        // 이 장면은 판(Pan) 대사만 나오므로 스피커명 = Pan으로 설정
        if (aboveText != null) aboveText.text = GetSpeakerNamePan();

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        currentLangKey = CurrentLanguage;
        switch (currentLangKey)
        {
            case "korean":   lines = languageCollector.KoreanLines3_3; break;
            case "english":  lines = languageCollector.EnglishLines3_3; break;
            case "japanese": lines = languageCollector.JapaneseLines3_3; break;
            case "chinese":  lines = languageCollector.ChineseLines3_3; break;
            case "kaza":     lines = languageCollector.KazaLines3_3; break;
            default:
                lines = languageCollector.KoreanLines3_3;
                currentLangKey = "korean";
                Debug.LogWarning($"[D3_3] Unknown language, default to Korean.");
                break;
        }
        Debug_LogLangSelected(lines?.Length ?? 0);
    }

    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            Debug.LogError($"[D3_3] Invalid line index {index} (len={lines?.Length ?? 0})");
            yield break;
        }

        // 활성 언어 TMP가 끊겼을 수 있으므로 보증
        EnsureTMPBound();

        UpdateVisuals(index);

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // 디버그: 라인 시작
        Debug_LogLine("BEGIN", index, lines[index]);

        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        // 디버그: 라인 완료
        Debug_LogLine("END", index, lines[index]);

        nextButton?.gameObject.SetActive(true);

        // 1번(두 번째 줄) 이후에 영상 재생 트리거
        if ((index == 1 || index == lines.Length - 1) && Pan_2Obj != null)
        {
            yield return StartCoroutine(JumpScaleInOut(Pan_2Obj, 0.5f));

            if (index == 1)
            {
                yield return new WaitForSeconds(0.5f);
                PlayEndingVideo();
            }
        }
    }

    private void UpdateVisuals(int idx)
    {
        if (backgroundImage != null) backgroundImage.enabled = (idx == 0);
        if (blackImageObj != null) blackImageObj.SetActive(idx == 1);

        Pan_4eyeclosedObj?.SetActive(false);
        Pan_2Obj?.SetActive(false);
        if (idx == 0) Pan_4eyeclosedObj?.SetActive(true);

        // 스피커명: 이 장면은 계속 Pan
        if (aboveText != null) aboveText.text = GetSpeakerNamePan();

        if (bgmSource != null)
        {
            if (idx == 0 && !bgmSource.isPlaying)
            {
                bgmSource.loop = true;
                bgmSource.Play();
            }
            else if (idx == 1 && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
        }
    }

    private void PlayEndingVideo()
    {
        if (endingVideoPlayer == null)
        {
            Debug.LogWarning("[D3_3] endingVideoPlayer가 설정되지 않았습니다.");
            return;
        }

        if (blackCoverPanel != null) blackCoverPanel.SetActive(true);

        if (backgroundImage != null) backgroundImage.enabled = false;
        if (blackImageObj != null) blackImageObj.SetActive(false);
        if (Pan_4eyeclosedObj != null) Pan_4eyeclosedObj.SetActive(false);
        if (Pan_2Obj != null) Pan_2Obj.SetActive(false);

        if (CarrotButton != null) CarrotButton.SetActive(false);
        if (DialogueImage != null) DialogueImage.SetActive(false);
        if (FirstPanel != null) FirstPanel.SetActive(false);
        if (SettingPanel != null) SettingPanel.SetActive(false);

        endingVideoPlayer.gameObject.SetActive(true);
        endingVideoPlayer.loopPointReached -= OnVideoFinished; // 중복 방지
        endingVideoPlayer.loopPointReached += OnVideoFinished;
        endingVideoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[D3_3] Ending video finished.");
        if (blackCoverPanel != null) blackCoverPanel.SetActive(false);
        SceneManager.LoadScene("Stage3_2");
    }

    private IEnumerator JumpScaleInOut(GameObject targetObj, float duration = 0.5f)
    {
        if (targetObj == null) yield break;

        targetObj.SetActive(true);
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scaleT = t / duration;
            targetObj.transform.localScale = Vector3.Lerp(startScale, endScale, scaleT);
            yield return null;
        }

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scaleT = t / duration;
            targetObj.transform.localScale = Vector3.Lerp(endScale, startScale, scaleT);
            yield return null;
        }

        targetObj.SetActive(false);
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null)
        {
            Debug.LogError("[D3_3] storyText가 바인딩되지 않았습니다.");
            yield break;
        }

        storyText.text = "";
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
        currentLangKey = lang;

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

            // 언어별 TMP 재바인딩
            aboveText = FindTMP(above);
            storyText = FindTMP(story);

            Debug_LogBind("Above", aboveText);
            Debug_LogBind("Story", storyText);

            if (aboveText == null || storyText == null)
                Debug.LogWarning("[D3_3] Active language TMP not found. Check children TextMeshProUGUI.");
        }
    }

    // 활성 언어 패널에서 TMP를 재탐색(안전망)
    private void EnsureTMPBound()
    {
        if (aboveText != null && storyText != null &&
            aboveText.gameObject.activeInHierarchy && storyText.gameObject.activeInHierarchy)
            return;

        RectTransform a, s;
        GetLangRoots(CurrentLanguage, out a, out s);
        if (a != null) aboveText = FindTMP(a);
        if (s != null) storyText = FindTMP(s);

        Debug_LogBind("Ensure.Above", aboveText);
        Debug_LogBind("Ensure.Story", storyText);
    }

    private void GetLangRoots(string lang, out RectTransform above, out RectTransform story)
    {
        switch (lang)
        {
            case "english":   above = English_Above;   story = English_Story;   return;
            case "japanese":  above = Japanese_Above;  story = Japanese_Story;  return;
            case "chinese":   above = Chinese_Above;   story = Chinese_Story;   return;
            case "kaza":      above = Kaza_Above;      story = Kaza_Story;      return;
            default:          above = Korean_Above;    story = Korean_Story;    return;
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
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        // 인덱스가 벗어나면 0으로
        if (index >= (lines?.Length ?? 0)) index = 0;

        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    // ===== 화자명(언어별) : Pan (Above1_2[1]) =====
    private string GetSpeakerNamePan()
    {
        switch (CurrentLanguage)
        {
            case "korean":   return SafeName(languageCollector?.KoreanAbove1_2, 1, "판");
            case "english":  return SafeName(languageCollector?.EnglishAbove1_2, 1, "Pan");
            case "japanese": return SafeName(languageCollector?.JapaneseAbove1_2, 1, "パーン");
            case "chinese":  return SafeName(languageCollector?.ChineseAbove1_2, 1, "潘");
            case "kaza":     return SafeName(languageCollector?.KazaAbove1_2,    1, "Пан");
            default:         return "Pan";
        }
    }
    private string SafeName(string[] arr, int idx, string fallback)
    {
        if (arr != null && arr.Length > idx && !string.IsNullOrEmpty(arr[idx])) return arr[idx];
        return fallback;
    }

    // ===== Debug helpers =====
    private void Debug_LogLangSelected(int totalLines)
    {
        if (!logDebug) return;
        Debug.Log($"[D3_3][LANG] selected={currentLangKey}, lines={totalLines}");
    }

    private void Debug_LogLine(string phase, int idx, string line)
    {
        if (!logDebug) return;
        string text = line ?? "";
        if (!logFullLine && text.Length > previewChars)
            text = text.Substring(0, previewChars) + "...";
        Debug.Log($"[D3_3][LINE {phase}] lang={currentLangKey}, idx={idx}, text=\"{text}\"");
    }

    private void Debug_LogBind(string which, TextMeshProUGUI tmp)
    {
        if (!logDebug) return;
        string name = tmp ? tmp.gameObject.name : "NULL";
        Debug.Log($"[D3_3][BIND] {which} -> {name}");
    }
}
