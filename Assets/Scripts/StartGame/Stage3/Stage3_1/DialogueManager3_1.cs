using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager3_1 : MonoBehaviour
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

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource bgmSource;

    [Header("표정 오브젝트")]
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_defaultObj;
    public GameObject Eco_readyObj;
    public GameObject Eco_smiledObj;
    public GameObject Eco_surprisedObj;
    public GameObject Pan_defaultObj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 스크립트")]
    public LanguageCollector3_1 languageCollector;

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

    private IEnumerator Start()
    {
        SetupLanguageUI();

        if (aboveText != null)
            aboveText.text = "에코";

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        LoadLinesForCurrentLanguage();
        index = 0;
        yield return StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_1; break;
            case "english": lines = languageCollector.EnglishLines3_1; break;
            case "japanese": lines = languageCollector.JapaneseLines3_1; break;
            case "chinese": lines = languageCollector.ChineseLines3_1; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_1; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines3_1;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        UpdateCharacterFace(index);
        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);
    }

    private void UpdateCharacterFace(int idx)
    {
        Eco_eyeclosedObj?.SetActive(false);
        Eco_defaultObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_smiledObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Pan_defaultObj?.SetActive(false);

        // 기본값: 에코
        string characterName = "에코";

        switch (idx)
        {
            case 0:
                Eco_surprisedObj?.SetActive(true);
                break;
            case 1:
            case 7:
            case 8:
            case 9:
            case 11:
            case 17:
            case 19:
                Pan_defaultObj?.SetActive(true);
                characterName = "판"; // Pan 이미지일 때는 "판"
                break;
            case 2:
            case 3:
            case 6:
            case 12:
            case 13:
            case 15:
                Eco_readyObj?.SetActive(true);
                break;
            case 4:
            case 18:
                Eco_defaultObj?.SetActive(true);
                break;
            case 5:
            case 10:
            case 16:
                Eco_eyeclosedObj?.SetActive(true);
                break;
            case 14:
                Eco_smiledObj?.SetActive(true);
                break;
        }

        if (aboveText != null)
            aboveText.text = characterName;
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
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length)
        {
            SceneManager.LoadScene("Stage3_2");
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
