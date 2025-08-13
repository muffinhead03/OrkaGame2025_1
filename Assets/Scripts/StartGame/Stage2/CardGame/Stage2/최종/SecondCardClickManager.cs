using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class SecondCardClickManager : MonoBehaviour
{
    [Header("게임 진행/전환")]
    [SerializeField] private float timeLimitSeconds = 120f; // 2분
    [SerializeField] private string successScene = "Stage2_3_1";
    [SerializeField] private string failScene    = "EtInArcadiaEgoAfterSecondCardGame";
    [SerializeField] private bool startTimerOnFirstInput = true;

    private bool carrotShaken = false;
    private bool gameEnded = false;
    private Coroutine timerCo;
    private HashSet<int> visitedCaptionCards = new HashSet<int>(); // 1-based (1,2,3,7,8,10)
    private static readonly int[] CaptionCards = { 1, 2, 3, 7, 8, 10 };

    [Header("입력 타이밍 옵션")]
    [SerializeField] private bool startMusicOnPointerDown = true;

    private int lastMusicCard = -1;
    private float lastMusicTime = -999f;
    [SerializeField] private float reTriggerBlockWindow = 0.2f;

    [Header("CardClick 루트(말풍선) - 초기 비활성")]
    [SerializeField] private GameObject cardClickRoot;

    [Header("자막 데이터 (SecondCardClickKeyedLinesMB 참조)")]
    [SerializeField] private SecondCardClickKeyedLinesMB secondLinesMB;

    [Header("언어별 컨테이너 (각 컨테이너 안에 TMP 1개)")]
    [SerializeField] private GameObject koreanClick;
    [SerializeField] private GameObject japaneseClick;
    [SerializeField] private GameObject englishClick;
    [SerializeField] private GameObject chineseClick;
    [SerializeField] private GameObject kazakhClick;

    [Header("대사 DB (슬롯별 6줄) - 자막엔 미사용, 호환용")]
    [SerializeField] private SecondCardClickLinesDB linesDB;

    [Header("오디오 (카드별 AudioSource)")]
    [SerializeField] private AudioSource[] cardAudioSources = new AudioSource[10];
    [SerializeField] private AudioSource persistentSource7;
    [SerializeField] private AudioSource sfxSource; // 미사용

    [Header("7번 카드 전용 SFX (0.5초 지연)")]
    [SerializeField] private AudioSource specialSfx7Source;

    [Header("9번 카드 전용 화면 물들임 패널")]
    [SerializeField] private Image tintPanelImage;
    [SerializeField] private Color tintColor = new Color(1f, 0f, 0f, 0.6f);
    [SerializeField] private float tintTotalDuration = 4f;

    [Header("디버그 옵션")]
    [SerializeField] private bool verboseLog = true;

    [Header("편의 옵션")]
    [SerializeField] private bool autoUnlockOnFirstClick = true;
    [SerializeField] private bool unlockOnStartForTest = false;

    // 내부 상태
    private string currentLang;              // normalized
    private TextMeshProUGUI activeTMP;       // 현재 언어 컨테이너 안의 TMP
    private bool uiLocked = true;            // 처음엔 잠금

    // 음악 동작 카드(1-based)
    private static readonly int[] CardsWithMusic = { 1, 2, 3, 4, 5, 7, 8, 10 };

    // ============= 유틸 =============

    private static bool TryGetFixedIndexForCard(int cardNumberOneBased, out int fixedIndex)
    {
        switch (cardNumberOneBased)
        {
            case 1:  fixedIndex = 0; return true;
            case 2:  fixedIndex = 1; return true;
            case 3:  fixedIndex = 2; return true;
            case 7:  fixedIndex = 3; return true;
            case 8:  fixedIndex = 4; return true;
            case 10: fixedIndex = 5; return true;
            default: fixedIndex = -1; return false; // 4,5,6,9 → 자막 없음
        }
    }

    private static bool Contains(int[] arr, int val)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == val) return true;
        return false;
    }

    // 루트의 활성/비활성만 담당 (언어 컨테이너는 건드리지 않음)
    private void SetBalloonRootActive(bool on)
    {
        if (!cardClickRoot) return;
        if (on)
        {
            if (!cardClickRoot.activeInHierarchy) EnableHierarchy(cardClickRoot);
            else if (!cardClickRoot.activeSelf)   cardClickRoot.SetActive(true);
        }
        else
        {
            if (cardClickRoot.activeSelf) cardClickRoot.SetActive(false);
        }
    }

    // 언어 컨테이너만 토글 (루트는 절대 건드리지 않음)
    private void ShowOnlyLanguageContainer(string lang)
    {
        if (koreanClick)   koreanClick.SetActive(false);
        if (japaneseClick) japaneseClick.SetActive(false);
        if (englishClick)  englishClick.SetActive(false);
        if (chineseClick)  chineseClick.SetActive(false);
        if (kazakhClick)   kazakhClick.SetActive(false);

        GameObject target = englishClick;
        switch (lang)
        {
            case "korean":   target = koreanClick;   break;
            case "japanese": target = japaneseClick; break;
            case "chinese":  target = chineseClick;  break;
            case "kazakh":   target = kazakhClick;   break;
        }

        if (target) target.SetActive(true);

        // TMP 재바인딩
        activeTMP = target ? target.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        if (activeTMP == null && cardClickRoot != null)
            activeTMP = cardClickRoot.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void EnsureActiveTMP()
    {
        if (activeTMP != null) return;
        GameObject[] candidates = { koreanClick, japaneseClick, chineseClick, kazakhClick, englishClick, cardClickRoot };
        foreach (var go in candidates)
        {
            if (!go) continue;
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) { activeTMP = tmp; break; }
        }
        if (activeTMP == null)
            Debug.LogWarning("[2ndCardClick] TextMeshProUGUI를 찾지 못했습니다.");
    }

    private void SetTMPColorBlack()
    {
        if (activeTMP == null) return;
        var col = activeTMP.color; col.a = 1f; col.r = 0f; col.g = 0f; col.b = 0f;
        activeTMP.color = col;
    }

    private static string NormalizeLang(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "english";
        raw = raw.ToLowerInvariant();
        if (raw == "chinese" || raw == "zh" || raw.StartsWith("zh-") || raw.Contains("chinese"))
            return "chinese";
        if (raw.StartsWith("ko") || raw == "korean")                         return "korean";
        if (raw.StartsWith("ja") || raw == "japanese")                       return "japanese";
        if (raw.StartsWith("ka") || raw == "kazakh" || raw == "kazakhstan")  return "kazakh";
        if (raw.StartsWith("en") || raw == "english")                        return "english";
        return "english";
    }

    // ============= 라이프사이클 =============

    void Awake()
    {
        // 언어 초기화
        var raw = LanguageManager.GetLanguage();
        currentLang = NormalizeLang(raw);

        // 시작은 잠금
        uiLocked = true;

        // 말풍선 루트 강제 OFF
        if (cardClickRoot && cardClickRoot.activeSelf)
            cardClickRoot.SetActive(false);

        // 언어 컨테이너 전부 OFF
        if (koreanClick)   koreanClick.SetActive(false);
        if (japaneseClick) japaneseClick.SetActive(false);
        if (englishClick)  englishClick.SetActive(false);
        if (chineseClick)  chineseClick.SetActive(false);
        if (kazakhClick)   kazakhClick.SetActive(false);

        activeTMP = null;

        // 틴트 패널 초기화
        if (tintPanelImage != null)
        {
            var c = tintPanelImage.color;
            c.a = 0f;
            tintPanelImage.color = c;
            tintPanelImage.gameObject.SetActive(false);
        }

        if (verboseLog)
            Debug.Log($"[2ndCardClick] Awake: lang={currentLang}, uiLocked={uiLocked}, rootActive={(cardClickRoot ? cardClickRoot.activeSelf : (bool?)null)}");

#if UNITY_EDITOR
        ValidateSetup();
#endif
    }

    void Start()
    {
        if (unlockOnStartForTest)
        {
            // 테스트 옵션: 잠금만 풀고, 바로 보여주지는 않음
            uiLocked = false;
            if (verboseLog) Debug.Log("[2ndCardClick] Start: unlockOnStartForTest (루트는 여전히 OFF)");
        }
    }

    void OnEnable()  { LanguageManager.OnLanguageChanged += OnLangChanged; }
    void OnDisable() { LanguageManager.OnLanguageChanged -= OnLangChanged; }

    private void OnLangChanged(string _)
    {
        currentLang = NormalizeLang(LanguageManager.GetLanguage());

        // 잠겨있으면 아무 것도 켜지 않음
        if (uiLocked)
        {
            if (verboseLog) Debug.Log($"[2ndCardClick] OnLangChanged(locked) → {currentLang}");
            return;
        }

        // 루트 상태는 유지. 루트가 켜져 있을 때만 언어 컨테이너를 교체
        if (cardClickRoot && cardClickRoot.activeInHierarchy && cardClickRoot.activeSelf)
        {
            ShowOnlyLanguageContainer(currentLang);
            if (verboseLog) Debug.Log($"[2ndCardClick] OnLangChanged visible → {currentLang}");
        }
        else
        {
            // 루트가 꺼져있으면 언어 컨테이너도 꺼둔다
            if (koreanClick)   koreanClick.SetActive(false);
            if (japaneseClick) japaneseClick.SetActive(false);
            if (englishClick)  englishClick.SetActive(false);
            if (chineseClick)  chineseClick.SetActive(false);
            if (kazakhClick)   kazakhClick.SetActive(false);
            activeTMP = null;
            if (verboseLog) Debug.Log($"[2ndCardClick] OnLangChanged hidden (root OFF) → {currentLang}");
        }
    }

    /// 외부에서 대화 끝나고 클릭 UI를 사용할 수 있게 할 때 호출
    public void UnlockAndShow()
    {
        // 잠금만 해제. 루트는 여전히 OFF
        uiLocked = false;
        // 언어 컨테이너는 누를 때 켜짐
        if (verboseLog) Debug.Log("[2ndCardClick] UnlockAndShow(): uiUnlocked, root remains OFF until caption card");
    }

    // ============= 입력 =============

    public void OnCardPointerDown(int slotIndexZeroBased)
    {
        if (uiLocked && autoUnlockOnFirstClick) UnlockAndShow();
        if (uiLocked) return;

        if (slotIndexZeroBased < 0 || slotIndexZeroBased >= 10)
        {
            Debug.LogWarning($"[2ndCardClick] (DOWN) invalid slot={slotIndexZeroBased}");
            return;
        }

        // 타이머 시작
        StartGameTimerIfNeeded();

        // 자막만 즉시 표시
        ShowCaptionForSlot(slotIndexZeroBased);

        int oneBased = slotIndexZeroBased + 1;

        // 음악 즉시 재생(옵션)
        if (startMusicOnPointerDown)
        {
            HandleMusicForCard(oneBased);
            lastMusicCard = oneBased;
            lastMusicTime = Time.unscaledTime;
        }

        // 방문 체크
        RegisterCardVisited(oneBased);

        // 클리어 체크
        if (!gameEnded && CheckClearCondition())
            TransitionSuccess();
    }

    public void OnCardClickedBySlot(int slotIndexZeroBased)
    {
        if (uiLocked && autoUnlockOnFirstClick) UnlockAndShow();
        if (uiLocked) return;

        if (slotIndexZeroBased < 0 || slotIndexZeroBased >= 10)
        {
            Debug.LogWarning($"[2ndCardClick] invalid slot={slotIndexZeroBased}");
            return;
        }

        int oneBased = slotIndexZeroBased + 1;

        // 음악 처리
        HandleMusicForCard(oneBased);

        // 9번 특수 이펙트
        if (oneBased == 9 && tintPanelImage != null)
        {
            StopCoroutine(nameof(TintRoutine));
            StartCoroutine(TintRoutine());
        }

        // 자막 처리
        if (TryGetFixedIndexForCard(oneBased, out int clickIdx))
        {
            // 1) 루트 켜기 (처음 보이는 순간)
            SetBalloonRootActive(true);

            // 2) 현재 언어 컨테이너만 ON (루트는 이미 ON)
            ShowOnlyLanguageContainer(currentLang);

            // 3) TMP 보장
            EnsureActiveTMP();
            if (activeTMP == null)
            {
                Debug.LogWarning("[2ndCardClick] activeTMP 없음");
                return;
            }

            // 4) 텍스트 표시
            string text = secondLinesMB ? secondLinesMB.GetLine(currentLang, clickIdx) : string.Empty;
            if (string.IsNullOrWhiteSpace(text) && secondLinesMB != null)
                text = secondLinesMB.GetLine("english", clickIdx);

            activeTMP.gameObject.SetActive(true);
            SetTMPColorBlack();
            activeTMP.SetText(text ?? string.Empty, true);

            if (verboseLog) Debug.Log($"[2ndCardClick] Caption shown: card={oneBased}, idx={clickIdx}, len={text?.Length}");
        }
        else
        {
            // 자막 없는 카드 → 루트 OFF
            SetBalloonRootActive(false);
            // 언어 컨테이너도 다 OFF (루트와 분리된 묶음이므로 확실히 내림)
            if (koreanClick)   koreanClick.SetActive(false);
            if (japaneseClick) japaneseClick.SetActive(false);
            if (englishClick)  englishClick.SetActive(false);
            if (chineseClick)  chineseClick.SetActive(false);
            if (kazakhClick)   kazakhClick.SetActive(false);
            activeTMP = null;

            if (verboseLog) Debug.Log($"[2ndCardClick] No caption for card {oneBased}");
        }
    }

    // 자막만 표시(음악/틴트 없음)
    private void ShowCaptionForSlot(int slotIndexZeroBased)
    {
        if (uiLocked && autoUnlockOnFirstClick) UnlockAndShow();
        if (uiLocked) return;

        int oneBased = slotIndexZeroBased + 1;

        if (TryGetFixedIndexForCard(oneBased, out int clickIdx))
        {
            // 루트 켜고 언어 컨테이너 선택
            SetBalloonRootActive(true);
            ShowOnlyLanguageContainer(currentLang);

            EnsureActiveTMP();
            if (activeTMP == null)
            {
                Debug.LogWarning("[2ndCardClick] (DOWN) activeTMP가 없습니다.");
                return;
            }

            string text = secondLinesMB ? secondLinesMB.GetLine(currentLang, clickIdx) : string.Empty;
            if (string.IsNullOrWhiteSpace(text) && secondLinesMB != null)
                text = secondLinesMB.GetLine("english", clickIdx);

            activeTMP.gameObject.SetActive(true);
            SetTMPColorBlack();
            activeTMP.SetText(text ?? string.Empty, true);

            if (verboseLog) Debug.Log($"[2ndCardClick] (DOWN) caption card={oneBased}, idx={clickIdx}, len={text?.Length}");
        }
        else
        {
            // 자막 없음 → 루트/컨테이너 OFF
            SetBalloonRootActive(false);
            if (koreanClick)   koreanClick.SetActive(false);
            if (japaneseClick) japaneseClick.SetActive(false);
            if (englishClick)  englishClick.SetActive(false);
            if (chineseClick)  chineseClick.SetActive(false);
            if (kazakhClick)   kazakhClick.SetActive(false);
            activeTMP = null;

            if (verboseLog) Debug.Log($"[2ndCardClick] (DOWN) no caption for card {oneBased}");
        }
    }

    // ============= 게임 흐름 =============

    private void StartGameTimerIfNeeded()
    {
        if (gameEnded) return;
        if (!startTimerOnFirstInput) return;
        if (timerCo != null) return;
        timerCo = StartCoroutine(TimerRoutine());
        if (verboseLog) Debug.Log("[2ndCardClick] Timer started");
    }

    private IEnumerator TimerRoutine()
    {
        float t = timeLimitSeconds;
        while (t > 0f && !gameEnded)
        {
            t -= Time.unscaledDeltaTime; // 일시정지 무시
            yield return null;
        }
        if (gameEnded) yield break;
        TransitionFail();
    }

    private void RegisterCardVisited(int oneBased)
    {
        for (int i = 0; i < CaptionCards.Length; i++)
            if (CaptionCards[i] == oneBased) { visitedCaptionCards.Add(oneBased); break; }
    }

    private bool IsAllCaptionCardsVisited()
    {
        for (int i = 0; i < CaptionCards.Length; i++)
            if (!visitedCaptionCards.Contains(CaptionCards[i])) return false;
        return true;
    }

    private bool CheckClearCondition()
    {
        return IsAllCaptionCardsVisited() && carrotShaken;
    }

    public void NotifyCarrotShaken()
    {
        if (carrotShaken) return;
        carrotShaken = true;
        if (verboseLog) Debug.Log("[2ndCardClick] Carrot shaken!");
        if (!gameEnded && CheckClearCondition()) TransitionSuccess();
    }

    private void TransitionSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (timerCo != null) StopCoroutine(timerCo);
        if (verboseLog) Debug.Log("[2ndCardClick] SUCCESS → Load " + successScene);
        SceneManager.LoadScene(successScene);
    }

    private void TransitionFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (verboseLog) Debug.Log("[2ndCardClick] FAIL (timeout) → Load " + failScene);
        SceneManager.LoadScene(failScene);
    }

    // ============= 이펙트/오디오 등 =============

    private void HandleMusicForCard(int oneBased)
    {
        bool needMusic = Contains(CardsWithMusic, oneBased);
        int idx = oneBased - 1;

        if (verboseLog) Debug.Log($"[2ndCardClick] Click card {oneBased} (needMusic={needMusic})");

        if (needMusic)
        {
            for (int i = 0; i < cardAudioSources.Length; i++)
            {
                if (i == idx) continue;
                var src = cardAudioSources[i];
                if (src && src.isPlaying) src.Stop();
            }

            if (idx >= 0 && idx < cardAudioSources.Length)
            {
                var src = cardAudioSources[idx];
                if (src)
                {
                    if (src.isPlaying) src.Stop();
                    src.Play();
                }
            }
        }

        if (oneBased == 7)
        {
            if (persistentSource7 && !persistentSource7.isPlaying)
                persistentSource7.Play();

            if (specialSfx7Source != null)
            {
                StopCoroutine(nameof(PlayDelayedSfx7));
                StartCoroutine(PlayDelayedSfx7(0.5f));
            }
        }
    }

    private IEnumerator PlayDelayedSfx7(float delay)
    {
        if (verboseLog) Debug.Log($"[2ndCardClick] 7번 보조 SFX {delay}초 후 재생 예정");
        yield return new WaitForSeconds(delay);

        if (specialSfx7Source != null)
        {
            if (specialSfx7Source.isPlaying)
                specialSfx7Source.Stop();
            specialSfx7Source.Play();
            if (verboseLog) Debug.Log("[2ndCardClick] 7번 보조 SFX 재생 (AudioSource)");
        }
        yield break;
    }

    private IEnumerator TintRoutine()
    {
        float half = Mathf.Max(0.01f, tintTotalDuration * 0.5f);

        tintPanelImage.gameObject.SetActive(true);

        Color upTarget = tintColor;
        upTarget.a = tintColor.a;

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / half);
            tintPanelImage.color = Color.Lerp(new Color(upTarget.r, upTarget.g, upTarget.b, 0f), upTarget, r);
            yield return null;
        }
        tintPanelImage.color = upTarget;

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / half);
            tintPanelImage.color = Color.Lerp(upTarget, new Color(upTarget.r, upTarget.g, upTarget.b, 0f), r);
            yield return null;
        }

        var c = tintPanelImage.color; c.a = 0f; tintPanelImage.color = c;
        tintPanelImage.gameObject.SetActive(false);

        if (verboseLog) Debug.Log("[2ndCardClick] 9번 물들임 효과 종료");
    }

    /// 부모가 꺼져 있어도 카드 루트를 확실히 켜줍니다.
    private void EnableHierarchy(GameObject go)
    {
        if (go == null) return;
        var t = go.transform;
        List<Transform> chain = new List<Transform>();
        while (t != null) { chain.Add(t); t = t.parent; }
        for (int i = chain.Count - 1; i >= 0; i--)
            chain[i].gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    private void OnValidate() => ValidateSetup();

    private void ValidateSetup()
    {
        if (secondLinesMB == null)
            Debug.LogWarning("[2ndCardClick][Setup] secondLinesMB가 비었습니다. 자막이 비게 됩니다.");

        CheckTMP("Korean",  koreanClick);
        CheckTMP("Japanese",japaneseClick);
        CheckTMP("English", englishClick);
        CheckTMP("Chinese", chineseClick);
        CheckTMP("Kazakh",  kazakhClick);

        if (cardAudioSources == null || cardAudioSources.Length < 10)
            Debug.LogWarning("[2ndCardClick][Setup] cardAudioSources 길이는 10(카드 1~10)이어야 합니다.");

        if (cardClickRoot == null)
            Debug.LogWarning("[2ndCardClick][Setup] cardClickRoot가 비었습니다.");
    }

    private void CheckTMP(string label, GameObject go)
    {
        if (!go) return;
        var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
            Debug.LogWarning($"[2ndCardClick][Setup] {label} 컨테이너에 TextMeshProUGUI가 없습니다.");
    }
#endif

    // --- 디버그 보조 ---
    [ContextMenu("DBG/Simulate Click 1")]  private void DBG_Click1()  => OnCardClickedBySlot(0);
    [ContextMenu("DBG/Simulate Click 7")]  private void DBG_Click7()  => OnCardClickedBySlot(6);
    [ContextMenu("DBG/Simulate Click 10")] private void DBG_Click10() => OnCardClickedBySlot(9);
}
