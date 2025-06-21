using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SlidingGameManager1Script : MonoBehaviour
{
    public static SlidingGameManager1Script Instance;

    public SlidingPuzzle1Script[] puzzleScripts; // Puzzle1 ~ Puzzle12 + EmptyPuzzle
    public Vector2[] boardPositions; // 13개 좌표

    public GameObject hairpin, firstLock, doorLock, secondLock;
    public CanvasGroup hairpinCanvas, firstLockCanvas, doorLockCanvas, secondLockCanvas;

    public CanvasGroup goatCanvas; // 염소 이미지에 붙은 CanvasGroup

    
    public TextMeshProUGUI timerText;
    private float timer = 600f;

    private Dictionary<int, int> puzzlePositionMap = new(); // puzzleNum -> positionIndex
    private Dictionary<int, SlidingPuzzle1Script> positionToPuzzle = new(); // positionIndex -> Script

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
        int emptyPos = puzzlePositionMap[0];
        int clickedPos = clicked.currentPositionIndex;

        if (moveRules[emptyPos].Contains(clickedPos))
        {
            // 자리 교환
            SlidingPuzzle1Script empty = positionToPuzzle[emptyPos];

            // 위치 교환
            clicked.SetPosition(emptyPos, boardPositions[emptyPos - 1]);
            empty.SetPosition(clickedPos, boardPositions[clickedPos - 1]);

            // 업데이트
            puzzlePositionMap[clicked.puzzleNumber] = emptyPos;
            puzzlePositionMap[0] = clickedPos;

            positionToPuzzle[emptyPos] = clicked;
            positionToPuzzle[clickedPos] = empty;

            Debug.Log($"{clicked.puzzleNumber}번 타일이 {clickedPos}번 좌표에 있다가 {emptyPos}번 좌표로 이동 가능합니다.");
            CheckClearConditions();
        }
        else
        {
            Debug.Log($"{clicked.puzzleNumber}번 타일이 {clickedPos}번 좌표에 이동 불가합니다.");
        }
    }

    void Update()
    {
        if (finalCleared) return;

        timer -= Time.deltaTime;
        int min = Mathf.FloorToInt(timer / 60);
        int sec = Mathf.FloorToInt(timer % 60);
        timerText.text = $"{min:00}:{sec:00}";
        
        UpdateGoatAlpha(); // 👈 염소 연출 처리

        if (timer <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Et in Arcadia ego_SlidingGameFirst");
        }
    }

    void CheckClearConditions()
    {
        if (!hairpinCleared && MatchCondition(new List<int> {3,5,8,9}, new List<List<int>> {
            new() {2,3,5,6}, new() {3,4,6,7}, new() {5,6,8,9},
            new() {6,7,9,10}, new() {8,9,11,12}, new() {9,10,12,13}
        }))
        {
            SetAlpha(hairpinCanvas, 1f);
            hairpinCleared = true;
        }

        if (!firstLockCleared && MatchCondition(new List<int> {5,2,6,1,7,12}, new List<List<int>> {
            new() {2,3,4,5,6,7}, new() {5,6,7,8,9,10}, new() {8,9,10,11,12,13}
        }))
        {
            SetAlpha(firstLockCanvas, 1f);
            firstLockCleared = true;
        }

        if (hairpinCleared && firstLockCleared && !finalCleared &&
            MatchCondition(new List<int> {8,6,7,11,12,3,10}, new List<List<int>> {
                new() {2,3,4,5,6,7,10}, new() {5,6,7,8,9,10,13}
        }))
        {
            SetAlpha(doorLockCanvas, 1f);
            SetAlpha(secondLockCanvas, 1f);
            finalCleared = true;
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
        float elapsedTime = 600f - timer;

        if (elapsedTime <= 45f)
        {
            goatCanvas.alpha = 0f; // 완전 투명
        }
        else if (elapsedTime <= 285f) // 45초 ~ 4분 45초
        {
            float progress = (elapsedTime - 45f) / (240f); // 0~1로 진행도 계산
            goatCanvas.alpha = Mathf.Clamp01(progress);
        }
        else
        {
            goatCanvas.alpha = 1f; // 완전 불투명
        }
    }

}
