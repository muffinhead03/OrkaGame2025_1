using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class CurrentGameStatus
{
    private const string PrefKey = "current_scene_number";

    private static int currentNumber = 0;

    // 번호 -> 씬 이름
    private static readonly Dictionary<int, string> numberToSceneName = new Dictionary<int, string>();
    // 이름 -> 번호 (로그 편의용)
    private static readonly Dictionary<string, int> sceneNameToNumber = new Dictionary<string, int>(StringComparer.Ordinal);

    public static bool HasSave => PlayerPrefs.HasKey(PrefKey) && PlayerPrefs.GetInt(PrefKey, 0) != 0;

    public static event Action<bool> OnSavePresenceChanged;

    // 씬 콜백 중복 구독 방지
    private static bool sceneHooksReady = false;

    // 🔴 앱 부팅 직후(첫 씬 로드 전) 무조건 초기화: 0 상태로 만들고 저장키 삭제 → Continue 비활성 보장
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ForceZeroAtBoot()
    {
        currentNumber = 0;

        if (PlayerPrefs.HasKey(PrefKey))
        {
            PlayerPrefs.DeleteKey(PrefKey);
            PlayerPrefs.Save();
        }

        OnSavePresenceChanged?.Invoke(false);
        Debug.Log("[CurrentGameStatus] Boot reset → currentNumber=0, HasSave=false");
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
                  $"(number={GetNumberBySceneName(toName)}), currentNumber={currentNumber}, HasSave={HasSave}");
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

    /// <summary>필요 시 읽어오기</summary>
    public static void Initialize()
    {
        EnsureSceneHooks();
        currentNumber = HasSave ? PlayerPrefs.GetInt(PrefKey, 0) : 0;
    }

    public static int GetCurrentNumber()
    {
        Initialize();
        return currentNumber;
    }

    public static void SetCurrentNumber(int number, bool save = true)
    {
        Initialize();
        currentNumber = Mathf.Max(0, number);
        if (save) Save();
    }

    /// <summary>진행도 저장 (0이면 키 삭제 → Continue 비활성)</summary>
    public static void Save()
    {
        if (currentNumber == 0)
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                PlayerPrefs.DeleteKey(PrefKey);
                PlayerPrefs.Save();
            }
            OnSavePresenceChanged?.Invoke(false);
            return;
        }

        PlayerPrefs.SetInt(PrefKey, currentNumber);
        PlayerPrefs.Save();
        OnSavePresenceChanged?.Invoke(true);
    }

    public static void ClearSave()
    {
        currentNumber = 0;
        if (PlayerPrefs.HasKey(PrefKey))
        {
            PlayerPrefs.DeleteKey(PrefKey);
            PlayerPrefs.Save();
        }
        OnSavePresenceChanged?.Invoke(false);
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

    /// <summary>Continue: 저장된 번호의 씬으로 이동</summary>
    public static void ContinueGame()
    {
        if (!HasSave)
        {
            Debug.LogWarning("[CurrentGameStatus] 세이브 없음: Continue 불가");
            return;
        }

        int target = GetCurrentNumber();
        LoadByNumber(target);
    }

    /// <summary>New Game: 시작 번호로 이동</summary>
    public static void NewGame(int startNumber, bool saveAtStart = false)
    {
        currentNumber = Mathf.Max(0, startNumber);

        if (saveAtStart)
        {
            Save();
        }
        else
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                PlayerPrefs.DeleteKey(PrefKey);
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

        // ⚠ 필요 없다면 아래 fallback을 삭제하세요.
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
