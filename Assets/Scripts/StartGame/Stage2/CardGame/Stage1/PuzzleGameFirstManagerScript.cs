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

    
    public GameObject carrotObject; // 당근 오브젝트 (Inspector에서 연결)
    private bool isDialoguePlaying = true; // 대사 출력 중 여부
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
        isDialoguePlaying = true;

        if (carrotObject != null)
            carrotObject.SetActive(false); // 대사 중 숨기기
    }

    public void EndDialogue()
    {
        isDialoguePlaying = false;

        if (carrotObject != null)
            carrotObject.SetActive(true); // 대사 끝나면 보이기
    }
    
    public void OnCarrotClicked()
    {
        Debug.Log("[당근 클릭] OnCarrotClicked() 호출됨");

        if (isDialoguePlaying)
        {
            Debug.Log("[당근 클릭] 대사 출력 중이라 무시됨");
            return;
        }

        if (isGameOver || isClearing)
        {
            Debug.Log("[당근 클릭] 이미 종료 상태라 무시됨");
            return;
        }

        if (CheckClearCondition())
        {
            Debug.Log("[당근 클릭] 정답 조건 충족, 연출 시작");
            isGameOver = true;
            isClearing = true;
            StartCoroutine(CarrotShakeAndLoadScene());
        }
        else
        {
            Debug.Log("[당근 클릭] 정답이 아님 → 아무 일도 없음");
        }
    }

    IEnumerator CarrotShakeSequence()
    {
        float shakeDuration = 0.5f;

        // 흔들기 방향: 좌 → 우 → 좌 → 우
        yield return RotateZ(carrotObject.transform, 20f, shakeDuration);
        yield return RotateZ(carrotObject.transform, -40f, 0.5f);
        yield return RotateZ(carrotObject.transform, 40f, 0.5f);
        yield return RotateZ(carrotObject.transform, -40f, 0.5f);

        // 원위치
        carrotObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        // 0.5초 후 다음 씬
        yield return new WaitForSeconds(0.5f);

        // 페이드 효과 (선택)
        if (fluteAudioSource != null) fluteAudioSource.Play();
        yield return StartCoroutine(FlashBlack());

        SceneManager.LoadScene("Stage2_3");
    }
    
    IEnumerator CarrotShakeAndLoadScene()
    {
        isGameOver = true;
        isClearing = true;

        float rotTime = 0.5f;

        // 애니메이션 효과
        yield return RotateZ(carrotObject.transform, 20f, rotTime);     // 좌
        yield return RotateZ(carrotObject.transform, -40f, 1.0f);       // 우
        yield return RotateZ(carrotObject.transform, 40f, 1.0f);        // 좌
        yield return RotateZ(carrotObject.transform, -40f, 1.0f);       // 우

        // 회전 리셋
        carrotObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // 사운드 재생
        Debug.Log("🎵 Playing flute...");
        if (fluteAudioSource != null)
        {
            fluteAudioSource.Play();
            yield return new WaitWhile(() => fluteAudioSource.isPlaying);
        }

        Debug.Log("🖤 Flashing black...");
        yield return StartCoroutine(FlashBlack());

        Debug.Log("🎯 Loading next scene: Stage2_3");
        SceneManager.LoadScene("Stage2_3");
    }

    IEnumerator RotateTo(Transform target, Quaternion to, float duration)
    {
        Quaternion from = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
    }
    IEnumerator RotateZ(Transform target, float relativeAngle, float duration)
    { Debug.Log($"[ROTATE] 시작 - 상대각도: {relativeAngle}, 시간: {duration}");
        float startZ = target.localEulerAngles.z;
        float targetZ = startZ + relativeAngle;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float z = Mathf.LerpAngle(startZ, targetZ, t);
            target.rotation = Quaternion.Euler(0f, 0f, z);

            yield return null;
        }

        target.localRotation = Quaternion.Euler(0f, 0f, targetZ);
        Debug.Log($"[ROTATE] 완료 → 최종 회전 Z: {targetZ}");
    }




    void Update()
    {
        if (!timerManuallyStarted || isGameOver || isClearing) return;
        if (IsPanelBlocking()) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, 15f);

        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            isGameOver = true;
            SceneManager.LoadScene("EtInArcadiaEgoAfterFirstCardGame");
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

            if (card == null)
            {
                Debug.Log($"🔍 Slot {i} is empty.");
                return false;
            }

            string expected = correctCardNames[i];
            string actual = card.name.Replace("(Clone)", "").Trim();

            if (actual != expected)
            {
                Debug.Log($"❌ Slot {i} mismatch: expected '{expected}', got '{actual}'");
                return false;
            }

            Debug.Log($"✅ Slot {i} matched: {actual}");
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
        // ✅ 더 이상 다음 씬 이동하지 않음
        // 👉 그냥 대사 후 당근 클릭을 기다림

        isGameOver = true;

        yield return new WaitForSeconds(2f);

        // 기존 연출 유지
        if (fluteAudioSource != null)
            fluteAudioSource.Play();
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
