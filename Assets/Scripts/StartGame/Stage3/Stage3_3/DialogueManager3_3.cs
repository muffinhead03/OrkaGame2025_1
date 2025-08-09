using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


public class DialogueManager3_3 : MonoBehaviour
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
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;
    public VideoPlayer endingVideoPlayer;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("배경 및 캐릭터")]
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

    [Header("대사 스크립트")]
    public LanguageCollector3_3 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 점프 애니메이션 관련
    private bool isJumpScaling = false;
    private float jumpTimer = 0f;
    private float jumpDuration = 0.3f;
    private Vector3 pan2TargetScale = Vector3.one;

    private void Awake()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNext);
        nextButton.gameObject.SetActive(false);

        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        SetupLanguageUI();

        if (aboveText != null)
            aboveText.text = "판";

        

        LoadLinesForCurrentLanguage();

        index = 0;
        StartCoroutine(ShowLineSequence());       
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_3; break;
            case "english": lines = languageCollector.EnglishLines3_3; break;
            case "japanese": lines = languageCollector.JapaneseLines3_3; break;
            case "chinese": lines = languageCollector.ChineseLines3_3; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_3; break;
            default: lines = languageCollector.KoreanLines3_3; break;
        }
    }

    private IEnumerator JumpScaleInOut(GameObject targetObj, float duration = 0.5f)
    {
        if (targetObj == null) yield break;

        targetObj.SetActive(true);
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        // 스케일 업 (등장)
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scaleT = t / duration;
            targetObj.transform.localScale = Vector3.Lerp(startScale, endScale, scaleT);
            yield return null;
        }

        // 잠시 유지 (선택 사항, 생략 가능)
        // yield return new WaitForSeconds(0.1f);

        // 스케일 다운 (사라짐)
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



    private IEnumerator ShowLineSequence()
    {
        UpdateVisuals(index);

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);

        if ((index == 1 || index == lines.Length - 1) && Pan_2Obj != null)
        {
            yield return StartCoroutine(JumpScaleInOut(Pan_2Obj, 0.5f));

            if (index == 1)
            {
                yield return new WaitForSeconds(0.5f); // 애니메이션 후 잠깐 대기
                PlayEndingVideo();
            }
        }


    }



    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("비디오 재생이 종료되었습니다.");

        // 예: 비디오 재생이 끝난 후 씬 전환 등 추가 작업이 필요하면 여기서 처리
        if (blackCoverPanel != null) blackCoverPanel.SetActive(false);

        // 씬 전환
        SceneManager.LoadScene("Stage3_2");
    }

    private void UpdateVisuals(int idx)
    {
        if (backgroundImage != null) backgroundImage.enabled = (idx == 0);
        if (blackImageObj != null) blackImageObj.SetActive(idx == 1);

        Pan_4eyeclosedObj?.SetActive(false);
        Pan_2Obj?.SetActive(false);
        if (idx == 0) Pan_4eyeclosedObj?.SetActive(true);

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
            Debug.LogWarning("endingVideoPlayer가 설정되어 있지 않습니다!");
            return;
        }

        // 검은색 커버 패널 활성화
        if (blackCoverPanel != null) blackCoverPanel.SetActive(true);

        // 비디오 재생 전에 이미지들 비활성화
        if (backgroundImage != null) backgroundImage.enabled = false;
        if (blackImageObj != null) blackImageObj.SetActive(false);
        if (Pan_4eyeclosedObj != null) Pan_4eyeclosedObj.SetActive(false);
        if (Pan_2Obj != null) Pan_2Obj.SetActive(false);

        // 새로 추가한 오브젝트 비활성화
        if (CarrotButton != null) CarrotButton.SetActive(false);
        if (DialogueImage != null) DialogueImage.SetActive(false);
        if (FirstPanel != null) FirstPanel.SetActive(false);
        if (SettingPanel != null) SettingPanel.SetActive(false);


        endingVideoPlayer.gameObject.SetActive(true);
        endingVideoPlayer.Play();

        // 비디오 종료 시 호출될 이벤트 등록
        endingVideoPlayer.loopPointReached += OnVideoFinished;
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
            above.anchoredPosition = AboPo;
            story.anchoredPosition = StoPo;
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        LoadLinesForCurrentLanguage();
        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
