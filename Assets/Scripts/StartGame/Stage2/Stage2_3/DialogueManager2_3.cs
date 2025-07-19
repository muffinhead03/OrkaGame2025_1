using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager2_3 : MonoBehaviour
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

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource ScreamingSound;

    [Header("표정 오브젝트")]
    public GameObject Narke_2Obj;
    public GameObject Narke_defaultObj;
    public GameObject Eco_surprisedObj;
    public GameObject Eco_smiledObj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 스크립트")]
    public LanguageCollector2_3 languageCollector;

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

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        LoadLinesForCurrentLanguage();

        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("[DialogueManager2_3] 대사가 없습니다.");
            return;
        }

        index = 0;
        StartCoroutine(ShowLineSequence());

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines2_3; break;
            case "english": lines = languageCollector.EnglishLines2_3; break;
            case "japanese": lines = languageCollector.JapaneseLines2_3; break;
            case "chinese": lines = languageCollector.ChineseLines2_3; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines2_3; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines2_3;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        if (lines == null || index >= lines.Length)
        {
            Debug.LogError("[ShowLineSequence] 유효하지 않은 대사 인덱스.");
            yield break;
        }

        UpdateCharacterFace(index);
        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        if (index == lines.Length - 1)
        {
            Debug.Log(">> 마지막 대사 - 페이드아웃 및 씬 전환 시작");
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(FadeOutAndLoadScene("CardgameSecondStage"));
        }
        else
        {
            nextButton?.gameObject.SetActive(true);
            Debug.Log("[ShowLineSequence] nextButton 활성화됨");
        }
    }

    private void UpdateCharacterFace(int idx)
    {
        if (Narke_2Obj) Narke_2Obj.SetActive(false);
        if (Narke_defaultObj) Narke_defaultObj.SetActive(false);
        if (Eco_surprisedObj) Eco_surprisedObj.SetActive(false);
        if (Eco_smiledObj) Eco_smiledObj.SetActive(false);

        switch (idx)
        {
            case 0:
                if (Eco_smiledObj) Eco_smiledObj.SetActive(true);
                ScreamingSound?.Play();
                if (bgmSource != null && !bgmSource.isPlaying)
                {
                    bgmSource.loop = true;
                    bgmSource.Play();
                }
                break;
            case 1:
                if (Eco_surprisedObj) Eco_surprisedObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying)
                    bgmSource.Stop();
                break;
            case 2:
                if (Narke_defaultObj) Narke_defaultObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying)
                    bgmSource.Stop();
                break;
            case 3:
                if (Narke_2Obj) Narke_2Obj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying)
                    bgmSource.Stop();
                break;
            case 4:
                if (Eco_smiledObj) Eco_smiledObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying)
                    bgmSource.Stop();
                break;
            default:
                if (Eco_smiledObj) Eco_smiledObj.SetActive(true);
                if (bgmSource != null && bgmSource.isPlaying)
                    bgmSource.Stop();
                break;
        }

        if (aboveText != null)
        {
            if ((Narke_2Obj && Narke_2Obj.activeSelf) || (Narke_defaultObj && Narke_defaultObj.activeSelf))
                aboveText.text = "나르케";
            else
                aboveText.text = "에코";
        }
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

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
            yield break;

        Debug.Log(">> 페이드 투 블랙 시작됨");

        Color startColor = Color.black;
        startColor.a = 0f;
        fadeImage.color = startColor;

        float duration = 1f;
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
        Debug.Log(">> 페이드 투 블랙 완료됨");
    }

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length)
        {
            StartCoroutine(FadeOutAndLoadScene("CardgameSecondStage"));
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

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);

            float duration = 1f;
            float elapsed = 0f;
            Color c = fadeImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
                fadeImage.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeImage.color = c;
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneName);
    }
}
