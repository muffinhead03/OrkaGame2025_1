using UnityEngine;
using UnityEngine.EventSystems;

public class Stagecard3Manager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;

    public Stagecard3PanelManager panelManager; // 패널 매니저 연결

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 패널 매니저에게 카드가 놓인 위치를 전달
        bool snapped = panelManager.TrySnapCard(this, rectTransform);

        // 실패하면 원래 위치로
        if (!snapped)
            rectTransform.anchoredPosition = originalPosition;
    }
}
