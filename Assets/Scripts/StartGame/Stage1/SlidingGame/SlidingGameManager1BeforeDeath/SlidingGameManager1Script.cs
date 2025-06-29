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

    private float timer = 10f;
    private bool isPaused = false;

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

        SetAlpha(hairpinCanvas, 1f);
        SetAlpha(firstLockCanvas, 1f);
        SetAlpha(doorLockCanvas, 1f);
        SetAlpha(secondLockCanvas, 1f);

        InitializeBoard();
    }

    void InitializeBoard()
    {
        positionToPuzzle.Clear();
        foreach (var puzzle in puzzleScripts)
        {
            var posIndex = puzzle.currentPositionIndex;
            puzzle.SetPosition(posIndex, boardPositions[posIndex - 1]);
            puzzlePositionMap[puzzle.puzzleNumber] = posIndex;
            positionToPuzzle[posIndex] = puzzle;
        }
    }

    public void TryMovePuzzle(SlidingPuzzle1Script clicked)
    {
        if (isPaused) return;

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
        Debug.Log($"🔁 Update 실행 중 | isPaused: {isPaused} | timer: {timer:F2}");

        if (isPaused || finalCleared)
        {
            Debug.Log("⏸ Update 멈춤: 일시정지 또는 완료됨");
            return;
        }

        timer -= Time.deltaTime;
        int min = Mathf.FloorToInt(timer / 60);
        int sec = Mathf.FloorToInt(timer % 60);
        timerText.text = $"{min:00}:{sec:00}";

        UpdateGoatAlpha();

        if (timer <= 0f)
        {
            bool allCleared = hairpinCleared && firstLockCleared && finalCleared;
            Debug.Log($"⏱ 타이머 종료 → 씬 이동: {(allCleared ? "Stage1_3" : "et_in_arcadua_egoAfterfirstSlidingDeath")}");
            SceneManager.LoadScene(allCleared ? "Stage1_3" : "et_in_arcadua_egoAfterfirstSlidingDeath");
        }
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
            Debug.Log("🔓 hairpin 클리어됨");
        }

        if (!firstLockCleared && MatchCondition(new List<int> { 5, 2, 6, 1, 7, 12 }, new List<List<int>> {
            new() {2,3,4,5,6,7}, new() {5,6,7,8,9,10}, new() {8,9,10,11,12,13}
        }))
        {
            SetAlpha(firstLockCanvas, 0.1f);
            firstLockCleared = true;
            Debug.Log("🔓 firstLock 클리어됨");
        }

        if (hairpinCleared && firstLockCleared && !finalCleared &&
            MatchCondition(new List<int> { 8, 6, 7, 11, 12, 3, 10 }, new List<List<int>> {
                new() {2,3,4,5,6,7,10}, new() {5,6,7,8,9,10,13}
            }))
        {
            SetAlpha(doorLockCanvas, 0.1f);
            SetAlpha(secondLockCanvas, 0.1f);
            finalCleared = true;
            Debug.Log("🔓 모든 퍼즐 클리어됨 (finalCleared = true)");
        }
    }

    bool MatchCondition(List<int> puzzleNums, List<List<int>> validPosSets)
    {
        var current = puzzleNums.Select(p => puzzlePositionMap[p]).OrderBy(x => x).ToList();
        return validPosSets.Any(set => set.OrderBy(x => x).SequenceEqual(current));
    }

    void SetAlpha(CanvasGroup group, float alpha)
    {
        group.alpha = alpha;
    }

    void UpdateGoatAlpha()
    {
        float elapsedTime = 10f - timer;
        if (elapsedTime <= 1f) goatCanvas.alpha = 0f;
        else goatCanvas.alpha = Mathf.Clamp01((elapsedTime - 1f) / 9f);
    }

    public void PauseGame()
    {
        isPaused = true;
        Debug.Log("✅ PauseGame() 호출됨: isPaused = true");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Debug.Log("▶️ ResumeGame() 호출됨: isPaused = false");
    }
}
