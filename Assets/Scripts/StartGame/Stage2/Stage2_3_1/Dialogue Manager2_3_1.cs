using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI aboveText;    // 이름 레이블
    public TextMeshProUGUI storyText;    // 대사 텍스트
    public Button nextButton;            // Next 버튼

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource waterSound;
    public AudioSource birdSound;
    public AudioSource grassSound;

    [Header("표정 오브젝트")]
    public GameObject Eco_eyeclosedObj;  // 눈 감은 표정
    public GameObject Eco_defaultObj;    // 기본 표정
    public GameObject Eco_surprisedObj;  // 놀란 표정
    public GameObject Eco_readyObj;      // 준비 표정

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 스크립트")]
    public LanguageCollector2_1 languageCollector;

    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        // Next 버튼 클릭 등록
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
        // 1) UI 언어 오브젝트 설정
        SetupLanguageUI();

        // 2) 이름 레이블 설정
        if (aboveText != null)
            aboveText.text = "에코";

        // 3) 배경 & BGM
        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 4) 대사 배열 로드
        LoadLinesForCurrentLanguage();

        // 5) 인덱스 초기화 후 첫 대사 시퀀스 시작
        index = 0;
        StartCoroutine(ShowLineSequence());
    }

    // 현재 언어에 맞춰 lines[] 에 대사들을 할당
    private void LoadLinesForCurrentLanguage()
    {
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        switch (lang)
        {
            case "korean": lines = languageCollector.KoreanLines2_1; break;
            case "english": lines = languageCollector.EnglishLines2_1; break;
            case "japanese": lines = languageCollector.JapaneseLines2_1; break;
            case "chinese": lines = languageCollector.ChineseLines2_1; break;
            case "kazahustan":
            case "kaza": lines = languageCollector.KazaLines2_1; break;
            default:
                Debug.LogWarning($"Unknown language '{lang}', default to Korean.");
                lines = languageCollector.KoreanLines2_1;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        // 1) 즉시 표정 교체
        UpdateCharacterFace(index);

        // 2) 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 3) 대사 타입 이펙트
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        // 4) Next 버튼 활성화
        nextButton?.gameObject.SetActive(true);
    }

    private void UpdateCharacterFace(int idx)
    {
        // 모든 표정 비활성화
        Eco_eyeclosedObj?.SetActive(false);
        Eco_defaultObj?.SetActive(false);
        Eco_surprisedObj?.SetActive(false);
        Eco_readyObj?.SetActive(false);

        // 필요한 표정만 활성화
        switch (idx)
        {
            case 0:
            case 2:
            case 8:
                Eco_eyeclosedObj?.SetActive(true);
                waterSound?.Play(); birdSound?.Play();
                break;
            case 1:
            case 5:
            case 7:
                Eco_defaultObj?.SetActive(true);
                if (idx == 5) grassSound?.Play();
                break;
            case 3:
            case 4:
            case 9:
                Eco_surprisedObj?.SetActive(true);
                break;
            case 6:
                Eco_readyObj?.SetActive(true);
                break;
            default:
                Eco_defaultObj?.SetActive(true);
                break;
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
            SceneManager.LoadScene("Stage2_2");
            return;
        }
        StartCoroutine(ShowLineSequence());
    }

    private void SetupLanguageUI()
    {
        // 모든 언어 UI 비활성화
        var all = new[] {
            Korean_Above, Korean_Story,
            English_Above, English_Story,
            Japanese_Above, Japanese_Story,
            Chinese_Above, Chinese_Story,
            Kaza_Above, Kaza_Story
        };
        foreach (var rt in all)
            rt?.gameObject.SetActive(false);

        // 현재 언어에 맞춰 다시 활성화
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
        // *index는 유지*한 채, lines만 새로 로드
        LoadLinesForCurrentLanguage();
        // 만약 대사 맨 앞에서 다시 타이핑하고 싶다면 index=0; 해주면 됩니다.

        // 코루틴 초기화 후, 동일한 인덱스의 대사 재실행
        StopAllCoroutines();
        StartCoroutine(ShowLineSequence());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
