using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PuzzleGameFirstManagerScript : MonoBehaviour
{
    [Header("퍼즐 슬롯/정답")]
    public CardGame1PanelManager[] slots;
    public string[] correctCardNames;

    [Header("타이머 UI")]
    public TextMeshProUGUI timerText;

    [Header("UI 패널(가림 여부)")]
    public GameObject firstPanel;    // 외부에서 할당
    public GameObject settingPanel;  // 외부에서 할당

    [Header("성공 연출 요소")]
    [SerializeField] private GameObject carrotObject;     // 당근 오브젝트
    [SerializeField] private AudioSource fluteAudioSource;
    [SerializeField] private CanvasGroup blackPanel;      // 페이드 플래시용(CanvasGroup)

    [Header("대사 종료 후 이미지 교체 (선택)")]
    [SerializeField] private Image targetUIImage;
    [SerializeField] private Sprite spriteAfterHint;
    [SerializeField] private bool setNativeSize = false;  // (권장: false) 원본 사이즈 강제
    private bool spriteSwapped = false;

    [Header("게임 진행/전환")]
    [SerializeField] private float timeLimitSeconds = 120f;         // ⏱ 2분
    [SerializeField] private string successScene = "Stage2_3_1";    // 성공
    [SerializeField] private string failScene    = "EtInArcadiaEgoAfterSecondCardGame"; // 실패

    // 내부 상태
    private float timeRemaining;
    private bool timerRunning = false;  // 수동 시작
    private bool isDialoguePlaying = true;
    private bool isGameOver = false;
    private bool isClearing = false;

    // ===== Unity Hooks =====
    void Start()
    {
        // 타이머 초기화
        timeRemaining = timeLimitSeconds;
        UpdateTimerDisplay();

        // 블랙 패널 초기화
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.blocksRaycasts = false;
            blackPanel.interactable = false;
        }

        // 대사 중에는 당근 숨김
        if (carrotObject != null) carrotObject.SetActive(false);

        isDialoguePlaying = true;
    }

    void Update()
    {
        if (!timerRunning || isGameOver || isClearing) return;
        if (IsPanelBlocking()) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, timeLimitSeconds);
        UpdateTimerDisplay();

        if (timeRemaining <= 0f)
        {
            isGameOver = true;
            SceneManager.LoadScene(failScene);
        }
    }

    // ===== 외부에서 호출 =====
    public void StartTimerManually()
    {
        if (!timerRunning)
        {
            timerRunning = true;
            Debug.Log("[TIMER] ▶ Timer started manually!");
        }
    }

    public void EndDialogue()
    {
        isDialoguePlaying = false;

        TrySwapPostDialogueSprite();

        if (carrotObject != null)
            carrotObject.SetActive(true); // 대사 끝나면 당근 보이기
    }

    public void OnCarrotClicked()
    {
        if (isDialoguePlaying)
        {
            Debug.Log("[Carrot] 대사 출력 중이라 무시");
            return;
        }
        if (isGameOver || isClearing)
        {
            Debug.Log("[Carrot] 이미 종료 상태");
            return;
        }

        if (CheckClearCondition())
        {
            Debug.Log("[Carrot] 정답! 흔들 연출 시작");
            isGameOver = true;
            isClearing = true;
            StartCoroutine(CarrotShakeAndLoadScene());
        }
        else
        {
            Debug.Log("[Carrot] 정답 아님 → 무시");
        }
    }

    // 대사 끝났는지 외부에서 체크할 때 사용 가능
    public bool IsDialogueComplete() => !isDialoguePlaying;

    // ===== 내부 로직 =====
    private bool CheckClearCondition()
    {
        if (slots == null || correctCardNames == null || slots.Length != correctCardNames.Length)
        {
            Debug.LogWarning("[CheckClear] slots / correctCardNames 길이가 맞지 않음");
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var card = slots[i]?.GetCurrentCard();
            if (card == null)
            {
                Debug.Log($"🔍 Slot {i} is empty.");
                return false;
            }

            string expected = correctCardNames[i];
            string actual = card.name.Replace("(Clone)", "").Trim();

            if (!string.Equals(actual, expected, System.StringComparison.Ordinal))
            {
                Debug.Log($"❌ Slot {i} mismatch: expected '{expected}', got '{actual}'");
                return false;
            }

            Debug.Log($"✅ Slot {i} matched: {actual}");
        }

        return true;
    }

    private void UpdateTimerDisplay()
    {
        int secondsTotal = Mathf.CeilToInt(timeRemaining);
        int minutes = secondsTotal / 60;
        int seconds = secondsTotal % 60;

        float t = 1f - (timeRemaining / Mathf.Max(1f, timeLimitSeconds));
        if (timerText != null)
        {
            timerText.color = Color.Lerp(Color.white, Color.red, t);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private bool IsPanelBlocking()
    {
        // center(0,0)에 패널이 오면 '가림'으로 간주하는 기존 규칙 유지
        Vector2 center = Vector2.zero;

        if (firstPanel != null && Vector2.Distance(firstPanel.transform.position, center) < 1f)
            return true;

        if (settingPanel != null && Vector2.Distance(settingPanel.transform.position, center) < 1f)
            return true;

        return false;
    }

    // ===== 연출(당근 흔들 + 사운드 + 플래시 + 성공 씬) =====
    private IEnumerator CarrotShakeAndLoadScene()
    {
        // 흔들기
        if (carrotObject != null)
        {
            yield return RotateZ(carrotObject.transform, +20f, 0.5f); // 좌
            yield return RotateZ(carrotObject.transform, -40f, 1.0f); // 우
            yield return RotateZ(carrotObject.transform, +40f, 1.0f); // 좌
            yield return RotateZ(carrotObject.transform, -40f, 1.0f); // 우
            carrotObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        // 사운드
        if (fluteAudioSource != null)
        {
            fluteAudioSource.Play();
            yield return new WaitWhile(() => fluteAudioSource.isPlaying);
        }

        // 플래시
        if (blackPanel != null)
            yield return FlashBlack(0.5f);

        // 다음 씬
        SceneManager.LoadScene(successScene);
    }

    private IEnumerator RotateZ(Transform target, float relativeAngle, float duration)
    {
        if (!target) yield break;

        float startZ = target.localEulerAngles.z;
        float targetZ = startZ + relativeAngle;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            float z = Mathf.LerpAngle(startZ, targetZ, k);
            target.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        target.localRotation = Quaternion.Euler(0f, 0f, targetZ);
    }

    private IEnumerator FlashBlack(float duration)
    {
        if (blackPanel == null) yield break;

        float half = duration * 0.5f;
        blackPanel.blocksRaycasts = true;

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
        blackPanel.blocksRaycasts = false;
    }

    // 대사 후 이미지 교체
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

        var rt = targetUIImage.rectTransform;
        Vector2 sizeDelta   = rt.sizeDelta;
        Vector2 anchoredPos = rt.anchoredPosition;
        Vector3 localScale  = rt.localScale;
        Vector2 anchorMin   = rt.anchorMin;
        Vector2 anchorMax   = rt.anchorMax;
        Vector2 pivot       = rt.pivot;
        Vector3 localRot    = rt.localEulerAngles;

        targetUIImage.preserveAspect = true;

        var arf = targetUIImage.GetComponent<AspectRatioFitter>();
        var csf = targetUIImage.GetComponent<ContentSizeFitter>();
        bool arfEnabled = false, csfEnabled = false;
        if (arf != null) { arfEnabled = arf.enabled; arf.enabled = false; }
        if (csf != null) { csfEnabled = csf.enabled; csf.enabled = false; }

        string beforeName = targetUIImage.sprite ? targetUIImage.sprite.name : "(null)";
        targetUIImage.sprite = spriteAfterHint;

        if (setNativeSize) targetUIImage.SetNativeSize(); // 필요시만 사용(크기 튐 주의)

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        rt.localScale = localScale;
        rt.localEulerAngles = localRot;

        if (arf != null) arf.enabled = arfEnabled;
        if (csf != null) csf.enabled = csfEnabled;

        Debug.Log($"[IMG] 스프라이트 교체: {beforeName} → {spriteAfterHint.name}");
    }
}
