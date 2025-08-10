using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoLoadSceneAfterDelay : MonoBehaviour
{
    [SerializeField] private string nextScene = "SlidingPuzzleIfDead";
    [SerializeField] private float delaySeconds = 4f;
    [SerializeField] private bool useRealtime = true; // 타임스케일 무시하고 기다릴지

    private IEnumerator Start()
    {
        if (useRealtime)
            yield return new WaitForSecondsRealtime(delaySeconds);
        else
            yield return new WaitForSeconds(delaySeconds);

        SceneManager.LoadScene(nextScene);
    }
}