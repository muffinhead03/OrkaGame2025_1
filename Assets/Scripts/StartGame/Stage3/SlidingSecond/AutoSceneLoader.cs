// 파일명: AutoSceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

[DefaultExecutionOrder(-1000)]
public class AutoSceneLoader : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 4f;
    [SerializeField] private string sceneName = "SlidingGameSecondIfDead";

    private static bool _alreadyQueued;   // 중복 로드 방지(씬에 같은 스크립트 여러 개 있을 때)
    private Coroutine _routine;

    private void Start()
    {
        if (_alreadyQueued) return;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[AutoSceneLoader] sceneName이 비어있습니다.");
            return;
        }
        _alreadyQueued = true;
        _routine = StartCoroutine(LoadAfterDelay());
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
    }

    private IEnumerator LoadAfterDelay()
    {
        // Time.timeScale과 무관하게 4초 대기
        float end = Time.unscaledTime + Mathf.Max(0f, delaySeconds);
        while (Time.unscaledTime < end) yield return null;

        // 빌드 세팅에 있는지 확인 (이름 기준)
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[AutoSceneLoader] 빌드 세팅에 '{sceneName}' 씬이 없습니다. " +
                           "File > Build Settings… 에서 Scenes In Build에 추가하세요.");
            yield break;
        }

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"[AutoSceneLoader] SceneManager.LoadSceneAsync 실패: {sceneName}");
            yield break;
        }
        op.allowSceneActivation = true;
        // 선택: 로딩 진행 로그
        while (!op.isDone) yield return null;
    }

    private bool IsSceneInBuildSettings(string targetName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, targetName, System.StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    // 필요하면 즉시 호출용(예: 디버그 버튼)
    public void TriggerNow() => StartCoroutine(LoadAfterDelay());
}
