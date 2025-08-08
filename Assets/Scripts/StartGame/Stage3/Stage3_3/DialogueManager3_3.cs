using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager3_3 : MonoBehaviour
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
    public Image endingImage;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("배경 및 캐릭터")]
    public Image backgroundImage;
    public GameObject blackImageObj;
    public GameObject Pan_4eyeclosedObj;
    public GameObject Pan_2Obj;

    [Header("오디오")]
    public AudioSource bgmSource;

    [Header("대사 스크립트")]
    public LanguageCollector3_3 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 점프 애니메이션 관련
    private bool isJumpScaling = false;
    private float jumpTimer = 0f;
    private float jumpDuration = 0.3f;
    private Vector3 pan2TargetScale = Vector3.one;

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

        if (aboveText != null)
            aboveText.text = "판";

        if (endingImage != null)
            endingImage.gameObject.SetActive(false); // 명시적으로 비활성화


        LoadLinesForCurrentLanguage();

        index = 0;
        StartCoroutine(ShowLineSequence());       
    }

    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines3_3; break;
            case "english": lines = languageCollector.EnglishLines3_3; break;
            case "japanese": lines = languageCollector.JapaneseLines3_3; break;
            case "chinese": lines = languageCollector.ChineseLines3_3; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines3_3; break;
            default: lines = languageCollector.KoreanLines3_3; break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        UpdateVisuals(index);

        yield return new WaitForSeconds(0.5f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        nextButton?.gameObject.SetActive(true);

        // 판 이미지가 점프 없이 그냥 평범하게 보이도록 처리
        if ((index == 1 || index == lines.Length - 1) && Pan_2Obj != null)
        {
            Pan_2Obj.transform.localScale = Vector3.one;
            Pan_2Obj.SetActive(true);
        }

        // 마지막 대사면 2초 대기 후 엔딩 이미지 페이드인
        if (index == lines.Length - 1)
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(FadeInEndingImage());
        }
    }

    private void UpdateVisuals(int idx)
    {
        if (backgroundImage != null) backgroundImage.enabled = (idx == 0);
        if (blackImageObj != null) blackImageObj.SetActive(idx == 1);

        Pan_4eyeclosedObj?.SetActive(false);
        Pan_2Obj?.SetActive(false);
        if (idx == 0) Pan_4eyeclosedObj?.SetActive(true);

        if (bgmSource != null)
        {
            if (idx == 0 && !bgmSource.isPlaying)
            {
                bgmSource.loop = true;
                bgmSource.Play();
            }
            else if (idx == 1 && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
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
        if (index >= lines.Length)
        {
            return;
        }
        StartCoroutine(ShowLineSequence());
    }

    private IEnumerator FadeInEndingImage()
    {
        Debug.Log("FadeInEndingImage 호출됨");

        if (endingImage == null)
        {
            Debug.LogWarning("endingImage가 null입니다!");
            yield break;
        }

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
