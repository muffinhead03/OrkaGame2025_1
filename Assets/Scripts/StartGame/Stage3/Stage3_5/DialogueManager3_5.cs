using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Video;


public class DialogueManager3_5 : MonoBehaviour
{
    [Header("언어 오브젝트")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("비디오 재생 시 비활성화될 UI")]
    public GameObject CarrotButton;
    public GameObject DialogueImage;
    public GameObject FirstPanel;
    public GameObject SettingPanel;


    [Header("UI 요소")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;
    public VideoPlayer endingVideoPlayer;

    [Header("배경 이미지 오브젝트")]
    public GameObject Real_bg1Obj;
    public GameObject Real_bg2Obj;
    public GameObject BlackImageObj;
    public GameObject WhiteImageObj;
    public GameObject BackGroundObj;

    [Header("캐릭터 이미지 오브젝트")]
    public GameObject Eco_real6Obj, Eco_real_defaultObj, Eco_real5Obj, Eco_real2Obj;
    public GameObject Eco_real3Obj, Eco_laughObj, Eco_banjunObj, Eco_real4Obj, Pan_realObj;
    public GameObject Eco_real9Obj;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("사운드")]
    public AudioSource BGMSource, BreathSource, BirdSource, FestivalSource;
    public AudioSource DoorGoriSource, DoorOpenSource, FluteSource, FallSoundSource;

    [Header("페이드 이미지")]
    public Image whiteImage;

    [Header("대사")]
    public LanguageCollector3_5 languageCollector;
    private string[] lines;
    private int index = 0;
    private Coroutine typingCoroutine;
    private GameObject[] characterObjs;
    private GameObject[] backgroundObjs;

    
    private void Awake()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnClickNext);
        nextButton.gameObject.SetActive(false);

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
            BackGroundObj, BlackImageObj, WhiteImageObj,
            Real_bg1Obj, Real_bg2Obj
        };

        SetupLanguageUI();
        LoadLinesForCurrentLanguage();
        index = 0;
        ShowCurrentDialogue();
    }

    private void ShowCurrentDialogue()
    {
        FestivalSource?.Stop();
        DoorGoriSource?.Stop();
        DoorOpenSource?.Stop();
        FluteSource?.Stop();
        FallSoundSource?.Stop();

        ApplyBackground(index);
        ApplyCharacter(index);

        // aboveline 처리 + index==9 즉시 리턴
        if (index == 9)
        {
            aboveText.text = "";
            storyText.text = "";
            nextButton.gameObject.SetActive(true);
            return;
        }
        else if (index == 11 || index == 12)
        {
            aboveText.text = "???";
        }
        else if (index == 0 || index == 1 || index == 2 || index == 3 ||
                 index == 4 || index == 5 || index == 6 || index == 7 ||
                 index == 8 || index == 10)
        {
            aboveText.text = "에코";
        }
        else
        {
            aboveText.text = "";
        }

        // 대사 텍스트 설정 (이름 제거)
        string rawLine = lines[index];
        string displayLine = rawLine;
        int colonIdx = rawLine.IndexOf(':');
        if (colonIdx >= 0 && colonIdx < rawLine.Length - 1)
            displayLine = rawLine.Substring(colonIdx + 1).Trim();

        // 타이핑 코루틴 (한 번만)
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(displayLine));

        // 사운드/연출
        if (index == 3) DoorGoriSource?.Play();

        if (index == 10)
        {
            FluteSource?.Play();
            nextButton.gameObject.SetActive(false);
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


    private void ApplyBackground(int idx)
    {
        foreach (var bg in backgroundObjs)
            if (bg != null) bg.SetActive(false);

        if (idx == 7)
            BlackImageObj?.SetActive(true);
        else if (idx == 11 || idx == 12)
            Real_bg2Obj?.SetActive(true);
        else if ((idx >= 1 && idx <= 6) || idx == 8 || idx == 9 || idx == 10)
            Real_bg1Obj?.SetActive(true);
        else
            Real_bg1Obj?.SetActive(true);
    }

    private void ApplyCharacter(int idx)
    {
        foreach (var obj in characterObjs)
            if (obj != null) obj.SetActive(false);

        switch (idx)
        {
            case 0:
            case 2:
            case 10:
                Eco_real6Obj?.SetActive(true);
                break;
            case 1:
                Eco_real_defaultObj?.SetActive(true);
                break;
            case 3:
                Eco_real5Obj?.SetActive(true);
                break;
            case 4:
                Eco_real2Obj?.SetActive(true);
                break;
            case 5:
                Eco_real3Obj?.SetActive(true);
                break;
            case 6:
                Eco_real9Obj?.SetActive(true);
                break;
            case 7:
                Eco_banjunObj?.SetActive(true);
                break;
            case 8:
                Eco_real4Obj?.SetActive(true);
                break;
            case 9:
                if (Pan_realObj != null)
                {
                    Pan_realObj.SetActive(true);
                }
                break;

            case 11:
            case 12:
                break;
        }
    }

    private IEnumerator TypeText(string fullText)
    {
        storyText.text = "";
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 사운드 이벤트 처리
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
            // 4가지 오브젝트 비활성화
            CarrotButton?.SetActive(false);
            DialogueImage?.SetActive(false);
            FirstPanel?.SetActive(false);
            SettingPanel?.SetActive(false);

            // 대사 출력 완료 후 WhiteImageObj 페이드인
            yield return StartCoroutine(FadeToImage(WhiteImageObj, 3f));

            // 페이드인 완료 후 2초 대기
            yield return new WaitForSeconds(2f);

            // 동영상 재생
            PlayEndingVideo();
        }


        if (index != 10)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private void PlayEndingVideo()
    {
        if (endingVideoPlayer == null)
        {
            Debug.LogWarning("endingVideoPlayer가 설정되어 있지 않습니다!");
            return;
        }

        CarrotButton?.SetActive(false);
        DialogueImage?.SetActive(false);
        FirstPanel?.SetActive(false);
        SettingPanel?.SetActive(false);


        endingVideoPlayer.gameObject.SetActive(true);
        endingVideoPlayer.Play();

        // 필요 시 비디오 종료 콜백 등록
        endingVideoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("비디오 재생이 종료되었습니다.");

        // 동영상 종료 후 처리 필요 시 여기에 작성
        // 예: 다음 씬 로드, UI 활성화 등
    }


    public void OnClickNext()
    {
        nextButton.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length) return;
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

        yield return StartCoroutine(FadeToImage(Real_bg2Obj, 3f)); // 배경 페이드인

        index++; // 자동 진행
        ShowCurrentDialogue(); // 다음 대사 출력 (case 11)
    }

    private IEnumerator FadeToImage(GameObject targetObj, float duration)
    {
        if (targetObj == null) yield break;

        targetObj.SetActive(true);  // 활성화 먼저
        var image = targetObj.GetComponent<Image>();
        if (image == null) yield break;

        Color color = image.color;
        color.a = 0f;
        image.color = color;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
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
        foreach (var rt in all)
            rt?.gameObject.SetActive(false);

        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        RectTransform above = Korean_Above, story = Korean_Story;
        switch (lang)
        {
            case "english": above = English_Above; story = English_Story; break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese": above = Chinese_Above; story = Chinese_Story; break;
            case "kazahustan":
            case "kaza": above = Kaza_Above; story = Kaza_Story; break;
        }
        if (above != null && story != null)
        {
            above.gameObject.SetActive(true);
            story.gameObject.SetActive(true);
            above.anchoredPosition = new Vector2(-750f, 160f);
            story.anchoredPosition = new Vector2(-250f, -20f);
        }
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_5; break;
            case "english": lines = languageCollector.EnglishLines3_5; break;
            case "japanese": lines = languageCollector.JapaneseLines3_5; break;
            case "chinese": lines = languageCollector.ChineseLines3_5; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_5; break;
            default: lines = languageCollector.KoreanLines3_5; break;
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        LoadLinesForCurrentLanguage();
        StopAllCoroutines();
        index = 0;
        ShowCurrentDialogue();
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
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
                if (BGMSource != null && !BGMSource.isPlaying)
                    BGMSource.Play();
                break;

            case 7:
                if (BGMSource != null && BGMSource.isPlaying)
                    BGMSource.Stop(); // 7에서는 BGM 중지
                break;

            case 8:
                if (BreathSource != null && !BreathSource.isPlaying)
                    BreathSource.Play();
                break;

            case 9:
                if (BGMSource != null && BGMSource.isPlaying)
                    BGMSource.Stop();
                if (BreathSource != null && BreathSource.isPlaying)
                    BreathSource.Stop();
                break;

            case 10:
                if (BreathSource != null && !BreathSource.isPlaying)
                    BreathSource.Play();
                break;

            case 11:
            case 12:
                if (BirdSource != null && !BirdSource.isPlaying)
                {
                    BirdSource.loop = true;
                    BirdSource.Play();
                }

                if (index == 12 && FestivalSource != null && FestivalSource.isPlaying)
                {
                    FestivalSource.Stop(); // 12에서 Festival 멈춤
                }
                break;
        }

    }
}