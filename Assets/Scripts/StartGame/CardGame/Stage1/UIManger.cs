using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject settingPopupPanel;
    private GameManager gameManager;
    private AudioSource[] pausedAudioSources;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // ▶️ [Resume] 버튼용
    public void OnResumeButtonClicked()
    {
        settingPopupPanel.SetActive(false);

        // 타이머 재개
        if (gameManager != null)
        {
            gameManager.isTimerPaused = false;
        }

        // 음악 재개
        if (pausedAudioSources != null)
        {
            foreach (var audio in pausedAudioSources)
            {
                if (audio != null) audio.UnPause();
            }
        }

        Debug.Log("▶️ 게임 재개됨 (타이머 + 오디오)");
    }

    // 🛑 [Setting 버튼]에서 호출됨 → 게임 일시정지
    public void OnSettingButtonClicked()
    {
        settingPopupPanel.SetActive(true);

        if (gameManager != null)
        {
            gameManager.isTimerPaused = true;
        }

        // 현재 재생 중인 모든 오디오 일시정지
        AudioSource[] all = FindObjectsOfType<AudioSource>();
        pausedAudioSources = System.Array.FindAll(all, a => a.isPlaying);

        foreach (var audio in pausedAudioSources)
        {
            audio.Pause();
        }

        Debug.Log("⏸️ 게임 일시정지됨 (타이머 + 오디오)");
    }

    // 🚪 [Exit] 버튼용 → 메인 화면으로 이동
    public void OnExitButtonClicked()
    {
        Debug.Log("🚪 Exit → 메인 화면(C)으로 이동");
        SceneManager.LoadScene("mainMenuPanel");
    }
}