using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    private bool sceneLoading = false;

    public CanvasGroup hairpinCanvasAfterDeath, firstLockCanvasAfterDeath, doorLockCanvasAfterDeath, secondLockCanvasAfterDeath;
    public CanvasGroup goatCanvasAfterDeath;

    public TextMeshProUGUI timerTextAfterDeath;

    private float timer = 600f;
    private float goatElapsed = 0f;

    private readonly Dictionary<CanvasGroup, float> _lockedAlpha = new();
    private readonly Dictionary<int, int> puzzlePositionMapAfterDeath = new();
    private readonly Dictionary<int, SlidingPuzzle1ScriptAfterDeath> positionToPuzzleAfterDeath = new();

    private bool hairpinCleared = false, firstLockCleared = false, finalCleared = false;

    private HashSet<string> _hairpinAllowed, _firstLockAllowed, _finalAllowed;

    private readonly Dictionary<int, List<int>> moveRulesAfterDeath = new()
    {
        {1, new() {3}}, {2, new() {3,5}}, {3, new() {1,2,4,6}},
        {4, new() {3,7}}, {5, new() {2,6,8}}, {6, new() {3,5,7,9}},
        {7, new() {4,6,10}}, {8, new() {5,9,11}}, {9, new() {6,8,10,12}},
        {10, new() {7,9,13}}, {11, new() {8,12}}, {12, new() {9,11,13}}, {13, new() {10,12}}
    };

    // ===== UI 패널이 열려있을 때(활성 + 원점) 일시정지 & 입력 차단 =====
    [Header("일시정지 트리거 패널(원점(0,0,0)일 때 타이머/염소/입력 모두 멈춤)")]
    [SerializeField] private RectTransform firstPanel;
    [SerializeField] private RectTransform settingPanel;
    [SerializeField] private bool requireActiveForPause = true;

    [Header("입력 차단 대상(패널 열릴 때 자동 비활성)")]
    [SerializeField] private CanvasGroup[] uiRootsToBlock;          // 퍼즐/아이템 UI 루트에 CanvasGroup 달아서 등록
    [SerializeField] private Selectable[] selectablesToDisable;     // 버튼/슬라이더 등
    [SerializeField] private Behaviour[] behavioursToDisable;       // Hover 스크립트, EventTrigger 등
    [SerializeField] private Collider[] colliders3DToDisable;       // 3D 콜라이더
    [SerializeField] private Collider2D[] colliders2DToDisable;     // 2D 콜라이더

    private bool lastPaused = false;

    private bool IsRectAtOrigin(RectTransform rt)
    {
        if (rt == null) return false;
        if (requireActiveForPause && !rt.gameObject.activeInHierarchy) return false;

        const float eps = 0.01f;
        Vector2 ap = rt.anchoredPosition;
        Vector3 lp = rt.localPosition;
        bool apZero = Mathf.Abs(ap.x) <= eps && Mathf.Abs(ap.y) <= eps;
        bool lpZero = Mathf.Abs(lp.x) <= eps && Mathf.Abs(lp.y) <= eps && Mathf.Abs(lp.z) <= eps;
        return apZero || lpZero;
    }

    private bool IsUIPaused()
    {
        return IsRectAtOrigin(firstPanel) || IsRectAtOrigin(settingPanel);
    }

    private void ApplyInputBlock(bool block)
    {
        if (uiRootsToBlock != null)
        {
            foreach (var cg in uiRootsToBlock)
            {
                if (!cg) continue;
                cg.blocksRaycasts = !block;
                cg.interactable   = !block;
                // alpha는 그대로 둡니다(보이기는 하되 입력만 차단).
            }
        }
        if (selectablesToDisable != null)
            foreach (var s in selectablesToDisable) if (s) s.interactable = !block;

        if (behavioursToDisable != null)
            foreach (var b in behavioursToDisable) if (b) b.enabled = !block;

        if (colliders3DToDisable != null)
            foreach (var c in colliders3DToDisable) if (c) c.enabled = !block;

        if (colliders2DToDisable != null)
            foreach (var c in colliders2DToDisable) if (c) c.enabled = !block;
    }
    // =============================================================

    void Awake()
    {
        Instance = this;

        SetAlphaAfterDeath(hairpinCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(firstLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(doorLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(secondLockCanvasAfterDeath, 1f);

        InitializeBoardAfterDeath();

        _hairpinAllowed = new HashSet<string>(new[]
        {
            MakeKey(new[]{2,3,5,6}),
            MakeKey(new[]{3,4,6,7}),
            MakeKey(new[]{5,6,8,9}),
            MakeKey(new[]{6,7,9,10}),
            MakeKey(new[]{8,9,11,12}),
            MakeKey(new[]{9,10,12,13})
        });

        _firstLockAllowed = new HashSet<string>(new[]
        {
            MakeKey(new[]{2,3,4,5,6,7}),
            MakeKey(new[]{5,6,7,8,9,10}),
            MakeKey(new[]{8,9,10,11,12,13})
        });

        _finalAllowed = new HashSet<string>(new[]
        {
            MakeKey(new[]{2,3,4,5,6,7,10}),
            MakeKey(new[]{5,6,7,8,9,10,13})
        });

        // 시작 시 현재 상태에 맞춰 입력 차단 적용
        ApplyInputBlock(IsUIPaused());
    }

    void InitializeBoardAfterDeath()
    {
        positionToPuzzleAfterDeath.Clear();
        puzzlePositionMapAfterDeath.Clear();

        foreach (var puzzle in puzzleScriptsAfterDeath)
        {
            var posIndex = puzzle.currentPositionIndex;
            puzzle.SetPosition(posIndex, boardPositionsAfterDeath[posIndex - 1]);
            puzzlePositionMapAfterDeath[puzzle.puzzleNumber] = posIndex; // 0(빈칸) 포함
            positionToPuzzleAfterDeath[posIndex] = puzzle;
        }
    }

    public void TryMovePuzzle(SlidingPuzzle1ScriptAfterDeath clicked)
    {
        // 패널 열려 있을 때 입력 차단
        if (IsUIPaused()) return;

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
        if (finalCleared || sceneLoading) return;

        bool uiPaused = IsUIPaused();

        // 상태 변동 시 입력 차단 토글
        if (uiPaused != lastPaused)
        {
            ApplyInputBlock(uiPaused);
            lastPaused = uiPaused;
        }

        if (!uiPaused)
        {
            timer -= Time.deltaTime;
            goatElapsed += Time.deltaTime;
        }

        int min = Mathf.FloorToInt(Mathf.Max(0f, timer) / 60f);
        int sec = Mathf.FloorToInt(Mathf.Max(0f, timer) % 60f);
        if (timerTextAfterDeath != null)
            timerTextAfterDeath.text = $"{min:00}:{sec:00}";

        UpdateGoatAlphaAfterDeath();

        if (timer <= 0f && !sceneLoading)
        {
            if (!finalCleared)
                TransitionToFail(0.5f);
        }
    }

    private void TransitionToSuccess(float delay = 1.5f)
    {
        if (sceneLoading) return;
        sceneLoading = true;
        StartCoroutine(DelayLoadSceneAfterDeath("Stage1_3", delay));
    }

    private void TransitionToFail(float delay = 0.5f)
    {
        if (sceneLoading) return;
        sceneLoading = true;
        StartCoroutine(DelayLoadSceneAfterDeath("EtInArcadiaEgoAfterSecondSlidingDeath", delay));
    }

    // ---- 안정적인 클리어 판정 + 디버그 로그 ----
    void CheckClearConditionsAfterDeath()
    {
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
            Debug.Log("[HAIRPIN] OK");
            SetAlphaAfterDeath(hairpinCanvasAfterDeath, 0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
            hairpinCleared = true;
        }

        if (!firstLockCleared && MatchAnyExactMapping(
                new List<int> { 1, 2, 5, 6, 7, 12 },
                new List<List<int>> {
                    new() { 5, 3, 2, 4, 6, 7  },
                    new() { 8, 6, 5, 7, 9, 10 },
                    new() { 11, 9, 8, 10, 12, 13 }
                }))
        {
            Debug.Log("[FIRST] OK");
            SetAlphaAfterDeath(firstLockCanvasAfterDeath, 0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
            firstLockCleared = true;
        }

        if (hairpinCleared && firstLockCleared && !finalCleared &&
            MatchAnyExactMapping(
                new List<int> { 3, 6, 7, 8, 10, 11, 12 },
                new List<List<int>> {
                    new() { 7, 3, 4, 2, 10, 5, 6 },
                    new() { 10, 6, 7, 5, 13, 8, 9 }
                }))
        {
            Debug.Log("[FINAL] OK -> Success transition");
            SetAlphaAfterDeath(doorLockCanvasAfterDeath,   0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
            SetAlphaAfterDeath(secondLockCanvasAfterDeath, 0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
            finalCleared = true;

            TransitionToSuccess(1.5f);
        }
    }

    bool MatchExactMapping(IList<int> tilesInOrder, IList<int> positionsInOrder)
    {
        if (tilesInOrder == null || positionsInOrder == null || tilesInOrder.Count != positionsInOrder.Count)
            return false;

        for (int i = 0; i < tilesInOrder.Count; i++)
        {
            int tile = tilesInOrder[i];
            int wantPos = positionsInOrder[i];
            if (!puzzlePositionMapAfterDeath.TryGetValue(tile, out int curPos) || curPos != wantPos)
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

    private bool TryGetPositionsKeyForTiles(int[] tileNums, out string key)
    {
        key = null;
        var positions = new List<int>(tileNums.Length);
        foreach (var t in tileNums)
        {
            if (!puzzlePositionMapAfterDeath.TryGetValue(t, out var pos))
                return false;
            positions.Add(pos);
        }
        key = MakeKey(positions);
        return true;
    }

    private static string MakeKey(IEnumerable<int> nums)
    {
        var list = nums.ToList();
        list.Sort();
        return string.Join(",", list);
    }

    void SetAlphaAfterDeath(CanvasGroup group, float alpha, bool lockIt = false, bool raycast = true, bool interact = true, bool ignoreParents = false)
    {
        if (group == null) return;
        group.alpha = alpha;
        group.blocksRaycasts = raycast;
        group.interactable = interact;
        group.ignoreParentGroups = ignoreParents;

        if (lockIt) _lockedAlpha[group] = alpha;
        else _lockedAlpha.Remove(group);
    }

    void UpdateGoatAlphaAfterDeath()
    {
        float elapsedTime = goatElapsed;

        if (elapsedTime <= 45f)       goatCanvasAfterDeath.alpha = 0f;
        else if (elapsedTime <= 285f) goatCanvasAfterDeath.alpha = Mathf.Clamp01((elapsedTime - 45f) / 240f);
        else                          goatCanvasAfterDeath.alpha = 1f;
    }

    private void DisableClickedObject(GameObject clicked = null)
    {
        var go = clicked != null ? clicked : EventSystem.current?.currentSelectedGameObject;
        if (go != null) go.SetActive(false);
    }

    // ===== 아이템: 버튼에서 직접 연결되는 API (패널 열려 있으면 무시) =====
    public void UseDrugBagAfterDeath()              { if (IsUIPaused()) return; DoUseDrugBag(); DisableClickedObject(); }
    public void UseJusaAfterDeath()                 { if (IsUIPaused()) return; DoUseJusa();    DisableClickedObject(); }
    public void UseCoffeeAfterDeath()               { if (IsUIPaused()) return; DoUseCoffee();  DisableClickedObject(); }
    public void UseFluteAfterDeath()                { if (IsUIPaused()) return; DoUseFlute();   DisableClickedObject(); }

    public void UseDrugBagAfterDeath(GameObject go) { if (IsUIPaused()) return; DoUseDrugBag(); DisableClickedObject(go); }
    public void UseJusaAfterDeath(GameObject go)    { if (IsUIPaused()) return; DoUseJusa();    DisableClickedObject(go); }
    public void UseCoffeeAfterDeath(GameObject go)  { if (IsUIPaused()) return; DoUseCoffee();  DisableClickedObject(go); }
    public void UseFluteAfterDeath(GameObject go)   { if (IsUIPaused()) return; DoUseFlute();   DisableClickedObject(go); }

    private void DoUseDrugBag()
    {
        SetAlphaAfterDeath(hairpinCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(firstLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(doorLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(secondLockCanvasAfterDeath, 1f);
        hairpinCleared = true; firstLockCleared = true; finalCleared = true;
        TransitionToSuccess(2f);
    }

    private void DoUseJusa()
    {
        timer += 420f;
    }

    private void DoUseCoffee()
    {
        if (!hairpinCleared && !firstLockCleared)
        {
            ApplyPuzzleLayoutAfterDeath(new List<(int pos, int puzzle)> {
                (1,0),(2,1),(3,2),(4,6),(5,4),(6,3),(7,5),(8,7),(9,8),(10,9),(11,10),(12,11),(13,13)
            });
        }
        else if (hairpinCleared && !firstLockCleared)
        {
            ApplyPuzzleLayoutAfterDeath(new List<(int pos, int puzzle)> {
                (1,0),(2,5),(3,2),(4,6),(5,1),(6,7),(7,12),(8,4),(9,11),(10,3),(11,9),(12,8),(13,10)
            });
        }
        else if (hairpinCleared && firstLockCleared)
        {
            ApplyPuzzleLayoutAfterDeath(new List<(int pos, int puzzle)> {
                (1,0),(2,9),(3,2),(4,5),(5,8),(6,6),(7,7),(8,11),(9,12),(10,3),(11,1),(12,4),(13,10)
            });
            TransitionToSuccess(3f);
        }

        CheckClearConditionsAfterDeath();
    }

    private void DoUseFlute()
    {
        timer -= 240f;
        if (timer <= 0)
            StartCoroutine(DelayLoadSceneAfterDeath("EtInArcadiaEgoAfterSecondSlidingDeath", 3f));
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

    void LateUpdate()
    {
        if (_lockedAlpha.Count == 0) return;
        foreach (var kv in _lockedAlpha)
        {
            if (kv.Key == null) continue;
            if (!Mathf.Approximately(kv.Key.alpha, kv.Value))
                kv.Key.alpha = kv.Value;
        }
    }
}
