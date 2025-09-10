using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoLoadCardGameFirstStage : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 4f;
    [SerializeField] private string sceneName = "CardGameFirstStage";

    private void Start()
    {
        // StartCoroutine으로 지연 후 씬 로드
        StartCoroutine(LoadAfterDelay());
    }

    private System.Collections.IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(sceneName);
    }
}