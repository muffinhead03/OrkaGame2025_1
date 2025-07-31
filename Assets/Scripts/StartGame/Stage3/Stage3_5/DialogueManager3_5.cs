using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager3_5 : MonoBehaviour
{
    [Header("언어 오브젝트")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("UI 요소")]
    public TextMeshProUGUI aboveText;
    public TextMeshProUGUI storyText;
    public Button nextButton;

    [Header("배경 이미지 오브젝트")]
    public GameObject BackGroundObj;
    public GameObject BlackImageObj;
    public GameObject SunBackGroundObj;
    public GameObject WhiteImageObj;

    [Header("캐릭터 이미지 오브젝트")]
    public GameObject Eco_real6Obj, Eco_real_defaultObj, Eco_real5Obj, Eco_real2Obj;
    public GameObject Eco_real3Obj, Eco_laughObj, Eco_banjunObj, Eco_real4Obj;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("사운드")]
    public AudioSource BGMSource, BreathSource, BirdSource, FestivalSource;
    public AudioSource DoorGoriSource, DoorOpenSource, FluteSource, FallSoundSource;

    [Header("페이드 이미지")]
    public Image fadeImage;

    [Header("대사")]
    public LanguageCollector3_5 languageCollector;
    private string[] lines;
    private int index = 0;
    private Coroutine typingCoroutine;
    private GameObject[] characterObjs;
    private GameObject[] backgroundObjs;

    private void Awake()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnClickNext);
        nextButton.gameObject.SetActive(false);

        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        characterObjs = new[] {
            Eco_real6Obj, Eco_real_defaultObj, Eco_real5Obj, Eco_real2Obj,
            Eco_real3Obj, Eco_laughObj, Eco_banjunObj, Eco_real4Obj
        };

        backgroundObjs = new[] {
            BackGroundObj, BlackImageObj, SunBackGroundObj, WhiteImageObj
        };

        SetupLanguageUI();
        LoadLinesForCurrentLanguage();
        index = 0;
        ShowCurrentDialogue();
    }

    private void ShowCurrentDialogue()
    {
        // 일회성 오디오 정지
        FestivalSource?.Stop();
        DoorGoriSource?.Stop();
        DoorOpenSource?.Stop();
        FluteSource?.Stop();
        FallSoundSource?.Stop();

        ApplyBackground(index);
        ApplyCharacter(index);

        // 대사 시작 시점에서 재생되는 사운드
        if (index == 3 && DoorGoriSource != null)
            DoorGoriSource.Play();

        if (index == 9 && FluteSource != null)
        {
            FluteSource.Play(); //  즉시 재생
            StartCoroutine(PlayFluteThenFall()); // Flute 끝나고 Fall 재생
        }

        if (index == 10 && FestivalSource != null)
        {
            FestivalSource.loop = false;
            FestivalSource.Play();
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
    }

    private void ApplyBackground(int idx)
    {
        foreach (var bg in backgroundObjs)
            if (bg != null) bg.SetActive(false);
        BlackImageObj?.SetActive(false);
        SunBackGroundObj?.SetActive(false);
        WhiteImageObj?.SetActive(false);
        BackGroundObj?.SetActive(false);

        if (idx == 7)
            BlackImageObj?.SetActive(true);
        else if (idx == 9 || idx == 10)
        {
            SunBackGroundObj?.SetActive(true);
        }
        else if (idx == 11)
        {
            // WhiteImageObj는 페이드인에서 따로 활성화되므로 여기선 그대로 둠
        }
        else
        {
            BackGroundObj?.SetActive(true);
        }
    }

    private void ApplyCharacter(int idx)
    {
        foreach (var ch in characterObjs)
            if (ch != null) ch.SetActive(false);

        switch (idx)
        {
            case 0:
            case 2:
            case 9: Eco_real6Obj?.SetActive(true); break;
            case 1: Eco_real_defaultObj?.SetActive(true); break;
            case 3: Eco_real5Obj?.SetActive(true); break;
            case 4: Eco_real2Obj?.SetActive(true); break;
            case 5: Eco_real3Obj?.SetActive(true); break;
            case 6: Eco_laughObj?.SetActive(true); break;
            case 7: Eco_banjunObj?.SetActive(true); break;
            case 8: Eco_real4Obj?.SetActive(true); break;
        }
    }

    private IEnumerator TypeText(string fullText)
    {
        storyText.text = "";
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        //  대사 종료 후 재생되는 오디오
        if (index == 1 && FestivalSource != null)
        {
            FestivalSource.loop = false;
            FestivalSource.Play();
        }
        else if (index == 6 && DoorOpenSource != null)
        {
            DoorOpenSource.Play();
        }
        else if (index == 11)
        {
            StartCoroutine(FadeToImage(WhiteImageObj, 3f));
        }

        nextButton.gameObject.SetActive(true);
    }

    public void OnClickNext()
    {
        nextButton.gameObject.SetActive(false);
        index++;
        if (index >= lines.Length) return;
        ShowCurrentDialogue();
    }

    private IEnumerator PlayFluteThenFall()
    {
        if (FluteSource != null)
        {
            FluteSource.Play();
            yield return new WaitWhile(() => FluteSource.isPlaying);
        }

        if (FallSoundSource != null)
        {
            FallSoundSource.Play();
            yield return new WaitWhile(() => FallSoundSource.isPlaying);
        }

        StartCoroutine(FadeToImage(SunBackGroundObj, 3f));
    }

    private IEnumerator FadeToImage(GameObject targetObj, float duration)
    {
        if (targetObj == null) yield break;

        var image = targetObj.GetComponent<Image>();
        if (image == null) yield break;

        Color color = image.color;
        color.a = 0;
        image.color = color;
        targetObj.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / duration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
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
            above.anchoredPosition = new Vector2(-750f, 160f);
            story.anchoredPosition = new Vector2(-250f, -20f);
        }
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_5; break;
            case "english": lines = languageCollector.EnglishLines3_5; break;
            case "japanese": lines = languageCollector.JapaneseLines3_5; break;
            case "chinese": lines = languageCollector.ChineseLines3_5; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_5; break;
            default: lines = languageCollector.KoreanLines3_5; break;
        }
    }

    private void OnLanguageChanged(string newLang)
    {
        LoadLinesForCurrentLanguage();
        StopAllCoroutines();
        index = 0;
        ShowCurrentDialogue();
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Update()
    {
        //  반복 재생 사운드
        if (index >= 0 && index <= 6 && BGMSource != null && !BGMSource.isPlaying)
            BGMSource.Play();
        else if ((index == 8 || index == 9) && BreathSource != null && !BreathSource.isPlaying)
            BreathSource.Play();
        else if ((index == 10 || index == 11) && BirdSource != null && !BirdSource.isPlaying)
            BirdSource.Play();
    }
}
