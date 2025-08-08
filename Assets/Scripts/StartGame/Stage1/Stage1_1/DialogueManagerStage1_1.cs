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
    public AudioSource bgmSource;
    public AudioSource FestivalSound;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("대사 스크립트")]
    public LanguageCollector1_1 languageCollector;

    private string[] lines;
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

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines1_1; break;
            case "english": lines = languageCollector.EnglishLines1_1; break;
            case "japanese": lines = languageCollector.JapaneseLines1_1; break;
            case "chinese": lines = languageCollector.ChineseLines1_1; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines1_1; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
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
        // 배경 초기화 - 둘 다 끔
        if (BlackImage != null) BlackImage.SetActive(false);
        if (Real_bg1_Image != null)
        {
            Real_bg1_Image.gameObject.SetActive(false);
            // 투명도 1로 초기화 (페이드인 시작 전)
            Color c = Real_bg1_Image.color;
            c.a = 1f;
            Real_bg1_Image.color = c;
        }

        // 캐릭터 초기화
        if (Real_echo_default != null) Real_echo_default.SetActive(false);
        if (Real_echo_2 != null) Real_echo_2.SetActive(false);
        if (Real_echo_5 != null) Real_echo_5.SetActive(false);

        // 사운드 초기화
        if (bgmSource != null)
        {
            if (idx < 2 && bgmSource.isPlaying) bgmSource.Stop();
        }
        if (FestivalSound != null)
        {
            if (idx != 2 && FestivalSound.isPlaying) FestivalSound.Stop();
        }

        switch (idx)
        {
            case 0:
                // BlackImage 켜고 Real_bg1 꺼짐
                if (BlackImage != null) BlackImage.SetActive(true);
                break;

            case 1:
                // BlackImage 켠 상태에서 2초 대기
                if (BlackImage != null) BlackImage.SetActive(true);

                // 2초 대기
                yield return new WaitForSeconds(2f);

               

                if (Real_bg1_Image != null)
                {
                    Real_bg1_Image.gameObject.SetActive(true);
                    Real_bg1_Image.sprite = Real_bg1_Sprite;

                    // 페이드인 코루틴 실행
                    yield return StartCoroutine(FadeInImage(Real_bg1_Image, 1.5f));
                }

                // 캐릭터 등장
                if (Real_echo_default != null) Real_echo_default.SetActive(true);
                break;

            case 2:
                if (Real_bg1_Image != null)
                {
                    Real_bg1_Image.gameObject.SetActive(true);
                    Real_bg1_Image.sprite = Real_bg1_Sprite;

                    Color c = Real_bg1_Image.color;
                    c.a = 1f;
                    Real_bg1_Image.color = c;
                }
                if (Real_echo_2 != null) Real_echo_2.SetActive(true);
                if (bgmSource != null && !bgmSource.isPlaying)
                {
                    bgmSource.loop = true;
                    bgmSource.Play();
                }
                if (FestivalSound != null && !FestivalSound.isPlaying)
                {
                    FestivalSound.Play();
                }
                break;

            case 3:
                if (Real_bg1_Image != null)
                {
                    Real_bg1_Image.gameObject.SetActive(true);
                    Real_bg1_Image.sprite = Real_bg1_Sprite;

                    Color c = Real_bg1_Image.color;
                    c.a = 1f;
                    Real_bg1_Image.color = c;
                }
                if (Real_echo_5 != null) Real_echo_5.SetActive(true);
                if (FestivalSound != null && FestivalSound.isPlaying)
                {
                    FestivalSound.Stop();
                }
                if (bgmSource != null && !bgmSource.isPlaying)
                {
                    bgmSource.loop = true;
                    bgmSource.Play();
                }
                break;
        }

        yield break;
    }

    private IEnumerator FadeInImage(Image img, float duration)
    {
        Color c = img.color;
        c.a = 0f;
        img.color = c;

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