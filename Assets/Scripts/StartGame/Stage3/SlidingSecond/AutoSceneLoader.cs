// 파일명: AutoSceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoSceneLoader : MonoBehaviour
{
    [SerializeField] float delaySeconds = 4f;
    [SerializeField] string sceneName = "SlidingGameSecond";

    bool loading = false;

    void OnEnable()
    {
        StartCoroutine(LoadAfterDelay());
    }

    IEnumerator LoadAfterDelay()
    {
        if (loading) yield break;
        loading = true;

        yield return new WaitForSecondsRealtime(delaySeconds);
        SceneManager.LoadScene(sceneName);
    }
}