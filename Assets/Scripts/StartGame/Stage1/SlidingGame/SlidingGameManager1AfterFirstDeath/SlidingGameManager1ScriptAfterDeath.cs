using UnityEngine;
using UnityEngine.EventSystems; // 클릭 오브젝트 비활성화용
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
    private bool sceneLoading = false;   // 씬 중복 로딩 방지

    public CanvasGroup hairpinCanvasAfterDeath, firstLockCanvasAfterDeath, doorLockCanvasAfterDeath, secondLockCanvasAfterDeath;
    public CanvasGroup goatCanvasAfterDeath;

    public TextMeshProUGUI timerTextAfterDeath;

    private float timer = 600f;
    private float goatElapsed = 0f; // 염소 전용 경과 시간

    private readonly Dictionary<CanvasGroup, float> _lockedAlpha = new(); // 알파 고정
    private readonly Dictionary<int, int> puzzlePositionMapAfterDeath = new();
    private readonly Dictionary<int, SlidingPuzzle1ScriptAfterDeath> positionToPuzzleAfterDeath = new();

    private bool hairpinCleared = false, firstLockCleared = false, finalCleared = false;

    // 허용 세트(정답) 캐시
    private HashSet<string> _hairpinAllowed, _firstLockAllowed, _finalAllowed;

    private readonly Dictionary<int, List<int>> moveRulesAfterDeath = new()
    {
        {1, new() {3}}, {2, new() {3,5}}, {3, new() {1,2,4,6}},
        {4, new() {3,7}}, {5, new() {2,6,8}}, {6, new() {3,5,7,9}},
        {7, new() {4,6,10}}, {8, new() {5,9,11}}, {9, new() {6,8,10,12}},
        {10, new() {7,9,13}}, {11, new() {8,12}}, {12, new() {9,11,13}}, {13, new() {10,12}}
    };

    #region ⏸ UI 패널 원점일 때 일시정지
    [Header("일시정지 트리거 패널(원점(0,0,0)일 때 타이머/염소 멈춤)")]
    [SerializeField] private RectTransform firstPanel;   // FirstPanel
    [SerializeField] private RectTransform settingPanel; // SettingPanel
    [Tooltip("패널이 활성(ActiveInHierarchy)일 때만 일시정지 판정할지 여부")]
    [SerializeField] private bool requireActiveForPause = true;

    private bool IsRectAtOrigin(RectTransform rt)
    {
        if (rt == null) return false;
        if (requireActiveForPause && !rt.gameObject.activeInHierarchy) return false;

        // anchoredPosition(캔버스) / localPosition(예외) 둘 다 체크, 오차 허용
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
    #endregion

    void Awake()
    {
        Instance = this;

        SetAlphaAfterDeath(hairpinCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(firstLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(doorLockCanvasAfterDeath, 1f);
        SetAlphaAfterDeath(secondLockCanvasAfterDeath, 1f);

        InitializeBoardAfterDeath();

        // 정답 세트 키 캐싱
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
    }

    void InitializeBoardAfterDeath()
    {
        positionToPuzzleAfterDeath.Clear();
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

        // 🔒 패널이 (0,0,0)에 있으면 시간/염소 경과를 멈춤
        bool uiPaused = IsUIPaused();

        if (!uiPaused)
        {
            timer -= Time.deltaTime;
            goatElapsed += Time.deltaTime; // 염소는 타이머와 무관하지만 같은 조건으로 일시정지
        }

        // 타이머 텍스트(정지 중이면 값 유지)
        int min = Mathf.FloorToInt(Mathf.Max(0f, timer) / 60f);
        int sec = Mathf.FloorToInt(Mathf.Max(0f, timer) % 60f);
        if (timerTextAfterDeath != null)
            timerTextAfterDeath.text = $"{min:00}:{sec:00}";

        UpdateGoatAlphaAfterDeath();

        // 시간 초과(단, UI에 의해 일시정지였다면 timer가 줄지 않으니 여기 안 들어옴)
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
        // Hairpin
        if (!hairpinCleared)
        {
            var hairpinTiles = new[] { 3, 5, 8, 9 };
            if (TryGetPositionsKeyForTiles(hairpinTiles, out var key))
            {
                Debug.Log($"[HAIRPIN] currentKey = {key}");
                if (_hairpinAllowed.Contains(key))
                {
                    Debug.Log("Hairpin 조건 충족 → 클리어 처리");
                    SetAlphaAfterDeath(hairpinCanvasAfterDeath, 0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
                    hairpinCleared = true;
                }
            }
            else
            {
                Debug.LogWarning("[HAIRPIN] 키 계산 실패: puzzlePositionMapAfterDeath에 누락된 타일이 있습니다(3,5,8,9 확인).");
            }
        }

        // First lock
        if (!firstLockCleared)
        {
            var firstLockTiles = new[] { 5, 2, 6, 1, 7, 12 };
            if (TryGetPositionsKeyForTiles(firstLockTiles, out var key))
            {
                Debug.Log($"[FIRST] currentKey = {key}");
                if (_firstLockAllowed.Contains(key))
                {
                    Debug.Log("FirstLock 조건 충족 → 클리어 처리");
                    SetAlphaAfterDeath(firstLockCanvasAfterDeath, 0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
                    firstLockCleared = true;
                }
            }
            else
            {
                Debug.LogWarning("[FIRST] 키 계산 실패: (5,2,6,1,7,12) 중 누락 존재.");
            }
        }

        // Final
        if (hairpinCleared && firstLockCleared && !finalCleared)
        {
            var finalTiles = new[] { 8, 6, 7, 11, 12, 3, 10 };
            if (TryGetPositionsKeyForTiles(finalTiles, out var key))
            {
                Debug.Log($"[FINAL] currentKey = {key}");
                if (_finalAllowed.Contains(key))
                {
                    Debug.Log("Final 조건 충족 → 문 잠금 클리어 처리");
                    SetAlphaAfterDeath(doorLockCanvasAfterDeath,   0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
                    SetAlphaAfterDeath(secondLockCanvasAfterDeath, 0.1f, lockIt: true, raycast: false, interact: false, ignoreParents: true);
                    finalCleared = true;

                    // ✅ 최종 성공 → 성공 씬으로 전환
                    TransitionToSuccess(1.5f);
                }
            }
            else
            {
                Debug.LogWarning("[FINAL] 키 계산 실패: (8,6,7,11,12,3,10) 중 누락 존재.");
            }
        }
    }

    // 현재 타일들 위치 키 만들기(안전 가드)
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

    // ---- 알파 설정(잠금 옵션 지원) ----
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

    // 염소 알파는 goatElapsed만 사용
    void UpdateGoatAlphaAfterDeath()
    {
        float elapsedTime = goatElapsed;

        if (elapsedTime <= 45f)       goatCanvasAfterDeath.alpha = 0f;
        else if (elapsedTime <= 285f) goatCanvasAfterDeath.alpha = Mathf.Clamp01((elapsedTime - 45f) / 240f);
        else                          goatCanvasAfterDeath.alpha = 1f;
    }

    // 클릭한 오브젝트 비활성화
    private void DisableClickedObject(GameObject clicked = null)
    {
        var go = clicked != null ? clicked : EventSystem.current?.currentSelectedGameObject;
        if (go != null) go.SetActive(false);
    }

    // ===== 아이템: 공개 API (버튼에 그대로 연결) =====
    public void UseDrugBagAfterDeath()              { DoUseDrugBag(); DisableClickedObject(); }
    public void UseJusaAfterDeath()                 { DoUseJusa();    DisableClickedObject(); }
    public void UseCoffeeAfterDeath()               { DoUseCoffee();  DisableClickedObject(); }
    public void UseFluteAfterDeath()                { DoUseFlute();   DisableClickedObject(); }

    public void UseDrugBagAfterDeath(GameObject go) { DoUseDrugBag(); DisableClickedObject(go); }
    public void UseJusaAfterDeath(GameObject go)    { DoUseJusa();    DisableClickedObject(go); }
    public void UseCoffeeAfterDeath(GameObject go)  { DoUseCoffee();  DisableClickedObject(go); }
    public void UseFluteAfterDeath(GameObject go)   { DoUseFlute();   DisableClickedObject(go); }

    // ===== 실제 동작 =====
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

    // 잠금 알파 재적용(외부 덮어쓰기 무력화)
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
