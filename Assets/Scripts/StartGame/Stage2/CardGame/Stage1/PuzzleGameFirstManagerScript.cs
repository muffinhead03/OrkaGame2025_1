using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PuzzleGameFirstManagerScript : MonoBehaviour
{
    private bool timerManuallyStarted = false;
    public CardGame1PanelManager[] slots;
    public string[] correctCardNames;

    public TextMeshProUGUI timerText;
    private float timeRemaining = 15f;
    private bool isGameOver = false;
    private bool isClearing = false;

    public CanvasGroup blackPanel; // 검은 화면용
    public AudioSource fluteAudioSource; // 피리 소리

    public GameObject firstPanel;    // 설정된 외부에서 할당
    public GameObject settingPanel;  // 설정된 외부에서 할당

    void Start()
    {
        // 처음엔 타이머 비활성화 상태
        timerManuallyStarted = false;

        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
            blackPanel.interactable = false;
        }
    }

    void Update()
    {
        if (!timerManuallyStarted || isGameOver || isClearing) return;

        // 패널이 중앙에 있으면 타이머 일시 정지
        if (IsPanelBlocking()) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, 15f);

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
        else
        {
            // 정답을 미리 충족했는지도 확인 (시간이 남았어도)
            if (CheckClearCondition())
            {
                isClearing = true;
                StartCoroutine(HandleCorrectClear());
            }
        }
    }

    void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        float t = 1f - (timeRemaining / 15f);
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

    public void StartTimerManually()
    {
        if (!timerManuallyStarted)
        {
            timerManuallyStarted = true;
            Debug.Log("[TIMER] ▶ Timer started manually!");
        }
    }

    bool IsPanelBlocking()
    {
        Vector2 center = Vector2.zero;

        if (firstPanel != null && Vector2.Distance(firstPanel.transform.position, center) < 1f)
            return true;

        if (settingPanel != null && Vector2.Distance(settingPanel.transform.position, center) < 1f)
            return true;

        return false;
    }

    IEnumerator HandleCorrectClear()
    {
        isGameOver = true; // 타이머 정지

        // 1. 2초 대기 (정답 맞추고 멈춘 상태)
        yield return new WaitForSeconds(2f);

        // 2. 깜빡임 (0.5초 페이드 인 & 아웃)
        yield return StartCoroutine(FlashBlack());

        // 3. 0.3초 후 피리 소리
        yield return new WaitForSeconds(0.3f);
        if (fluteAudioSource != null)
        {
            fluteAudioSource.Play();
        }

        // 4. 0.5초 후 다음 씬
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
