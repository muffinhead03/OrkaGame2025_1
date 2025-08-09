using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DialogueManager2_3_1 : MonoBehaviour
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
    public AudioSource glitchSound;

    [Header("표정 오브젝트")]
    public GameObject Eco_readyObj;
    public GameObject Narke_2Obj;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 스크립트")]
    public LanguageCollector2_3_1 languageCollector;

    [Header("언어별로 나타날 오브젝트들")]
    public GameObject[] koreanObjects;
    public GameObject[] englishObjects;
    public GameObject[] japaneseObjects;
    public GameObject[] chineseObjects;
    public GameObject[] kazaObjects;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;
    private Coroutine glitchCoroutine;

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
            Debug.LogError("[DialogueManager2_3_1] 대사가 없습니다.");
            return;
        }

        if (PlayerPrefs.HasKey("StartFromIndex"))
        {
            index = PlayerPrefs.GetInt("StartFromIndex");
            PlayerPrefs.DeleteKey("StartFromIndex"); // 다음 진입부터는 초기화
        }
        else
        {
            index = 0;
        }

        StartCoroutine(ShowLineSequence());
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines2_3_1; break;
            case "english": lines = languageCollector.EnglishLines2_3_1; break;
            case "japanese": lines = languageCollector.JapaneseLines2_3_1; break;
            case "chinese": lines = languageCollector.ChineseLines2_3_1; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines2_3_1; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines2_3_1;
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
        DisableAllLanguageObjects();
        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;


        nextButton?.gameObject.SetActive(true);

        if (index == 4)
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
                    if (parentObj != null && !parentObj.name.ToLower().Trim().StartsWith("ss"))
                    {
                       
                        parentObj.SetActive(true);

                        
                        Button[] buttons = parentObj.GetComponentsInChildren<Button>(true);  
                        foreach (var btn in buttons)
                        {
                            string btnName = btn.gameObject.name.ToLower().Trim();
                            Debug.Log($"[자식 버튼 찾음] {btnName}");

                            if (btnName.StartsWith("ss"))
                            {
                               
                                if (btnName.Contains("4_1"))
                                {
                                    btn.onClick.RemoveAllListeners();  // 중복 방지
                                    btn.onClick.AddListener(() => LoadScene("Stage2_4"));
                                }
                                else if (btnName.Contains("4_2"))
                                {
                                    btn.onClick.RemoveAllListeners();
                                    btn.onClick.AddListener(() => LoadScene("Stage2_5"));
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    private void UpdateCharacterFace(int idx)
    {
        Eco_readyObj?.SetActive(false);
        Narke_2Obj?.SetActive(false);

        

        switch (idx)
        {
            case 0:
            case 2:
                Eco_readyObj?.SetActive(true);
                break;
            case 1:
            case 3:
            case 4:
                Narke_2Obj?.SetActive(true);
                break;
        }

        if (aboveText != null)
        {
            if (Narke_2Obj != null && Narke_2Obj.activeSelf)
                aboveText.text = "나르케";
            else if (Eco_readyObj != null && Eco_readyObj.activeSelf)
                aboveText.text = "에코";
            else
                aboveText.text = "";
        }
    }

    private IEnumerator GlitchLoop()
    {
        while (true)
        {
            glitchSound?.Play();
            yield return new WaitForSeconds(1.5f);
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
        if (index >= 0 && index <= 2)
        {
            if (glitchCoroutine == null)
            {
                glitchCoroutine = StartCoroutine(GlitchLoop());
            }
        }
        else
        {
            if (glitchCoroutine != null)
            {
                StopCoroutine(glitchCoroutine);
                glitchCoroutine = null;
            }
            if (glitchSound != null && glitchSound.isPlaying)
            {
                glitchSound.Stop();
            }
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
