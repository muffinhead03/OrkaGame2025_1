using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManagerStage2_2 : MonoBehaviour
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
    public TextMeshProUGUI aboveText;    // �̸� ���̺�
    public TextMeshProUGUI storyText;    // ��� �ؽ�Ʈ
    public Button nextButton;            // Next ��ư

    [Header("Ÿ���� �ӵ�")]
    public float typingSpeed = 0.04f;

    [Header("�����")]
    public AudioSource bgmSource;

    [Header("ǥ�� ������Ʈ")]
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_defaultObj;
    public GameObject Eco_surprisedObj;
    public GameObject Eco_readyObj;
    public GameObject Eco_smiledObj;
    public GameObject Narke_2Obj;
    public GameObject Narke_defaultObj;

    [Header("��� �̹���")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("��� ��ũ��Ʈ")]
    public LanguageCollector2_2 languageCollector;

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
        // 1) UI ��� ������Ʈ ����
        SetupLanguageUI();

        // (���� ���̺� ���� ����)

        // 3) ��� & BGM
        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 4) ��� �迭 �ε�
        LoadLinesForCurrentLanguage();

        // 5) �ε��� �ʱ�ȭ �� ù ��� ������ ����
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines2_2; break;
            case "english": lines = languageCollector.EnglishLines2_2; break;
            case "japanese": lines = languageCollector.JapaneseLines2_2; break;
            case "chinese": lines = languageCollector.ChineseLines2_2; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines2_2; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines2_2;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
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
        // ��� ǥ�� ��Ȱ��ȭ
        Eco_eyeclosedObj?.SetActive(false);
        Eco_defaultObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_smiledObj?.SetActive(false);
        Narke_2Obj?.SetActive(false);
        Narke_defaultObj?.SetActive(false);

        // �ʿ��� ǥ���� Ȱ��ȭ
        switch (idx)
        {
            case 0:
            case 7:
            case 12:
            case 15:
            case 17:
            case 20:
            case 23:
            case 29:
                Narke_2Obj.SetActive(true);
                break;
            case 1:
            case 11:
            case 14:
            case 25:
            case 28:
                Eco_defaultObj.SetActive(true);
                break;
            case 2:
            case 4:
            case 6:
            case 9:
            case 10:
            case 13:
            case 16:
            case 19:
            case 27:
                Narke_defaultObj.SetActive(true);
                break;
            case 3:
            case 18:
            case 30:
                Eco_smiledObj.SetActive(true);
                break;
            case 5:
            case 21:
            case 22:
                Eco_surprisedObj.SetActive(true);
                break;
            case 8:
            case 26:
                Eco_readyObj.SetActive(true);
                break;
            case 24:
                Eco_eyeclosedObj.SetActive(true);
                break;
            default:
                Eco_defaultObj.SetActive(true);
                break;
        }

        // Ȱ��ȭ�� ǥ���� ���� ���̺� ����
        if (aboveText != null)
        {
            if (Narke_2Obj.activeSelf || Narke_defaultObj.activeSelf)
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

    private void OnNext()
    {
        nextButton?.gameObject.SetActive(false);

        index++;
        if (index >= lines.Length)
        {
            SceneManager.LoadScene("CardGameFirstStage");
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
    private void Update()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

       
    }

}