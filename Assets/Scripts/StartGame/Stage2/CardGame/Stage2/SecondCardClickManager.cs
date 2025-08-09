using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SecondCardClickManager : MonoBehaviour
{

    [Header("CardClick 루트 (초기 비활성 추천)")]
    [SerializeField] private GameObject cardClickRoot;

    [Header("언어별 컨테이너 (각 컨테이너 안에 TMP 1개 + 해당 언어 폰트 에셋 지정)")]
    [SerializeField] private GameObject koreanClick;
    [SerializeField] private GameObject japaneseClick;
    [SerializeField] private GameObject englishClick;
    [SerializeField] private GameObject chineseClick;
    [SerializeField] private GameObject kazakhClick;

    [Header("대사 DB (슬롯별 6줄)")]
    [SerializeField] private SecondCardClickLinesDB linesDB;

    [Header("오디오 (카드별 AudioSource)")]
    [Tooltip("카드별 오디오 소스(1~10번, 인덱스 0=1번 카드). 비어있으면 해당 카드 음악 없음으로 간주")]
    [SerializeField] private AudioSource[] cardAudioSources = new AudioSource[10];

    [Tooltip("7번 카드의 '지속되는' 사운드 전용(AudioSource). a조건 적용 제외, Stop하지 않음.")]
    [SerializeField] private AudioSource persistentSource7;

    [Tooltip("효과음/원샷 재생용(필요 없으면 비워둬도 됨)")]
    [SerializeField] private AudioSource sfxSource; // 현재 스크립트에서는 사용 안 하지만 남겨둠

    [Header("7번 카드 전용 SFX (0.5초 지연 재생) - AudioSource 직접 재생")]
    [SerializeField] private AudioSource specialSfx7Source;

    [Header("9번 카드 전용 화면 물들임 패널")]
    [Tooltip("Image가 달린 패널(최상단 캔버스 추천). 알파/컬러를 4초 동안 변화시킴")]
    [SerializeField] private Image tintPanelImage;
    [SerializeField] private Color tintColor = new Color(1f, 0f, 0f, 0.6f);
    [SerializeField] private float tintTotalDuration = 4f;

    [Header("디버그 옵션")]
    [SerializeField] private bool verboseLog = true;
    [SerializeField] private bool debugEchoInCaption = true;     // 클릭 디버그를 자막에도 찍기
    [SerializeField] private bool showReadyHintOnUnlock = true;  // Unlock 시 'READY' 메시지 표시
    [SerializeField] private string readyHintText = "READY: Click a card"; // 준비 메시지

    [Header("편의 옵션")]
    [Tooltip("처음 클릭하면 자동으로 UI를 풀고(Unlock) 즉시 처리까지 진행")]
    [SerializeField] private bool autoUnlockOnFirstClick = true;

    [Tooltip("테스트용. 시작하자마자 UnlockAndShow() 호출")]
    [SerializeField] private bool unlockOnStartForTest = false;

    // 내부 상태
    private string currentLang;              // normalized: korean/japanese/english/chinese/kazakh
    private TextMeshProUGUI activeTMP;       // 현재 언어 컨테이너 안의 TMP
    private bool uiLocked = true;            // 대사 끝날 때 풀기
    private int[] perSlotClickCount = new int[10];

    // 자막/음악 동작 카드(1-based 기준)
    private static readonly int[] CardsWithCaption = { 1, 2, 3, 7, 10 };
    private static readonly int[] CardsWithMusic   = { 1, 2, 3, 4, 5, 7, 8, 10 };
// 1,2,3,7,8,10번만 자막, 나머진 없음
    private static bool TryGetFixedIndexForCard(int cardNumberOneBased, out int fixedIndex)
    {
        switch (cardNumberOneBased)
        {
            case 1: fixedIndex = 0; return true;
            case 2: fixedIndex = 1; return true;
            case 3: fixedIndex = 2; return true;
            case 7: fixedIndex = 3; return true;
            case 8: fixedIndex = 4; return true;
            case 10: fixedIndex = 5; return true;
            default:
                fixedIndex = -1; return false; // 4,5,6,9 → 자막 없음
        }
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

        // 품질: 누락 레퍼런스 경고
        if (!cardClickRoot) Debug.LogWarning("[2ndCardClick] cardClickRoot가 비어 있습니다.");
        if (!koreanClick && !japaneseClick && !englishClick && !chineseClick && !kazakhClick)
            Debug.LogWarning("[2ndCardClick] 언어 컨테이너들이 전부 비어 있습니다. TMP 바인딩 실패 가능.");

        if (verboseLog) Debug.Log($"[2ndCardClick] Awake: lang={currentLang}, cardClickRootActive={cardClickRoot?.activeSelf}");
    }

    void Start()
    {
        if (unlockOnStartForTest)
        {
            UnlockAndShow();
            if (verboseLog) Debug.Log("[2ndCardClick] Start: unlockOnStartForTest=true → UnlockAndShow()");
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

    /// <summary>대사/클릭 UI를 사용할 시점에 호출 (대화 끝난 뒤 등)</summary>
    public void UnlockAndShow()
    {
        uiLocked = false;
        ActivateUIForCurrentLanguage();

        if (activeTMP != null)
        {
            if (showReadyHintOnUnlock)
                activeTMP.text = $"<color=#00AAFF>{readyHintText}</color>";
            else
                activeTMP.text = "";
            SetTMPColorBlack();
        }


        if (verboseLog) Debug.Log("[2ndCardClick] UnlockAndShow()");
    }

    /// <summary>슬롯(0~9) 클릭 시 호출</summary>
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

        // 음악은 기존 규칙 유지
        bool needMusic = Contains(CardsWithMusic, oneBased);
        if (verboseLog) Debug.Log($"[2ndCardClick] Click card {oneBased} (music={needMusic})");

        // === 음악 처리 (원래 로직 그대로 복붙/유지) ===
        // ... (생략)

        // 9번 특수 이펙트
        if (oneBased == 9 && tintPanelImage != null)
        {
            StopCoroutine(nameof(TintRoutine));
            StartCoroutine(TintRoutine());
        }

        // === 자막: 카드→고정 인덱스 ===
        if (TryGetFixedIndexForCard(oneBased, out int clickIdx))
        {
            ActivateUIForCurrentLanguage();
            if (activeTMP == null) { Debug.LogWarning("[2ndCardClick] activeTMP가 없습니다."); return; }

            string line = (linesDB != null) ? linesDB.GetLineBySlot(slotIndexZeroBased, clickIdx) : "";
            if (string.IsNullOrEmpty(line))
                line = $"[{currentLang}:{oneBased}-{clickIdx + 1}]";

            string debugHeader = debugEchoInCaption
                ? $"<color=#00AAFF>[DBG card={oneBased}, FIXED idx={clickIdx}]</color>\n"
                : "";

            activeTMP.gameObject.SetActive(true);
            SetTMPColorBlack();
            activeTMP.text = debugHeader + line;
            activeTMP.ForceMeshUpdate(true);

            if (verboseLog) Debug.Log($"[2ndCardClick] Caption show card={oneBased}, fixedIdx={clickIdx}, text='{line}'");
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

    /// <summary>
    /// 부모가 꺼져 있어도 카드 루트를 확실히 켜줍니다.
    /// </summary>
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

        if (verboseLog) Debug.Log($"[2ndCardClick] ActivateUI lang={currentLang}, " +
                                  $"root(activeSelf={cardClickRoot?.activeSelf}, inHierarchy={cardClickRoot?.activeInHierarchy}), " +
                                  $"target={(target?target.name:"NULL")}, tmp={(activeTMP?activeTMP.name:"NULL")}");
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

    // --- 디버그 보조 (선택) ---
    [ContextMenu("Dump Active States")]
    public void DumpActiveStates()
    {
        if (!cardClickRoot) { Debug.LogWarning("[2ndCardClick] cardClickRoot=null"); return; }
        var t = cardClickRoot.transform;
        string msg = "[2ndCardClick] ActiveStates:\n";
        while (t != null)
        {
            msg += $"- {t.name}: activeSelf={t.gameObject.activeSelf}, inHierarchy={t.gameObject.activeInHierarchy}\n";
            t = t.parent;
        }
        Debug.Log(msg);
    }
    
    // 에디터에서 우클릭 메뉴로 테스트
    [ContextMenu("DBG/Simulate Click 1")]
    private void DBG_Click1() => OnCardClickedBySlot(0);

    [ContextMenu("DBG/Simulate Click 7")]
    private void DBG_Click7() => OnCardClickedBySlot(6);

    [ContextMenu("DBG/Simulate Click 10")]
    private void DBG_Click10() => OnCardClickedBySlot(9);

}
