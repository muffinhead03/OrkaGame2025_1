using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Video;

public class DialogueManager3_5 : MonoBehaviour
{
    [Header("언어 컨테이너")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("시작 화면에서 비활성화할 UI")]
    public GameObject CarrotButton;
    public GameObject DialogueImage;
    public GameObject FirstPanel;
    public GameObject SettingPanel;

    [Header("UI")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;
    public VideoPlayer endingVideoPlayer;

    [Header("배경 오브젝트")]
    public GameObject Real_bg1Obj;
    public GameObject Real_bg2Obj;
    public GameObject BlackImageObj;
    public GameObject WhiteImageObj;
    public GameObject BackGroundObj;

    [Header("캐릭터 오브젝트")]
    public GameObject Eco_real6Obj, Eco_real_defaultObj, Eco_real5Obj, Eco_real2Obj;
    public GameObject Eco_real3Obj, Eco_laughObj, Eco_banjunObj, Eco_real4Obj, Pan_realObj;
    public GameObject Eco_real9Obj;

    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource BGMSource, BreathSource, BirdSource, FestivalSource;
    public AudioSource DoorGoriSource, DoorOpenSource, FluteSource, FallSoundSource;

    [Header("페이드 이미지")]
    public Image whiteImage;

    [Header("대사/화자")]
    public LanguageCollector3_5 languageCollector;
    private string[] lines;
    private int index = 0;
    private Coroutine typingCoroutine;
    private GameObject[] characterObjs;
    private GameObject[] backgroundObjs;

    // ===== 헬퍼: 언어 표준화 =====
    private string Lang => Normalize(LanguageManager.GetLanguage());
    private static string Normalize(string raw)
    {
        string s = (raw ?? "").Trim().ToLowerInvariant();
        if (s.StartsWith("en")) return "english";
        if (s.StartsWith("ko")) return "korean";
        if (s.StartsWith("ja")) return "japanese";
        if (s.StartsWith("zh")) return "chinese";
        if (s.StartsWith("kk") || s.Contains("kazakh") || s.Contains("kaza")) return "kazakh";
        return s;
    }

    private void Awake()
    {
        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
            nextButton.gameObject.SetActive(false);
        }

        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        characterObjs = new[] {
            Eco_real6Obj, Eco_real_defaultObj, Eco_real5Obj, Eco_real2Obj,
            Eco_real3Obj, Eco_laughObj, Eco_banjunObj, Eco_real4Obj, Pan_realObj, Eco_real9Obj
        };

        backgroundObjs = new[] {
            BackGroundObj, BlackImageObj, WhiteImageObj, Real_bg1Obj, Real_bg2Obj
        };

        SetupLanguageUI();
        LoadLinesForCurrentLanguage();
        index = 0;
        ShowCurrentDialogue();
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    // ===== 언어 컨테이너 & TMP 바인딩 =====
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
        switch (Lang)
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

            // 자식 이름이 "AboveText"/"StoryText"라면 우선 사용, 아니면 첫 TMPUGUI
            var aboveTf = above.Find("AboveText");
            var storyTf = story.Find("StoryText");
            aboveText = (aboveTf ? aboveTf.GetComponent<TextMeshProUGUI>() : above.GetComponentInChildren<TextMeshProUGUI>(true));
            storyText = (storyTf ? storyTf.GetComponent<TextMeshProUGUI>() : story.GetComponentInChildren<TextMeshProUGUI>(true));
        }
    }

    private void LoadLinesForCurrentLanguage()
    {
        if (languageCollector == null)
        {
            lines = new[] { "" };
            return;
        }

        switch (Lang)
        {
            case "korean":  lines = languageCollector.KoreanLines3_5;  break;
            case "english": lines = languageCollector.EnglishLines3_5; break;
            case "japanese":lines = languageCollector.JapaneseLines3_5;break;
            case "chinese": lines = languageCollector.ChineseLines3_5; break;
            case "kazakh":  lines = languageCollector.KazaLines3_5;    break;
            default:        lines = languageCollector.KoreanLines3_5;  break;
        }

        if (lines == null || lines.Length == 0)
            lines = new[] { "" };
    }

    private void OnLanguageChanged(string newLang)
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();
        StopAllCoroutines();
        index = 0;
        ShowCurrentDialogue();
    }

    // ===== 표시 루틴 =====
    private void ShowCurrentDialogue()
    {
        // 효과음 정리
        FestivalSource?.Stop();
        DoorGoriSource?.Stop();
        DoorOpenSource?.Stop();
        FluteSource?.Stop();
        FallSoundSource?.Stop();

        ApplyBackground(index);
        ApplyCharacter(index);

        // 화자명 표시
        if (index == 9)
        {
            // 무음 컷: 제목/본문 비움
            if (aboveText) aboveText.text = "";
            if (storyText) storyText.text = "";
            nextButton?.gameObject.SetActive(true);
            return;
        }
        else if (index == 11 || index == 12)
        {
            if (aboveText) aboveText.text = "???";
        }
        else
        {
            if (aboveText) aboveText.text = GetSpeakerNameForIndex(index);
        }

        // 본문(콜론 분리)
        string rawLine = (lines != null && index >= 0 && index < lines.Length) ? lines[index] : "";
        string displayLine = StripPrefixBeforeColon(rawLine);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(displayLine));

        // SFX 트리거
        if (index == 3) DoorGoriSource?.Play();

        if (index == 10)
        {
            FluteSource?.Play();
            nextButton?.gameObject.SetActive(false);
            StartCoroutine(PlayFluteThenFall());
        }

        if (index == 11)
        {
            if (FestivalSource != null)
            {
                FestivalSource.loop = false;
                FestivalSource.Play();
            }
        }

        if ((index == 11 || index == 12) && BirdSource != null && !BirdSource.isPlaying)
        {
            BirdSource.loop = true;
            BirdSource.Play();
        }
    }

    private string StripPrefixBeforeColon(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        int i = s.IndexOf(':');
        if (i >= 0 && i < s.Length - 1) return s.Substring(i + 1).Trim();
        return s;
    }

    // 인덱스 → 화자명(현지화)
    private string GetSpeakerNameForIndex(int idx)
    {
        // 9: 무음 컷(이미 처리), 11~12: ??? (이미 처리)
        // 나머지 규칙: 대부분 에코, 9만 판
        if (idx == 9) return GetNamePan();
        return GetNameEcho();
    }

    private string GetNameEcho()
    {
        var t = languageCollector ? languageCollector.GetAboveTable() : null;
        if (t != null && t.Length > 0 && !string.IsNullOrEmpty(t[0])) return t[0];
        return "Echo";
    }

    private string GetNamePan()
    {
        var t = languageCollector ? languageCollector.GetAboveTable() : null;
        if (t != null && t.Length > 1 && !string.IsNullOrEmpty(t[1])) return t[1];
        return "Pan";
    }

    private string GetNameNarke()
    {
        var t = languageCollector ? languageCollector.GetAboveTable() : null;
        if (t != null && t.Length > 2 && !string.IsNullOrEmpty(t[2])) return t[2];
        return "Narke";
    }

    private void ApplyBackground(int idx)
    {
        foreach (var bg in backgroundObjs) if (bg) bg.SetActive(false);

        if (idx == 7) BlackImageObj?.SetActive(true);
        else if (idx == 11 || idx == 12) Real_bg2Obj?.SetActive(true);
        else if ((idx >= 1 && idx <= 6) || idx == 8 || idx == 9 || idx == 10) Real_bg1Obj?.SetActive(true);
        else Real_bg1Obj?.SetActive(true);
    }

    private void ApplyCharacter(int idx)
    {
        foreach (var obj in characterObjs) if (obj) obj.SetActive(false);

        switch (idx)
        {
            case 0:
            case 2:
            case 10:
                Eco_real6Obj?.SetActive(true);  break;
            case 1:
                Eco_real_defaultObj?.SetActive(true); break;
            case 3:
                Eco_real5Obj?.SetActive(true);  break;
            case 4:
                Eco_real2Obj?.SetActive(true);  break;
            case 5:
                Eco_real3Obj?.SetActive(true);  break;
            case 6:
                Eco_real9Obj?.SetActive(true);  break;
            case 7:
                Eco_banjunObj?.SetActive(true); break;
            case 8:
                Eco_real4Obj?.SetActive(true);  break;
            case 9:
                Pan_realObj?.SetActive(true);   break;
            case 11:
            case 12:
                // none
                break;
        }
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null) yield break;

        storyText.text = "";
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 타이핑 완료 이벤트
        if (index == 1 && FestivalSource != null)
        {
            FestivalSource.loop = false;
            FestivalSource.Play();
        }
        else if (index == 6 && DoorOpenSource != null)
        {
            DoorOpenSource.Play();
        }
        else if (index == 12)
        {
            // 주요 UI 비활성화
            CarrotButton?.SetActive(false);
            DialogueImage?.SetActive(false);
            FirstPanel?.SetActive(false);
            SettingPanel?.SetActive(false);

            // 흰색 페이드
            yield return StartCoroutine(FadeToImage(WhiteImageObj, 3f));
            yield return new WaitForSeconds(2f);

            // 엔딩 영상 재생
            PlayEndingVideo();
        }

        if (index != 10)
            nextButton?.gameObject.SetActive(true);
    }

    private void PlayEndingVideo()
    {
        if (endingVideoPlayer == null)
        {
            Debug.LogWarning("endingVideoPlayer 가 설정되지 않았습니다!");
            return;
        }

        CarrotButton?.SetActive(false);
        DialogueImage?.SetActive(false);
        FirstPanel?.SetActive(false);
        SettingPanel?.SetActive(false);

        endingVideoPlayer.gameObject.SetActive(true);
        endingVideoPlayer.Play();
        endingVideoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("엔딩 영상이 종료되었습니다.");
        // 필요 시 후속 처리
    }

    public void OnClickNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= (lines?.Length ?? 0)) return;
        ShowCurrentDialogue();
    }

    private IEnumerator PlayFluteThenFall()
    {
        if (FluteSource != null)
        {
            FluteSource.Play();
            yield return new WaitWhile(() => FluteSource.isPlaying);
        }

        if (FallSoundSource != null)
        {
            FallSoundSource.Play();
            yield return new WaitWhile(() => FallSoundSource.isPlaying);
        }

        yield return StartCoroutine(FadeToImage(Real_bg2Obj, 3f)); // 다음 장면 페이드
        index++;
        ShowCurrentDialogue(); // case 11
    }

    private IEnumerator FadeToImage(GameObject targetObj, float duration)
    {
        if (targetObj == null) yield break;

        targetObj.SetActive(true);
        var image = targetObj.GetComponent<Image>();
        if (image == null) yield break;

        Color start = image.color;
        start.a = 0f;
        image.color = start;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / duration);
            image.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
    }

    private void Update()
    {
        switch (index)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
                if (BGMSource != null && !BGMSource.isPlaying) BGMSource.Play();
                break;

            case 7:
                if (BGMSource != null && BGMSource.isPlaying) BGMSource.Stop();
                break;

            case 8:
                if (BreathSource != null && !BreathSource.isPlaying) BreathSource.Play();
                break;

            case 9:
                if (BGMSource != null && BGMSource.isPlaying) BGMSource.Stop();
                if (BreathSource != null && BreathSource.isPlaying) BreathSource.Stop();
                break;

            case 10:
                if (BreathSource != null && !BreathSource.isPlaying) BreathSource.Play();
                break;

            case 11:
            case 12:
                if (BirdSource != null && !BirdSource.isPlaying)
                {
                    BirdSource.loop = true;
                    BirdSource.Play();
                }
                if (index == 12 && FestivalSource != null && FestivalSource.isPlaying)
                    FestivalSource.Stop();
                break;
        }
    }
}
