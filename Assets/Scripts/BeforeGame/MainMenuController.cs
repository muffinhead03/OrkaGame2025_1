using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // 게임 시작 → ContinueGame 씬 로드
    public void StartGame()
    {
        SceneManager.LoadScene("ContinueGame");
    }

    // 설정 화면 → SettingScene 씬 로드
    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingScene");
    }

    // 게임 종료 버튼도 필요하면 아래처럼 추가 가능!
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("게임 종료됨.");
     
    }
}