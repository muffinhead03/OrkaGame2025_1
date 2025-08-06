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

    private float timeRemaining = 25f;
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

        // 카드들이 비활성화되어 있다면 여기서 모두 켜줌
        foreach (var slot in slots)
        {
            if (slot != null && !slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(true);
            }
        }

        // 당근 오브젝트도 바로 활성화
        if (carrotObject != null)
            carrotObject.SetActive(true);

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
                // 정답은 맞췄지만 시간 초과 → 그냥 기다림
                isClearConditionMet = true;
                if (carrotObject != null)
                    carrotObject.SetActive(true); // 당근 보이기
            }
        }
        else if (CheckClearCondition() && !isClearConditionMet)
        {
            isClearConditionMet = true;
            if (carrotObject != null)
                carrotObject.SetActive(true); // 당근 보이기
        }
    }

    public void OnCarrotClicked()
    {
        if (!isClearConditionMet || isClearing) return;

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

}
