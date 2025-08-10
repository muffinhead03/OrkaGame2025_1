using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class SlidingGame2RuleExplainManger : MonoBehaviour
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

    [Header("대사 소스 (2차용)")]
    public SlidingGame2RuleExplainLines linesSource;

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

// === Debug options ===
    [SerializeField] private bool logDebug = true;   // 이미 있다면 이 줄은 생략
    [SerializeField] private bool logFullLine = false;
    [SerializeField] private int  previewChars = 60;

// 현재 선택된 언어 키 저장(english/korean/...)
    private string currentLangKey = "english";

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
            Debug.Log($"[RuleExplain2] First@Origin={firstBlocking}, Setting@Origin={settingBlocking}");
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
// --- Debug helpers ---
    private void Debug_LogLangSelected(int totalLines)
    {
        if (!logDebug) return;
        Debug.Log($"[RuleExplain2][LANG] selected={currentLangKey}, lines={totalLines}");
    }

    private void Debug_LogLine(string phase, int index, string line)
    {
        if (!logDebug) return;
        string text = line ?? "";
        if (!logFullLine && text.Length > previewChars)
            text = text.Substring(0, previewChars) + "...";
        Debug.Log($"[RuleExplain2][LINE {phase}] lang={currentLangKey}, idx={index}, text=\"{text}\"");
    }

// 인스펙터에서 한 번에 덤프할 수 있는 메뉴(선택)
    [ContextMenu("Debug/Dump All Lines")]
    private void Debug_DumpAllLines()
    {
        if (dialogueLines == null) { Debug.Log("[RuleExplain2][DUMP] (no lines)"); return; }
        for (int i = 0; i < dialogueLines.Length; i++)
            Debug_LogLine("DUMP", i, dialogueLines[i]);
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
        
        currentLangKey = (langRaw ?? "english").Trim().ToLowerInvariant();  // << 추가

    }

    private void LoadLines()
    {
        if (linesSource == null) linesSource = FindObjectOfType<SlidingGame2RuleExplainLines>();
        string key = (LanguageManager.GetLanguage() ?? "english").Trim().ToLowerInvariant();
        dialogueLines = (linesSource != null) ? linesSource.GetLinesFor(key) : new[] { " " };
        if (dialogueLines == null || dialogueLines.Length == 0) dialogueLines = new[] { " " };
        dialogueLines = (linesSource != null) ? linesSource.GetLinesFor(key) : new[] { " " };
        if (dialogueLines == null || dialogueLines.Length == 0) dialogueLines = new[] { " " };
        Debug_LogLangSelected(dialogueLines.Length);  // << 추가

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
            Debug_LogLine("COMPLETE", currentIndex, dialogueLines[currentIndex]); // << 추가
            isTyping = false;
            isFullyShown = true;
            if (nextButton != null) { nextButton.gameObject.SetActive(true); nextButton.interactable = true; }
            // FinishAndResume() 맨 위나 끝에
            if (logDebug) Debug.Log("[RuleExplain2][END] all lines finished.");

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
        // 안전 체크 먼저
        if (dialogueLines == null || activeText == null)
        {
            FinishAndResume();
            return;
        }

        if (currentIndex < 0) currentIndex = 0;
        if (currentIndex >= dialogueLines.Length)
        {
            FinishAndResume();
            return;
        }

        // 한 번만 선언
        string line = dialogueLines[currentIndex] ?? string.Empty;

        // 이제 로그
        Debug_LogLine("BEGIN", currentIndex, line);

        isTyping = true;
        isFullyShown = false;

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        activeText.text = string.Empty;

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
        isFullyShown = true;   // 끝났지만 인덱스는 클릭 때 올림

        if (nextButton != null) { nextButton.gameObject.SetActive(true); nextButton.interactable = true; }
    }

    private void FinishAndResume()
    {
        finishedAll = true;

        if (inputBlocker != null)
        {
            inputBlocker.blocksRaycasts = false;
            inputBlocker.interactable   = false;
        }

        // 마지막 대사 후 이미지/패널 비활성화
        if (dialogueImage != null) dialogueImage.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 타이머/퍼즐 재개
        ForceResumeManagers();

        enabled = false;
    }

    // ====== Pause/Resume (슬라이딩2 전용) ======
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
        if (SlidingGameManager2Script.Instance != null)
        {
            SlidingGameManager2Script.Instance.PauseGame();
            pausedByExplain = true;
        }
    }

    private void ForceResumeManagers()
    {
        if (!pausedByExplain) return;

        if (SlidingGameManager2Script.Instance != null)
            SlidingGameManager2Script.Instance.ResumeGame();

        pausedByExplain = false;
    }

    // ====== 유틸 ======
    private void SafeSetActive(GameObject go, bool on) { if (go != null) go.SetActive(on); }
    private TextMeshProUGUI GetTMP(GameObject go) { return go ? (go.GetComponent<TextMeshProUGUI>() ?? go.GetComponentInChildren<TextMeshProUGUI>(true)) : null; }
}
