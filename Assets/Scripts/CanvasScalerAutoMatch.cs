using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerAutoMatch : MonoBehaviour
{
    [Header("UI 콘텐츠 루트(필수) - 여기에 모든 UI를 넣으세요")]
    public RectTransform contentRoot;

    [Tooltip("항상 Height 기준(=1)으로만 스케일")]
    public bool alwaysHeightScale = true;

    [Tooltip("빈 값이면 2560x1440 고정")]
    public Vector2 overrideReferenceResolution = Vector2.zero;

    [Tooltip("런타임 중 해상도/창크기 변경에 실시간 대응")]
    public bool realtime = true;

    [Range(0f, 1f), Tooltip("화면비와 기준비가 거의 같은 경우 사용되는 매치 값")]
    public float matchWhenEqual = 0.5f;

    [Tooltip("콘텐츠 루트를 기준 종횡비로 고정(와이드면 좌우 필러박스)")]
    public bool clampToReferenceAspect = true;

    [Tooltip("Overlay일 때 남는 영역을 덮을 검은 배경(없으면 자동 생성)")]
    public Image backgroundImage;

    CanvasScaler scaler;
    Canvas canvas;

    float lastParentW = -1f, lastParentH = -1f;
    static readonly Vector2 FallbackResolution = new Vector2(2560, 1440);

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        canvas = GetComponent<Canvas>();
        Apply(true);
    }

    void Update()
    {
        if (!realtime) return;

        var parent = contentRoot ? contentRoot.parent as RectTransform : null;
        float w = parent ? parent.rect.width : Screen.width;
        float h = parent ? parent.rect.height : Screen.height;

        // 부모 Rect 크기 변화에만 반응
        if (Mathf.Abs(w - lastParentW) > 0.5f || Mathf.Abs(h - lastParentH) > 0.5f)
            Apply();
    }

    void Apply(bool first = false)
    {
        // 1) 기준 해상도 결정
        var refRes = (overrideReferenceResolution.sqrMagnitude > 0.01f)
            ? overrideReferenceResolution
            : FallbackResolution;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = refRes;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // 화면비는 Screen이 아니라 부모 Rect 기준으로 계산(더 정확)
        var parent = contentRoot ? contentRoot.parent as RectTransform : null;
        float parentW = parent ? parent.rect.width : Screen.width;
        float parentH = parent ? parent.rect.height : Screen.height;

        float screenRatio  = (parentH > 0f) ? parentW / parentH : 1f;
        float targetAspect = (refRes.y > 0f) ? refRes.x / refRes.y : (16f / 9f);

        lastParentW = parentW; lastParentH = parentH;

        // 2) Width/Height 매치 선택
        if (alwaysHeightScale)
        {
            scaler.matchWidthOrHeight = 1f; // 무조건 Height 기준
        }
        else
        {
            float eps = 0.0005f;
            if (Mathf.Abs(screenRatio - targetAspect) <= eps)
                scaler.matchWidthOrHeight = matchWhenEqual;
            else
                scaler.matchWidthOrHeight = (screenRatio > targetAspect) ? 1f : 0f; // 넓으면 Height, 좁으면 Width
        }

        // 3) 검은 배경 준비(Overlay만)
        EnsureBackground();

        // 4) 콘텐츠 루트를 기준 종횡비로 클램프 (레터/필러박스)
        if (!clampToReferenceAspect || contentRoot == null || parent == null) return;

        // 중앙 고정(늘어남 방지)
        contentRoot.anchorMin = contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        contentRoot.pivot = new Vector2(0.5f, 0.5f);

        if (screenRatio > targetAspect)
        {
            // 더 넓음 → 좌우 필러박스, 높이에 맞춤
            float targetW = parentH * targetAspect;
            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetW);
            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentH);
        }
        else
        {
            // 더 좁음 → 상하 레터박스, 너비에 맞춤
            float targetH = (targetAspect > 0f) ? parentW / targetAspect : parentH;
            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentW);
            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
        }

        contentRoot.anchoredPosition = Vector2.zero;
    }

    void EnsureBackground()
    {
        // Screen Space - Overlay 인 경우에만 패널 생성/유지
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            if (backgroundImage == null)
            {
                var parent = (contentRoot != null) ? contentRoot.parent : transform as RectTransform;

                var bgGO = new GameObject("BlackBarsBackground",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var rt = (RectTransform)bgGO.transform;
                rt.SetParent(parent, false);

                backgroundImage = bgGO.GetComponent<Image>();
                backgroundImage.raycastTarget = false;
                backgroundImage.color = Color.black;

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                bgGO.transform.SetSiblingIndex(0); // 가장 뒤
            }
            else
            {
                var rt = backgroundImage.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                backgroundImage.color = Color.black;
                backgroundImage.transform.SetSiblingIndex(0);
            }
        }
        // Camera/World Space 캔버스라면 카메라 배경색을 검정으로.
    }
}
