using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    // Start 버튼 클릭 시 호출되는 함수
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("SlidingPuzzle");
    }

    // Quit 버튼 클릭 시 호출되는 함수
    public void OnQuitButtonClicked()
    {
        // 에디터에서는 UnityEditor를 통해 플레이 모드 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}

