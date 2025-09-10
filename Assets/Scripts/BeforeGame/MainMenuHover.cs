using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;

    [Header("Sizing / Position (optional)")]
    public bool useNativeSizeOnHover = true;       // 호버 시 스프라이트 원본 크기 적용
    public bool useNativeSizeOnNormal = true;      // 복귀 시 normal 원본 크기 적용

    public bool overrideHoverSize = false;         // true면 sizeDelta 강제 지정
    public Vector2 hoverSize;                      // 예: (300, 120)

    public bool overrideHoverPosition = false;     // true면 anchoredPosition 강제 지정
    public Vector2 hoverAnchoredPos;               // 예: (0, -100)

    public bool overrideHoverScale = false;        // 스케일로 연출하고 싶을 때
    public Vector3 hoverScale = Vector3.one * 1.1f;

    private Image img;
    private RectTransform rt;

    // 원래 값 저장
    private Vector2 originalSize;
    private Vector2 originalAnchoredPos;
    private Vector3 originalScale;

    void Awake()
    {
        img = GetComponent<Image>();
        rt  = GetComponent<RectTransform>();

        originalSize        = rt.sizeDelta;
        originalAnchoredPos = rt.anchoredPosition;
        originalScale       = rt.localScale;

        if (img && normalSprite) img.sprite = normalSprite;
        if (useNativeSizeOnNormal && img && img.sprite) img.SetNativeSize();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (img && hoverSprite) img.sprite = hoverSprite;

        if (useNativeSizeOnHover && img && img.sprite) img.SetNativeSize();
        if (overrideHoverSize)      rt.sizeDelta       = hoverSize;
        if (overrideHoverPosition)  rt.anchoredPosition= hoverAnchoredPos;
        if (overrideHoverScale)     rt.localScale      = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (img && normalSprite) img.sprite = normalSprite;

        if (useNativeSizeOnNormal && img && img.sprite) img.SetNativeSize();
        else rt.sizeDelta = originalSize;

        rt.anchoredPosition = originalAnchoredPos;
        rt.localScale       = originalScale;
    }
}
