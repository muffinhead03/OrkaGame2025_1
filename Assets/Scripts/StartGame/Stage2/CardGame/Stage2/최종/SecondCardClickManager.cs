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
    [SerializeField] private bool startMusicOnPointerDown = true; // ↓ 누르는 즉시 음악 재생

// 같은 카드를 Down 직후 Up 할 때 재시작 튕김 방지용
    private int lastMusicCard = -1;
    private float lastMusicTime = -999f;
    [SerializeField] private float reTriggerBlockWindow = 0.2f; // 초
    [Header("CardClick 루트 (초기 비활성 추천)")]
    [SerializeField] private GameObject cardClickRoot;

    [Header("자막 데이터 (SecondCardClickKeyedLinesMB 참조)")]
    [SerializeField] private SecondCardClickKeyedLinesMB secondLinesMB;

    [Header("언어별 컨테이너 (각 컨테이너 안에 TMP 1개 + 해당 언어 폰트 에셋 지정)")]
    [SerializeField] private GameObject koreanClick;
    [SerializeField] private GameObject japaneseClick;
    [SerializeField] private GameObject englishClick;
    [SerializeField] private GameObject chineseClick;
    [SerializeField] private GameObject kazakhClick;

    [Header("대사 DB (슬롯별 6줄) - 자막엔 미사용, 호환용")]
    [SerializeField] private SecondCardClickLinesDB linesDB;

    [Header("오디오 (카드별 AudioSource)")]
    [Tooltip("카드별 오디오 소스(1~10번, 인덱스 0=1번 카드). 비어있으면 해당 카드 음악 없음으로 간주")]
    [SerializeField] private AudioSource[] cardAudioSources = new AudioSource[10];

    [Tooltip("7번 카드의 '지속되는' 사운드 전용(AudioSource). Stop하지 않음.")]
    [SerializeField] private AudioSource persistentSource7;

    [Tooltip("효과음/원샷 재생용(필요 없으면 비워둬도 됨)")]
    [SerializeField] private AudioSource sfxSource; // 현재 스크립트에서는 미사용

    [Header("7번 카드 전용 SFX (0.5초 지연 재생) - AudioSource 직접 재생")]
    [SerializeField] private AudioSource specialSfx7Source;

    [Header("9번 카드 전용 화면 물들임 패널")]
    [Tooltip("Image가 달린 패널(최상단 캔버스 추천). 알파/컬러를 4초 동안 변화시킴")]
    [SerializeField] private Image tintPanelImage;
    [SerializeField] private Color tintColor = new Color(1f, 0f, 0f, 0.6f);
    [SerializeField] private float tintTotalDuration = 4f;

    [Header("디버그 옵션 (콘솔 전용 로그)")]
    [SerializeField] private bool verboseLog = true;

    [Header("편의 옵션")]
    [Tooltip("처음 클릭하면 자동으로 UI를 풀고(Unlock) 즉시 처리까지 진행")]
    [SerializeField] private bool autoUnlockOnFirstClick = true;

    [Tooltip("테스트용. 시작하자마자 UnlockAndShow() 호출")]
    [SerializeField] private bool unlockOnStartForTest = false;

    // 내부 상태
    private string currentLang;              // normalized: korean/japanese/english/chinese/kazakh
    private TextMeshProUGUI activeTMP;       // 현재 언어 컨테이너 안의 TMP
    private bool uiLocked = true;            // 대사 끝날 때 풀기

    // 음악 동작 카드(1-based 기준)
    private static readonly int[] CardsWithMusic = { 1, 2, 3, 4, 5, 7, 8, 10 };

    // 자막 적용 카드 고정 인덱스 매핑 (1,2,3,7,8,10 → 0~5)
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
            default:
                fixedIndex = -1; return false; // 4,5,6,9 → 자막 없음
        }
    }
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
            t -= Time.unscaledDeltaTime; // 일시정지 무시하고 돌리려면 unscaled 사용
            yield return null;
        }
        if (gameEnded) yield break;

        // 타임오버 → 실패 전환
        TransitionFail();
    }

    private void RegisterCardVisited(int oneBased)
    {
        // 자막 대상 카드만 카운트
        for (int i = 0; i < CaptionCards.Length; i++)
            if (CaptionCards[i] == oneBased) { visitedCaptionCards.Add(oneBased); break; }
    }

    private bool IsAllCaptionCardsVisited()
    {
        // 1,2,3,7,8,10이 모두 눌렸는지
        for (int i = 0; i < CaptionCards.Length; i++)
            if (!visitedCaptionCards.Contains(CaptionCards[i])) return false;
        return true;
    }

    private bool CheckClearCondition()
    {
        // 조건: 모든 자막 카드 방문 + 당근 흔들기 완료
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

    void Awake()
    {
        var raw = LanguageManager.GetLanguage();
        currentLang = NormalizeLang(raw);

        if (cardClickRoot) cardClickRoot.SetActive(false);

        // tintPanel 초기 세팅 (처음엔 투명)
        if (tintPanelImage != null)
        {
            var c = tintPanelImage.color;
            c.a = 0f;
            tintPanelImage.color = c;
            tintPanelImage.gameObject.SetActive(false);
        }

        if (verboseLog)
            Debug.Log($"[2ndCardClick] Awake: lang={currentLang}, rootActive={cardClickRoot?.activeSelf}");

#if UNITY_EDITOR
        ValidateSetup();
#endif
    }

    void Start()
    {
        if (unlockOnStartForTest)
        {
            UnlockAndShow();
            if (verboseLog) Debug.Log("[2ndCardClick] Start: unlockOnStartForTest → UnlockAndShow()");
        }
    }

    void OnEnable()  { LanguageManager.OnLanguageChanged += OnLangChanged; }
    void OnDisable() { LanguageManager.OnLanguageChanged -= OnLangChanged; }

    private void OnLangChanged(string _)
    {
        currentLang = NormalizeLang(LanguageManager.GetLanguage());
        if (!uiLocked && cardClickRoot && cardClickRoot.activeInHierarchy)
            ActivateUIForCurrentLanguage();
        else
            RebindTMPOnly();

        if (verboseLog) Debug.Log($"[2ndCardClick] OnLangChanged → {currentLang}");
    }

    /// 대사/클릭 UI를 사용할 시점에 호출 (대화 끝난 뒤 등)
    public void UnlockAndShow()
    {
        uiLocked = false;
        ActivateUIForCurrentLanguage();

        if (activeTMP != null)
        {
            activeTMP.text = ""; // READY 같은 표시 제거
            SetTMPColorBlack();
        }

        if (verboseLog) Debug.Log("[2ndCardClick] UnlockAndShow()");
    }

    /// 슬롯(0~9) 클릭 시 호출
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

        // === 음악 처리 ===
        HandleMusicForCard(oneBased);

        // === 9번 특수 이펙트 ===
        if (oneBased == 9 && tintPanelImage != null)
        {
            StopCoroutine(nameof(TintRoutine));
            StartCoroutine(TintRoutine());
        }

        // === 자막 처리 (고정 인덱스 매핑) ===
        if (TryGetFixedIndexForCard(oneBased, out int clickIdx))
        {
            ActivateUIForCurrentLanguage();
            EnsureActiveTMP();

            if (activeTMP == null)
            {
                Debug.LogWarning("[2ndCardClick] activeTMP가 없습니다. 언어 컨테이너에 TextMeshProUGUI가 있는지 확인하세요.");
                return;
            }

            // 데이터에서 현재 언어로 1줄 (MB 내부에서도 폴백하나, 보수적 안전망 유지)
            string text = secondLinesMB ? secondLinesMB.GetLine(currentLang, clickIdx) : string.Empty;
            if (string.IsNullOrWhiteSpace(text) && secondLinesMB != null)
                text = secondLinesMB.GetLine("english", clickIdx);

            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogWarning($"[2ndCardClick] 빈 자막: card={oneBased}, idx={clickIdx}. secondLinesMB/영문 라인 채워주세요.");
                // 이 줄을 보여주고 싶지 않으면 return; 로 바꾸세요.
                text = ""; 
            }

            // 루트/컨테이너 보이기 + 출력
            if (cardClickRoot && !cardClickRoot.activeInHierarchy) EnableHierarchy(cardClickRoot);
            if (cardClickRoot && !cardClickRoot.activeSelf) cardClickRoot.SetActive(true);

            activeTMP.gameObject.SetActive(true);
            SetTMPColorBlack();
            activeTMP.SetText(text, true);

            if (verboseLog) Debug.Log($"[2ndCardClick] Caption shown: card={oneBased}, idx={clickIdx}, len={text?.Length}");
        }
        else
        {
            // 자막 없는 카드(4,5,6,9)
            if (cardClickRoot && cardClickRoot.activeSelf)
                cardClickRoot.SetActive(false);
            if (verboseLog) Debug.Log($"[2ndCardClick] No caption for card {oneBased}");
        }
    }

    // ===== Helpers =====

    private void HandleMusicForCard(int oneBased)
    {
        bool needMusic = Contains(CardsWithMusic, oneBased);
        int idx = oneBased - 1;

        if (verboseLog) Debug.Log($"[2ndCardClick] Click card {oneBased} (needMusic={needMusic})");

        // 새 카드가 '음악 카드'일 때만 기존 음악을 모두 정리하고 교체
        if (needMusic)
        {
            // 다른 카드 소스는 정지 (7번의 persistentSource7은 건드리지 않음)
            for (int i = 0; i < cardAudioSources.Length; i++)
            {
                if (i == idx) continue; // 현재 카드 제외
                var src = cardAudioSources[i];
                if (src && src.isPlaying) src.Stop();
            }

            // 현재 카드 소스 재생
            if (idx >= 0 && idx < cardAudioSources.Length)
            {
                var src = cardAudioSources[idx];
                if (src)
                {
                    if (src.isPlaying) src.Stop(); // 다시 누르면 리트리거
                    src.Play();
                }
            }
        }
        // 음악 없는 카드(예: 6, 9) 클릭 시에는 기존 음악 유지

        // 7번 특수: 지속 소스는 계속, 보조 SFX는 지연 재생
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
// 마우스를/손가락을 '내리는' 순간 호출용 (자막만 표시, 연출/음향 X)
    // 마우스를/손가락을 '내리는' 순간 호출
    public void OnCardPointerDown(int slotIndexZeroBased)
    {
        if (uiLocked && autoUnlockOnFirstClick) UnlockAndShow();
        if (uiLocked) return;

        if (slotIndexZeroBased < 0 || slotIndexZeroBased >= 10)
        {
            Debug.LogWarning($"[2ndCardClick] (DOWN) invalid slot={slotIndexZeroBased}");
            return;
        }

        // 1) 타이머 시작(최초 입력 시)
        StartGameTimerIfNeeded();

        // 2) 자막 즉시 표시
        ShowCaptionForSlot(slotIndexZeroBased);

        int oneBased = slotIndexZeroBased + 1;

        // 3) 음악 즉시 재생(옵션)
        if (startMusicOnPointerDown)
        {
            HandleMusicForCard(oneBased);
            lastMusicCard = oneBased;                  // 업(Click) 시 중복 재생 방지용
            lastMusicTime = Time.unscaledTime;
        }

        // 4) 방문 체크(자막 카드만 기록)
        RegisterCardVisited(oneBased);

        // 5) 전부 방문 + 당근 흔들기 완료 시 즉시 클리어
        if (!gameEnded && CheckClearCondition())
            TransitionSuccess();
    }



// 자막만 표시하는 내부 유틸 (음악/틴트 등 연출 없음)
    private void ShowCaptionForSlot(int slotIndexZeroBased)
    {
        int oneBased = slotIndexZeroBased + 1;

        if (TryGetFixedIndexForCard(oneBased, out int clickIdx))
        {
            ActivateUIForCurrentLanguage();
            EnsureActiveTMP();

            if (activeTMP == null)
            {
                Debug.LogWarning("[2ndCardClick] (DOWN) activeTMP가 없습니다. 언어 컨테이너에 TMP가 있는지 확인하세요.");
                return;
            }

            string text = secondLinesMB ? secondLinesMB.GetLine(currentLang, clickIdx) : string.Empty;
            if (string.IsNullOrWhiteSpace(text) && secondLinesMB != null)
                text = secondLinesMB.GetLine("english", clickIdx);

            // 비어있으면 그냥 빈 문자열 유지(디버그 문구는 자막에 안 넣음)
            if (cardClickRoot && !cardClickRoot.activeInHierarchy) EnableHierarchy(cardClickRoot);
            if (cardClickRoot && !cardClickRoot.activeSelf) cardClickRoot.SetActive(true);

            activeTMP.gameObject.SetActive(true);
            SetTMPColorBlack();
            activeTMP.SetText(text ?? string.Empty, true);

            if (verboseLog) Debug.Log($"[2ndCardClick] (DOWN) caption card={oneBased}, idx={clickIdx}, len={text?.Length}");
        }
        else
        {
            // 자막 없는 카드면 숨김
            if (cardClickRoot && cardClickRoot.activeSelf)
                cardClickRoot.SetActive(false);
            if (verboseLog) Debug.Log($"[2ndCardClick] (DOWN) no caption for card {oneBased}");
        }
    }


    private IEnumerator PlayDelayedSfx7(float delay)
    {
        if (verboseLog) Debug.Log($"[2ndCardClick] 7번 보조 SFX {delay}초 후 재생 예정");
        yield return new WaitForSeconds(delay);

        if (specialSfx7Source != null)
        {
            if (specialSfx7Source.isPlaying)
                specialSfx7Source.Stop();   // 다시 누르면 리트리거

            specialSfx7Source.Play();
            if (verboseLog) Debug.Log("[2ndCardClick] 7번 보조 SFX 재생 (AudioSource)");
        }
    }

    private IEnumerator TintRoutine()
    {
        float half = Mathf.Max(0.01f, tintTotalDuration * 0.5f);

        tintPanelImage.gameObject.SetActive(true);

        Color upTarget = tintColor;
        upTarget.a = tintColor.a;

        // 상승(half초)
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / half);
            tintPanelImage.color = Color.Lerp(new Color(upTarget.r, upTarget.g, upTarget.b, 0f), upTarget, r);
            yield return null;
        }
        tintPanelImage.color = upTarget;

        // 복귀(half초)
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / half);
            tintPanelImage.color = Color.Lerp(upTarget, new Color(upTarget.r, upTarget.g, upTarget.b, 0f), r);
            yield return null;
        }
        // 완전 투명으로
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

    private void ActivateUIForCurrentLanguage()
    {
        // 잠금 여부와 상관없이 '먼저' 루트를 켬 (부모까지)
        if (cardClickRoot)
        {
            if (!cardClickRoot.activeInHierarchy)
                EnableHierarchy(cardClickRoot);
            else if (!cardClickRoot.activeSelf)
                cardClickRoot.SetActive(true);
        }

        // 그 다음 잠금 가드
        if (uiLocked) return;

        if (koreanClick)   koreanClick.SetActive(false);
        if (japaneseClick) japaneseClick.SetActive(false);
        if (englishClick)  englishClick.SetActive(false);
        if (chineseClick)  chineseClick.SetActive(false);
        if (kazakhClick)   kazakhClick.SetActive(false);

        GameObject target = englishClick;
        switch (currentLang)
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

        if (verboseLog)
            Debug.Log($"[2ndCardClick] ActivateUI lang={currentLang}, target={(target?target.name:"NULL")}, tmp={(activeTMP?activeTMP.name:"NULL")}, rootActive={cardClickRoot?.activeSelf}");
    }

    private void RebindTMPOnly()
    {
        GameObject[] candidates = { koreanClick, japaneseClick, chineseClick, kazakhClick, englishClick, cardClickRoot };
        activeTMP = null;
        foreach (var go in candidates)
        {
            if (go == null) continue;
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) { activeTMP = tmp; break; }
        }
    }

    private void EnsureActiveTMP()
    {
        if (activeTMP != null) return;
        RebindTMPOnly();
        if (activeTMP == null)
            Debug.LogWarning("[2ndCardClick] TextMeshProUGUI를 찾지 못했습니다. 언어 컨테이너에 TMP가 들어있는지 확인하세요.");
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

    private static bool Contains(int[] arr, int val)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == val) return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate() => ValidateSetup();

    private void ValidateSetup()
    {
        // 1) 데이터 참조
        if (secondLinesMB == null)
            Debug.LogWarning("[2ndCardClick][Setup] secondLinesMB가 비었습니다. 자막이 비게 됩니다.");

        // 2) 컨테이너에 TMP가 있는지 간단 체크
        CheckTMP("Korean",  koreanClick);
        CheckTMP("Japanese",japaneseClick);
        CheckTMP("English", englishClick);
        CheckTMP("Chinese", chineseClick);
        CheckTMP("Kazakh",  kazakhClick);

        // 3) 카드 오디오 길이
        if (cardAudioSources == null || cardAudioSources.Length < 10)
            Debug.LogWarning("[2ndCardClick][Setup] cardAudioSources 길이는 10(카드 1~10)이어야 합니다.");

        // 4) 루트 null 체크
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

    // --- 디버그 보조 (선택) ---
    [ContextMenu("DBG/Simulate Click 1")]  private void DBG_Click1()  => OnCardClickedBySlot(0);
    [ContextMenu("DBG/Simulate Click 7")]  private void DBG_Click7()  => OnCardClickedBySlot(6);
    [ContextMenu("DBG/Simulate Click 10")] private void DBG_Click10() => OnCardClickedBySlot(9);
}
