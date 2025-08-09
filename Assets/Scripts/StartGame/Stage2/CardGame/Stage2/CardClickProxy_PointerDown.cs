using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickProxy_PointerDown : MonoBehaviour, IPointerDownHandler
{
    public SecondCardClickManager manager;
    [Range(0,9)] public int slotIndex;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager != null) manager.OnCardClickedBySlot(slotIndex);
    }
}