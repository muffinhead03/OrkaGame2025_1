using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManagerStage2_1 : MonoBehaviour
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
    public TextMeshProUGUI aboveText;    // “에코” 고정 라벨
    public TextMeshProUGUI storyText;    // 대사 텍스트
    public Button nextButton;            // Next 버튼

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource bgmSource;
    public AudioSource waterSound;
    public AudioSource birdSound;
    public AudioSource grassSound;

    [Header("캐릭터 이미지 관리")]
    public Image characterImage;
    public Sprite Eco_eyeclosed;
    public Sprite Eco_default;
    public Sprite Eco_surprised;
    public Sprite Eco_ready;

    [Header("배경 이미지")]
    public Image backgroundImage;
    public Sprite backGroundSprite;

    [Header("대사 관리")]
    public LanguageCollector2_1 languageCollector;

    private string[] lines;
    private int index = 0;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        // Next 버튼 클릭 리스너 등록
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNext);
        }

        // 언어 시스템 초기화
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        // UI 언어별 배치
        SetupLanguageUI();

        // Above 텍스트 설정
        if (aboveText != null)
            aboveText.text = "에코";

        // 배경 이미지 & BGM 재생
        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 대사 배열 로드
        lines = languageCollector != null ? languageCollector.GetLines() : new string[0];
        index = 0;

        // 첫 대사 실행
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
        StartCoroutine(PlayLineCoroutine());
    }

    private IEnumerator PlayLineCoroutine()
    {
        // 버튼 숨기기
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        // 표정 및 효과음 매핑
        switch (index)
        {
            case 0:
                if (characterImage != null) characterImage.sprite = Eco_eyeclosed;
                waterSound?.Play(); birdSound?.Play();
                break;
            case 1:
                if (characterImage != null) characterImage.sprite = Eco_default;
                break;
            case 2:
                if (characterImage != null) characterImage.sprite = Eco_eyeclosed;
                break;
            case 3:
            case 4:
                if (characterImage != null) characterImage.sprite = Eco_surprised;
                break;
            case 5:
                if (characterImage != null) characterImage.sprite = Eco_default;
                grassSound?.Play();
                break;
            case 6:
                if (characterImage != null) characterImage.sprite = Eco_ready;
                break;
            case 7:
                if (characterImage != null) characterImage.sprite = Eco_default;
                break;
            case 8:
                if (characterImage != null) characterImage.sprite = Eco_eyeclosed;
                break;
            case 9:
                if (characterImage != null) characterImage.sprite = Eco_surprised;
                break;
        }

        // 타입 라이터 이펙트 실행
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        // 버튼 활성화
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
    }

    private IEnumerator TypeText(string fullText)
    {
        if (storyText != null)
        {
            storyText.text = string.Empty;
            foreach (char c in fullText)
            {
                storyText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    private void OnNext()
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        index++;
        if (index >= lines.Length)
        {
            SceneManager.LoadScene("Stage2_2");
            return;
        }

        StartCoroutine(PlayLineCoroutine());
    }

    private void SetupLanguageUI()
    {
        // 모든 언어 객체 비활성화 (null 체크 추가)
        if (Korean_Above != null) Korean_Above.gameObject.SetActive(false);
        if (Korean_Story != null) Korean_Story.gameObject.SetActive(false);
        if (English_Above != null) English_Above.gameObject.SetActive(false);
        if (English_Story != null) English_Story.gameObject.SetActive(false);
        if (Japanese_Above != null) Japanese_Above.gameObject.SetActive(false);
        if (Japanese_Story != null) Japanese_Story.gameObject.SetActive(false);
        if (Chinese_Above != null) Chinese_Above.gameObject.SetActive(false);
        if (Chinese_Story != null) Chinese_Story.gameObject.SetActive(false);
        if (Kaza_Above != null) Kaza_Above.gameObject.SetActive(false);
        if (Kaza_Story != null) Kaza_Story.gameObject.SetActive(false);

        // 활성화될 언어 객체 선택
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        RectTransform above = null, story = null;
        switch (lang)
        {
            case "korean": above = Korean_Above; story = Korean_Story; break;
            case "english": above = English_Above; story = English_Story; break;
            case "japanese": above = Japanese_Above; story = Japanese_Story; break;
            case "chinese": above = Chinese_Above; story = Chinese_Story; break;
            case "kazahustan":
            case "kaza": above = Kaza_Above; story = Kaza_Story; break;
            default: above = Korean_Above; story = Korean_Story; break;
        }

        // 활성화 및 위치 설정
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
        SetupLanguageUI();
        // 대사 갱신 시 현재 라인 재출력
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        StartCoroutine(PlayLineCoroutine());
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}
