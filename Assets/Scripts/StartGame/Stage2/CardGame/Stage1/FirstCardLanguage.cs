using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FirstCardLanguage : MonoBehaviour
{
    public GameObject koreanText;
    public GameObject englishText;
    public GameObject japaneseText;
    public GameObject chineseText;
    public GameObject kazahustanText;

    public GameObject dialogueImage; // 클릭 영역 (Dialogue 박스)
    public CanvasGroup[] cardCanvasGroups; // 카드 그룹 6개

    private TextMeshProUGUI currentTextUI;
    private string[] dialogueLines;
    private int currentLineIndex = 0;

    private bool isTyping = false;
    private bool isWaitingForNext = false;
    public GameObject firstPanel;
    
    public PuzzleGameFirstManagerScript gameManager; // 타이머 수동 시작용


    /// <summary>
    /// ✅ PuzzleGameFirstManagerScript에서 대사 진행 중인지 확인할 수 있는 프로퍼티
    /// </summary>
    public bool IsDialogueActive => isTyping || isWaitingForNext;

    void Start()
    {
        LanguageManager.Initialize();
        LanguageManager.OnLanguageChanged += ApplyLanguage;
        ApplyLanguage(LanguageManager.GetLanguage());

        if (dialogueImage != null)
        {
            var button = dialogueImage.GetComponent<Button>();
            if (button == null)
            {
                button = dialogueImage.AddComponent<Button>();
            }
            button.onClick.AddListener(OnDialogueClick);
        }
    }

    void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= ApplyLanguage;
    }

    private void ApplyLanguage(string lang)
    {
        koreanText?.SetActive(false);
        englishText?.SetActive(false);
        japaneseText?.SetActive(false);
        chineseText?.SetActive(false);
        kazahustanText?.SetActive(false);

        lang = lang.ToLower();

        switch (lang)
        {
            case "korean":
                koreanText?.SetActive(true);
                currentTextUI = koreanText.GetComponent<TextMeshProUGUI>();
                dialogueLines = new string[]
                {
                    "여기가 어딘지 모르겠어...",
                    "왜 이런 곳에 혼자 있는 거지?",
                    "분명 뭔가 잘못된 게 틀림없어.",
                    "주변을 좀 살펴봐야겠어.",
                    "저기 무슨 소리가 들리는 것 같아.",
                    "가까이 가면 뭔가 알 수 있을지도 몰라.",
                    "그래, 조심히 가보자."
                };
                break;
            case "english":
                englishText?.SetActive(true);
                currentTextUI = englishText.GetComponent<TextMeshProUGUI>();
                dialogueLines = new string[]
                {
                    "I don't know where this is...",
                    "Why am I alone in a place like this?",
                    "Something must be wrong.",
                    "I should look around.",
                    "I think I hear something over there.",
                    "Maybe I'll figure it out if I get closer.",
                    "Okay, let's go carefully."
                };
                break;
            case "japanese":
                japaneseText?.SetActive(true);
                currentTextUI = japaneseText.GetComponent<TextMeshProUGUI>();
                dialogueLines = new string[]
                {
                    "ここがどこかわからない……。",
                    "どうしてこんな場所に一人でいるんだろう？",
                    "きっと何かがおかしい。",
                    "周囲を調べてみよう。",
                    "あっちから音が聞こえる気がする。",
                    "近づけば何かわかるかもしれない。",
                    "よし、慎重に行こう。"
                };
                break;
            case "chinese":
                chineseText?.SetActive(true);
                currentTextUI = chineseText.GetComponent<TextMeshProUGUI>();
                dialogueLines = new string[]
                {
                    "我不知道这里是哪里……",
                    "为什么我会独自在这个地方？",
                    "肯定发生了什么不对劲的事情。",
                    "我得看看周围的情况。",
                    "好像从那边传来什么声音。",
                    "靠近一点或许能知道发生了什么。",
                    "好，慢慢靠近看看。"
                };
                break;
            case "kazahustan":
                kazahustanText?.SetActive(true);
                currentTextUI = kazahustanText.GetComponent<TextMeshProUGUI>();
                dialogueLines = new string[]
                {
                    "Бұл жердің қайда екенін білмеймін...",
                    "Неге мен осындай жерде жалғызбын?",
                    "Бір нәрсе дұрыс емес сияқты.",
                    "Маңа-айналаны қарап шығуым керек.",
                    "Ана жақтан бір дыбыс естілгендей.",
                    "Жақындасам, мүмкін не болып жатқанын түсінермін.",
                    "Жарайды, абайлап барайын."
                };
                break;
            default:
                Debug.LogWarning("[FirstCardLanguage] Unknown language");
                return;
        }

        currentLineIndex = 0;
        Debug.Log($"[Dialogue] 🔰 시작 대사: Line {currentLineIndex + 1}/{dialogueLines.Length}");
        StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
    }

    
    private void OnDialogueClick()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            currentTextUI.text = dialogueLines[currentLineIndex];
            isTyping = false;
            isWaitingForNext = true;
            return;
        }

        if (isWaitingForNext)
        {
            bool isLastLine = (currentLineIndex == dialogueLines.Length - 1); // ✅ 먼저 판단!

            if (!isLastLine)
            {
                currentLineIndex++;
                Debug.Log($"[Dialogue] ▶ 다음 대사: Line {currentLineIndex + 1}/{dialogueLines.Length}");
                StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
            }

            else
            {
                EndDialogue();

                if (gameManager != null)
                {
                    Debug.Log("[Dialogue] Last line clicked → Start timer manually.");
                    
                    gameManager.StartTimerManually();
                }
            }
        }
    }




    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        isWaitingForNext = false;
        currentTextUI.text = "";
        EnableCardControl(false);

        foreach (char c in line)
        {
            currentTextUI.text += c;
            yield return new WaitForSeconds(0.04f);
        }

        isTyping = false;
        isWaitingForNext = true;
    }

    private void EndDialogue()
    {
        currentTextUI.text = "";
        EnableCardControl(true);

        dialogueLines = null;
        isWaitingForNext = false;
        isTyping = false;

        if (dialogueImage != null)
        {
            dialogueImage.SetActive(false);
        }

        if (firstPanel != null)
        {
            firstPanel.SetActive(false); // ✅ 핵심
        }

        Debug.Log("[Dialogue] Finished. Dialogue deactivated.");
    }


    private void EnableCardControl(bool enable)
    {
        if (cardCanvasGroups != null)
        {
            foreach (var cg in cardCanvasGroups)
            {
                if (cg != null)
                {
                    cg.interactable = enable;
                    cg.blocksRaycasts = enable;
                }
            }
        }
    }
    
    
    
}
