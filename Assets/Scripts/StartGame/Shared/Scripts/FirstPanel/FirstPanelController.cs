using UnityEngine;
using UnityEngine.UI; // CanvasGroup

public class FirstPanelController : MonoBehaviour
{
    [Header("패널들 (초기엔 둘 다 비활성화 권장)")]
    public GameObject firstPanel;
    public GameObject settingPanel;

    [Header("커버 패널 (뒤 클릭 방지용, 시작 시 비활성 권장)")]
    [SerializeField] private GameObject coverPanel;   // ⬅ 전체 화면 상단에 위치한 투명/반투명 패널

    [Header("옵션")]
    public bool autoDisableOnAwake = true;   // 실행 시작 시 둘 다 꺼두기
    public bool bringToFront       = true;   // 열 때 맨 앞으로

    [Header("중앙 위치 (UI가 아닌 일반 Transform용)")]
    public Vector3 center = Vector3.zero;

    // ===== Exit 시 새 패널 사용/생성 옵션 =====
    [Header("Exit Panel (씬에 배치된 패널을 우선 사용)")]
    [SerializeField] private GameObject exitPanel;          // ✅ 씬에 배치된 Exit 패널(초기 비활성화)
    [Header("Exit → Spawn New Panel (exitPanel이 비어있을 때만 사용)")]
    [SerializeField] private GameObject exitPanelPrefab;    // ⬅ 프리팹 (선택)
    [SerializeField] private Transform exitPanelParent;     // 비워두면 가장 가까운 Canvas 사용
    [SerializeField] private bool spawnOnce = true;         // 한 번만 생성하고 재사용
    [SerializeField] private bool stretchExitPanelToFull = true; // 부모 캔버스에 풀스크린으로 맞춤
    [SerializeField] private bool hideOthersWhenExitPanel = true; // Exit 패널 열 때 기존 패널 숨김
    [SerializeField] private bool destroyOnExitClose = false;     // 닫을 때 인스턴스 파괴

    private GameObject exitPanelInstance;

    void Awake()
    {
        if (autoDisableOnAwake)
        {
            if (firstPanel)    firstPanel.SetActive(false);
            if (settingPanel)  settingPanel.SetActive(false);
            if (coverPanel)    coverPanel.SetActive(false); // ★ 커버는 기본 비활성
            if (exitPanel)     exitPanel.SetActive(false);  // ★ 씬에 배치된 Exit 패널 비활성 보장
        }
    }

    // ===== 공용: 패널 열기/닫기 =====
    void OpenPanel(GameObject panel)
    {
        if (!panel) { Debug.LogError("[FirstPanelController] OpenPanel: panel이 비어있어요."); return; }

        if (HasInactiveParent(panel))
            Debug.LogWarning($"[FirstPanelController] '{panel.name}' 상위 중 비활성 오브젝트가 있습니다.");

        var rt = panel.transform as RectTransform;
        if (rt != null) rt.anchoredPosition = Vector2.zero;
        else            panel.transform.localPosition = center;

        var cg = panel.GetComponent<CanvasGroup>();
        if (cg) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }

        panel.SetActive(true);
        if (bringToFront) panel.transform.SetAsLastSibling();

        Debug.Log($"[FirstPanelController] OpenPanel -> {panel.name} (activeInHierarchy={panel.activeInHierarchy})");
    }

    void ClosePanel(GameObject panel)
    {
        if (!panel) return;
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg) { cg.interactable = false; cg.blocksRaycasts = false; }
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

    // ===== 커버 패널 on/off =====
    void ShowCover(bool show)
    {
        if (!coverPanel) return;
        var cg = coverPanel.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.blocksRaycasts = show; // 뒤 클릭 차단
            cg.interactable   = false;
        }
        coverPanel.SetActive(show);
        if (show && bringToFront) coverPanel.transform.SetAsLastSibling();
    }

    // ===== 버튼 연결용 메서드 =====
    public void OnFirstOpen()
    {
        ShowCover(true);
        OpenPanel(firstPanel);
        ClosePanel(settingPanel);
    }

    public void OnOpenSetting()
    {
        ClosePanel(firstPanel);
        OpenPanel(settingPanel);
    }

    public void OnClose()
    {
        ClosePanel(firstPanel);
        ClosePanel(settingPanel);
        ShowCover(false);
    }

    public void OnCloseSetting()
    {
        ClosePanel(settingPanel);
        OpenPanel(firstPanel);
    }

    // ✅ TurnOff 버튼: 현재(첫) 패널 끄고 커버 해제
    public void OnTurnOffFirstPanel()
    {
        ClosePanel(firstPanel);
        ShowCover(false);
    }

    // ✅ Exit 버튼: 씬에 있는 패널이 있으면 그것만 활성화, 없으면 프리팹 생성
    public void OnExitOpenNewPanel()
    {
        // 1) 씬에 배치된 패널을 우선 사용
        if (exitPanel != null)
        {
            if (hideOthersWhenExitPanel)
            {
                ClosePanel(firstPanel);
                ClosePanel(settingPanel);
                ShowCover(true); // 필요 시 뒤 클릭 차단 유지
            }
            OpenPanel(exitPanel);
            return;
        }

        // 2) 없으면 프리팹 생성/활성화
        var inst = EnsureExitPanelInstance();
        if (!inst) return;

        if (hideOthersWhenExitPanel)
        {
            ClosePanel(firstPanel);
            ClosePanel(settingPanel);
            ShowCover(true);
        }
        OpenPanel(inst);
    }

    // (선택) Exit 패널 닫기 버튼
    public void OnExitPanelClose()
    {
        if (exitPanel) { ClosePanel(exitPanel); ShowCover(false); return; }

        if (!exitPanelInstance) { ShowCover(false); return; }

        if (destroyOnExitClose)
        {
            Destroy(exitPanelInstance);
            exitPanelInstance = null;
        }
        else
        {
            ClosePanel(exitPanelInstance);
        }
        ShowCover(false);
    }

    // ===== 내부 유틸 =====
    GameObject EnsureExitPanelInstance()
    {
        if (spawnOnce && exitPanelInstance)
        {
            if (bringToFront) exitPanelInstance.transform.SetAsLastSibling();
            return exitPanelInstance;
        }

        if (!exitPanelPrefab)
        {
            Debug.LogError("[FirstPanelController] exitPanelPrefab이 비었습니다. 프리팹을 연결하거나, exitPanel(씬 오브젝트)을 지정하세요.");
            return null;
        }

        var parent = exitPanelParent ? exitPanelParent : GetNearestCanvasTransform();
        exitPanelInstance = Instantiate(exitPanelPrefab, parent, worldPositionStays: false);
        exitPanelInstance.name = exitPanelPrefab.name + "(Inst)";

        var rt = exitPanelInstance.GetComponent<RectTransform>();
        if (rt != null)
        {
            if (stretchExitPanelToFull)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            else
            {
                rt.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            exitPanelInstance.transform.localPosition = center;
        }

        exitPanelInstance.SetActive(false);
        if (bringToFront) exitPanelInstance.transform.SetAsLastSibling();

        return exitPanelInstance;
    }

    Transform GetNearestCanvasTransform()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas) return canvas.transform;
        return transform.root ? transform.root : transform;
    }
}
