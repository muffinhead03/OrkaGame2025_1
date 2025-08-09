using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstCardLanguageManager : MonoBehaviour
{
    private float blockByCarrotUntil = 0f;
    [SerializeField] private RectTransform[] neverAdvanceButtons; 
    [SerializeField] private RectTransform carrotButton;
    [SerializeField] private Camera uiCamera;

    public GameObject koreanText;
    public GameObject japaneseText;
    public GameObject englishText;
    public GameObject chineseText;
    public GameObject kazakhstanText;

    public GameObject firstPanel;
    public GameObject settingPanel;
    public GameObject dialoguePanel;

    private TextMeshProUGUI activeText;
    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFullyShown = false;
    private bool isDialogueEnded = false;
    private bool isLastLineShown = false;

    public float typingSpeed = 0.04f;

    public PuzzleGameFirstManagerScript gameManager;
    public GameObject[] cardPanels;
    [SerializeField] private GameObject[] settingPanelButtons;

    [Header("UI Raycast (세팅패널 클릭 차단용)")]
    [SerializeField] private GraphicRaycaster uiRaycaster;
    [SerializeField] private EventSystem eventSystem;

    // 🔑 최종 클릭 후 켤 대상(CardClick 루트). 인스펙터에 드래그!
    [SerializeField] private GameObject cardClickRootToActivate;

    // (선택) 언어별 버튼 활성화용
    [SerializeField] private FirstCardClickManager firstCardClickManager;

    void Start()
    {
        SetLanguageActive(LanguageManager.GetLanguage());
        // ✅ CardClick은 시작에 확실히 꺼두자 (누군가 씬에서 켜놓았을 수 있으니 방어)
        if (cardClickRootToActivate != null)
        {
            if (cardClickRootToActivate.activeSelf || cardClickRootToActivate.activeInHierarchy)
            {
                Debug.LogWarning($"[DEBUG][FirstCard] 시작 시 CardClick이 켜져 있어 꺼줍니다. activeSelf={cardClickRootToActivate.activeSelf}, inHierarchy={cardClickRootToActivate.activeInHierarchy}, path={GetPath(cardClickRootToActivate.transform)}");
            }
            cardClickRootToActivate.SetActive(false);
            Debug.Log($"[DEBUG][FirstCard] Start() → CardClick 강제 비활성화 완료. activeSelf={cardClickRootToActivate.activeSelf}, inHierarchy={cardClickRootToActivate.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[DEBUG][FirstCard] cardClickRootToActivate가 null 입니다. 인스펙터에 대상 오브젝트를 지정하세요.");
        }

        ShowNextDialogue();
    }

    public void OnCarrotButtonClick()  => BlockDialogueAdvance(0.2f);
    public void OnCarrotButtonDown()   => BlockDialogueAdvance(0.2f);
    private void BlockDialogueAdvance(float duration) => blockByCarrotUntil = Time.unscaledTime + duration;

    void Update()
    {
        if (isDialogueEnded) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.unscaledTime < blockByCarrotUntil) return;
            if (IsPointerOverBlockedUI()) return;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) return;
            if (ClickedCarrotThisFrame()) return;

            Debug.Log($"[DEBUG][FirstCard] 클릭 수신. isTyping={isTyping}, isFullyShown={isFullyShown}, isLastLineShown={isLastLineShown}, idx={currentLineIndex}/{(dialogueLines!=null?dialogueLines.Length:0)}");
            OnDialogueClick();
        }

        if (IsPanelBlockingCenter())
        {
            if (gameManager != null) gameManager.enabled = false;
        }
        else
        {
            if (isFullyShown && gameManager != null)
                gameManager.enabled = true;
        }
    }

    bool IsPointerOverBlockedUI()
    {
        if (uiRaycaster == null || eventSystem == null) return false;

        var eventData = new PointerEventData(eventSystem) { position = Input.mousePosition };
        var results = new System.Collections.Generic.List<RaycastResult>();
        uiRaycaster.Raycast(eventData, results);

        bool firstOrSettingAtOrigin =
            (firstPanel != null && ((RectTransform)firstPanel.transform).localPosition == Vector3.zero) ||
            (settingPanel != null && ((RectTransform)settingPanel.transform).localPosition == Vector3.zero);

        if (firstOrSettingAtOrigin)
        {
            bool overDialogue = false;
            foreach (var hit in results)
            {
                var t = hit.gameObject ? hit.gameObject.transform : null;
                if (t == null) continue;
                if (dialoguePanel != null && t.IsChildOf(dialoguePanel.transform))
                {
                    overDialogue = true;
                    break;
                }
            }

            if (!overDialogue && results.Count > 0)
                return true;
        }

        foreach (var hit in results)
        {
            if (hit.gameObject == null) continue;
            Transform t = hit.gameObject.transform;

            if (settingPanel != null && t.IsChildOf(settingPanel.transform))
                return true;

            if (neverAdvanceButtons != null)
            {
                for (int i = 0; i < neverAdvanceButtons.Length; i++)
                {
                    var rt = neverAdvanceButtons[i];
                    if (rt == null) continue;
                    if (t.IsChildOf(rt.transform)) return true;
                }
            }

            if (carrotButton != null && t.IsChildOf(carrotButton.transform))
            {
                if (firstOrSettingAtOrigin) return true;
            }
        }

        return false;
    }

    bool IsPanelAtCenter()
    {
        Vector3 center = Vector3.zero;
        if (firstPanel != null && Vector3.Distance(firstPanel.transform.position, center) < 0.1f) return true;
        if (settingPanel != null && Vector3.Distance(settingPanel.transform.position, center) < 0.1f) return true;
        return false;
    }

    public void OnDialogueClick()
    {
        if (isDialogueEnded) return;

        if (isLastLineShown && !isTyping)
        {
            Debug.Log("[DEBUG][FirstCard] 마지막 줄이 이미 표시됨 + 추가 클릭 → EndDialogueAndHide()");
            EndDialogueAndHide();
            return;
        }

        if (isTyping)
        {
            Debug.Log("[DEBUG][FirstCard] 타이핑 중 → 즉시 완성 처리");
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            if (dialogueLines != null && currentLineIndex < dialogueLines.Length)
                activeText.text = dialogueLines[currentLineIndex];

            isTyping = false;
            isFullyShown = true;
        }
        else
        {
            ShowNextDialogue();
        }
    }

    void ShowNextDialogue()
    {
        if (dialogueLines == null || activeText == null)
        {
            Debug.LogError("[DEBUG][FirstCard] dialogueLines 또는 activeText가 null 입니다. 언어 세팅/참조 확인!");
            return;
        }

        if (isTyping)
        {
            if (typingCoroutine != null) { StopCoroutine(typingCoroutine); typingCoroutine = null; }
            if (currentLineIndex < dialogueLines.Length)
                activeText.text = dialogueLines[currentLineIndex];
            isTyping = false;
            isFullyShown = true;
            return;
        }

        if (isFullyShown) currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length || currentLineIndex >= 7)
        {
            isLastLineShown = true;
            Debug.Log($"[DEBUG][FirstCard] 마지막 줄에 도달. currentLineIndex={currentLineIndex}, lines={dialogueLines.Length}. 이제 '추가 클릭'에서 EndDialogueAndHide() 호출 예정.");
            return;
        }

        isFullyShown = false;
        isLastLineShown = false;
        activeText.text = string.Empty;

        Debug.Log($"[DEBUG][FirstCard] TypeLine 시작. lineIndex={currentLineIndex}, text=\"{dialogueLines[currentLineIndex]}\"");
        typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        foreach (char c in line)
        {
            activeText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
        isFullyShown = true;
        currentLineIndex++;

        bool isLast =
            currentLineIndex >= 7 ||
            (dialogueLines != null && currentLineIndex >= dialogueLines.Length);

        Debug.Log($"[DEBUG][FirstCard] TypeLine 완료. 다음 인덱스={currentLineIndex}, isLast={isLast}");

        if (isLast)
        {
            isLastLineShown = true;
            Debug.Log("[DEBUG][FirstCard] 마지막 줄 표시 완료 상태. (아직 켜지지 않음) 다음 클릭에서 종료 + CardClick 켜짐");
            yield break;
        }
    }

    void EndDialogueAndHide()
    {
        if (isDialogueEnded) return;
        isDialogueEnded = true;

        EnableCardMovement(true);

        if (gameManager != null)
        {
            gameManager.StartTimerManually();
            gameManager.EndDialogue();
        }

        // ✅ 여기! 기존 ActivateUIForCurrentLanguage() 말고 UnlockAndShow() 호출
        if (firstCardClickManager != null)
        {
            Debug.Log("[DEBUG][FirstCard] 마지막 클릭 → FirstCardClickManager.UnlockAndShow() 호출");
            firstCardClickManager.UnlockAndShow();
        }
        else
        {
            Debug.LogError("[DEBUG][FirstCard] firstCardClickManager 참조가 NULL입니다. 인스펙터에 할당하세요.");
        }

        // 대사창 끄기 (CardClick이 그 아래 자식이면 같이 꺼지니 주의!)
        var target = dialoguePanel != null ? dialoguePanel : this.gameObject;
        target.SetActive(false);

        enabled = false;
    }


    void EnableCardMovement(bool allow)
    {
        if (cardPanels == null) return;

        foreach (GameObject card in cardPanels)
        {
            if (card == null) continue;

            var dragger = card.GetComponent<CardGame1Manager>();
            if (dragger != null) dragger.canDrag = allow;
        }
        Debug.Log($"[DEBUG][FirstCard] 카드 드래그 {(allow ? "허용" : "차단")}");
    }

    void SetLanguageActive(string lang)
    {
        if (koreanText)     koreanText.SetActive(false);
        if (japaneseText)   japaneseText.SetActive(false);
        if (englishText)    englishText.SetActive(false);
        if (chineseText)    chineseText.SetActive(false);
        if (kazakhstanText) kazakhstanText.SetActive(false);

        switch (lang)
        {
            case "korean":
                if (koreanText)
                {
                    koreanText.SetActive(true);
                    activeText = koreanText.GetComponent<TextMeshProUGUI>();
                    var s = koreanText.GetComponent<FirstCardLanguageScript>();
                    dialogueLines = s != null ? s.koreanLines : null;
                }
                break;
            case "japanese":
                if (japaneseText)
                {
                    japaneseText.SetActive(true);
                    activeText = japaneseText.GetComponent<TextMeshProUGUI>();
                    var s = japaneseText.GetComponent<FirstCardLanguageScript>();
                    dialogueLines = s != null ? s.japaneseLines : null;
                }
                break;
            case "chinese":
                if (chineseText)
                {
                    chineseText.SetActive(true);
                    activeText = chineseText.GetComponent<TextMeshProUGUI>();
                    var s = chineseText.GetComponent<FirstCardLanguageScript>();
                    dialogueLines = s != null ? s.chineseLines : null;
                }
                break;
            case "kazakh":
            case "kazakhstan":
                if (kazakhstanText)
                {
                    kazakhstanText.SetActive(true);
                    activeText = kazakhstanText.GetComponent<TextMeshProUGUI>();
                    var s = kazakhstanText.GetComponent<FirstCardLanguageScript>();
                    dialogueLines = s != null ? s.kazakhstanLines : null;
                }
                break;
            default:
                if (englishText)
                {
                    englishText.SetActive(true);
                    activeText = englishText.GetComponent<TextMeshProUGUI>();
                    var s = englishText.GetComponent<FirstCardLanguageScript>();
                    dialogueLines = s != null ? s.englishLines : null;
                }
                break;
        }

        Debug.Log($"[DEBUG][FirstCard] 언어 세팅 완료. lang={lang}, lines={(dialogueLines!=null?dialogueLines.Length:0)}, textGO={(activeText!=null?activeText.gameObject.name:"null")}");
    }

    bool IsPanelBlockingCenter()
    {
        var center = Vector2.zero;
        if (firstPanel != null && Vector2.Distance((Vector2)firstPanel.transform.position, center) < 1f) return true;
        if (settingPanel != null && Vector2.Distance((Vector2)settingPanel.transform.position, center) < 1f) return true;
        return false;
    }

    bool ClickedCarrotThisFrame()
    {
        if (carrotButton == null || !carrotButton.gameObject.activeInHierarchy) return false;

        Vector2 screenPos = Input.mousePosition;
        bool inside = RectTransformUtility.RectangleContainsScreenPoint(carrotButton, screenPos, uiCamera);
        return inside;
    }

    // ====== DEBUG HELPER ======
    bool SafeActivate(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("[DEBUG][FirstCard] SafeActivate 실패: 대상이 null 입니다.");
            return false;
        }

        string path = GetPath(go.transform);
        Debug.Log($"[DEBUG][FirstCard] SafeActivate 시도: {go.name}, activeSelf={go.activeSelf}, inHierarchy={go.activeInHierarchy}, path={path}");

        if (IsAnyParentInactive(go, out Transform culprit))
        {
            Debug.LogError($"[DEBUG][FirstCard] 부모 중 비활성 객체가 있어 활성화가 보이지 않을 수 있음. culprit={culprit.name}, path={GetPath(culprit)}");
        }

        go.SetActive(true);

        Debug.Log($"[DEBUG][FirstCard] SafeActivate 결과: activeSelf={go.activeSelf}, inHierarchy={go.activeInHierarchy}");
        return go.activeSelf; // activeInHierarchy는 부모 영향 받을 수 있음
    }

    string GetPath(Transform t)
    {
        if (t == null) return "null";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
        while (t.parent != null)
        {
            t = t.parent;
            sb.Insert(0, t.name + "/");
        }
        return sb.ToString();
    }

    bool IsAnyParentInactive(GameObject go, out Transform culprit)
    {
        culprit = null;
        if (go == null) return false;
        Transform p = go.transform;
        while (p != null)
        {
            if (!p.gameObject.activeSelf)
            {
                culprit = p;
                return true;
            }
            p = p.parent;
        }
        return false;
    }
}
