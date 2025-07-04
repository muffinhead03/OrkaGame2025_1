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
    public Button nextButton;            // Next 버튼
    public Image fadeImage;              // 풀스크린 검은색 Image (Inspector에서 드래그)

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    [Header("오디오")]
    public AudioSource bgmSource;        // BGM (case 0에서만 재생)
    public AudioSource ScreamingSound;   // 효과음

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

    // 현재 대사 목록과 인덱스
    private string[] lines;
    private int index;
    private Coroutine typingCoroutine;

    // 동적 레퍼런스: 활성 언어에 맞춘 TMP 컴포넌트
    private TextMeshProUGUI aboveText;
    private TextMeshProUGUI storyText;

    private void Awake()
    {
        // Next 버튼 세팅
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
        // 1) 풀스크린 페이드 투명 초기화 + 클릭 방해 안 함
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }

        // 2) 언어 UI 설정 및 TMP 레퍼런스 연결
        SetupLanguageUI();

        // 3) 배경 설정 (BGM 재생은 case 0에서)
        if (backgroundImage != null && backGroundSprite != null)
            backgroundImage.sprite = backGroundSprite;

        // 4) 대사 로드
        LoadLinesForCurrentLanguage();

        // 5) 첫 대사 실행
        index = 0;
        StartCoroutine(ShowLineSequence());
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
                Debug.LogWarning($"Unknown language '{{lang}}', default to Korean.");
                lines = languageCollector.KoreanLines2_3;
                break;
        }
    }

    private IEnumerator ShowLineSequence()
    {
        // 1) Next 버튼 숨김
        nextButton.gameObject.SetActive(false);

        // 2) 표정 및 캐릭터 교체
        UpdateCharacterFace(index);

        // 3) 대사 시작 전 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 4) 타이핑 효과 시작
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(lines[index]));
        yield return typingCoroutine;

        // 5) 4번째 대사 끝난 뒤에만 풀스크린 페이드아웃
        if (index == 4)
            yield return StartCoroutine(FadeToBlack());

        // 6) Next 버튼 활성화
        nextButton.gameObject.SetActive(true);
    }

    private void UpdateCharacterFace(int idx)
    {
        // 모든 캐릭터 표정 비활성화
        Narke_2Obj.SetActive(false);
        Narke_defaultObj.SetActive(false);
        Eco_surprisedObj.SetActive(false);
        Eco_smiledObj.SetActive(false);

        // idx 별 표정 및 사운드
        switch (idx)
        {
            case 0:
                Eco_smiledObj.SetActive(true);
                bgmSource?.Play();
                ScreamingSound?.Play();
                break;
            case 1:
                Eco_surprisedObj.SetActive(true);
                break;
            case 2:
                Narke_defaultObj.SetActive(true);
                break;
            case 3:
                Narke_2Obj.SetActive(true);
                break;
            case 4:
                Eco_smiledObj.SetActive(true);
                break;
            default:
                Eco_smiledObj.SetActive(true);
                break;
        }

        // 레이블 업데이트
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

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;
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

    private void OnNext()
    {
        index++;
        if (index >= lines.Length)
        {
            SceneManager.LoadScene("Stage2_3");
            return;
        }
        StartCoroutine(ShowLineSequence());
    }

    private void SetupLanguageUI()
    {
        // 모든 언어 UI 비활성화
        var allAbove = new[] { Korean_Above, English_Above, Japanese_Above, Chinese_Above, Kaza_Above };
        var allStory = new[] { Korean_Story, English_Story, Japanese_Story, Chinese_Story, Kaza_Story };
        foreach (var rt in allAbove) rt.gameObject.SetActive(false);
        foreach (var rt in allStory) rt.gameObject.SetActive(false);

        // 활성 언어 선택
        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();
        RectTransform a = Korean_Above, s = Korean_Story;
        switch (lang)
        {
            case "english": a = English_Above; s = English_Story; break;
            case "japanese": a = Japanese_Above; s = Japanese_Story; break;
            case "chinese": a = Chinese_Above; s = Chinese_Story; break;
            case "kazahustan":
            case "kaza": a = Kaza_Above; s = Kaza_Story; break;
        }
        // 활성화 및 위치 설정
        a.gameObject.SetActive(true);
        s.gameObject.SetActive(true);
        a.anchoredPosition = AboPo;
        s.anchoredPosition = StoPo;

        // TMP 컴포넌트 할당
        aboveText = a.GetComponent<TextMeshProUGUI>();
        storyText = s.GetComponent<TextMeshProUGUI>();
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
