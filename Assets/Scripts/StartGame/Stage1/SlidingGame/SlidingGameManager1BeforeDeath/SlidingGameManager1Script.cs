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
        if (isPaused || sceneLoading) return;

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

    void CheckClearConditions()
    {
        if (!hairpinCleared && MatchCondition(new List<int> { 3, 5, 8, 9 }, new List<List<int>> {
            new() {2,3,5,6}, new() {3,4,6,7}, new() {5,6,8,9},
            new() {6,7,9,10}, new() {8,9,11,12}, new() {9,10,12,13}
        }))
        {
            SetAlpha(hairpinCanvas, 0.1f);
            hairpinCleared = true;
        }

        if (!firstLockCleared && MatchCondition(new List<int> { 5, 2, 6, 1, 7, 12 }, new List<List<int>> {
            new() {2,3,4,5,6,7}, new() {5,6,7,8,9,10}, new() {8,9,10,11,12,13}
        }))
        {
            SetAlpha(firstLockCanvas, 0.1f);
            firstLockCleared = true;
        }

        if (hairpinCleared && firstLockCleared && !finalCleared &&
            MatchCondition(new List<int> { 8, 6, 7, 11, 12, 3, 10 }, new List<List<int>> {
                new() {2,3,4,5,6,7,10}, new() {5,6,7,8,9,10,13}
            }))
        {
            SetAlpha(doorLockCanvas, 0.1f);
            SetAlpha(secondLockCanvas, 0.1f);
            finalCleared = true;

            // ✅ 시간 안에 전부 클리어 → 즉시 성공 씬 로딩
            LoadSuccessScene();
        }
    }

    bool MatchCondition(List<int> puzzleNums, List<List<int>> validPosSets)
    {
        var current = puzzleNums.Select(p => puzzlePositionMap[p]).OrderBy(x => x).ToList();
        return validPosSets.Any(set => set.OrderBy(x => x).SequenceEqual(current));
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
