using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager2_4 : MonoBehaviour
{
    [Header("��� ������Ʈ")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("�⺻ ��ġ��")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    
    [Header("UI ���")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Image backgroundImage;
    public Sprite backGroundSprite;
    public GameObject Narke_2Obj;
    public Image endingImage;

    [Header("Ÿ���� �ӵ�")]
    public float typingSpeed = 0.04f;

    [Header("�����")]
    public AudioSource fluteSource;
    public AudioSource breathSource;

    [Header("��� ��ũ��Ʈ")]
    public LanguageCollector2_4 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    
    public GameObject nextButton;

    private void Awake()
    {
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        if (nextButton != null)
            nextButton.SetActive(false);

        SetupLanguageUI();

        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        if (Narke_2Obj != null)
            Narke_2Obj.SetActive(true);

        LoadLinesForCurrentLanguage();
        index = 0;

        // 1. ��� ��� ����
        StartCoroutine(ShowLineSequence());

        // 2. �Ҹ� ���� ��� & �����̹��� ���̵���
        StartCoroutine(PlaySoundSequenceAndFade());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines2_4; break;
            case "english": lines = languageCollector.EnglishLines2_4; break;
            case "japanese": lines = languageCollector.JapaneseLines2_4; break;
            case "chinese": lines = languageCollector.ChineseLines2_4; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines2_4; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', defaulting to Korean.");
                lines = languageCollector.KoreanLines2_4;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        if (aboveText != null) aboveText.text = "나르케";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;
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

        // 타이핑 끝났으면 버튼 켜주기
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
    }


    private IEnumerator PlaySoundSequenceAndFade()
    {
        // fluteSound ���
        if (fluteSource != null)
        {
            fluteSource.Play();
            yield return new WaitWhile(() => fluteSource.isPlaying);
        }

        // flute ���� �� �� breath ���
        if (breathSource != null)
        {
            breathSource.Play();
        }

        // breath ���� �� 2�� ��ٸ��� ���� �̹��� ���̵���
        yield return new WaitForSeconds(2f);
        StartCoroutine(FadeInEndingImage());
    }

    private IEnumerator FadeInEndingImage()
    {
        if (endingImage == null) yield break;

        Color color = endingImage.color;
        color.a = 0;
        endingImage.color = color;
        endingImage.gameObject.SetActive(true);

        float fadeDuration = 2.0f;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            endingImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        // index 정보 저장
        PlayerPrefs.SetInt("StartFromIndex", 4); // Stage2_3_1에서 index 4부터 시작하게

        // Stage2_3_1로 이동
        SceneManager.LoadScene("Stage2_3_1");
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
        Start();
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    public void OnNextClicked()
    {
        Debug.Log("Next 버튼 눌림");

        nextButton.gameObject.SetActive(false);

        index++;
        if (index < lines.Length)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(lines[index]));
        }
        else
        {
            Debug.Log("모든 대사 종료 → 엔딩 처리 시작");
            StartCoroutine(PlaySoundSequenceAndFade());
        }
    }


}
