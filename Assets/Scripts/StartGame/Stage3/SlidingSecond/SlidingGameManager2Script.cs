using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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
    // 기존 문제: 0<->2는 있었지만 2쪽에 0이 빠져 있었습니다.
    // 아래는 모든 간선이 양방향이 되도록 보정한 버전입니다.
    private readonly Dictionary<int, List<int>> moveRules = new()
    {
        {0, new(){2}},                // 0 <-> 2
        {1, new(){2,4}},              // 1 <-> 2,4
        {2, new(){0,1,3,5}},          // 2 <-> 0,1,3,5  ★ 0 추가
        {3, new(){2,6,7}},            // 3 <-> 2,6,7   ★ 7 추가(7가 3을 가리키므로 대칭)
        {4, new(){1,5,7}},            // 4 <-> 1,5,7
        {5, new(){2,4,6,8}},          // 5 <-> 2,4,6,8
        {6, new(){3,5,9}},            // 6 <-> 3,5,9
        {7, new(){3,4,8}},            // 7 <-> 3,4,8   ★ 4 추가(4가 7을 가리키므로 대칭)
        {8, new(){5,7,9}},            // 8 <-> 5,7,9
        {9, new(){6,8}},              // 9 <-> 6,8
    };

    // ---- 판정 상태 ----
    private bool keyCleared   = false;
    private bool lock1Cleared = false;
    private bool lock2Cleared = false;

    // ---- 판정 규칙 ----
    // 열쇠: 타일 2,4,6,9 -> (0,1,4,3), (1,2,5,4), (3,4,7,6), (4,5,8,7)
    private readonly int[] keyTiles = {2,4,6,9};
    private readonly List<HashSet<int>> keyValidSets = new()
    {
        new HashSet<int>{0,1,4,3},
        new HashSet<int>{1,2,5,4},
        new HashSet<int>{3,4,7,6},
        new HashSet<int>{4,5,8,7},
    };

    // 1차 자물쇠: 타일 1,3,5,7 -> (2,5,8,4), (1,4,7,3)
    private readonly int[] lockTiles = {1,3,5,7};
    private readonly List<HashSet<int>> lock1ValidSets = new()
    {
        new HashSet<int>{2,5,8,4},
        new HashSet<int>{1,4,7,3},
    };

    // 2차 자물쇠: 타일 1,3,5,7 -> (6,0,3,4), (7,1,4,5)
    private readonly List<HashSet<int>> lock2ValidSets = new()
    {
        new HashSet<int>{6,0,3,4},
        new HashSet<int>{7,1,4,5},
    };

    // --------------------------

    void Awake()
    {
        Instance = this;

        ApplyTimerSettings();
        ResetTimer();

        // 아이콘 알파 초기화(미지정이면 무시)
        SetAlpha(keyIcon,   1f);
        SetAlpha(lock1Icon, 1f);
        SetAlpha(lock2Icon, 1f);

        InitializeBoard();

        ValidateSetup();
        ValidateMoveRulesSymmetric();   // ★ 이동 규칙 대칭성 검증

        if (debugLogs)
        {
            PrintNeighborTable();       // ★ 빈칸이 0~9일 때 교체 가능한 위치 테이블
            if (puzzlePositionMap.ContainsKey(0))
                LogNeighborsForEmpty(puzzlePositionMap[0]); // 현재 빈칸 기준 안내
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
                LogNeighborsForEmpty(emptyPos); // ★ 어떤 위치로만 이동 가능한지 안내
            }
            return;
        }

        // 스왑(빈칸 오브젝트가 없더라도 방어적으로 처리)
        positionToPuzzle.TryGetValue(emptyPos, out var empty);

        if (debugLogs)
            Debug.Log($"[MOVE] empty:{emptyPos} <-> clicked:{clickedPos}  | tile#{clicked.puzzleNumber}");

        // 클릭한 타일을 빈칸 자리로 이동
        clicked.SetPosition(emptyPos, boardPositions[emptyPos]);

        if (empty != null)
        {
            // 빈칸 오브젝트가 있으면 그걸 클릭 위치로 이동
            empty.SetPosition(clickedPos, boardPositions[clickedPos]);
            positionToPuzzle[clickedPos] = empty;
        }
        else
        {
            // 빈칸 오브젝트가 없으면, 클릭 자리는 "비어있음"으로 두기
            positionToPuzzle.Remove(clickedPos);
            if (debugLogs) Debug.LogWarning("[SlidingGM2] 빈칸 오브젝트가 없어 clickedPos를 비워둡니다.");
        }

        // 맵 갱신
        puzzlePositionMap[clicked.puzzleNumber] = emptyPos;
        puzzlePositionMap[0] = clickedPos; // 빈칸은 클릭한 자리로 이동
        positionToPuzzle[emptyPos] = clicked;

        if (debugLogs)
        {
            DumpState("[After Swap]");
            LogNeighborsForEmpty(clickedPos); // ★ 새 빈칸 기준 안내
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

        if (timer <= 0f) LoadFailScene();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        float display = Mathf.Max(0f, timer);
        int min = Mathf.FloorToInt(display / 60f);
        int sec = Mathf.FloorToInt(display % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    // ===== 판정 로직 =====
    private void CheckJudgements()
    {
        // 1) Key
        if (!keyCleared && MatchSet(keyTiles, keyValidSets))
        {
            keyCleared = true;
            SetAlpha(keyIcon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Key CLEARED");
        }

        // 2) Lock1 (Key 선행)
        if (keyCleared && !lock1Cleared && MatchSet(lockTiles, lock1ValidSets))
        {
            lock1Cleared = true;
            SetAlpha(lock1Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock1 CLEARED");
        }

        // 3) Lock2 (Lock1 선행)
        if (keyCleared && lock1Cleared && !lock2Cleared && MatchSet(lockTiles, lock2ValidSets))
        {
            lock2Cleared = true;
            SetAlpha(lock2Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock2 CLEARED (ALL DONE)");
        }
    }

    // tiles: 체크할 퍼즐 번호 모음 (예: 2,4,6,9)
    // validSets: 허용 위치 "집합" 목록 (순서 무시)
    private bool MatchSet(int[] tiles, List<HashSet<int>> validSets)
    {
        var current = new HashSet<int>(tiles.Select(t => puzzlePositionMap[t]));
        return validSets.Any(set => set.SetEquals(current));
    }

    // ===================== 유틸/디버그 =====================

    // ★ 이동 규칙 대칭성 검증
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

    // ★ “빈칸이 X일 때 어디와 교체 가능한가?” 로그
    private void LogNeighborsForEmpty(int emptyPos)
    {
        if (!moveRules.TryGetValue(emptyPos, out var nbs))
        {
            Debug.LogWarning($"[SlidingGM2] emptyPos {emptyPos}의 이웃 정보 없음");
            return;
        }

        // 각 이웃 칸에 현재 어떤 타일이 있는지도 함께 출력
        string info = string.Join(", ", nbs.Select(p =>
        {
            string who = positionToPuzzle.TryGetValue(p, out var piece) ? $"tile#{piece.puzzleNumber}" : "empty";
            return $"{p}({who})";
        }));

        Debug.Log($"[NEIGHBORS] empty@{emptyPos} -> can swap with [{string.Join(",", nbs)}]  | occupants: {info}");
    }

    // ★ 전체 테이블 출력(빈칸이 0~9일 때 이동 가능 위치)
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

    private void SetAlpha(CanvasGroup g, float a)
    {
        if (g == null) return;
        g.alpha = a;
    }
}
