using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager3_4_backup
    : MonoBehaviour
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
    public Image fadeImage;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("배경 이미지")]
    public Image Arcadia_bg;
    public Image arcadia_red;
    public Image real_bg;
    public GameObject blackImageObj;

    [Header("캐릭터")]
    public GameObject Pan_defaultObj;
    public GameObject Pan_3Obj;
    public GameObject Eco_readyObj;
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_smiledObj;
    public GameObject Eco_tearObj;

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource glitchSound;
    public AudioSource footSound;
    public AudioSource fluteSound;

    [Header("대사 스크립트")]
    public LanguageCollector3_4 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

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
        LoadLinesForCurrentLanguage();

        if (blackImageObj != null)
            blackImageObj.SetActive(false);

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_4; break;
            case "english": lines = languageCollector.EnglishLines3_4; break;
            case "japanese": lines = languageCollector.JapaneseLines3_4; break;
            case "chinese": lines = languageCollector.ChineseLines3_4; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_4; break;
            default: lines = languageCollector.KoreanLines3_4; break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        UpdateVisuals(index);
        UpdateAboveText(index);

        if (index == 2 && bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);

        switch (index)
        {
            case 0:
                glitchSound?.Play();
                break;

            case 1:
                footSound?.Play();
                break;

            case 8:
                fluteSound?.Play();

                if (blackImageObj != null && fadeImage != null)
                    StartCoroutine(FadeToBlack(4f));
                break;
        }
    }

    private void UpdateVisuals(int idx)
    {
        Arcadia_bg.enabled = (idx == 0 || idx == 1);
        arcadia_red.enabled = (idx >= 2 && idx <= 7);
        real_bg.enabled = (idx == 8);

        Pan_defaultObj?.SetActive(false);
        Pan_3Obj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_eyeclosedObj?.SetActive(false);
        Eco_smiledObj?.SetActive(false);
        Eco_tearObj?.SetActive(false);

        switch (idx)
        {
            case 0: Pan_defaultObj?.SetActive(true); break;
            case 1:
            case 3:
            case 5:
            case 6: Eco_readyObj?.SetActive(true); break;
            case 2: Pan_3Obj?.SetActive(true); break;
            case 4: Eco_eyeclosedObj?.SetActive(true); break;
            case 7: Eco_smiledObj?.SetActive(true); break;
            case 8: Eco_tearObj?.SetActive(true); break;
        }
    }

    private void UpdateAboveText(int idx)
    {
        if (aboveText == null) return;
        if (idx == 0 || idx == 2) aboveText.text = "판";
        else aboveText.text = "에코";
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

    private IEnumerator FadeToBlack(float duration)
    {
        if (blackImageObj != null)
            blackImageObj.SetActive(true);

        Color color = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 1f);
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length)
            return;
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

        above?.gameObject.SetActive(true);
        story?.gameObject.SetActive(true);
        above.anchoredPosition = AboPo;
        story.anchoredPosition = StoPo;
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