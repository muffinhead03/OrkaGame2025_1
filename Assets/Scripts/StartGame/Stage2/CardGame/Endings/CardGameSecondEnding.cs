using UnityEngine;
using UnityEngine.SceneManagement;

public class CardGameSecondEnding : MonoBehaviour
{
    void Start()
    {
        // 5초 후에 LoadNextScene 메서드 실행
        Invoke("LoadNextScene", 5f);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("CardgameSecondStage");
    }
}