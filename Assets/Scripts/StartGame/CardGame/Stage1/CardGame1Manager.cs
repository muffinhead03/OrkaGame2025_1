using UnityEngine;
using UnityEngine.EventSystems;

public class CardGame1Manager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalPosition;
    private Transform originalParent;
    private CardGame1PanelManager currentSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("❌ Canvas를 찾을 수 없습니다. 카드 오브젝트가 Canvas 아래에 있어야 합니다!");
        }
    }

    public void SetCurrentSlot(CardGame1PanelManager slot)
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 시작");

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        canvasGroup.blocksRaycasts = false;

        // 슬롯에서 제거
        DetachFromSlot();

        // 최상위 Canvas로 이동 (드래그가 UI 위에 보이게)
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 끝");
        canvasGroup.blocksRaycasts = true;

        // 슬롯들과 겹치는지 검사
        CardGame1PanelManager[] slots = FindObjectsOfType<CardGame1PanelManager>();
        foreach (var slot in slots)
        {
            if (IsOverlappingEnough(slot.GetComponent<RectTransform>()))
            {
                if (slot.TryPlaceCard(this))
                {
                    Debug.Log("✅ 카드가 슬롯에 배치됨");
                    return;
                }
                else
                {
                    Debug.Log("❌ 슬롯에 이미 카드가 있음");
                }
            }
        }

        // ❗ 슬롯과 겹치지 않으면, 현재 위치에 남기되, 부모는 원래대로 되돌리기
        transform.SetParent(originalParent);
    }


    // 슬롯과 얼마나 겹치는지 확인하는 함수
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
}
