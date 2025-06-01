using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요


public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Setup")]
    public RectTransform puzzleParent;
    public GameObject[] tilePrefabs; // PuzzleTile_0 ~ PuzzleTile_12

    private PuzzleTile[] tiles = new PuzzleTile[13];
    private Dictionary<int, Vector2> tilePositions = new Dictionary<int, Vector2>();
    public GameObject hairPin;
    public GameObject firstLock;
    public GameObject secondLock;
    public GameObject doorCircle;

    private bool hairPinOn = false;
    private bool firstLockOn = false;
    private bool secondLockOn = false;
    
    private bool hairPinLocked = false;
    private bool firstLockLocked = false;
    private bool secondLockLocked = false;

    
        

    [Header("Transition")]
    public GameObject blackFlashOverlay; // 검정색 UI 오브젝트 (Image 컴포넌트 필요)

    private bool transitionStarted = false;


    void Start()
    {
        InitPositions();
        SetupTiles();

        SetUIAlpha(hairPin, 0.3f);
        SetUIAlpha(firstLock, 0.3f);
        SetUIAlpha(secondLock, 0.3f);
        SetUIAlpha(doorCircle, 0.3f);
    }

    void InitPositions()
    {
        tilePositions = new Dictionary<int, Vector2>
        {
            { 0, new Vector2(0, 546) },
            { 1, new Vector2(-303, 273) },
            { 2, new Vector2(0, 273) },
            { 3, new Vector2(303, 273) },
            { 4, new Vector2(-303, 0) },
            { 5, new Vector2(0, 0) },
            { 6, new Vector2(303, 0) },
            { 7, new Vector2(-303, -273) },
            { 8, new Vector2(0, -273) },
            { 9, new Vector2(303, -273) },
            {10, new Vector2(-303, -541) },
            {11, new Vector2(0, -541) },
            {12, new Vector2(303, -541) }
        };
    }

    void SetupTiles()
    {
        foreach (Transform child in puzzleParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 13; i++)
        {
            if (tilePrefabs[i] == null) continue;

            GameObject tileObj = Instantiate(tilePrefabs[i], puzzleParent);
            RectTransform rt = tileObj.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchoredPosition = tilePositions[i];

            PuzzleTile tile = tileObj.GetComponent<PuzzleTile>();
            tile.tileIndex = i;
            tile.currentSlot = i;
            tile.puzzleManager = this;

            tiles[i] = tile;
        }
    }

    public void OnTileClicked(PuzzleTile clickedTile)
    {
        int clickedIndex = clickedTile.currentSlot;
        int emptyIndex = GetEmptySlotIndex();

        if (GetMovableIndices(clickedIndex).Contains(emptyIndex))
        {
            Swap(clickedIndex, emptyIndex);
        }
    }

    int GetEmptySlotIndex()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null && tiles[i].tileIndex == 0)
                return i;
        }
        return -1;
    }

    void Swap(int from, int to)
    {
        PuzzleTile a = tiles[from];
        PuzzleTile b = tiles[to];

        tiles[from] = b;
        tiles[to] = a;

        a.currentSlot = to;
        b.currentSlot = from;

        a.GetComponent<RectTransform>().anchoredPosition = tilePositions[to];
        b.GetComponent<RectTransform>().anchoredPosition = tilePositions[from];

        CheckPuzzleConditions();
    }

    void CheckPuzzleConditions()
{
    PrintTilesSummary();

    // HairPin 조건
    int[][] hairPinPatterns = new int[][] {
        new int[] { 1, 2, 4, 5 },
        new int[] { 2, 3, 5, 6 },
        new int[] { 4, 5, 7, 8 },
        new int[] { 5, 6, 8, 9 },
        new int[] { 7, 8, 10, 11 },
        new int[] { 8, 9, 11, 12 }
    };

    int[] hairPinTarget = new int[] { 3, 5, 8, 9 };
    bool hairPinMatched = false;

    foreach (var pattern in hairPinPatterns)
    {
        if (MatchExactTiles(pattern, hairPinTarget))
        {
            Debug.Log($"✅ HairPin 매칭된 위치: [{string.Join(",", pattern)}]");
            hairPinMatched = true;
            break;
        }
    }

    if (!hairPinLocked && hairPinMatched)
    {
        SetUIAlpha(hairPin, 1f);
        hairPinOn = true;
        hairPinLocked = true;
    }
    else if (!hairPinLocked)
    {
        SetUIAlpha(hairPin, 0.3f);
        hairPinOn = false;
    }


    // FirstLock 조건
    int[][] firstLockPatterns = new int[][] {
        new int[] { 1, 2, 3, 4, 5, 6 },
        new int[] { 4, 5, 6, 7, 8, 9 },
        new int[] { 7, 8, 9, 10, 11, 12 }
    };
    int[] firstLockTarget = new int[] { 5, 2, 6, 1, 7, 12 };
    bool firstLockMatched = false;

    foreach (var pattern in firstLockPatterns)
    {
        if (MatchExactTiles(pattern, firstLockTarget))
        {
            Debug.Log($"✅ FirstLock 매칭된 위치: [{string.Join(",", pattern)}]");
            firstLockMatched = true;
            break;
        }
    }

    if (!firstLockLocked && firstLockMatched)
    {
        SetUIAlpha(firstLock, 1f);
        firstLockOn = true;
        firstLockLocked = true;
    }
    else if (!firstLockLocked)
    {
        SetUIAlpha(firstLock, 0.3f);
        firstLockOn = false;
    }


    // SecondLock + DoorCircle 조건
    int[][] secondLockPatterns = new int[][] {
        new int[] { 1, 2, 3, 4, 5, 6, 9 },
        new int[] { 4, 5, 6, 7, 8, 9, 12 }
    };
    int[] secondLockTarget = new int[] { 8, 6, 7, 11, 12, 3, 10 };
    bool secondLockMatched = false;

    foreach (var pattern in secondLockPatterns)
    {
        if (MatchExactTiles(pattern, secondLockTarget))
        {
            Debug.Log($"✅ SecondLock 매칭된 위치: [{string.Join(",", pattern)}]");
            secondLockMatched = true;
            break;
        }
    }

    if (secondLockMatched)
    {
        SetUIAlpha(secondLock, 1f);
        SetUIAlpha(doorCircle, 1f);
        secondLockOn = true;
    }
    else
    {
        SetUIAlpha(secondLock, 0.3f);
        SetUIAlpha(doorCircle, 0.3f);
        secondLockOn = false;
    }
    
    // 모든 요소가 불투명한 경우 트리거
    if (!transitionStarted && hairPinOn && firstLockOn && secondLockOn)
    {
        transitionStarted = true;
        StartCoroutine(HandlePuzzleCompletion());
    }

    
}

    IEnumerator HandlePuzzleCompletion()
    {
        Debug.Log("🌟 모든 잠금 해제 완료! 트랜지션 시작");

        // 음악 중단
        AudioSource bgm = FindObjectOfType<AudioSource>();
        if (bgm != null) bgm.Stop();

        // 깜빡임: 알파 0 → 1 → 0
        if (blackFlashOverlay.TryGetComponent(out Image img))
        {
            // 알파 0 → 1
            yield return StartCoroutine(FadeAlpha(img, 0f, 1f, 0.25f));
            // 알파 1 → 0
            yield return StartCoroutine(FadeAlpha(img, 1f, 0f, 0.25f));
        }

        // 씬 전환
        Debug.Log("🚪 다음 씬으로 이동: 'A'");
        SceneManager.LoadScene("A");
    }


    bool MatchExactTiles(int[] slots, int[] targets)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (targets[i] == -1) continue;
            if (tiles[slots[i]] == null || tiles[slots[i]].tileIndex != targets[i]) return false;
        }
        return true;
    }

    bool IsTileInSlots(int tileIndex, int[] slots)
    {
        foreach (int i in slots)
        {
            if (tiles[i] != null && tiles[i].tileIndex == tileIndex)
                return true;
        }
        return false;
    }

    void SetUIAlpha(GameObject obj, float alpha)
    {
        if (obj.TryGetComponent(out Image img))
        {
            Color color = img.color;
            float beforeAlpha = color.a;

            color.a = alpha;
            img.color = color;

            Debug.Log($"🎨 {obj.name} alpha 변경됨: {beforeAlpha} → {alpha}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {obj.name} 에서 Image 컴포넌트를 찾을 수 없습니다!");
        }
    }


    List<int> GetMovableIndices(int tileIndex)
    {
        return tileIndex switch
        {
            0 => new List<int> { 2 },
            1 => new List<int> { 2, 4 },
            2 => new List<int> { 0, 1, 3, 5 },
            3 => new List<int> { 2, 6 },
            4 => new List<int> { 1, 5, 7 },
            5 => new List<int> { 2, 4, 6, 8 },
            6 => new List<int> { 3, 5, 9 },
            7 => new List<int> { 4, 8, 10 },
            8 => new List<int> { 5, 7, 9, 11 },
            9 => new List<int> { 6, 8, 12 },
            10 => new List<int> { 7, 11 },
            11 => new List<int> { 8, 10, 12 },
            12 => new List<int> { 9, 11 },
            _ => new List<int>()
        };
    }
    void PrintTilesSummary()
    {
        Debug.Log("🧩 [타일 슬롯 상태 요약]");
        for (int i = 0; i < tiles.Length; i++)
        {
            string info = tiles[i] != null
                ? $"Slot {i}: TileIndex {tiles[i].tileIndex}"
                : $"Slot {i}: null";
            Debug.Log(info);
        }
    }

    IEnumerator FadeAlpha(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            c.a = Mathf.Lerp(from, to, t);
            img.color = c;
            yield return null;
        }

        c.a = to;
        img.color = c;
    }
    
    

    
}
