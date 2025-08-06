using UnityEngine;
using UnityEngine.EventSystems;

public class CardGame2Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Canvas canvas;
    private CardGame2PanelManager currentSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;

        // 드래그 시 최상위로 올림
        transform.SetParent(canvas.transform);

        // 슬롯에서 분리
        DetachFromSlot();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        CardGame2PanelManager[] slots = GameObject.FindObjectsOfType<CardGame2PanelManager>();
        foreach (var slot in slots)
        {
            if (IsOverlappingEnough(slot.GetComponent<RectTransform>()))
            {
                if (slot.TryPlaceCard(this))
                    return;
            }
        }

        // 슬롯과 충분히 겹치지 않으면: 아무 것도 안 함 (그 자리에 그대로)
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.2f; // 마우스 올라갔을 때 확대
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one; // 마우스 나갔을 때 원래 크기
    }

    private bool IsOverlappingEnough(RectTransform slot)
    {
        Rect cardRect = GetWorldRect(rectTransform);
        Rect slotRect = GetWorldRect(slot);

        Rect intersection = RectIntersection(cardRect, slotRect);
        float intersectionArea = intersection.width * intersection.height;
        float cardArea = cardRect.width * cardRect.height;

        return intersectionArea >= cardArea * 0.5f;
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }

    private Rect RectIntersection(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        if (xMax >= xMin && yMax >= yMin)
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        else
            return new Rect(0, 0, 0, 0);
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
