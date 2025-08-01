using UnityEngine;
using UnityEngine.EventSystems;

public class CardGame2Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

        // 최상위로 옮기기
        transform.SetParent(canvas.transform);

        // 슬롯 분리
        DetachFromSlot();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 슬롯들과 충돌 검사
        CardGame2PanelManager[] slots = GameObject.FindObjectsOfType<CardGame2PanelManager>();
        foreach (var slot in slots)
        {
            if (IsOverlappingEnough(slot.GetComponent<RectTransform>()))
            {
                if (slot.TryPlaceCard(this))
                {
                    return; // 슬롯에 배치됨
                }
            }
        }

        // ⛔ 슬롯이랑 안 겹치면 → 현재 위치 그대로 두기 (그대로 UI에 남기기)
        // 아무 것도 안 함!
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
