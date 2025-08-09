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
    [SerializeField] 
    private float totalTimeSeconds = 60f; // ✅ 전체 제한시간 (기본 60초)
    private float timeRemaining;
    private bool isGameOver = false;
    private bool isClearing = false;

    public CanvasGroup blackPanel; // 검은 화면용
    public AudioSource fluteAudioSource; // 피리 소리

    public GameObject firstPanel;    // 설정된 외부에서 할당
    public GameObject settingPanel;  // 설정된 외부에서 할당

    
    public GameObject carrotObject; // 당근 오브젝트 (Inspector에서 연결)
    private bool isDialoguePlaying = true; // 대사 출력 중 여부
    
    [Header("Post-Dialogue Image Swap")]
    [SerializeField] private Image targetUIImage;     // 유지할 오브젝트의 Image 컴포넌트
    [SerializeField] private Sprite spriteAfterHint;  // 대사 끝나고 바꿀 새 스프라이트
    [SerializeField] private bool setNativeSize = false; // 필요하면 원본 사이즈 맞추기
    private bool spriteSwapped = false; // 중복 교체 방지
    
    void Start()
    {
        // 처음엔 타이머 비활성화 상태
        timerManuallyStarted = false;

        // ✅ 타이머 초기값 세팅
        timeRemaining = totalTimeSeconds;
        UpdateTimerDisplay(); // 초기 화면에 바로 표시

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

        // ✅ 여기서 이미지 교체 수행 (CardClick 활성화 시점과 동일 프레임)
        TrySwapPostDialogueSprite();

        if (carrotObject != null)
            carrotObject.SetActive(true); // 대사 끝나면 보이기
    }
    
    private void TrySwapPostDialogueSprite()
    {
        if (spriteSwapped) return; 
        spriteSwapped = true;

        if (targetUIImage == null)
        {
            Debug.LogWarning("[IMG] targetUIImage가 비어있어요. 인스펙터에 UI Image를 넣어주세요.");
            return;
        }
        if (spriteAfterHint == null)
        {
            Debug.LogWarning("[IMG] spriteAfterHint가 비어있어요. 교체할 스프라이트를 넣어주세요.");
            return;
        }

        // ✅ RectTransform 물리량 백업
        var rt = targetUIImage.rectTransform;
        Vector2 sizeDelta   = rt.sizeDelta;
        Vector2 anchoredPos = rt.anchoredPosition;
        Vector3 localScale  = rt.localScale;
        Vector2 anchorMin   = rt.anchorMin;
        Vector2 anchorMax   = rt.anchorMax;
        Vector2 pivot       = rt.pivot;
        Vector3 localRot    = rt.localEulerAngles;

        // (선택) 자동으로 Aspect 유지 – 큰 스프라이트가 들어와도 너비/높이 유지
        targetUIImage.preserveAspect = true;

        // (선택) 혹시 붙어있을 수 있는 사이즈 관련 컴포넌트들 잠깐 꺼두기
        var arf = targetUIImage.GetComponent<AspectRatioFitter>();
        var csf = targetUIImage.GetComponent<ContentSizeFitter>();
        bool arfEnabled = false, csfEnabled = false;
        if (arf != null) { arfEnabled = arf.enabled; arf.enabled = false; }
        if (csf != null) { csfEnabled = csf.enabled; csf.enabled = false; }

        // 교체
        string beforeName = targetUIImage.sprite ? targetUIImage.sprite.name : "(null)";
        targetUIImage.sprite = spriteAfterHint;

        // ❌ SetNativeSize는 쓰지 않는 것이 핵심 (크기 튀는 원인)
        // if (setNativeSize) targetUIImage.SetNativeSize();

        // ✅ RectTransform 물리량 복원
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        rt.localScale = localScale;
        rt.localEulerAngles = localRot;

        // 비활성했던 보조 컴포넌트 복구
        if (arf != null) arf.enabled = arfEnabled;
        if (csf != null) csf.enabled = csfEnabled;

        Debug.Log($"[IMG] 스프라이트 교체 완료: {beforeName} → {spriteAfterHint.name} | " +
                  $"pos={rt.anchoredPosition}, size={rt.sizeDelta}, scale={rt.localScale}");
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
        // ✅ 상한/하한을 totalTimeSeconds 기준으로
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, totalTimeSeconds);

        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            isGameOver = true;
            SceneManager.LoadScene("EtInArcadiaEgoAfterFirstCardGame");
        }
    }



    void UpdateTimerDisplay()
    {
        // ✅ MM:SS 표기 (원하면 그대로 00:SS도 가능)
        int secondsTotal = Mathf.CeilToInt(timeRemaining);
        int minutes = secondsTotal / 60;
        int seconds = secondsTotal % 60;

        // ✅ 색상 보간도 총 시간 기준
        float t = 1f - (timeRemaining / totalTimeSeconds);
        Color newColor = Color.Lerp(Color.white, Color.red, t);
        timerText.color = newColor;

        timerText.text = $"{minutes:00}:{seconds:00}";
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
    // PuzzleGameFirstManagerScript 내부 아무데나(예: EndDialogue() 아래)에 추가
    public bool IsDialogueComplete()
    {
        // 대사가 끝났다면 true
        return !isDialoguePlaying;
    }

}
