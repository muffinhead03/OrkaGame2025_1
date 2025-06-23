using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    private Animator animator;
    private bool isFading = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartFadeOut(string nextSceneName)
    {
        if (isFading) return;
        isFading = true;
        animator.SetTrigger("FadeOut");
        StartCoroutine(WaitAndLoadScene(nextSceneName));
    }

    private System.Collections.IEnumerator WaitAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(1f); // FadeOut 애니메이션 길이만큼 기다림
        SceneManager.LoadScene(sceneName);
    }
}
