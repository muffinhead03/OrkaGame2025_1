using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject languagePanel;   // ⬅ 인스펙터에 할당
    [SerializeField] private bool hidePanelOnStart = true;

    private void Start()
    {
        // 초기에는 비활성화
        if (hidePanelOnStart && languagePanel != null)
            languagePanel.SetActive(false);
    }

    // Start 버튼
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("SlidingPuzzle");
    }

    // Quit 버튼
    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Language 버튼
    public void OnLanguageButtonClicked()
    {
        if (languagePanel == null)
        {
            Debug.LogError("[MainScreen] languagePanel이 할당되지 않았습니다.");
            return;
        }
        languagePanel.SetActive(true);   // 필요하면 Toggle로 바꿔도 됨
        // languagePanel.SetActive(!languagePanel.activeSelf);
    }

    // 닫기 버튼(패널 안에 Close 버튼 연결용)
    public void OnCloseLanguagePanel()
    {
        if (languagePanel != null)
            languagePanel.SetActive(false);
    }
}