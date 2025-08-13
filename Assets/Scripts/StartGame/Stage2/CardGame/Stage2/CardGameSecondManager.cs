using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CardGameSecondManager : MonoBehaviour
{
    [Header("카드 슬롯 및 정답 이름")]
    public CardGame2PanelManager[] slots = new CardGame2PanelManager[10];
    public string[] correctCardNames = new string[10];

    [Header("UI 및 화면 효과")]
    public TextMeshProUGUI timerText;
    public CanvasGroup blackPanel;
    public GameObject carrotObject;

    // ✅ 일시정지 기준 패널들
    [Header("일시정지 트리거 패널들")]
    public RectTransform firstPanel;     // 인스펙터에 할당
    public RectTransform settingPanel;   // 인스펙터에 할당
    [Tooltip("패널이 (0,0,0)에 있는 것으로 간주할 허용 오차(픽셀)")]
    public float centerTolerance = 1f;

    // ✅ 25 → 120초
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
            carrotObject.SetActive(true);

        UpdateTimerDisplay();
    }

    void Update()
    {
        if (isGameOver || isClearing) return;

        // 🔴 여기서 패널 상태를 보고 타이머/클리어 체크를 멈춤
        if (IsPausedByPanel())
        {
            // 멈춘 상태에서도 표시만 업데이트하고 종료
            UpdateTimerDisplay();
            return;
        }

        timeRemaining -= Time.deltaTime;

        // ✅ 최대값도 25 → 120으로 변경
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

    // ✅ 패널이 활성 & 중앙이면 true
    bool IsPausedByPanel()
    {
        return IsPanelBlocking(firstPanel) || IsPanelBlocking(settingPanel);
    }

    bool IsPanelBlocking(RectTransform rt)
    {
        if (rt == null) return false;
        if (!rt.gameObject.activeInHierarchy) return false;

        // UI 기준 중앙 여부 체크(허용 오차 포함)
        Vector3 p = rt.anchoredPosition3D;
        return p.sqrMagnitude <= centerTolerance * centerTolerance;
    }

    public void OnCarrotClicked()
    {
        // 일시정지 중에는 클릭 무시
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

        // ✅ 색상 변화 비율도 120초 기준
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
}
