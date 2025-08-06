using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DialogueManager3_2 : MonoBehaviour
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
    public AudioSource kwangSound;

    [Header("표정 오브젝트")]
    public GameObject Eco_smiledObj;
    public GameObject Eco_eyeclosedObj;
    public GameObject Eco_readyObj;
    public GameObject Eco_surprisedObj;

    public GameObject Pan_defaultObj;
    public GameObject Pan_4eyeclosedObj;

    public GameObject Narke_defaultObj;
    public GameObject Narke_2Obj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 스크립트")]
    public LanguageCollector3_2 languageCollector;

    [Header("언어별로 나타날 오브젝트들")]
    public GameObject[] koreanObjects;
    public GameObject[] englishObjects;
    public GameObject[] japaneseObjects;
    public GameObject[] chineseObjects;
    public GameObject[] kazaObjects;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;
    private bool kwangPlayed = false;

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
            Debug.LogError("[DialogueManager3_2] 대사가 없습니다.");
            return;
        }

        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_2; break;
            case "english": lines = languageCollector.EnglishLines3_2; break;
            case "japanese": lines = languageCollector.JapaneseLines3_2; break;
            case "chinese": lines = languageCollector.ChineseLines3_2; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_2; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines3_2;
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

        if (index == 1 && !kwangPlayed)
        {
            kwangSound?.Play();
            kwangPlayed = true;

            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.Stop();
        }
        else if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        UpdateCharacterFace(index);
        DisableAllLanguageObjects();

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);

        if (index == lines.Length - 1)
        {
            string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
            GameObject[] targetObjects = null;

            switch (lang)
            {
                case "korean": targetObjects = koreanObjects; break;
                case "english": targetObjects = englishObjects; break;
                case "japanese": targetObjects = japaneseObjects; break;
                case "chinese": targetObjects = chineseObjects; break;
                case "kazahustan":
                case "kaza": targetObjects = kazaObjects; break;
            }

            if (targetObjects != null)
            {
                foreach (var parentObj in targetObjects)
                {
                    if (parentObj != null && !parentObj.name.ToLower().Trim().StartsWith("2"))
                        parentObj.SetActive(true);

                    Button[] buttons = parentObj.GetComponentsInChildren<Button>(true);
                    foreach (var btn in buttons)
                    {
                        string btnName = btn.gameObject.name.ToLower().Trim();
                        Debug.Log($"[선택지 버튼 찾음] {btnName}");

                        if (btnName.StartsWith("ss"))
                        {
                            if (btnName.Contains("2_1"))
                            {
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => LoadScene("Stage3_3"));
                            }
                            else if (btnName.Contains("2_2"))
                            {
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => LoadScene("Stage3_4"));
                            }
                        }
                    }
                }
            }
        }
    } // ← 여기 중괄호 추가됨

    private void UpdateCharacterFace(int idx)
    {
        Eco_smiledObj?.SetActive(false);
        Eco_eyeclosedObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Pan_defaultObj?.SetActive(false);
        Pan_4eyeclosedObj?.SetActive(false);
        Narke_defaultObj?.SetActive(false);
        Narke_2Obj?.SetActive(false);

        switch (idx)
        {
            case 0: Eco_smiledObj?.SetActive(true); break;
            case 1:
            case 3:
            case 4:
            case 6:
            case 8:
            case 10:
            case 15: Pan_defaultObj?.SetActive(true); break;
            case 5: Pan_4eyeclosedObj?.SetActive(true);break;
            case 2:
            case 9: Eco_eyeclosedObj?.SetActive(true); break;
            case 7: Eco_readyObj?.SetActive(true); break;
            case 11:
            case 12: Narke_defaultObj?.SetActive(true); break;
            case 13: Narke_2Obj?.SetActive(true); break;
            case 14: Eco_surprisedObj?.SetActive(true); break;
        }

        if (aboveText != null)
        {
            if ((Narke_defaultObj != null && Narke_defaultObj.activeSelf) ||
                (Narke_2Obj != null && Narke_2Obj.activeSelf))
            {
                aboveText.text = "나르케";
            }
            else if ((Eco_smiledObj != null && Eco_smiledObj.activeSelf) ||
                     (Eco_readyObj != null && Eco_readyObj.activeSelf) ||
                     (Eco_eyeclosedObj != null && Eco_eyeclosedObj.activeSelf) ||
                     (Eco_surprisedObj != null && Eco_surprisedObj.activeSelf))
            {
                aboveText.text = "에코";
            }
            else if ((Pan_defaultObj != null && Pan_defaultObj.activeSelf) ||
                     (Pan_4eyeclosedObj != null && Pan_4eyeclosedObj.activeSelf)) // 여기 추가됨
            {
                aboveText.text = "판";
            }
            else
            {
                aboveText.text = "";
            }
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

        UpdateLanguageSpecificObjects(newLang);
    }

    private void UpdateLanguageSpecificObjects(string lang)
    {
        DisableAllLanguageObjects();

        GameObject[] target = null;
        switch (lang.Trim().ToLower())
        {
            case "korean": target = koreanObjects; break;
            case "english": target = englishObjects; break;
            case "japanese": target = japaneseObjects; break;
            case "chinese": target = chineseObjects; break;
            case "kaza":
            case "kazahustan": target = kazaObjects; break;
        }

        if (target != null)
        {
            foreach (var obj in target)
                if (obj != null) obj.SetActive(true);
        }
    }


    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Update()
    {
        if (bgmSource != null && !bgmSource.isPlaying && index != 1)
        {
            bgmSource.Play();
        }
    }

    private void DisableAllLanguageObjects()
    {
        foreach (var obj in koreanObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in englishObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in japaneseObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in chineseObjects) if (obj != null) obj.SetActive(false);
        foreach (var obj in kazaObjects) if (obj != null) obj.SetActive(false);
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
