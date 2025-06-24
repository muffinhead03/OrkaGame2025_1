using UnityEngine;
using UnityEngine.EventSystems;

public class CardGame2Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private CardGame2PanelManager currentSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalParent = rectTransform.parent;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (rectTransform.parent == originalParent)
        {
            rectTransform.position = originalPosition;
        }
    }

    public void SetCurrentSlot(CardGame2PanelManager slot)
    {
        currentSlot = slot;
    }

    public void DetachFromSlot()
    {
        if (currentSlot != null)
        {
            currentSlot.RemoveCard();
            currentSlot = null;
        }
    }
}