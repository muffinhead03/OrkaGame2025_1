using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject continueRoot; // 버튼 오브젝트 자체(옵션)

    [Header("New Game")]
    [SerializeField] private int newGameStartNumber = 1;

    private void OnEnable()
    {
        CurrentGameStatus.OnSavePresenceChanged += HandleSavePresenceChanged;

        // 강제 초기화(안전빵)
        CurrentGameStatus.Initialize();

        RefreshContinueButton("OnEnable");
    }

    private void OnDisable()
    {
        CurrentGameStatus.OnSavePresenceChanged -= HandleSavePresenceChanged;
    }

    private void HandleSavePresenceChanged(bool hasSave)
    {
        ApplyContinueState(hasSave, "OnSavePresenceChanged");
    }

    private void RefreshContinueButton(string who)
    {
        bool hasSave = CurrentGameStatus.HasSave;
        int saved    = PlayerPrefs.GetInt("current_scene_number", 0);
        Debug.Log($"[MainMenuButtons/{who}] HasSave={hasSave}, savedNumber={saved}");
        ApplyContinueState(hasSave, who);
    }

    private void ApplyContinueState(bool hasSave, string who)
    {
        if (continueButton != null)
            continueButton.interactable = hasSave;

        // 버튼을 숨기는 식이라면 이 줄도 사용
        if (continueRoot != null)
            continueRoot.SetActive(hasSave);

        Debug.Log($"[MainMenuButtons/{who}] Continue {(hasSave ? "ENABLED" : "DISABLED")}");
    }

    // --- UI 이벤트 ---

    public void OnNewGameClicked()
    {
        Debug.Log("[MainMenuButtons] NewGame clicked");
        CurrentGameStatus.NewGame(newGameStartNumber);
    }

    public void OnContinueClicked()
    {
        Debug.Log("[MainMenuButtons] Continue clicked");
        if (CurrentGameStatus.HasSave)
            CurrentGameStatus.ContinueGame();
    }

    public void OnClearSaveClicked()
    {
        Debug.Log("[MainMenuButtons] ClearSave clicked");
        CurrentGameStatus.ClearSave();
        RefreshContinueButton("ClearSave");
    }
}
