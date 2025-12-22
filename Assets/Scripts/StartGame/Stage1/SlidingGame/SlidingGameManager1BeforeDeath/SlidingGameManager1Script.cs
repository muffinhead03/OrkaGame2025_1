// SlidingGameManager1BeforeDeathScript.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class SlidingGameManager1BeforeDeathScript : MonoBehaviour
{
    public static SlidingGameManager1BeforeDeathScript Instance;

    public SlidingPuzzle1Script[] puzzleScripts;
    public Vector2[] boardPositions;

    public GameObject hairpin, firstLock, doorLock, secondLock;
    public CanvasGroup hairpinCanvas, firstLockCanvas, doorLockCanvas, secondLockCanvas;
    public CanvasGroup goatCanvas;
    public TextMeshProUGUI timerText;

    // ✅ 총 11분
    [SerializeField] private float totalDurationSeconds = 11f * 60f;
    // ✅ 염소 페이드 인은 처음 7분(420초) 동안 0→1
    [SerializeField] private float goatFadeDurationSeconds = 7f * 60f;

    private float timer;
    private bool isPaused = false;
    private bool sceneLoading = false;    // ⬅ 중복 로딩 방지

    private Dictionary<int, int> puzzlePositionMap = new();
    private Dictionary<int, SlidingPuzzle1Script> positionToPuzzle = new();

    private bool hairpinCleared = false, firstLockCleared = false, finalCleared = false;
    [Header("Pause when these panels are active @ origin")]
    [SerializeField] private RectTransform settingPanel;
    [SerializeField] private RectTransform firstPanel;

    // ✅ 기존 isPaused는 외부(다른 시스템) 일시정지를 담당하도록 분리
    private bool isPausedExternal = false;   // PauseGame/ResumeGame 전용
    private bool pausedByPanels   = false;   // 패널 상태로 인한 자동 일시정

    private readonly Dictionary<int, List<int>> moveRules = new()
    {
        {1, new() {3}}, {2, new() {3,5}}, {3, new() {1,2,4,6}},
        {4, new() {3,7}}, {5, new() {2,6,8}}, {6, new() {3,5,7,9}},
        {7, new() {4,6,10}}, {8, new() {5,9,11}}, {9, new() {6,8,10,12}},
        {10, new() {7,9,13}}, {11, new() {8,12}}, {12, new() {9,11,13}}, {13, new() {10,12}}
    };

    void Awake()
    {
        Instance = this;

        timer = totalDurationSeconds;

        SetAlpha(hairpinCanvas, 1f);
        SetAlpha(firstLockCanvas, 1f);
        SetAlpha(doorLockCanvas, 1f);
        SetAlpha(secondLockCanvas, 1f);

        if (goatCanvas != null) goatCanvas.alpha = 0f;

        InitializeBoard();
    }

    void InitializeBoard()
    {
        positionToPuzzle.Clear();
        foreach (var puzzle in puzzleScripts)
        {
            var posIndex = puzzle.currentPositionIndex;
            puzzle.SetPosition(posIndex, boardPositions[posIndex - 1]);
            puzzlePositionMap[puzzle.puzzleNumber] = posIndex; // 0(빈칸) 포함
            positionToPuzzle[posIndex] = puzzle;
        }
    }

    public void TryMovePuzzle(SlidingPuzzle1Script clicked)
    {
        if (isPaused || sceneLoading) return;

        int emptyPos = puzzlePositionMap[0];
        int clickedPos = clicked.currentPositionIndex;

        if (moveRules[emptyPos].Contains(clickedPos))
        {
            SlidingPuzzle1Script empty = positionToPuzzle[emptyPos];

            clicked.SetPosition(emptyPos, boardPositions[emptyPos - 1]);
            empty.SetPosition(clickedPos, boardPositions[clickedPos - 1]);

            puzzlePositionMap[clicked.puzzleNumber] = emptyPos;
            puzzlePositionMap[0] = clickedPos;

            positionToPuzzle[emptyPos] = clicked;
            positionToPuzzle[clickedPos] = empty;

            CheckClearConditions();
        }
    }

    void Update()
    {
        UpdatePanelPause();
        if (isPausedExternal || pausedByPanels || sceneLoading) return;

        // 타이머 감소
        timer -= Time.deltaTime;

        // 염소 페이드
        UpdateGoatAlpha();

        // UI 타이머 표시(음수 방지)
        float display = Mathf.Max(0f, timer);
        int min = Mathf.FloorToInt(display / 60f);
        int sec = Mathf.FloorToInt(display % 60f);
        if (timerText != null) timerText.text = $"{min:00}:{sec:00}";

        // 시간 초과 → 실패 씬
        if (timer <= 0f)
            LoadFailScene();
    }
    private static bool IsAtOriginAndActive(RectTransform rt)
    {
        if (rt == null || !rt.gameObject.activeInHierarchy) return false;
        // anchoredPosition(주), 예외적으로 localPosition도 0,0,0이면 원점으로 간주
        return rt.anchoredPosition.sqrMagnitude <= 0.0001f || rt.localPosition.sqrMagnitude <= 0.0001f;
    }

    // ✅ 추가: 패널 상태에 따라 자동 일시정지 토글
    private void UpdatePanelPause()
    {
        bool shouldPause =
            IsAtOriginAndActive(settingPanel) ||
            IsAtOriginAndActive(firstPanel);

        // 상태 변화시에만 플래그 갱신 (원한다면 여기서 로그)
        if (pausedByPanels != shouldPause)
            pausedByPanels = shouldPause;
    }

    void CheckClearConditions()
    {
        // 1) 머리핀: 타일 [3,5,8,9] 가 아래 후보와 "정확히" 매핑될 때만 통과
        if (!hairpinCleared && MatchAnyExactMapping(
                new List<int> { 3, 5, 8, 9 },
                new List<List<int>> {
                    new() { 2, 3, 5, 6 },
                    new() { 3, 4, 6, 7 },
                    new() { 5, 6, 8, 9 },
                    new() { 6, 7, 9, 10 },
                    new() { 8, 9, 11, 12 },
                    new() { 9, 10, 12, 13 }
                }))
        {
            SetAlpha(hairpinCanvas, 0.1f);
            hairpinCleared = true;
        }

// 2) 1차 자물쇠: 타일 [1,2,5,6,7,12] 또는 [1,2,10,6,7,12] 가 후보와 "정확히" 매핑
        if (!firstLockCleared && (
                MatchAnyExactMapping(
                    new List<int> { 1, 2, 5, 6, 7, 12 },
                    new List<List<int>> {
                        new() { 5, 3, 2, 4, 6, 7  },
                        new() { 8, 6, 5, 7, 9, 10 },
                        new() { 11, 9, 8, 10, 12, 13 }
                    }) 
                ||
                MatchAnyExactMapping(
                    new List<int> { 1, 2, 10, 6, 7, 12 },
                    new List<List<int>> {
                        new() { 5, 3, 2, 4, 6, 7  },
                        new() { 8, 6, 5, 7, 9, 10 },
                        new() { 11, 9, 8, 10, 12, 13 }
                    })
            ))
        {
            SetAlpha(firstLockCanvas, 0.1f);
            firstLockCleared = true;
        }


        // 3) 최종: 타일 [3,6,7,8,10,11,12] 가 후보와 "정확히" 매핑
        if (hairpinCleared && firstLockCleared && !finalCleared &&
            MatchAnyExactMapping(
                new List<int> { 3, 6, 7, 8, 10, 11, 12 },
                new List<List<int>> {
                    new() { 7, 3, 4, 2, 10, 5, 6 },
                    new() { 10, 6, 7, 5, 13, 8, 9 }
                }))
        {
            SetAlpha(doorLockCanvas, 0.1f);
            SetAlpha(secondLockCanvas, 0.1f);
            finalCleared = true;
            LoadSuccessScene();
        }
    }


/* ===== 보조 함수들 ===== */

// (집합) + (특정 타일 고정 매핑) 조합 검사 — 머리핀 그대로 사용
bool MatchSetWithFixed(List<int> tiles, List<int> targetPositions, Dictionary<int,int> fixedMap)
{
    var current = tiles.Select(t => puzzlePositionMap[t]).OrderBy(x => x).ToList();
    var target  = targetPositions.OrderBy(x => x).ToList();
    if (!current.SequenceEqual(target)) return false;

    if (fixedMap != null)
    {
        foreach (var kv in fixedMap)
            if (!puzzlePositionMap.TryGetValue(kv.Key, out int pos) || pos != kv.Value)
                return false;
    }
    return true;
}

bool MatchAnySetWithFixed(List<int> tiles, List<List<int>> validSets, Dictionary<int,int> fixedMap)
{
    foreach (var set in validSets)
        if (MatchSetWithFixed(tiles, set, fixedMap)) return true;
    return false;
}

// ===== 새로 추가: "정확 매핑" 검사 (타일[i] -> positions[i] 이어야 함)
bool MatchExactMapping(IList<int> tilesInOrder, IList<int> positionsInOrder)
{
    if (tilesInOrder == null || positionsInOrder == null || tilesInOrder.Count != positionsInOrder.Count)
        return false;

    for (int i = 0; i < tilesInOrder.Count; i++)
    {
        int tile = tilesInOrder[i];
        int wantPos = positionsInOrder[i];
        if (!puzzlePositionMap.TryGetValue(tile, out int curPos) || curPos != wantPos)
            return false;
    }
    return true;
}

bool MatchAnyExactMapping(IList<int> tilesInOrder, List<List<int>> mappingCandidates)
{
    foreach (var candidate in mappingCandidates)
        if (MatchExactMapping(tilesInOrder, candidate)) return true;
    return false;
}



    void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group == null) return;
        group.alpha = alpha;
    }

    // ✅ 염소: 처음 7분 동안 서서히 0→1, 이후 1 유지
    void UpdateGoatAlpha()
    {
        if (goatCanvas == null) return;
        float clamped = Mathf.Clamp(timer, 0f, totalDurationSeconds);
        float elapsed = totalDurationSeconds - clamped; // 0에서 증가
        float t = (goatFadeDurationSeconds > 0f) ? Mathf.Clamp01(elapsed / goatFadeDurationSeconds) : 1f;
        goatCanvas.alpha = t;
    }

    // ==== 씬 전환 유틸 ====
    private void LoadSuccessScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        SceneManager.LoadScene("Stage1_3");
    }

    private void LoadFailScene()
    {
        if (sceneLoading) return;
        sceneLoading = true;
        SceneManager.LoadScene("et_in_arcadua_egoAfterfirstSlidingDeath");
    }

    // 외부에서 일시정지/재개
    public void PauseGame()  { isPaused = true;  }
    public void ResumeGame() { isPaused = false; }
}
