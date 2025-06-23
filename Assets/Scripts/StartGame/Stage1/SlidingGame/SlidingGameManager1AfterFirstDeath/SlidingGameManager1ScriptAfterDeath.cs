using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlidingGameManager1ScriptAfterDeath : MonoBehaviour
{
    public static SlidingGameManager1ScriptAfterDeath Instance;

    public SlidingPuzzle1ScriptAfterDeath[] puzzleScriptsAfterDeath;
    public Vector2[] boardPositionsAfterDeath;

    public GameObject hairpinAfterDeath, firstLockAfterDeath, doorLockAfterDeath, secondLockAfterDeath;
    public GameObject DrugBagAfterDeath, JusaAfterDeath, coffeeAfterDeath, fluteAfterDeath;

    public CanvasGroup hairpinCanvasAfterDeath, firstLockCanvasAfterDeath, doorLockCanvasAfterDeath, secondLockCanvasAfterDeath;
    public CanvasGroup goatCanvasAfterDeath;

    public TextMeshProUGUI timerTextAfterDeath;
    private float timer = 600f;

    private Dictionary<int, int> puzzlePositionMapAfterDeath = new();
    private Dictionary<int, SlidingPuzzle1ScriptAfterDeath> positionToPuzzleAfterDeath = new();

    private bool hairpinCleared = false, firstLockCleared = false, finalCleared = false;

    private readonly Dictionary<int, List<int>> moveRulesAfterDeath = new()
    {
        {1, new() {3}}, {2, new() {3,5}}, {3, new() {1,2,4,6}},
        {4, new() {3,7}}, {5, new() {2,6,8}}, {6, new() {3,5,7,9}},
        {7, new() {4,6,10}}, {8, new() {5,9,11}}, {9, new() {6,8,10,12}},
        {10, new() {7,9,13}}, {11, new() {8,12}}, {12, new() {9,11,13}}, {13, new() {10,12}}
    };

    void Awake()
    {
        Instance = this;
    
        // 초기 CanvasGroup들 전부 불투명하게 설정
        SetAlphaAfterDeath(hairpinCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(firstLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(doorLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(secondLockCanvasAfterDeath, 1f);

        InitializeBoardAfterDeath();
    }


    void InitializeBoardAfterDeath()
    {
        positionToPuzzleAfterDeath.Clear();
        foreach (var puzzle in puzzleScriptsAfterDeath)
        {
            var posIndex = puzzle.currentPositionIndex;
            puzzle.SetPosition(posIndex, boardPositionsAfterDeath[posIndex - 1]);
            puzzlePositionMapAfterDeath[puzzle.puzzleNumber] = posIndex;
            positionToPuzzleAfterDeath[posIndex] = puzzle;
        }
    }

    public void TryMovePuzzle(SlidingPuzzle1ScriptAfterDeath clicked)
    {
        int emptyPos = puzzlePositionMapAfterDeath[0];
        int clickedPos = clicked.currentPositionIndex;

        if (moveRulesAfterDeath[emptyPos].Contains(clickedPos))
        {
            SlidingPuzzle1ScriptAfterDeath empty = positionToPuzzleAfterDeath[emptyPos];

            clicked.SetPosition(emptyPos, boardPositionsAfterDeath[emptyPos - 1]);
            empty.SetPosition(clickedPos, boardPositionsAfterDeath[clickedPos - 1]);

            puzzlePositionMapAfterDeath[clicked.puzzleNumber] = emptyPos;
            puzzlePositionMapAfterDeath[0] = clickedPos;

            positionToPuzzleAfterDeath[emptyPos] = clicked;
            positionToPuzzleAfterDeath[clickedPos] = empty;

            Debug.Log($"{clicked.puzzleNumber}번 타일이 {clickedPos}번 → {emptyPos}번 좌표로 이동");
            CheckClearConditionsAfterDeath();
        }
        else
        {
            Debug.Log($"{clicked.puzzleNumber}번 타일은 {clickedPos}번에서 이동 불가");
        }
    }

    void Update()
    {
        if (finalCleared) return;

        timer -= Time.deltaTime;
        int min = Mathf.FloorToInt(timer / 60);
        int sec = Mathf.FloorToInt(timer % 60);
        timerTextAfterDeath.text = $"{min:00}:{sec:00}";

        UpdateGoatAlphaAfterDeath();

        if (timer <= 0)
        {
            if (!hairpinCleared || !firstLockCleared || !finalCleared)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("EtInArcadiaEgoAfterSecondSlidingDeath");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Et in Arcadia ego_SlidingGameFirst");
            }
        }
    }

    void CheckClearConditionsAfterDeath()
    {
        // hairpin
        if (!hairpinCleared && MatchConditionAfterDeath(new List<int> {3,5,8,9}, new List<List<int>> {
                new() {2,3,5,6}, new() {3,4,6,7}, new() {5,6,8,9},
                new() {6,7,9,10}, new() {8,9,11,12}, new() {9,10,12,13}
            }))
        {
            Debug.Log("Hairpin 조건 충족 → 클리어 처리");
            SetAlphaAfterDeath(hairpinCanvasAfterDeath, 0.1f); // 변경
            hairpinCleared = true;
        }

        // first lock
        if (!firstLockCleared && MatchConditionAfterDeath(new List<int> {5,2,6,1,7,12}, new List<List<int>> {
                new() {2,3,4,5,6,7}, new() {5,6,7,8,9,10}, new() {8,9,10,11,12,13}
            }))
        {
            Debug.Log("FirstLock 조건 충족 → 클리어 처리");
            SetAlphaAfterDeath(firstLockCanvasAfterDeath, 0.1f); // 변경
            firstLockCleared = true;
        }

        // door lock (final clear)
        if (hairpinCleared && firstLockCleared && !finalCleared &&
            MatchConditionAfterDeath(new List<int> {8,6,7,11,12,3,10}, new List<List<int>> {
                new() {2,3,4,5,6,7,10}, new() {5,6,7,8,9,10,13}
            }))
        {
            Debug.Log("Final 조건 충족 → 문 잠금 클리어 처리");
            SetAlphaAfterDeath(doorLockCanvasAfterDeath, 0.1f);   // 변경
            SetAlphaAfterDeath(secondLockCanvasAfterDeath, 0.1f); // 변경
            finalCleared = true;
        }
    }



    bool MatchConditionAfterDeath(List<int> puzzleNums, List<List<int>> validPosSets)
    {
        var current = puzzleNums.Select(p => puzzlePositionMapAfterDeath[p]).OrderBy(x => x).ToList();
        Debug.Log($"MatchCondition check - 퍼즐 번호들: {string.Join(",", puzzleNums)}");
        Debug.Log($"현재 위치들: {string.Join(",", current)}");

        foreach (var set in validPosSets)
        {
            var orderedSet = set.OrderBy(x => x).ToList();
            Debug.Log($"비교 대상 위치 세트: {string.Join(",", orderedSet)}");

            if (orderedSet.SequenceEqual(current))
            {
                Debug.Log("일치하는 위치 세트 발견!");
                return true;
            }
        }

        Debug.Log("일치하는 위치 세트 없음");
        return false;
    }


    void SetAlphaAfterDeath(CanvasGroup group, float alpha)
    {
        if (group == null)
        {
            Debug.LogWarning("SetAlphaAfterDeath: group is null!");
            return;
        }

        Debug.Log($"SetAlphaAfterDeath: {group.gameObject.name} alpha → {alpha}");

        group.alpha = alpha;
        group.interactable = true;
        group.blocksRaycasts = true;
    }



    void UpdateGoatAlphaAfterDeath()
    {
        float elapsedTime = 600f - timer;

        if (elapsedTime <= 45f)
        {
            goatCanvasAfterDeath.alpha = 0f;
        }
        else if (elapsedTime <= 285f)
        {
            float progress = (elapsedTime - 45f) / 240f;
            goatCanvasAfterDeath.alpha = Mathf.Clamp01(progress);
        }
        else
        {
            goatCanvasAfterDeath.alpha = 1f;
        }
    }

    public void UseDrugBagAfterDeath()
    {
        SetAlphaAfterDeath(hairpinCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(firstLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(doorLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(secondLockCanvasAfterDeath, 1f);
        hairpinCleared = true;
        firstLockCleared = true;
        finalCleared = true;
        StartCoroutine(DelayLoadSceneAfterDeath("Stage1_3", 2f));
    }

    public void UseJusaAfterDeath()
    {
        timer += 420f;
    }

    public void UseCoffeeAfterDeath()
    {
        if (!hairpinCleared && !firstLockCleared)
        {
            SetAlphaAfterDeath(hairpinCanvasAfterDeath, 1f);
            hairpinCleared = true;
            ApplyPuzzleLayoutAfterDeath(new List<(int, int)> {
                (1,0),(2,1),(3,2),(4,6),(5,4),(6,3),(7,5),(8,7),(9,8),(10,9),(11,10),(12,11),(13,13)
            });
        }
        else if (hairpinCleared && !firstLockCleared)
        {
            ApplyPuzzleLayoutAfterDeath(new List<(int, int)> {
                (1,0),(2,5),(3,2),(4,6),(5,1),(6,7),(7,12),(8,4),(9,11),(10,3),(11,9),(12,8),(13,10)
            });
        }
        else if (hairpinCleared && firstLockCleared)
        {
            ApplyPuzzleLayoutAfterDeath(new List<(int, int)> {
                (1,0),(2,9),(3,2),(4,5),(5,8),(6,6),(7,7),(8,11),(9,12),(10,3),(11,1),(12,4),(13,10)
            });
            StartCoroutine(DelayLoadSceneAfterDeath("Stage1_3", 3f));
        }
        
        CheckClearConditionsAfterDeath();

    }

    public void UseFluteAfterDeath()
    {
        timer -= 240f;
        if (timer <= 0)
        {
            StartCoroutine(DelayLoadSceneAfterDeath("EtInArcadiaEgoAfterSecondSlidingDeath", 3f));
        }
    }

    IEnumerator DelayLoadSceneAfterDeath(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    void ApplyPuzzleLayoutAfterDeath(List<(int pos, int puzzle)> layout)
    {
        foreach (var (pos, puzzleNum) in layout)
        {
            var puzzle = puzzleScriptsAfterDeath.FirstOrDefault(p => p.puzzleNumber == puzzleNum);
            if (puzzle != null)
            {
                puzzle.SetPosition(pos, boardPositionsAfterDeath[pos - 1]);
                puzzlePositionMapAfterDeath[puzzle.puzzleNumber] = pos;
                positionToPuzzleAfterDeath[pos] = puzzle;
            }
        }
    }
}
