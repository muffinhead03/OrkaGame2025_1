using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManagerStage1_1 : MonoBehaviour
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
    public GameObject nextButton;
    public CanvasGroup blackFade;

    [Header("타이핑 및 텍스트 처리")]
    public TypewriterEffect typewriterEffect;
    public LanguageCollector1_1 languageCollector;

    [Header("오디오")]
    public AudioSource mainBGM;
    public AudioSource softMusic;

    private string[] currentLines;
    private int lineIndex = 0;

    private void Start()
    {
        SetupLanguageUI(); // 언어에 따라 UI 오브젝트 선택 및 배치

        currentLines = languageCollector.GetLines();// 현재 언어에 맞는 대사 배열 가져오기

        Debug.Log($"[Start] Current language: {LanguageManager.GetLanguage()}");
        Debug.Log($"[Start] Total lines: {currentLines.Length}");

        StartCoroutine(PlayDialogueSequence());// 첫 대사 출력 시작
    }



    private IEnumerator PlayDialogueSequence()
    {
        nextButton.SetActive(false);

        Debug.Log("[PlayDialogueSequence] Showing line 0");
        yield return StartCoroutine(ShowLine(currentLines[0]));

        nextButton.SetActive(true);
    }

    public void OnNextButtonClicked()
    {
        nextButton.SetActive(false);
        lineIndex++;

        // ✅ 현재 언어 기준으로 다시 대사 배열 갱신
        currentLines = languageCollector.GetLines();

        Debug.Log($"[OnNextButtonClicked] lineIndex = {lineIndex}");
        Debug.Log($"[OnNextButtonClicked] Language: {LanguageManager.GetLanguage()}");

        if (lineIndex >= currentLines.Length)
        {
            Debug.LogWarning("[OnNextButtonClicked] No more lines to show!");
            return;
        }

        switch (lineIndex)
        {
            case 1:
                StartCoroutine(FadeOutAndNextLine());
                break;
            case 2:
                StartCoroutine(PlayLineWithDelay(2, 0.1f, mainBGM));
                break;
            case 3:
                StartCoroutine(PlayLineWithDelay(3, 0.5f, softMusic));
                break;
            case 4:
                StartCoroutine(PlayLineWithDelay(4, 0.5f));
                break;
            case 5:
                StartCoroutine(PlayLineWithDelay(5, 0.5f));
                break;
            default:
                StartCoroutine(PlayLineWithDelay(lineIndex, 0.5f));
                break;
        }
    }

    private IEnumerator FadeOutAndNextLine()
    {
        float duration = 1f;
        float elapsed = 0f;

        Debug.Log("[FadeOutAndNextLine] Fading out...");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackFade.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        blackFade.alpha = 0f;
        yield return new WaitForSeconds(0.1f);

        Debug.Log("[FadeOutAndNextLine] Showing line 1");
        yield return StartCoroutine(ShowLine(currentLines[1]));

        nextButton.SetActive(true);
    }

    private IEnumerator PlayLineWithDelay(int index, float delay, AudioSource audio = null)
    {
        Debug.Log($"[PlayLineWithDelay] Waiting {delay}s then showing line {index}");

        yield return new WaitForSeconds(delay);

        if (audio != null)
        {
            audio.Play();
            Debug.Log("[PlayLineWithDelay] Audio played");
        }

        if (index >= currentLines.Length)
        {
            Debug.LogWarning($"[PlayLineWithDelay] Line index {index} out of bounds!");
            yield break;
        }

        yield return StartCoroutine(ShowLine(currentLines[index]));

        nextButton.SetActive(true);
    }

    private IEnumerator ShowLine(string line)
    {
        Debug.Log($"[ShowLine] Showing: {line}");

        typewriterEffect.SetText(line);
        yield return new WaitUntil(() => typewriterEffect.IsComplete);

        Debug.Log("[ShowLine] Typing complete");
    }

    private void SetupLanguageUI()
    {
        string lang = LanguageManager.GetLanguage(); // 🔥 이미 소문자로 처리됨
        DisableAllLangObjects();

        switch (lang)
        {
            case "korean":
                SetActiveAndPosition(Korean_Above, Korean_Story);
                break;
            case "english":
                SetActiveAndPosition(English_Above, English_Story);
                break;
            case "japanese":
                SetActiveAndPosition(Japanese_Above, Japanese_Story);
                break;
            case "chinese":
                SetActiveAndPosition(Chinese_Above, Chinese_Story);
                break;
            case "kazahustan":
            case "kaza":
                SetActiveAndPosition(Kaza_Above, Kaza_Story);
                break;
            default:
                Debug.LogWarning($"[SetupLanguageUI] Unknown language '{lang}'");
                break;
        }
    }



    private void SetActiveAndPosition(RectTransform above, RectTransform story)
    {
        above.gameObject.SetActive(true);
        story.gameObject.SetActive(true);
        above.anchoredPosition = AboPo;
        story.anchoredPosition = StoPo;
    }

    private void DisableAllLangObjects()
    {
        Korean_Above?.gameObject.SetActive(false);
        Korean_Story?.gameObject.SetActive(false);
        English_Above?.gameObject.SetActive(false);
        English_Story?.gameObject.SetActive(false);
        Japanese_Above?.gameObject.SetActive(false);
        Japanese_Story?.gameObject.SetActive(false);
        Chinese_Above?.gameObject.SetActive(false);
        Chinese_Story?.gameObject.SetActive(false);
        Kaza_Above?.gameObject.SetActive(false);
        Kaza_Story?.gameObject.SetActive(false);
    }
    
    private void Awake()
    {
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += OnLanguageChanged; // ✅ 이벤트 등록
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged; // ✅ 이벤트 해제
    }

    private void OnLanguageChanged(string newLang)
    {
        Debug.Log($"[DialogueManager] Language changed to: {newLang}");
    
        SetupLanguageUI(); // UI 갱신
        currentLines = languageCollector.GetLines(); // 대사 갱신
        typewriterEffect.StopTyping(); // 현재 텍스트 멈추고
        typewriterEffect.SetText(currentLines[lineIndex]); // 다시 시작
    }



}
