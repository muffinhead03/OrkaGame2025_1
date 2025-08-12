using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManagerStage1_1 : MonoBehaviour
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

    [Header("배경 이미지")]
    public GameObject BlackImage;
    public Image Real_bg1_Image;
    public Sprite Real_bg1_Sprite;

    [Header("캐릭터 이미지")]
    public GameObject Real_echo_default;
    public GameObject Real_echo_2;
    public GameObject Real_echo_5;

    [Header("오디오")]
    public AudioSource bgmSource;       // BGM
    public AudioSource FestivalSound;   // 축제 사운드

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("대사 스크립트")]
    public LanguageCollector1_1 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // ====== 오디오 헬퍼들 ======
    void PlayBGM()
    {
        if (bgmSource == null) return;
        bgmSource.loop = true;
        // Pause 상태면 이어서, 아니면 새로 재생
        if (bgmSource.isPlaying) return;
        bgmSource.UnPause(); // Pause 상태면 이어서
        if (!bgmSource.isPlaying) bgmSource.Play(); // 완전 정지 상태면 Play
    }

    void PauseBGM()
    {
        if (bgmSource == null) return;
        if (bgmSource.isPlaying) bgmSource.Pause();
    }

    void StopBGM()
    {
        if (bgmSource == null) return;
        if (bgmSource.isPlaying) bgmSource.Stop();
    }

    void PlayFestival()
    {
        if (FestivalSound == null) return;
        if (!FestivalSound.isPlaying) FestivalSound.Play();
    }

    void StopFestival()
    {
        if (FestivalSound == null) return;
        if (FestivalSound.isPlaying) FestivalSound.Stop();
    }
    // ==========================

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNext);
            nextButton.gameObject.SetActive(false);
        }

        // 시작 시 모든 사운드 꺼두기(요구사항: 첫 대사엔 사운드 없음)
        StopBGM();
        StopFestival();

        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        SetupLanguageUI();
        LoadLinesForCurrentLanguage();

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLowerInvariant();
        switch (lang)
        {
            case "korean":   lines = languageCollector.KoreanLines1_1;   break;
            case "english":  lines = languageCollector.EnglishLines1_1;  break;
            case "japanese": lines = languageCollector.JapaneseLines1_1; break;
            case "chinese":  lines = languageCollector.ChineseLines1_1;  break;
            case "kazakh":   lines = languageCollector.KazaLines1_1;     break;
            default:
                Debug.LogWarning($"Unknown language: {lang}. Fallback to Korean.");
                lines = languageCollector.KoreanLines1_1;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        yield return UpdateVisuals(index);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);
    }

    private IEnumerator UpdateVisuals(int idx)
    {
        // ===== 공통 초기화: 화면 연출만 초기화(오디오는 각 케이스에서 명확히 제어) =====
        if (Real_bg1_Image != null)
        {
            Real_bg1_Image.gameObject.SetActive(false);
            Color c = Real_bg1_Image.color; c.a = 1f; Real_bg1_Image.color = c;
        }
        if (BlackImage != null) BlackImage.SetActive(false);

        if (Real_echo_default != null) Real_echo_default.SetActive(false);
        if (Real_echo_2 != null) Real_echo_2.SetActive(false);
        if (Real_echo_5 != null) Real_echo_5.SetActive(false);
        // ======================================================================

        switch (idx)
        {
            case 0:
                // 첫 대사: 사운드/연출 없음 (요구사항)
                StopFestival();
                StopBGM();
                if (BlackImage != null) BlackImage.SetActive(true);
                break;

            case 1:
                // 두 번째 대사: BGM 시작, 배경 서서히 등장
                if (BlackImage != null) BlackImage.SetActive(true);
                yield return new WaitForSeconds(2f);

                if (Real_bg1_Image != null)
                {
                    Real_bg1_Image.gameObject.SetActive(true);
                    Real_bg1_Image.sprite = Real_bg1_Sprite;
                    yield return StartCoroutine(FadeInImage(Real_bg1_Image, 1.5f));
                }
                if (Real_echo_default != null) Real_echo_default.SetActive(true);

                StopFestival();
                PlayBGM(); // ★ BGM 시작
                break;

            case 2:
                // 세 번째 대사: 축제 사운드 재생 + BGM 잠시 멈춤
                if (Real_bg1_Image != null)
                {
                    Real_bg1_Image.gameObject.SetActive(true);
                    Real_bg1_Image.sprite = Real_bg1_Sprite;
                    var c = Real_bg1_Image.color; c.a = 1f; Real_bg1_Image.color = c;
                }
                if (Real_echo_2 != null) Real_echo_2.SetActive(true);

                PlayFestival(); // ★ 축제 사운드 켜기
                PauseBGM();     // ★ BGM 잠시 멈춤
                break;

            case 3:
                // 네 번째 대사: 축제 사운드 정지 + BGM 다시 재생(이어듣기)
                if (Real_bg1_Image != null)
                {
                    Real_bg1_Image.gameObject.SetActive(true);
                    Real_bg1_Image.sprite = Real_bg1_Sprite;
                    var c = Real_bg1_Image.color; c.a = 1f; Real_bg1_Image.color = c;
                }
                if (Real_echo_5 != null) Real_echo_5.SetActive(true);

                StopFestival();
                PlayBGM(); // ★ BGM 재개(일시정지 상태면 UnPause, 아니면 Play)
                break;
        }
    }

    private IEnumerator FadeInImage(Image img, float duration)
    {
        Color c = img.color; c.a = 0f; img.color = c;
        img.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / duration);
            img.color = c;
            yield return null;
        }
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText == null) yield break;

        storyText.text = "";
        foreach (char ch in fullText)
        {
            storyText.text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);

        index++;
        if (lines == null || index >= lines.Length)
        {
            SceneManager.LoadScene("Stage1_2");
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

        string lang = LanguageManager.GetLanguage()?.Trim().ToLowerInvariant();
        RectTransform above = Korean_Above, story = Korean_Story;

        switch (lang)
        {
            case "english":  above = English_Above;  story = English_Story;  break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese":  above = Chinese_Above;  story = Chinese_Story;  break;
            case "kazakh":   above = Kaza_Above;     story = Kaza_Story;     break;
            // default: Korean
        }

        if (above && story)
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
