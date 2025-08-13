using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Graphic, Image

public class SlidingGameManager2Script : MonoBehaviour
{
    public static SlidingGameManager2Script Instance;

    [Header("Board (UI Panel)")]
    public RectTransform boardPanel;

    [Header("Puzzle Refs (0=빈칸, 1~9 퍼즐)")]
    public SlidingPuzzle2Script[] puzzleScripts; // 반드시 10개 (0~9)

    [Header("Board Positions (anchored, index 0~9)")]
    public Vector2[] boardPositions; // 반드시 10개

    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Timer")]
    [SerializeField, Min(0)] private int startMinutes = 5;
    [SerializeField, HideInInspector] private float totalDurationSeconds = 300f;

    [Header("Scene Names")]
    [SerializeField] private string successSceneName = "Stage3_2";
    [SerializeField] private string failSceneName    = "EtArcadiaEndingIfslidingdeath";

    [Header("Right-side Icons (CanvasGroup)")]
    public CanvasGroup keyIcon;      // 열쇠
    public CanvasGroup lock1Icon;    // 1차 자물쇠
    public CanvasGroup lock2Icon;    // 2차 자물쇠
    [SerializeField, Range(0f,1f)] private float clearedAlpha = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private float timer;
    private bool isPaused = false;
    private bool sceneLoading = false;

    // puzzleNumber(0~9) -> pos(0~9)
    private readonly Dictionary<int, int> puzzlePositionMap = new();
    // pos(0~9) -> piece
    private readonly Dictionary<int, SlidingPuzzle2Script> positionToPuzzle = new();

    // ===== 이동 규칙(대칭 보장) =====
    private readonly Dictionary<int, List<int>> moveRules = new()
    {
        {0, new(){2}},                // 0 <-> 2
        {1, new(){2,4}},              // 1 <-> 2,4
        {2, new(){0,1,3,5}},          // 2 <-> 0,1,3,5
        {3, new(){2,6,7}},            // 3 <-> 2,6,7
        {4, new(){1,5,7}},            // 4 <-> 1,5,7
        {5, new(){2,4,6,8}},          // 5 <-> 2,4,6,8
        {6, new(){3,5,9}},            // 6 <-> 3,5,9
        {7, new(){3,4,8}},            // 7 <-> 3,4,8
        {8, new(){5,7,9}},            // 8 <-> 5,7,9
        {9, new(){6,8}},              // 9 <-> 6,8
    };

    // ---- 판정 상태 ----
    private bool keyCleared   = false; // 열쇠(선행)
    private bool lock1Cleared = false; // 첫 번째 자물쇠(열쇠 후)
    private bool lock2Cleared = false; // 두 번째 자물쇠(열쇠+첫자물쇠 후)

    // ---- 열쇠(순서 매핑, 4가지 중 하나) ----
    private static readonly int[] keyTilesOrdered = { 2, 4, 9 };
    private static readonly int[][] keyTargetPositionOptions =
    {
        new int[] { 1, 2, 4 }, // 2→1, 4→2, 9→4
        new int[] { 2, 3, 5 }, // 2→2, 4→3, 9→5
        new int[] { 4, 5, 7 }, // 2→4, 4→5, 9→7
        new int[] { 5, 6, 8 }, // 2→5, 4→6, 9→8
    };

    // ---- Lock1(순서 매핑, 두 가지 패턴 허용: A/B) ----
    [Header("Ordered Match — Lock1 (열쇠 후)")]
    [SerializeField] private int[] lock1TilesOrdered = { 1, 3, 5, 7 };
    [SerializeField] private int[] lock1TargetPositionsOrderedA = { 2, 5, 8, 4 }; // 1→2, 3→5, 5→8, 7→4
    [SerializeField] private int[] lock1TargetPositionsOrderedB = { 3, 6, 9, 5 }; // 1→3, 3→6, 5→9, 7→5

    // ---- Lock2(순서 매핑, 두 가지 패턴 허용: A/B) ----
    [Header("Ordered Match — Lock2 (열쇠+Lock1 후)")]
    [SerializeField] private int[] lock2TilesOrdered = { 1, 3, 5, 6 };
    [SerializeField] private int[] lock2TargetPositionsOrderedA = { 7, 1, 4, 5 }; // A: 1→7,3→1,5→4,6→5
    [SerializeField] private int[] lock2TargetPositionsOrderedB = { 8, 2, 5, 6 }; // B: 1→8,3→2,5→5,6→6

    // --------------------------

    void Awake()
    {
        Instance = this;

        ApplyTimerSettings();
        ResetTimer();

        // 아이콘 알파 초기화
        SetAlpha(keyIcon,   1f);
        SetAlpha(lock1Icon, 1f);
        SetAlpha(lock2Icon, 1f);

        InitializeBoard();

        ValidateSetup();
        ValidateMoveRulesSymmetric();

        if (debugLogs)
        {
            PrintNeighborTable();
            if (puzzlePositionMap.ContainsKey(0))
                LogNeighborsForEmpty(puzzlePositionMap[0]);
            DumpState("[Init Dump]");
        }
    }

    private void ApplyTimerSettings()
    {
        totalDurationSeconds = Mathf.Max(0f, startMinutes * 60f);
    }

    public void ResetTimer()
    {
        timer = totalDurationSeconds;
        UpdateTimerUI();
    }

    private void InitializeBoard()
    {
        positionToPuzzle.Clear();
        puzzlePositionMap.Clear();

        if (boardPanel == null) { Debug.LogError("[SlidingGM2] boardPanel 비어있음"); return; }
        if (boardPositions == null || boardPositions.Length != 10)
        { Debug.LogError($"[SlidingGM2] boardPositions 길이 {boardPositions?.Length ?? 0} (10 필요)"); return; }
        if (puzzleScripts == null || puzzleScripts.Length != 10)
        { Debug.LogError($"[SlidingGM2] puzzleScripts 길이 {puzzleScripts?.Length ?? 0} (10 필요: 0번 포함)"); return; }

        foreach (var p in puzzleScripts)
        {
            if (p == null) { Debug.LogError("[SlidingGM2] puzzleScripts에 null 있음"); continue; }

            var rt = p.GetComponent<RectTransform>();
            if (rt.transform.parent != boardPanel) rt.SetParent(boardPanel, false);

            int posIndex = p.currentPositionIndex;
            if (posIndex < 0 || posIndex > 9)
            { Debug.LogError($"[SlidingGM2] {p.name} currentPositionIndex={posIndex} (0~9)"); continue; }

            if (positionToPuzzle.ContainsKey(posIndex))
            { Debug.LogError($"[SlidingGM2] 위치 {posIndex} 중복: {positionToPuzzle[posIndex].name} / {p.name}"); continue; }

            positionToPuzzle[posIndex] = p;
            puzzlePositionMap[p.puzzleNumber] = posIndex;

            p.SetPosition(posIndex, boardPositions[posIndex]);
        }
    }

    public void TryMovePuzzle(SlidingPuzzle2Script clicked)
    {
        if (isPaused || sceneLoading || clicked == null) return;

        if (!puzzlePositionMap.ContainsKey(0))
        { Debug.LogError("[SlidingGM2] 빈칸(퍼즐번호 0) 없음"); return; }

        int emptyPos = puzzlePositionMap[0];
        int clickedPos = clicked.currentPositionIndex;

        if (!moveRules.TryGetValue(emptyPos, out var neighbors))
        { Debug.LogError($"[SlidingGM2] moveRules에 emptyPos {emptyPos} 없음"); return; }

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
            if (debugLogs) Debug.LogWarning("[SlidingGM2] 빈칸 오브젝트가 없어 clickedPos를 비워둡니다.");
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

        // 이동 후 판정
        CheckJudgements();
        if (lock2Cleared) LoadSuccessScene();
    }

    void Update()
    {
        if (isPaused || sceneLoading) return;

        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            // 시간 종료 시: 모두 클리어면 성공, 아니면 실패
            if (IsAllCleared()) LoadSuccessScene();
            else LoadFailScene();
            return;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        float display = Mathf.Max(0f, timer);
        int min = Mathf.FloorToInt(display / 60f);
        int sec = Mathf.FloorToInt(display % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    // ===== 판정 로직 (모두 '순서 정확 매칭') =====
    private void CheckJudgements()
    {
        // 1) Key: 4가지 옵션 중 하나라도 '정확 매핑'이면 클리어
        if (!keyCleared && MatchOrderedAny(keyTilesOrdered, keyTargetPositionOptions))
        {
            keyCleared = true;
            SetAlpha(keyIcon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Key CLEARED (ordered options)");
        }

        // 2) Lock1: 열쇠 후 + A/B 중 하나 '정확 매핑'
        if (keyCleared && !lock1Cleared &&
            (MatchOrderedSingle(lock1TilesOrdered, lock1TargetPositionsOrderedA) ||
             MatchOrderedSingle(lock1TilesOrdered, lock1TargetPositionsOrderedB)))
        {
            lock1Cleared = true;
            SetAlpha(lock1Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock1 CLEARED (ordered A/B)");
        }

        // 3) Lock2: 열쇠+Lock1 후 + A/B 중 하나 '정확 매핑'
        if (keyCleared && lock1Cleared && !lock2Cleared &&
            (MatchOrderedSingle(lock2TilesOrdered, lock2TargetPositionsOrderedA) ||
             MatchOrderedSingle(lock2TilesOrdered, lock2TargetPositionsOrderedB)))
        {
            lock2Cleared = true;
            SetAlpha(lock2Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock2 CLEARED (ordered, ALL DONE)");
        }
    }

    private bool IsAllCleared() => keyCleared && lock1Cleared && lock2Cleared;

    // --- helpers: ordered matching ---
    private bool MatchOrderedAny(int[] tiles, int[][] options)
    {
        if (tiles == null || options == null || options.Length == 0) return false;
        foreach (var opt in options)
            if (MatchOrderedSingle(tiles, opt)) return true;
        return false;
    }

    // 타일 i 가 targetPositions[i] 에 '정확히' 있어야 true
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

    // ===================== 유틸/디버그 =====================

    private void ValidateMoveRulesSymmetric()
    {
        foreach (var kv in moveRules)
        {
            int a = kv.Key;
            foreach (var b in kv.Value)
            {
                if (!moveRules.TryGetValue(b, out var list) || !list.Contains(a))
                    Debug.LogWarning($"[SlidingGM2] moveRules 비대칭: {a} -> {b} (역방향 누락)");
            }
        }
    }

    private void LogNeighborsForEmpty(int emptyPos)
    {
        if (!moveRules.TryGetValue(emptyPos, out var nbs))
        {
            Debug.LogWarning($"[SlidingGM2] emptyPos {emptyPos}의 이웃 정보 없음");
            return;
        }

        string info = string.Join(", ", nbs.Select(p =>
        {
            string who = positionToPuzzle.TryGetValue(p, out var piece) ? $"tile#{piece.puzzleNumber}" : "empty";
            return $"{p}({who})";
        }));

        Debug.Log($"[NEIGHBORS] empty@{emptyPos} -> can swap with [{string.Join(",", nbs)}]  | occupants: {info}");
    }

    private void PrintNeighborTable()
    {
        Debug.Log("====== SlidingGM2 Neighbor Table (emptyPos -> neighbors) ======");
        for (int i = 0; i < 10; i++)
        {
            if (moveRules.TryGetValue(i, out var nbs))
                Debug.Log($"empty@{i} -> [{string.Join(",", nbs)}]");
            else
                Debug.Log($"empty@{i} -> [None]");
        }
        Debug.Log("================================================================");
    }

    private void LoadSuccessScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        SceneManager.LoadScene(successSceneName);
    }

    private void LoadFailScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        SceneManager.LoadScene(failSceneName);
    }

    public void PauseGame()  { isPaused = true;  }
    public void ResumeGame() { isPaused = false; }

    private void ValidateSetup()
    {
        if (!puzzlePositionMap.ContainsKey(0))
            Debug.LogError("[SlidingGM2] 퍼즐번호 0(빈칸) 누락");

        for (int i = 0; i < 10; i++)
            if (!positionToPuzzle.ContainsKey(i))
                Debug.LogWarning($"[SlidingGM2] 위치 {i} 에 퍼즐 없음");
    }

    private void DumpState(string tag)
    {
        var line = string.Join(", ",
            Enumerable.Range(0, 10).Select(pos =>
                positionToPuzzle.TryGetValue(pos, out var p) ? $"{pos}:{p.puzzleNumber}" : $"{pos}:-"));
        Debug.Log($"{tag} {line}");
    }

    // CanvasGroup + UI Graphic + SpriteRenderer 모두 알파 적용
    private void SetAlpha(CanvasGroup g, float a)
    {
        a = Mathf.Clamp01(a);
        bool touched = false;

        if (g != null)
        {
            g.alpha = a;
            touched = true;

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
        else
        {
            if (debugLogs) Debug.LogWarning("[SlidingGM2] SetAlpha 대상(CanvasGroup) 미할당 – 인스펙터 연결 필요");
        }

        if (!touched && debugLogs)
            Debug.LogWarning("[SlidingGM2] SetAlpha 적용 대상이 없습니다.");
    }
}
