// SceneAnchor.cs
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 각 씬에 붙여 두는 앵커:
/// - 이 씬의 고유 번호 지정
/// - 이 번호가 가리키는 '실제 로드할 씬'을 이름 또는 SceneAsset로 지정
/// - 진입 시 번호 반영/저장 옵션
/// </summary>
[AddComponentMenu("Game/Scene Anchor")]
public class SceneAnchor : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("이 씬(또는 이 번호가 대표하는 진행 단계)의 고유 번호")]
    public int sceneNumber = 0;

    [Header("Target Scene (Continue/NewGame가 로드할 대상)")]
    [Tooltip("직접 씬 이름을 지정합니다. (Build Settings에 등록 필요)")]
    public string targetSceneName = "";

    #if UNITY_EDITOR
    [Tooltip("편의를 위한 에디터 전용 필드. 지정 시 targetSceneName을 자동 동기화합니다.")]
    public SceneAsset targetSceneAsset;
    #endif

    [Header("Auto Apply")]
    [Tooltip("씬이 로드되면 현재 번호만 반영(저장 안 함)")]
    public bool setNumberOnAwake = true;

    [Tooltip("씬이 시작되면 현재 번호를 저장까지 수행")]
    public bool saveOnStart = false;

    private void Awake()
    {
        // 에디터에서 SceneAsset로 이름이 잡혔다면 동기화(플레이 타임에도 안전)
        #if UNITY_EDITOR
        if (targetSceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(targetSceneAsset);
            string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(nameNoExt))
                targetSceneName = nameNoExt;
        }
        #endif

        // 1) 이 번호 → 타겟 씬 이름 등록
        if (!string.IsNullOrEmpty(targetSceneName))
            CurrentGameStatus.RegisterScene(sceneNumber, targetSceneName);
        else
        {
            // 타겟 이름 미지정 시, 현재 씬 이름을 기본으로 등록
            CurrentGameStatus.RegisterScene(sceneNumber, gameObject.scene.name);
        }

        // 2) 번호 반영
        if (setNumberOnAwake)
            CurrentGameStatus.SetCurrentNumber(sceneNumber, save: false);
    }

    private void Start()
    {
        if (saveOnStart)
            CurrentGameStatus.SetCurrentNumber(sceneNumber, save: true);
    }

    // --- 버튼에서 바로 쓰기 좋은 유틸 ---

    /// <summary>이 번호로 저장</summary>
    public void SaveHere()
    {
        CurrentGameStatus.SetCurrentNumber(sceneNumber, save: true);
        Debug.Log($"[SceneAnchor] Saved CurrentNumber = {sceneNumber}");
    }

    /// <summary>마지막 저장 지점으로 이동</summary>
    public void ContinueFromLastSave()
    {
        CurrentGameStatus.ContinueGame();
    }

    /// <summary>New Game: 지정 번호로 덮어쓰기 + 이동</summary>
    public void NewGameFrom(int startNumber)
    {
        CurrentGameStatus.NewGame(startNumber);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneAnchor))]
public class SceneAnchorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var anchor = (SceneAnchor)target;

        // 유효성 안내
        if (string.IsNullOrEmpty(anchor.targetSceneName))
        {
            EditorGUILayout.HelpBox(
                "Target Scene Name이 비어있습니다. 비워두면 '현재 씬 이름'으로 등록됩니다.\n" +
                "Continue/NewGame가 해당 번호를 로드할 때 현재 씬으로 이동하게 됩니다.",
                MessageType.Info);
        }
        else
        {
            // 빌드 세팅에 있는지 간단히 안내
            bool inBuild = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == anchor.targetSceneName) { inBuild = true; break; }
            }
            if (!inBuild)
                EditorGUILayout.HelpBox(
                    $"'{anchor.targetSceneName}' 이(가) Build Settings에 등록되어 있지 않을 수 있습니다.",
                    MessageType.Warning);
        }
    }
}
#endif
