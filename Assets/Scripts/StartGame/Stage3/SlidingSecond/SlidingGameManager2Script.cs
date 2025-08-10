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

    // 인접 규칙(요청한 표를 0-기반으로 사용)
    private readonly Dictionary<int, List<int>> moveRules = new()
    {
        {0, new(){2}},
        {1, new(){2,4}},
        {2, new(){1,3,5}},
        {3, new(){2,6}},
        {4, new(){1,5,7}},
        {5, new(){2,4,6,8}},
        {6, new(){3,5,9}},
        {7, new(){3,8}},
        {8, new(){5,7,9}},
        {9, new(){6,8}},
    };

    // ---- 판정 상태 ----
    private bool keyCleared   = false;
    private bool lock1Cleared = false;
    private bool lock2Cleared = false;

    // ---- 판정 규칙 (요청한 1기반 표를 0기반으로 변환해 정의) ----
    // 열쇠: 타일 2,4,6,9  -> 위치 세트 (1,2,5,4), (2,3,6,5), (4,5,8,7), (5,6,9,8)  (1기반)
    //      0기반으로:                  (0,1,4,3), (1,2,5,4), (3,4,7,6), (4,5,8,7)
    private readonly int[] keyTiles = {2,4,6,9};
    private readonly List<HashSet<int>> keyValidSets = new()
    {
        new HashSet<int>{0,1,4,3},
        new HashSet<int>{1,2,5,4},
        new HashSet<int>{3,4,7,6},
        new HashSet<int>{4,5,8,7},
    };

    // 1차 자물쇠: 타일 1,3,5,7 -> (3,6,9,5), (2,5,8,4) (1기반)
    // 0기반:                         (2,5,8,4), (1,4,7,3)
    private readonly int[] lockTiles = {1,3,5,7};
    private readonly List<HashSet<int>> lock1ValidSets = new()
    {
        new HashSet<int>{2,5,8,4},
        new HashSet<int>{1,4,7,3},
    };

    // 2차 자물쇠: 타일 1,3,5,7 -> (7,1,4,5), (8,2,5,6) (1기반)
    // 0기반:                         (6,0,3,4), (7,1,4,5)
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
        if (debugLogs) DumpState("[Init Dump]");
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
                Debug.Log($"[BLOCKED] empty:{emptyPos} <-/-> clicked:{clickedPos}  | allowed:[{string.Join(",", neighbors)}]");
            return;
        }

        // 스왑
        var empty = positionToPuzzle[emptyPos];

        if (debugLogs)
            Debug.Log($"[MOVE] empty:{emptyPos} <-> clicked:{clickedPos}  | tile#{clicked.puzzleNumber}");

        clicked.SetPosition(emptyPos, boardPositions[emptyPos]);
        empty.SetPosition(clickedPos, boardPositions[clickedPos]);

        puzzlePositionMap[clicked.puzzleNumber] = emptyPos;
        puzzlePositionMap[0] = clickedPos;

        positionToPuzzle[emptyPos] = clicked;
        positionToPuzzle[clickedPos] = empty;

        if (debugLogs) DumpState("[After Swap]");

        // 이동 후 판정
        CheckJudgements();
        // 최종 클리어도 여기서 처리
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
        // 1) Key (아직 안 깼으면)
        if (!keyCleared && MatchSet(keyTiles, keyValidSets))
        {
            keyCleared = true;
            SetAlpha(keyIcon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Key CLEARED");
        }

        // 2) Lock1 (Key가 먼저여야 함)
        if (keyCleared && !lock1Cleared && MatchSet(lockTiles, lock1ValidSets))
        {
            lock1Cleared = true;
            SetAlpha(lock1Icon, clearedAlpha);
            if (debugLogs) Debug.Log("[JUDGE] Lock1 CLEARED");
        }

        // 3) Lock2 (Lock1까지 완료 후에만)
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
        // 현재 해당 타일들의 "위치 인덱스(0~9)"를 집합으로 가져오기
        var current = new HashSet<int>(tiles.Select(t => puzzlePositionMap[t]));
        // 어떤 허용 세트와도 정확히 같으면 true
        return validSets.Any(set => set.SetEquals(current));
    }

    // =====================

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
