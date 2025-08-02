using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FirstCardLanguageManager : MonoBehaviour
{
    public GameObject koreanText;
    public GameObject japaneseText;
    public GameObject englishText;
    public GameObject chineseText;
    public GameObject kazakhstanText;

    public GameObject firstPanel;
    public GameObject settingPanel;

    private TextMeshProUGUI activeText;
    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFullyShown = false;

    public float typingSpeed = 0.04f;

    public PuzzleGameFirstManagerScript gameManager; // 연결 필수
    public GameObject[] cardPanels; // 카드 이동 비활성화 처리 대상

    void Start()
    {
        SetLanguageActive(LanguageManager.GetLanguage());
        ShowNextDialogue();
    }

    void SetLanguageActive(string lang)
    {
        Debug.Log($"[LANG] Trying to activate language: {lang}");
        koreanText.SetActive(false);
        japaneseText.SetActive(false);
        englishText.SetActive(false);
        chineseText.SetActive(false);
        kazakhstanText.SetActive(false);

        switch (lang)
        {
            case "korean":
                koreanText.SetActive(true);
                activeText = koreanText.GetComponent<TextMeshProUGUI>();
                dialogueLines = koreanText.GetComponent<FirstCardLanguageScript>().koreanLines;
                break;
            case "japanese":
                japaneseText.SetActive(true);
                activeText = japaneseText.GetComponent<TextMeshProUGUI>();
                dialogueLines = japaneseText.GetComponent<FirstCardLanguageScript>().japaneseLines;
                break;
            case "chinese":
                chineseText.SetActive(true);
                activeText = chineseText.GetComponent<TextMeshProUGUI>();
                dialogueLines = chineseText.GetComponent<FirstCardLanguageScript>().chineseLines;
                break;
            case "kazakh":
            case "kazakhstan":
                kazakhstanText.SetActive(true);
                activeText = kazakhstanText.GetComponent<TextMeshProUGUI>();
                dialogueLines = kazakhstanText.GetComponent<FirstCardLanguageScript>().kazakhstanLines;
                break;
            default:
                englishText.SetActive(true);
                activeText = englishText.GetComponent<TextMeshProUGUI>();
                dialogueLines = englishText.GetComponent<FirstCardLanguageScript>().englishLines;
                break;
        }
    }

    void Update()
    {
        if (Vector2.Distance(firstPanel.transform.position, Vector2.zero) < 1f ||
            Vector2.Distance(settingPanel.transform.position, Vector2.zero) < 1f)
        {
            // 패널이 중앙에 있음 → 타이머 강제 정지
            gameManager.enabled = false;
        }
        else
        {
            // 패널이 꺼짐 → 타이머 작동 가능 (단, 대사 완료 후만)
            if (isFullyShown)
                gameManager.enabled = true;
        }
    }

    public void OnDialogueClick()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            activeText.text = dialogueLines[currentLineIndex];
            isTyping = false;
            isFullyShown = true;
            EnableCardMovement(true);
        }
        else
        {
            ShowNextDialogue();
        }
    }



    void ShowNextDialogue()
    {
        if (currentLineIndex >= dialogueLines.Length)
        {
            isFullyShown = true;
            EnableCardMovement(true);
            gameManager.StartTimerManually();
            return;
        }

        activeText.text = "";
        isFullyShown = false;
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
        isFullyShown = true;

        currentLineIndex++;

        // ✅ 대사가 전부 끝났을 때만 드래그 허용 + 타이머 시작
        if (currentLineIndex >= dialogueLines.Length)
        {
            EnableCardMovement(true); // ← 여기에만!
            gameManager.StartTimerManually();
        }
    }

    void OnEnable()
    {
        LanguageManager.OnLanguageChanged += HandleLanguageChange;
    }

    void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= HandleLanguageChange;
    }

    void HandleLanguageChange(string newLang)
    {
        Debug.Log($"🌐 언어 변경됨 → {newLang}");

        // 기존 텍스트 비활성화
        if (activeText != null)
            activeText.gameObject.SetActive(false);

        // 언어에 맞는 대사 배열과 텍스트 다시 연결
        SetLanguageActive(newLang);

        // 👉 현재 인덱스를 유지하고 새로운 언어로 같은 위치부터 이어감
        if (currentLineIndex < dialogueLines.Length)
        {
            ShowNextDialogue(); // 같은 인덱스 위치에서 재시작
        }
    }



    void EnableCardMovement(bool allow)
    {
        if (cardPanels != null)
        {
            foreach (GameObject card in cardPanels)
            {
                if (card != null)
                {
                    var dragger = card.GetComponent<CardGame1Manager>();
                    if (dragger != null)
                    {
                        dragger.canDrag = allow;
                    }
                }
            }
        }
    }
    
    public bool IsDialogueComplete()
    {
        return currentLineIndex >= dialogueLines.Length;
    }



}
