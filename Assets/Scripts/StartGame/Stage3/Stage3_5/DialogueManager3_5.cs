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
        ApplyBackground(index);
        ApplyCharacter(index);
        ManageLoopingSounds(index);

        if (index == 3 && DoorGoriSource != null) DoorGoriSource.Play();
        if (index == 9) StartCoroutine(HandleFluteThenFall());

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
    }

    private void ApplyBackground(int idx)
    {
        foreach (var bg in backgroundObjs)
            if (bg != null) bg.SetActive(false);

        if (idx == 7)
            BlackImageObj?.SetActive(true);
        else if (idx == 10 || idx == 11 || idx == 9)
            SunBackGroundObj?.SetActive(true);
        else
            BackGroundObj?.SetActive(true);
    }

    private void ApplyCharacter(int idx)
    {
        foreach (var ch in characterObjs)
            if (ch != null) ch.SetActive(false);

        switch (idx)
        {
            case 0:
            case 2:
            case 9:
                Eco_real6Obj?.SetActive(true); break;
            case 1:
                Eco_real_defaultObj?.SetActive(true); break;
            case 3:
                Eco_real5Obj?.SetActive(true); break;
            case 4:
                Eco_real2Obj?.SetActive(true); break;
            case 5:
                Eco_real3Obj?.SetActive(true); break;
            case 6:
                Eco_laughObj?.SetActive(true); break;
            case 7:
                Eco_banjunObj?.SetActive(true); break;
            case 8:
                Eco_real4Obj?.SetActive(true); break;
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

    private void ManageLoopingSounds(int idx)
    {
        StopLoopingSounds();

        if (idx >= 0 && idx <= 6)
        {
            if (BGMSource != null) { BGMSource.loop = true; BGMSource.Play(); }
        }
        else if (idx == 8 || idx == 9)
        {
            if (BreathSource != null) { BreathSource.loop = true; BreathSource.Play(); }
        }
        else if (idx == 10 || idx == 11)
        {
            if (BirdSource != null) { BirdSource.loop = true; BirdSource.Play(); }
            if (idx == 10 && FestivalSource != null)
            {
                FestivalSource.loop = true;
                FestivalSource.Play();
            }
        }
    }

    private void StopLoopingSounds()
    {
        if (BGMSource != null && BGMSource.isPlaying) BGMSource.Stop();
        if (BreathSource != null && BreathSource.isPlaying) BreathSource.Stop();
        if (BirdSource != null && BirdSource.isPlaying) BirdSource.Stop();
        if (FestivalSource != null && FestivalSource.loop) FestivalSource.Stop();
    }

    private IEnumerator HandleFluteThenFall()
    {
        if (FluteSource != null)
        {
            FluteSource.Play();
            yield return new WaitWhile(() => FluteSource.isPlaying);
        }

        if (FallSoundSource != null) FallSoundSource.Play();
        yield return new WaitForSeconds(3f);
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
}