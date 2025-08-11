using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CardClickTarget : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("씬에 있는 SecondCardClickManager")]
    public SecondCardClickManager manager;

    [Range(0, 9)]
    public int slotIndexZeroBased = 0;

    [Header("Drag Settings")]
    [Tooltip("EventSystem.pixelDragThreshold 사용")]
    public bool useEventSystemThreshold = true;

    [Tooltip("포인터 이동이 이 픽셀보다 크면 드래그로 간주")]
    public float dragThreshold = 10f;

    [Tooltip("드래그여도 클릭으로 처리")]
    public bool clickEvenIfDragged = true;

    [Tooltip("드래그가 끝날 때(손 떼는 순간)도 클릭 발사")]
    public bool fireOnEndDrag = true;

    [Tooltip("업 시점에 커서가 자기/자식 위에 있어야만 클릭 인정")]
    public bool requireReleaseOverSelf = false;

    [Header("Debug (Console only)")]
    public bool debugLog = false;

    private Vector2 downPos;
    private bool dragged;

    

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragged && (eventData.position - downPos).sqrMagnitude > dragThreshold * dragThreshold)
        {
            dragged = true;
            if (debugLog) Debug.Log($"[CardClickTarget] Drag start slot={slotIndexZeroBased}");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (debugLog) Debug.Log($"[CardClickTarget] EndDrag slot={slotIndexZeroBased}, dragged={dragged}");
        if (dragged && fireOnEndDrag && clickEvenIfDragged)
            FireClick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (debugLog) Debug.Log($"[CardClickTarget] Up slot={slotIndexZeroBased}, dragged={dragged}");
        if (dragged && !clickEvenIfDragged) return;
        FireClick(eventData);
    }

    private void FireClick(PointerEventData eventData)
    {
        if (!manager)
        {
            Debug.LogWarning("[CardClickTarget] manager가 비었습니다.");
            return;
        }

        if (requireReleaseOverSelf)
        {
            var go = eventData.pointerCurrentRaycast.gameObject;
            if (go == null || (go != gameObject && !go.transform.IsChildOf(transform)))
            {
                if (debugLog) Debug.Log("[CardClickTarget] release not over self → ignore");
                return;
            }
        }

        manager.OnCardClickedBySlot(Mathf.Clamp(slotIndexZeroBased, 0, 9));
        if (debugLog) Debug.Log($"[CardClickTarget] CLICK fired → slot={slotIndexZeroBased}");
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        downPos = eventData.position;
        dragged = false;
        if (useEventSystemThreshold && EventSystem.current != null)
            dragThreshold = EventSystem.current.pixelDragThreshold;

        // ★ 다운 순간 자막 표시
        if (manager) manager.OnCardPointerDown(slotIndexZeroBased);
    }

}
