using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CardGameSecondManager : MonoBehaviour
{
    [Header("카드 슬롯 및 정답 이름")]
    public CardGame2PanelManager[] slots = new CardGame2PanelManager[10];

    [System.Serializable]
    public class SlotAnswer
    {
        public string[] acceptableNames;
    }

    public SlotAnswer[] correctCardNames = new SlotAnswer[10];

    [Header("UI 및 화면 효과")]
    public TextMeshProUGUI timerText;
    public CanvasGroup blackPanel;
    public GameObject carrotObject;

    [Header("일시정지 트리거 패널들")]
    public RectTransform firstPanel;
    public RectTransform settingPanel;

    [Tooltip("패널이 (0,0,0)에 있는 것으로 간주할 허용 오차(픽셀)")]
    public float centerTolerance = 1f;

    private float timeRemaining = 120f;
    private bool isGameOver = false;
    private bool isClearing = false;
    private bool isClearConditionMet = false;

    void Start()
    {
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
            blackPanel.interactable = false;
        }

        foreach (var slot in slots)
        {
            if (slot != null && !slot.gameObject.activeSelf)
                slot.gameObject.SetActive(true);
        }

        if (carrotObject != null)
            carrotObject.SetActive(false);

        UpdateTimerDisplay();
    }

    void Update()
    {
        if (isGameOver || isClearing) return;

        if (IsPausedByPanel())
        {
            UpdateTimerDisplay();
            return;
        }

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, 120f);

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
                isClearConditionMet = true;
                if (carrotObject != null)
                    carrotObject.SetActive(true);
            }
        }
        else if (CheckClearCondition() && !isClearConditionMet)
        {
            isClearConditionMet = true;
            if (carrotObject != null)
                carrotObject.SetActive(true);
        }
    }

    bool IsPausedByPanel()
    {
        return IsPanelBlocking(firstPanel) || IsPanelBlocking(settingPanel);
    }

    bool IsPanelBlocking(RectTransform rt)
    {
        if (rt == null) return false;
        if (!rt.gameObject.activeInHierarchy) return false;

        Vector3 p = rt.anchoredPosition3D;
        return p.sqrMagnitude <= centerTolerance * centerTolerance;
    }

    public void OnCarrotClicked()
    {
        if (!isClearConditionMet || isClearing || IsPausedByPanel()) return;

        Debug.Log("🥕 당근 클릭됨 → 연출 및 씬 전환 시작");
        StartCoroutine(CarrotShakeAndLoadScene());
    }

    IEnumerator CarrotShakeAndLoadScene()
    {
        isClearing = true;

        float rotTime = 0.4f;

        yield return RotateZ(carrotObject.transform, 20f, rotTime);
        yield return RotateZ(carrotObject.transform, -40f, 0.4f);
        yield return RotateZ(carrotObject.transform, 40f, 0.4f);
        yield return RotateZ(carrotObject.transform, -40f, 0.4f);

        carrotObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        yield return new WaitForSeconds(0.5f);

        if (blackPanel != null)
            yield return StartCoroutine(FlashBlack());

        SceneManager.LoadScene("Stage2_3_1");
    }

    IEnumerator RotateZ(Transform target, float relativeAngle, float duration)
    {
        float startZ = target.localEulerAngles.z;
        float targetZ = startZ + relativeAngle;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float z = Mathf.LerpAngle(startZ, targetZ, t);
            target.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        target.localRotation = Quaternion.Euler(0f, 0f, targetZ);
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

    void UpdateTimerDisplay()
    {
        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        float t = 1f - (timeRemaining / 120f);
        Color newColor = Color.Lerp(Color.white, Color.red, t);

        if (timerText != null)
        {
            timerText.color = newColor;
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    bool CheckClearCondition()
    {
        Debug.Log("===== 카드 정답 검사 시작 =====");

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                Debug.Log($"[슬롯 {i}] 슬롯 자체가 NULL");
                return false;
            }

            var card = slots[i].GetCurrentCard();

            if (card == null)
            {
                Debug.Log($"[슬롯 {i}] 카드 없음");
                return false;
            }

            string cardName = card.name.Replace("(Clone)", "").Trim();
            Debug.Log($"[슬롯 {i}] 현재 카드 이름: {cardName}");

            if (correctCardNames[i] == null)
            {
                Debug.Log($"[슬롯 {i}] correctCardNames NULL");
                return false;
            }

            if (correctCardNames[i].acceptableNames == null || correctCardNames[i].acceptableNames.Length == 0)
            {
                Debug.Log($"[슬롯 {i}] acceptableNames 비어 있음");
                return false;
            }

            Debug.Log($"[슬롯 {i}] 허용된 정답 목록:");

            bool matchFound = false;

            foreach (string acceptable in correctCardNames[i].acceptableNames)
            {
                Debug.Log($"   → {acceptable}");

                if (cardName == acceptable)
                {
                    Debug.Log($"[슬롯 {i}] 정답 매칭 성공!");
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                Debug.Log($"[슬롯 {i}] ❌ 정답 불일치");
                return false;
            }
        }

        Debug.Log("🎉 모든 슬롯 정답! 클리어 조건 만족");
        return true;
    }
}