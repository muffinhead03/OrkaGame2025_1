using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // UI 클릭용

/// 죽고 나서 전용: 아이템을 클릭하면 즉시 클리어 → 성공 씬으로 이동
public class SlidingGameManager2AfterDeath : MonoBehaviour
{
    [Header("씬 이동")]
    [SerializeField] private string successSceneName = "Stage3_2";
    [SerializeField] private float loadDelay = 0.1f; // 살짝 딜레이 후 로드(연출 안정)

    [Header("우측 아이콘 (옵션)")]
    public CanvasGroup keyIcon;      // 열쇠
    public CanvasGroup lock1Icon;    // 1차 자물쇠
    public CanvasGroup lock2Icon;    // 2차 자물쇠
    [Range(0f,1f)] public float clearedAlpha = 0.3f;

    [Header("사운드/연출(옵션)")]
    public AudioSource sfxOnClear;   // 클릭 시 효과음 등

    [Header("클릭해서 넘어가는 오브젝트")]
    [Tooltip("이 오브젝트를 클릭하면 성공 씬(Stage3_2)으로 넘어갑니다.")]
    public GameObject clickToClearObject;

    [Tooltip("클릭 오브젝트가 비어있으면, 자동으로 풀스크린 투명 버튼을 생성합니다.")]
    public bool autoCreateFullscreenButtonIfNone = true;

    private bool sceneLoading = false;

    private void Awake()
    {
        // 아이콘 초기 알파
        SetAlpha(keyIcon,   1f);
        SetAlpha(lock1Icon, 1f);
        SetAlpha(lock2Icon, 1f);

        // 클릭 트리거 세팅
        if (clickToClearObject != null)
        {
            AttachClickHelper(clickToClearObject);
        }
        else if (autoCreateFullscreenButtonIfNone)
        {
            CreateFullscreenClickButton(); // 화면 어디나 클릭하면 넘어가게
        }
    }

    // 버튼이나 클릭 타겟에서 이 메서드를 호출하면 전부 클리어 처리
    public void OnClearClick()
    {
        if (sceneLoading) return;
        sceneLoading = true;

        // 아이콘을 "클리어(반투명)" 상태로
        SetAlpha(keyIcon,   clearedAlpha);
        SetAlpha(lock1Icon, clearedAlpha);
        SetAlpha(lock2Icon, clearedAlpha);

        if (sfxOnClear) sfxOnClear.Play();

        // 성공 씬으로
        if (loadDelay > 0f)
            StartCoroutine(LoadAfterDelay());
        else
            SceneManager.LoadScene(successSceneName);
    }

    private System.Collections.IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(successSceneName);
    }

    // ---------- 클릭 트리거 부착/생성 ----------

    private void AttachClickHelper(GameObject go)
    {
        if (!go) return;

        // UI 오브젝트면 UI 헬퍼 부착
        bool isUI = go.GetComponent<Graphic>() != null || go.GetComponentInParent<Canvas>() != null;
        if (isUI)
        {
            var ui = go.GetComponent<UIElementClickToClear>();
            if (ui == null) ui = go.AddComponent<UIElementClickToClear>();
            ui.manager = this;
        }
        else
        {
            // 월드 오브젝트면 콜라이더 클릭 헬퍼 부착 (콜라이더 없으면 BoxCollider 추가)
            if (!go.GetComponent<Collider>() && !go.GetComponent<Collider2D>())
            {
                go.AddComponent<BoxCollider>(); // 3D 기본
            }
            var w = go.GetComponent<ColliderClickToClear>();
            if (w == null) w = go.AddComponent<ColliderClickToClear>();
            w.manager = this;
        }
    }

    private void CreateFullscreenClickButton()
    {
        // Canvas 준비
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("AutoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // EventSystem 준비
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // 버튼 생성 (풀스크린, 투명)
        var btnGO = new GameObject("ClickToClearButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIElementClickToClear));
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0); // 완전 투명

        var helper = btnGO.GetComponent<UIElementClickToClear>();
        helper.manager = this;
    }

    // CanvasGroup + 하위 UI/Sprite 모두 알파 반영(안전)
    private void SetAlpha(CanvasGroup g, float a)
    {
        if (!g) return;
        a = Mathf.Clamp01(a);
        g.alpha = a;

        var graphics = g.GetComponentsInChildren<Graphic>(true);
        foreach (var gr in graphics)
        {
            var c = gr.color; c.a = a; gr.color = c;
        }
        var srs = g.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            var c = sr.color; c.a = a; sr.color = c;
        }
    }
}

/* -------------------- 클릭 타겟용 헬퍼 2종 -------------------- */
// UI 오브젝트(버튼/이미지 등)를 직접 클릭해서 클리어
public class UIElementClickToClear : MonoBehaviour, IPointerClickHandler
{
    public SlidingGameManager2AfterDeath manager;
    public void OnPointerClick(PointerEventData eventData) => manager?.OnClearClick();
}

// 2D/3D 월드 오브젝트를 클릭해서 클리어
public class ColliderClickToClear : MonoBehaviour
{
    public SlidingGameManager2AfterDeath manager;
    private void OnMouseDown() => manager?.OnClearClick();
}
