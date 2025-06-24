using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CardGame2Manager : MonoBehaviour
{
    [Header("카드 슬롯 및 정답 이름")]
    public CardGame2PanelManager[] slots = new CardGame2PanelManager[10];
    public string[] correctCardNames = new string[10];

    [Header("UI 및 화면 효과")]
    public TextMeshProUGUI timerText;
    public CanvasGroup blackPanel;

    private float timeRemaining = 25f;
    private bool isGameOver = false;
    private bool isClearing = false;

    void Start()
    {
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
            blackPanel.interactable = false;
        }

        UpdateTimerDisplay();
    }

    void Update()
    {
        if (isGameOver || isClearing) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, 25f);
        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            isGameOver = true;

            if (!CheckClearCondition())
            {
                SceneManager.LoadScene("EtInArcadiaEgoAfterFirstCardGame");
            }
            else
            {
                StartCoroutine(HandleCorrectClear());
            }
        }
        else if (CheckClearCondition())
        {
            isClearing = true;
            StartCoroutine(HandleCorrectClear());
        }
    }

    void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        float t = 1f - (timeRemaining / 25f);
        Color newColor = Color.Lerp(Color.white, Color.red, t);
        timerText.color = newColor;
        timerText.text = $"00:{seconds:00}";
    }

    bool CheckClearCondition()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var card = slots[i].GetCurrentCard();
            if (card == null || card.name.Replace("(Clone)", "").Trim() != correctCardNames[i])
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator HandleCorrectClear()
    {
        isGameOver = true;

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FlashBlack());

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Stage2_5");
    }

    IEnumerator FlashBlack()
    {
        float duration = 0.5f;
        float half = duration / 2f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            blackPanel.alpha = Mathf.Lerp(0f, 1f, t / half);
            yield return null;
        }
        blackPanel.alpha = 1f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            blackPanel.alpha = Mathf.Lerp(1f, 0f, t / half);
            yield return null;
        }
        blackPanel.alpha = 0f;
    }
}
