using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;                     // 타이머 표시용
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//
// AfterDeath 전용 매니저 (+ 5분 타이머, 성공/실패 분기)
// - 퍼즐 이동 규칙 적용
// - 열쇠/Lock1/Lock2: 타일→정확 위치 매핑(순서 고려) 판정
// - 화면/지정 오브젝트 클릭 시 즉시 클리어(성공 씬 이동)
// - 타이머: 5분, 0초에 성공 미완이면 실패 씬 이동
//   Key:  (2,4,9) → {1,2,4}/{2,3,5}/{4,5,7}/{5,6,8} 중 하나
//   Lock1:(1,3,5,7) → A:{2,5,8,4} 또는 B:{3,6,9,5}  (열쇠 후)
//   Lock2:(1,3,5,6) → A:{7,1,4,5} 또는 B:{8,2,5,6}  (열쇠+Lock1 후)
//
public class SlidingGameManager2AfterDeath : MonoBehaviour
{
    // ================== [A] 씬 이동/스킵 ==================
    [Header("씬 이동")]
    [SerializeField] private string successSceneName = "Stage3_2";
    [SerializeField] private string failSceneName    = "EtArcadiaEndingIfslidingdeath";
    [SerializeField] private float loadDelay = 0.1f;

    [Header("우측 아이콘 (옵션)")]
    public CanvasGroup keyIcon;      // 열쇠
    public CanvasGroup lock1Icon;    // 1차 자물쇠
    public CanvasGroup lock2Icon;    // 2차 자물쇠
    [Range(0f,1f)] public float clearedAlpha = 0.3f;

    [Header("사운드/연출(옵션)")]
    public AudioSource sfxOnClear;

    [Header("클릭해서 넘어가는 오브젝트")]
    [Tooltip("이 오브젝트를 클릭하면 성공 씬으로 넘어갑니다. 비어있으면 풀스크린 투명 버튼 자동 생성(옵션).")]
    public GameObject clickToClearObject;
    public bool autoCreateFullscreenButtonIfNone = true;

    // ================== [TIMER] ==================
    [Header("Timer")]
    [SerializeField, Min(0)] private int startMinutes = 5;           // 기본 5분
    [SerializeField, HideInInspector] private float totalDurationSeconds = 300f;
    [SerializeField] private TextMeshProUGUI timerText;               // 선택 연결
    private float timer;
    private bool sceneLoading = false;
    private bool clearedByClick = false;                              // 클릭으로 성공했는지

    // ================== [B] 퍼즐 보드/이동 규칙 ==================
    public static SlidingGameManager2AfterDeath Instance;

    [Header("Board (UI Panel)")]
    public RectTransform boardPanel;

    [Header("Puzzle Refs (0=빈칸, 1~9 퍼즐)")]
    [Tooltip("퍼즐 10개(0~9). 0은 빈칸 역할 오브젝트(또는 더미)")]
    public SlidingPuzzle2AfterDeath[] puzzleScripts; // 반드시 10개

    [Header("Board Positions (anchored, index 0~9)")]
    public Vector2[] boardPositions; // 반드시 10개

    [Header("Move Rules (Editable)")]
    [Tooltip("emptyPos -> neighbors 매핑. 비어있으면 기본 규칙 자동 채움")]
    [SerializeField] private List<NeighborRule> moveRulesSerialized = new();
    [SerializeField] private bool autoSymmetrizeRules = true;
    [SerializeField] private bool autoFillDefaultRulesIfEmpty = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    // puzzleNumber(0~9) -> pos(0~9)
    private readonly Dictionary<int, int> puzzlePositionMap = new();
    // pos(0~9) -> piece
    private readonly Dictionary<int, SlidingPuzzle2AfterDeath> positionToPuzzle = new();

    // 조회 빠른 이웃 맵 (emptyPos -> neighbor set)
    private readonly Dictionary<int, HashSet<int>> moveRulesMap = new();

    [System.Serializable]
    public class NeighborRule
    {
        public int emptyPos;
        public List<int> neighbors = new();
    }

    // ================== [C] 클리어 판정(순서 매핑) ==================
    private bool keyCleared   = false;
    private bool lock1Cleared = false;
    private bool lock2Cleared = false;

    // Key: {2,4,9} → 4 패턴 중 하나(순서 매핑)
    private static readonly int[] keyTilesOrdered = { 2, 4, 9 };
    private static readonly int[][] keyTargetPositionOptions =
    {
        new int[] { 1, 2, 4 }, // 2→1, 4→2, 9→4
        new int[] { 2, 3, 5 }, // 2→2, 4→3, 9→5
        new int[] { 4, 5, 7 }, // 2→4, 4→5, 9→7
        new int[] { 5, 6, 8 }, // 2→5, 4→6, 9→8
    };

    // Lock1: 두 패턴 허용(순서 매핑)
    [Header("Ordered Match — Lock1 (열쇠 후)")]
    [SerializeField] private int[] lock1TilesOrdered = { 1, 3, 5, 7 };
    [SerializeField] private int[] lock1TargetPositionsOrderedA = { 2, 5, 8, 4 }; // A: 1→2,3→5,5→8,7→4
    [SerializeField] private int[] lock1TargetPositionsOrderedB = { 3, 6, 9, 5 }; // B: 1→3,3→6,5→9,7→5

    // Lock2: 두 패턴 허용(순서 매핑)
    [Header("Ordered Match — Lock2 (열쇠+Lock1 후)")]
    [SerializeField] private int[] lock2TilesOrdered = { 1, 3, 5, 6 };
    [SerializeField] private int[] lock2TargetPositionsOrderedA = { 7, 1, 4, 5 };
    [SerializeField] private int[] lock2TargetPositionsOrderedB = { 8, 2, 5, 6 };

    // ================== 라이프사이클 ==================
    private void Awake()
    {
        Instance = this;

        // 타이머 초기화
        totalDurationSeconds = Mathf.Max(0f, startMinutes * 60f);
        timer = totalDurationSeconds;
        UpdateTimerUI();

        // 아이콘 초기 알파
        SetAlpha(keyIcon,   1f);
        SetAlpha(lock1Icon, 1f);
        SetAlpha(lock2Icon, 1f);

        // 클릭 스킵 트리거
        if (clickToClearObject != null)
            AttachClickHelper(clickToClearObject);
        else if (autoCreateFullscreenButtonIfNone)
            CreateFullscreenClickButton();

        // 이동 규칙/보드 초기화
        BuildMoveRulesMap();
        InitializeBoard();

        // 규칙 점검/로그
        ValidateMoveRulesSymmetric();
        if (debugLogs)
        {
            PrintNeighborTable();
            if (puzzlePositionMap.ContainsKey(0))
                LogNeighborsForEmpty(puzzlePositionMap[0]);
            DumpState("[Init Dump]");
        }
    }

    private void Update()
    {
        if (sceneLoading) return;

        // 타이머 진행
        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            // 시간 만료 시: 클릭 성공 or 퍼즐 3단계 성공 중 하나도 없으면 실패
            if (!SuccessAchieved())
                LoadFailScene();
            else
                LoadSuccessScene(); // 혹시 바로 전 프레임에 성공했는데 아직 씬 안넘어갔다면
        }
    }

    private bool SuccessAchieved() => clearedByClick || (keyCleared && lock1Cleared && lock2Cleared);

    private void UpdateTimerUI()
    {
        if (!timerText) return;
        float t = Mathf.Max(0f, timer);
        int min = Mathf.FloorToInt(t / 60f);
        int sec = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    // ================== [D] 클릭 → 즉시 클리어 ==================
    public void OnClearClick()
    {
        if (sceneLoading) return;
        clearedByClick = true;                 // 클릭으로 성공 마킹
        SetAlpha(keyIcon,   clearedAlpha);     // UI도 즉시 성공 표시
        SetAlpha(lock1Icon, clearedAlpha);
        SetAlpha(lock2Icon, clearedAlpha);
        if (sfxOnClear) sfxOnClear.Play();
        LoadSuccessScene();                    // 즉시 성공 씬
    }

    private IEnumerator LoadAfterDelaySuccess()
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(successSceneName);
    }

    private IEnumerator LoadAfterDelayFail()
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(failSceneName);
    }

    private void LoadSuccessScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        if (loadDelay > 0f) StartCoroutine(LoadAfterDelaySuccess());
        else SceneManager.LoadScene(successSceneName);
    }

    private void LoadFailScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        if (loadDelay > 0f) StartCoroutine(LoadAfterDelayFail());
        else SceneManager.LoadScene(failSceneName);
    }

    // ---------- 클릭 트리거 부착/생성 ----------
    private void AttachClickHelper(GameObject go)
    {
        if (!go) return;

        bool isUI = go.GetComponent<Graphic>() != null || go.GetComponentInParent<Canvas>() != null;
        if (isUI)
        {
            var ui = go.GetComponent<UIElementClickToClear>();
            if (ui == null) ui = go.AddComponent<UIElementClickToClear>();
            ui.manager = this;
        }
        else
        {
            if (!go.GetComponent<Collider>() && !go.GetComponent<Collider2D>())
                go.AddComponent<BoxCollider>();
            var w = go.GetComponent<ColliderClickToClear>();
            if (w == null) w = go.AddComponent<ColliderClickToClear>();
            w.manager = this;
        }
    }

    private void CreateFullscreenClickButton()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("AutoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (FindObjectOfType<EventSystem>() == null)
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var btnGO = new GameObject("ClickToClearButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIElementClickToClear));
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        btnGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        btnGO.GetComponent<UIElementClickToClear>().manager = this;
    }

    // ================== [E] 보드/이동 ==================
    private void BuildMoveRulesMap()
    {
        moveRulesMap.Clear();

        if (moveRulesSerialized == null || moveRulesSerialized.Count == 0)
        {
            if (autoFillDefaultRulesIfEmpty)
            {
                moveRulesSerialized = new List<NeighborRule>
                {
                    new NeighborRule{ emptyPos=0, neighbors=new List<int>{2} },
                    new NeighborRule{ emptyPos=1, neighbors=new List<int>{2,4} },
                    new NeighborRule{ emptyPos=2, neighbors=new List<int>{0,1,3,5} },
                    new NeighborRule{ emptyPos=3, neighbors=new List<int>{2,6,7} },
                    new NeighborRule{ emptyPos=4, neighbors=new List<int>{1,5,7} },
                    new NeighborRule{ emptyPos=5, neighbors=new List<int>{2,4,6,8} },
                    new NeighborRule{ emptyPos=6, neighbors=new List<int>{3,5,9} },
                    new NeighborRule{ emptyPos=7, neighbors=new List<int>{3,4,8} },
                    new NeighborRule{ emptyPos=8, neighbors=new List<int>{5,7,9} },
                    new NeighborRule{ emptyPos=9, neighbors=new List<int>{6,8} },
                };
            }
            else
            {
                Debug.LogWarning("[AfterDeath] moveRulesSerialized 비어있음");
                return;
            }
        }

        foreach (var rule in moveRulesSerialized)
        {
            if (!moveRulesMap.TryGetValue(rule.emptyPos, out var set))
            {
                set = new HashSet<int>();
                moveRulesMap[rule.emptyPos] = set;
            }
            foreach (var nb in rule.neighbors) set.Add(nb);
        }

        if (autoSymmetrizeRules)
        {
            var pairs = new List<(int a, int b)>();
            foreach (var kv in moveRulesMap)
            {
                int a = kv.Key;
                foreach (var b in kv.Value) pairs.Add((a, b));
            }
            foreach (var (a, b) in pairs)
            {
                if (!moveRulesMap.TryGetValue(b, out var setB))
                {
                    setB = new HashSet<int>();
                    moveRulesMap[b] = setB;
                }
                setB.Add(a);
            }
        }
    }

    private void InitializeBoard()
    {
        positionToPuzzle.Clear();
        puzzlePositionMap.Clear();

        if (boardPanel == null) { Debug.LogError("[AfterDeath] boardPanel 비어있음"); return; }
        if (boardPositions == null || boardPositions.Length != 10)
        { Debug.LogError($"[AfterDeath] boardPositions 길이 {boardPositions?.Length ?? 0} (10 필요)"); return; }
        if (puzzleScripts == null || puzzleScripts.Length != 10)
        { Debug.LogError($"[AfterDeath] puzzleScripts 길이 {puzzleScripts?.Length ?? 0} (10 필요: 0~9)"); return; }

        foreach (var p in puzzleScripts)
        {
            if (p == null) { Debug.LogError("[AfterDeath] puzzleScripts에 null 있음"); continue; }

            var rt = p.GetComponent<RectTransform>();
            if (rt.transform.parent != boardPanel) rt.SetParent(boardPanel, false);

            int posIndex = p.currentPositionIndex;
            if (posIndex < 0 || posIndex > 9)
            { Debug.LogError($"[AfterDeath] {p.name} currentPositionIndex={posIndex} (0~9)"); continue; }

            if (positionToPuzzle.ContainsKey(posIndex))
            { Debug.LogError($"[AfterDeath] 위치 {posIndex} 중복: {positionToPuzzle[posIndex].name} / {p.name}"); continue; }

            positionToPuzzle[posIndex] = p;
            puzzlePositionMap[p.puzzleNumber] = posIndex;

            p.SetPosition(posIndex, boardPositions[posIndex]);
        }
    }

    // 퍼즐 타일 클릭 시 호출
    public void TryMovePuzzle(SlidingPuzzle2AfterDeath clicked)
    {
        if (sceneLoading || clicked == null) return;

        if (!puzzlePositionMap.ContainsKey(0))
        { Debug.LogError("[AfterDeath] 빈칸(퍼즐번호 0) 없음"); return; }

        int emptyPos = puzzlePositionMap[0];
        int clickedPos = clicked.currentPositionIndex;

        if (!moveRulesMap.TryGetValue(emptyPos, out var neighbors))
        { Debug.LogError($"[AfterDeath] moveRules에 emptyPos {emptyPos} 없음"); return; }

        if (!neighbors.Contains(clickedPos))
        {
            if (debugLogs)
            {
                Debug.Log($"[BLOCKED] empty:{emptyPos} <-/-> clicked:{clickedPos}");
                LogNeighborsForEmpty(emptyPos);
            }
            return;
        }

        // 스왑
        positionToPuzzle.TryGetValue(emptyPos, out var empty);

        if (debugLogs)
            Debug.Log($"[MOVE] empty:{emptyPos} <-> clicked:{clickedPos}  | tile#{clicked.puzzleNumber}");

        clicked.SetPosition(emptyPos, boardPositions[emptyPos]);

        if (empty != null)
        {
            empty.SetPosition(clickedPos, boardPositions[clickedPos]);
            positionToPuzzle[clickedPos] = empty;
        }
        else
        {
            positionToPuzzle.Remove(clickedPos);
            if (debugLogs) Debug.LogWarning("[AfterDeath] 빈칸 오브젝트 없음");
        }

        // 맵 갱신
        puzzlePositionMap[clicked.puzzleNumber] = emptyPos;
        puzzlePositionMap[0] = clickedPos;
        positionToPuzzle[emptyPos] = clicked;

        if (debugLogs)
        {
            DumpState("[After Swap]");
            LogNeighborsForEmpty(clickedPos);
        }

        // 이동 후 판정 (열쇠→Lock1→Lock2, 모두 '순서 정확 매칭')
        CheckJudgements();

        // 퍼즐 3단계가 모두 끝났다면 즉시 성공
        if (SuccessAchieved())
            LoadSuccessScene();
    }

    // ================== [F] 판정 ==================
    private void CheckJudgements()
    {
        // Key
        if (!keyCleared && MatchOrderedAny(keyTilesOrdered, keyTargetPositionOptions))
        {
            keyCleared = true;
            SetAlpha(keyIcon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Key CLEARED (ordered options)");
        }

        // Lock1 (열쇠 후 + A/B 중 하나)
        if (keyCleared && !lock1Cleared &&
            (MatchOrderedSingle(lock1TilesOrdered, lock1TargetPositionsOrderedA) ||
             MatchOrderedSingle(lock1TilesOrdered, lock1TargetPositionsOrderedB)))
        {
            lock1Cleared = true;
            SetAlpha(lock1Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock1 CLEARED (ordered A/B)");
        }

        // Lock2 (열쇠+Lock1 후 + A/B 중 하나)
        if (keyCleared && lock1Cleared && !lock2Cleared &&
            (MatchOrderedSingle(lock2TilesOrdered, lock2TargetPositionsOrderedA) ||
             MatchOrderedSingle(lock2TilesOrdered, lock2TargetPositionsOrderedB)))
        {
            lock2Cleared = true;
            SetAlpha(lock2Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock2 CLEARED (ordered)");
        }
    }

    private bool MatchOrderedAny(int[] tiles, int[][] options)
    {
        if (tiles == null || options == null || options.Length == 0) return false;
        foreach (var opt in options)
            if (MatchOrderedSingle(tiles, opt)) return true;
        return false;
    }

    private bool MatchOrderedSingle(int[] tiles, int[] targetPositions)
    {
        if (tiles == null || targetPositions == null) return false;
        if (tiles.Length == 0 || targetPositions.Length == 0) return false;
        if (tiles.Length != targetPositions.Length) return false;

        for (int i = 0; i < tiles.Length; i++)
        {
            int tile = tiles[i];
            int want = targetPositions[i];

            if (!puzzlePositionMap.TryGetValue(tile, out var curPos)) return false;
            if (curPos != want) return false;
        }
        return true;
    }

    // ================== [G] 유틸/디버그 ==================
    private void ValidateMoveRulesSymmetric()
    {
        foreach (var kv in moveRulesMap)
        {
            int a = kv.Key;
            foreach (var b in kv.Value)
            {
                if (!moveRulesMap.TryGetValue(b, out var set) || !set.Contains(a))
                    Debug.LogWarning($"[AfterDeath] moveRules 비대칭: {a} -> {b}");
            }
        }
    }

    private void LogNeighborsForEmpty(int emptyPos)
    {
        if (!moveRulesMap.TryGetValue(emptyPos, out var nbs))
        {
            Debug.LogWarning($"[AfterDeath] emptyPos {emptyPos}의 이웃 정보 없음");
            return;
        }

        string info = string.Join(", ", nbs.Select(p =>
        {
            string who = positionToPuzzle.TryGetValue(p, out var piece) ? $"tile#{piece.puzzleNumber}" : "empty";
            return $"{p}({who})";
        }));

        Debug.Log($"[NEIGHBORS] empty@{emptyPos} -> [{string.Join(",", nbs)}] | occupants: {info}");
    }

    private void PrintNeighborTable()
    {
        Debug.Log("====== AfterDeath Neighbor Table ======");
        for (int i = 0; i < 10; i++)
        {
            if (moveRulesMap.TryGetValue(i, out var nbs))
                Debug.Log($"empty@{i} -> [{string.Join(",", nbs)}]");
            else
                Debug.Log($"empty@{i} -> [None]");
        }
        Debug.Log("=======================================");
    }

    private void DumpState(string tag)
    {
        var line = string.Join(", ",
            Enumerable.Range(0, 10).Select(pos =>
                positionToPuzzle.TryGetValue(pos, out var p) ? $"{pos}:{p.puzzleNumber}" : $"{pos}:-"));
        Debug.Log($"{tag} {line}");
    }

    // CanvasGroup + 하위 UI/Sprite 모두 알파 반영
    private void SetAlpha(CanvasGroup g, float a)
    {
        if (!g) return;
        a = Mathf.Clamp01(a);
        g.alpha = a;

        var graphics = g.GetComponentsInChildren<Graphic>(true);
        foreach (var gr in graphics)
        {
            var c = gr.color; c.a = a; gr.color = c;
        }
        var srs = g.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            var c = sr.color; c.a = a; sr.color = c;
        }
    }
}

/* -------------------- 클릭 타겟용 헬퍼 2종 -------------------- */
public class UIElementClickToClear : MonoBehaviour, IPointerClickHandler
{
    public SlidingGameManager2AfterDeath manager;
    public void OnPointerClick(PointerEventData eventData) => manager?.OnClearClick();
}

public class ColliderClickToClear : MonoBehaviour
{
    public SlidingGameManager2AfterDeath manager;
    private void OnMouseDown() => manager?.OnClearClick();
}
