using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class SlidingGameRuleExplain : MonoBehaviour
{
    [Header("언어별 텍스트 오브젝트 (각 오브젝트에 TMP_Text)")]
    public GameObject koreanText;
    public GameObject englishText;
    public GameObject japaneseText;
    public GameObject chineseText;
    public GameObject kazakhText;

    [Header("룰 설명 패널(전체)")]
    public GameObject dialoguePanel;      // 전체 컨테이너

    [Header("룰 설명 이미지(예: 대화창 프레임 등)")]
    public GameObject dialogueImage;      // 마지막에 같이 끌 대상 (없으면 비워도 됨)

    [Header("다음 버튼(선택)")]
    public Button nextButton;

    [Header("타이핑")]
    public float typingSpeed = 0.04f;

    [Header("대사 소스")]
    public SlidingGameRuleExplainLines linesSource;

    [Header("입력 차단(선택)")]
    public CanvasGroup inputBlocker;

    [Header("클릭 무시 조건 (설정 버튼/패널 등)")]
    [SerializeField] private GraphicRaycaster uiRaycaster;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private RectTransform settingPanel;               // 설정 패널(활성+원점이면 진행 차단)
    [SerializeField] private RectTransform[] neverAdvanceButtons;      // 클릭해도 진행하지 않을 버튼들
    [SerializeField] private RectTransform dialogueRoot;               // 대사 영역(여기 위 클릭은 허용)
    [SerializeField] private Camera uiCamera;                          // (선택)
    [SerializeField] private RectTransform firstPanel;                 // 첫 화면 패널(활성+원점이면 진행 차단)

    [Header("디버그")]
    public bool logDebug = false;

    private TextMeshProUGUI activeText;
    private string[] dialogueLines;
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isFullyShown = false;    // 현재 줄이 모두 출력된 상태인지
    private bool finishedAll = false;
    private Coroutine typingCo;
    private float blockByButtonUntil = 0f;
    private bool pausedByExplain = false;

    // ====== 유틸: 원점 체크 ======
    private static bool IsAtOrigin(Transform t)
    {
        if (t == null) return false;
        if (t is RectTransform rt)
            return rt.anchoredPosition.sqrMagnitude <= 0.0001f || rt.localPosition.sqrMagnitude <= 0.0001f;
        return t.localPosition.sqrMagnitude <= 0.0001f;
    }
    private bool IsPanelAtOrigin()
    {
        bool firstBlocking   = firstPanel   != null && firstPanel.gameObject.activeInHierarchy   && IsAtOrigin(firstPanel);
        bool settingBlocking = settingPanel != null && settingPanel.gameObject.activeInHierarchy && IsAtOrigin(settingPanel);
#if UNITY_EDITOR
        if (logDebug)
            Debug.Log($"[RuleExplain] First@Origin={firstBlocking}, Setting@Origin={settingBlocking}");
#endif
        return firstBlocking || settingBlocking;
    }

    private void Awake()
    {
        LanguageManager.Initialize();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => { BlockAdvance(0.05f); OnAdvance(); });
            nextButton.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ForcePauseManagers();
        StartCoroutine(KeepPausedWhileOpen());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        // 퍼즐/타이머 일시정지(안전)
        ForcePauseManagers();

        if (dialoguePanel != null && !dialoguePanel.activeSelf)
            dialoguePanel.SetActive(true);
        if (dialogueImage != null && !dialogueImage.activeSelf)
            dialogueImage.SetActive(true);

        if (inputBlocker != null)
        {
            inputBlocker.gameObject.SetActive(true);
            inputBlocker.blocksRaycasts = true;
            inputBlocker.interactable   = true;
            inputBlocker.alpha = Mathf.Max(inputBlocker.alpha, 0.001f);
        }

        SetLanguageActive(LanguageManager.GetLanguage());
        LoadLines();

        currentIndex = 0;
        BeginTypingCurrent();   // 처음 줄 타이핑 시작
    }

    private void Update()
    {
        if (finishedAll) return;

        // 🔒 First/Setting 패널이 (0,0,0)에 있으면 진행 차단
        if (IsPanelAtOrigin()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.unscaledTime < blockByButtonUntil) return;
            if (IsPointerOverBlockedUI()) return;  // 설정/차단 UI 위 클릭 무시

            OnAdvance();
        }
    }

    // ====== 클릭 차단 유틸 ======
    private void BlockAdvance(float duration) => blockByButtonUntil = Time.unscaledTime + duration;

    private bool IsPointerOverBlockedUI()
    {
        if (uiRaycaster == null || eventSystem == null) return false;

        var eventData = new PointerEventData(eventSystem) { position = Input.mousePosition };
        var results = new System.Collections.Generic.List<RaycastResult>();
        uiRaycaster.Raycast(eventData, results);

        // 1) 설정 패널 위면 막기 (활성일 때만)
        if (settingPanel != null && settingPanel.gameObject.activeInHierarchy)
        {
            foreach (var hit in results)
            {
                if (hit.gameObject == null) continue;
                if (hit.gameObject.transform.IsChildOf(settingPanel)) return true;
            }
        }

        // 2) neverAdvanceButtons 위면 막기
        if (neverAdvanceButtons != null)
        {
            foreach (var hit in results)
            {
                if (hit.gameObject == null) continue;
                var t = hit.gameObject.transform;
                for (int i = 0; i < neverAdvanceButtons.Length; i++)
                {
                    var rt = neverAdvanceButtons[i];
                    if (rt != null && t.IsChildOf(rt)) return true;
                }
            }
        }

        return false;
    }

    // ====== 언어 처리 ======
    private void SetLanguageActive(string langRaw)
    {
        string lang = (langRaw ?? "english").Trim().ToLowerInvariant();

        SafeSetActive(koreanText,  false);
        SafeSetActive(englishText, false);
        SafeSetActive(japaneseText,false);
        SafeSetActive(chineseText, false);
        SafeSetActive(kazakhText,  false);

        switch (lang)
        {
            case "korean":  SafeSetActive(koreanText, true);  activeText = GetTMP(koreanText);  break;
            case "english": SafeSetActive(englishText, true); activeText = GetTMP(englishText); break;
            case "japanese":SafeSetActive(japaneseText, true);activeText = GetTMP(japaneseText);break;
            case "chinese": SafeSetActive(chineseText, true); activeText = GetTMP(chineseText); break;
            case "kazakh":  SafeSetActive(kazakhText, true);  activeText = GetTMP(kazakhText);  break;
            default:        SafeSetActive(englishText, true); activeText = GetTMP(englishText); break;
        }
    }

    private void LoadLines()
    {
        if (linesSource == null) linesSource = FindObjectOfType<SlidingGameRuleExplainLines>();
        string key = (LanguageManager.GetLanguage() ?? "english").Trim().ToLowerInvariant();
        dialogueLines = (linesSource != null) ? linesSource.GetLinesFor(key) : new[] { " " };
        if (dialogueLines == null || dialogueLines.Length == 0) dialogueLines = new[] { " " };
    }

    // ====== 진행 ======
    public void OnAdvance()
    {
        if (finishedAll) return;
        if (IsPanelAtOrigin()) return;

        // 1) 타이핑 중 클릭 → 즉시 완성(다음 줄로는 넘어가지 않음)
        if (isTyping)
        {
            if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
            if (activeText != null && dialogueLines != null && currentIndex < dialogueLines.Length)
                activeText.text = dialogueLines[currentIndex];

            isTyping = false;
            isFullyShown = true;
            if (nextButton != null) { nextButton.gameObject.SetActive(true); nextButton.interactable = true; }
            return;
        }

        // 2) 이미 완성된 상태에서 클릭 → 다음 줄로
        if (isFullyShown)
        {
            currentIndex++;
            if (currentIndex >= dialogueLines.Length)
            {
                FinishAndResume();
                return;
            }
            BeginTypingCurrent();
            return;
        }

        // 3) 그 외(안전)
        BeginTypingCurrent();
    }

    private void BeginTypingCurrent()
    {
        if (dialogueLines == null || activeText == null)
        {
            FinishAndResume();
            return;
        }
        if (currentIndex >= dialogueLines.Length)
        {
            FinishAndResume();
            return;
        }

        isTyping = true;
        isFullyShown = false;

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        activeText.text = string.Empty;

        string line = dialogueLines[currentIndex] ?? "";
        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
        typingCo = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        foreach (char c in line)
        {
            if (activeText != null) activeText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCo = null;
        isTyping = false;
        isFullyShown = true;   // ← 끝났지만 인덱스는 올리지 않음(사용자 클릭에서 올림)

        if (nextButton != null) { nextButton.gameObject.SetActive(true); nextButton.interactable = true; }
    }

    private void FinishAndResume()
    {
        if (finishedAll) return;
        finishedAll = true;

        // 다음 프레임까지 입력 흡수: 당근 버튼이 Down만 받고 Up을 못 받는 상황 방지
        StartCoroutine(FinishRoutine());
    }
    private IEnumerator FinishRoutine()
    {
        // 1) 당장 블로커 끄지 말고, "마우스 업"을 기다린다.
        if (inputBlocker != null)
        {
            inputBlocker.gameObject.SetActive(true);
            inputBlocker.blocksRaycasts = true;   // 이 프레임 업 이벤트 흡수
            inputBlocker.interactable   = true;
            inputBlocker.alpha = Mathf.Max(inputBlocker.alpha, 0.001f);
        }

        // 2) 마우스가 올라갈 때까지 대기 (업 이벤트를 화면 상에서 소모)
        while (Input.GetMouseButton(0))
            yield return null; // 다음 프레임까지

        // 3) 혹시 눌림 상태가 남지 않도록 선택 해제
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);

        // 4) 한 프레임 더 쉬고 닫기 (UI 전환 안전)
        yield return null;

        // 5) 룰 설명 UI 비활성화
        if (dialogueImage != null) dialogueImage.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 6) 블로커 해제
        if (inputBlocker != null)
        {
            inputBlocker.blocksRaycasts = false;
            inputBlocker.interactable   = false;
            // 필요하면 inputBlocker.gameObject.SetActive(false);
        }

        // 7) Next 버튼 숨김 + 선택 해제(안전)
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (eventSystem != null) eventSystem.SetSelectedGameObject(null);

        // 8) 퍼즐 재개
        ForceResumeManagers();

        // 9) 같은 프레임에 또 클릭 들어오지 않도록 살짝 쿨타임
        BlockAdvance(0.1f);

        enabled = false;
    }
    // ====== Pause/Resume 보강 (Before 전용) ======
    private IEnumerator KeepPausedWhileOpen()
    {
        while (enabled && dialoguePanel != null && dialoguePanel.activeInHierarchy && !finishedAll)
        {
            ForcePauseManagers();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ForcePauseManagers()
    {
        if (SlidingGameManager1BeforeDeathScript.Instance != null)
        {
            SlidingGameManager1BeforeDeathScript.Instance.PauseGame();
            pausedByExplain = true;
        }
    }

    private void ForceResumeManagers()
    {
        if (!pausedByExplain) return;

        if (SlidingGameManager1BeforeDeathScript.Instance != null)
            SlidingGameManager1BeforeDeathScript.Instance.ResumeGame();

        pausedByExplain = false;
    }

    // ====== 유틸 ======
    private void SafeSetActive(GameObject go, bool on) { if (go != null) go.SetActive(on); }
    private TextMeshProUGUI GetTMP(GameObject go) { return go ? (go.GetComponent<TextMeshProUGUI>() ?? go.GetComponentInChildren<TextMeshProUGUI>(true)) : null; }
}
