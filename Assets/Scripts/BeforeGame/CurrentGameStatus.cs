using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class CurrentGameStatus
{
    // ✅ 2개 키로 분리
    // - BootKey: 부팅 시 0으로 만들기 위한 "임시/부팅용" 키 (ForceZeroAtBoot가 건드려도 됨)
    // - SaveKey: Continue가 사용할 "진짜 저장" 키 (부팅에도 유지되어야 함)
    private const string BootKey = "current_scene_number";   // 기존 키: 부팅용으로만 사용
    private const string SaveKey = "save_scene_number";      // 새 키: 진짜 저장(Continue)

    private static int currentNumber = 0;

    // 번호 -> 씬 이름
    private static readonly Dictionary<int, string> numberToSceneName = new Dictionary<int, string>();
    // 이름 -> 번호 (로그 편의용)
    private static readonly Dictionary<string, int> sceneNameToNumber = new Dictionary<string, int>(StringComparer.Ordinal);

    // ✅ Continue 여부는 "진짜 저장" 기준
    public static bool HasSave => PlayerPrefs.HasKey(SaveKey) && PlayerPrefs.GetInt(SaveKey, 0) != 0;

    public static event Action<bool> OnSavePresenceChanged;

    // 씬 콜백 중복 구독 방지
    private static bool sceneHooksReady = false;

    // 🔴 앱 부팅 직후(첫 씬 로드 전) 무조건 초기화: 0 상태로 만들고 "부팅용 키"만 초기화
    //    ※ 진짜 저장(SaveKey)은 건드리지 않음 → 부팅해도 Continue 유지 가능
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ForceZeroAtBoot()
    {
        currentNumber = 0;

        // ✅ 부팅용 키만 삭제/리셋
        if (PlayerPrefs.HasKey(BootKey))
        {
            PlayerPrefs.DeleteKey(BootKey);
            PlayerPrefs.Save();
        }

        // ⚠ 여기서 OnSavePresenceChanged(false)를 쏘면,
        // SaveKey가 살아있는데도 UI가 순간적으로 꺼질 수 있음.
        // 메인메뉴에서 HasSave를 읽어 Refresh 하면 충분하므로 생략.
        Debug.Log($"[CurrentGameStatus] Boot reset → currentNumber=0, BootKey cleared, HasSave(real)={HasSave}");
    }

    // 도중 재컴파일/도메인 리로드 대비 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RuntimeInit()
    {
        currentNumber = 0;
        numberToSceneName.Clear();
        sceneNameToNumber.Clear();
        sceneHooksReady = false;
    }

    // 첫 진입/로드 시 한 번만 씬 이벤트 구독
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void HookSceneCallbacks() => EnsureSceneHooks();

    private static void EnsureSceneHooks()
    {
        if (sceneHooksReady) return;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneHooksReady = true;

        var cur = SceneManager.GetActiveScene();
        if (cur.IsValid())
        {
            Debug.Log($"[CurrentGameStatus] Active at start: '{cur.name}' (number={GetNumberBySceneName(cur.name)})");
        }
    }

    private static void OnActiveSceneChanged(Scene from, Scene to)
    {
        string fromName = from.IsValid() ? from.name : "(none)";
        string toName = to.IsValid() ? to.name : "(none)";
        Debug.Log($"[CurrentGameStatus] ActiveScene changed: '{fromName}' → '{toName}' " +
                  $"(number={GetNumberBySceneName(toName)}), currentNumber={currentNumber}, HasSave(real)={HasSave}");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid()) return;
        Debug.Log($"[CurrentGameStatus] Scene loaded: '{scene.name}' (mode={mode}, number={GetNumberBySceneName(scene.name)})");
    }

    private static int GetNumberBySceneName(string sceneName)
    {
        return sceneNameToNumber.TryGetValue(sceneName, out var num) ? num : -1;
    }

    /// <summary>
    /// 필요 시 읽어오기:
    /// - 런타임 currentNumber는 "진짜 저장(SaveKey)"에서만 읽는다
    /// - BootKey는 Continue 판단/로드에 관여하지 않는다
    /// </summary>
    public static void Initialize()
    {
        EnsureSceneHooks();
        currentNumber = HasSave ? PlayerPrefs.GetInt(SaveKey, 0) : 0;
    }

    public static int GetCurrentNumber()
    {
        Initialize();
        return currentNumber;
    }

    public static int GetSavedNumber()
    {
        return PlayerPrefs.GetInt(SaveKey, 0);
    }

    public static void SetCurrentNumber(int number, bool save = true)
    {
        EnsureSceneHooks();
        currentNumber = Mathf.Max(0, number);
        if (save) Save();
    }

    /// <summary>
    /// ✅ 진짜 진행도 저장은 SaveKey에만 저장.
    /// (0이면 SaveKey 삭제 → Continue 비활성)
    /// </summary>
    public static void Save()
    {
        if (currentNumber == 0)
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
            OnSavePresenceChanged?.Invoke(false);
            return;
        }

        PlayerPrefs.SetInt(SaveKey, currentNumber);
        PlayerPrefs.Save();
        OnSavePresenceChanged?.Invoke(true);
    }

    /// <summary>진짜 저장 삭제</summary>
    public static void ClearSave()
    {
        currentNumber = 0;
        if (PlayerPrefs.HasKey(SaveKey))
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }
        OnSavePresenceChanged?.Invoke(false);
    }

    /// <summary>
    /// (선택) 부팅용 키를 0으로 세팅/삭제하고 싶을 때 사용.
    /// 게임 로직에는 영향 없음.
    /// </summary>
    public static void ClearBootKey()
    {
        if (PlayerPrefs.HasKey(BootKey))
        {
            PlayerPrefs.DeleteKey(BootKey);
            PlayerPrefs.Save();
        }
    }

    /// <summary>번호 → 씬 이름 등록</summary>
    public static void RegisterScene(int number, string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        numberToSceneName[number] = sceneName;
        sceneNameToNumber[sceneName] = number; // 역매핑
    }

    public static string GetSceneName(int number)
    {
        return numberToSceneName.TryGetValue(number, out var n) ? n : string.Empty;
    }

    /// <summary>Continue: 저장된 번호(SaveKey)의 씬으로 이동</summary>
    public static void ContinueGame()
    {
        if (!HasSave)
        {
            Debug.LogWarning("[CurrentGameStatus] 세이브 없음: Continue 불가");
            return;
        }

        int target = PlayerPrefs.GetInt(SaveKey, 0);
        LoadByNumber(target);
    }

    /// <summary>
    /// New Game: 시작 번호로 이동
    /// - saveAtStart=true면 즉시 SaveKey에 저장
    /// - false면 SaveKey를 지워서 Continue 끔 (원래 동작 유지)
    /// </summary>
    public static void NewGame(int startNumber, bool saveAtStart = false)
    {
        currentNumber = Mathf.Max(0, startNumber);

        if (saveAtStart)
        {
            Save();
        }
        else
        {
            // ✅ 진짜 저장만 지움
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
                OnSavePresenceChanged?.Invoke(false);
            }
        }

        LoadByNumber(currentNumber);
    }

    public static void StartFreshAtScene0()
    {
        ClearSave();
        LoadByNumber(0);
    }

    public static void BootToScene0_NoSave()
    {
        // 부팅용 상태로 "현재 번호"만 0으로 세팅. (진짜 저장은 건드리지 않음)
        SetCurrentNumber(0, save: false);
        LoadByNumber(0);
    }

    /// <summary>공통 로딩: 등록 이름 우선 → (마지막 수단) 빌드 인덱스</summary>
    private static void LoadByNumber(int number)
    {
        Debug.Log($"[CurrentGameStatus] Loading by number: {number} " +
                  $"→ name='{(numberToSceneName.TryGetValue(number, out var nm) ? nm : "(unmapped)")}'");

        if (numberToSceneName.TryGetValue(number, out var sceneName) && !string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        // ⚠ sceneNumber가 BuildIndex와 1:1이 아니면 이 fallback은 오동작할 수 있음.
        if (number >= 0 && number < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(number);
            return;
        }

        Debug.LogError($"[CurrentGameStatus] 번호 {number} 에 해당하는 씬을 찾지 못했습니다. (등록/빌드세팅 확인)");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameDebug/Clear Save (Reset to 0)")]
    private static void _Menu_ClearSave()
    {
        ClearSave();
        Debug.Log("[CurrentGameStatus] Save cleared (0). HasSave=" + HasSave);
    }

    [UnityEditor.MenuItem("GameDebug/Start Fresh at Scene 0")]
    private static void _Menu_StartFreshAtScene0()
    {
        StartFreshAtScene0();
    }

    [UnityEditor.MenuItem("GameDebug/Boot to Scene 0 (No Save)")]
    private static void _Menu_BootNoSave()
    {
        BootToScene0_NoSave();
    }
#endif
}
