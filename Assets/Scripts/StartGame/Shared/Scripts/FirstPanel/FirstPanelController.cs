using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // CanvasGroup 사용 시

public class FirstPanelController : MonoBehaviour
{
    [Header("패널들 (초기엔 둘 다 비활성화로 시작 권장)")]
    public GameObject firstPanel;
    public GameObject settingPanel;

    [Header("옵션")]
    public bool autoDisableOnAwake = true;   // 실행 시작 시 둘 다 꺼두기
    public bool bringToFront       = true;   // 열 때 맨 앞으로

    [Header("중앙 위치 (UI가 아닌 일반 Transform용)")]
    public Vector3 center = Vector3.zero;

    void Awake()
    {
        if (autoDisableOnAwake)
        {
            if (firstPanel)  firstPanel.SetActive(false);
            if (settingPanel) settingPanel.SetActive(false);
        }
    }

    // ===== 공용: 패널 열기/닫기 =====
    void OpenPanel(GameObject panel)
    {
        if (!panel) { Debug.LogError("[FirstPanelController] OpenPanel: panel이 비어있어요."); return; }

        // 부모가 꺼져있으면 자식만 켜도 안 보임 → 경고
        if (HasInactiveParent(panel))
            Debug.LogWarning($"[FirstPanelController] '{panel.name}'의 상위 오브젝트 중 비활성 상태가 있습니다. 상위도 켜주세요.");

        var rt = panel.transform as RectTransform;
        if (rt != null)
            rt.anchoredPosition = Vector2.zero; // UI는 anchoredPosition으로 중앙
        else
            panel.transform.localPosition = center; // 일반 Transform fallback

        // CanvasGroup이 있으면 보이도록
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        panel.SetActive(true);
        if (bringToFront) panel.transform.SetAsLastSibling();

        Debug.Log($"[FirstPanelController] OpenPanel -> {panel.name} (activeInHierarchy={panel.activeInHierarchy})");
    }

    void ClosePanel(GameObject panel)
    {
        if (!panel) return;
        panel.SetActive(false);
        Debug.Log($"[FirstPanelController] ClosePanel -> {panel.name}");
    }

    bool HasInactiveParent(GameObject go)
    {
        var t = go.transform.parent;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) return true;
            t = t.parent;
        }
        return false;
    }

    // ===== 버튼 연결용 메서드 =====
    public void OnFirstOpen()
    {
        OpenPanel(firstPanel);
        ClosePanel(settingPanel);
    }

    public void OnOpenSetting()
    {
        ClosePanel(firstPanel);
        OpenPanel(settingPanel); // ★ 활성/위치/앞쪽 배치까지 처리
    }

    public void OnClose()
    {
        ClosePanel(firstPanel);
        ClosePanel(settingPanel);
    }

    public void OnCloseSetting()
    {
        ClosePanel(settingPanel);
        OpenPanel(firstPanel);
    }

    public void OnExitToMainMenu()
    {
        SceneManager.LoadScene("mainMenuPanel");
    }
}
