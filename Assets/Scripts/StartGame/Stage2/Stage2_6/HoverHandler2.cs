using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject targetToShow;
    public GameObject targetToHide;

    private bool isHovered = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovered) return;
        isHovered = true;

        if (targetToShow != null) targetToShow.SetActive(true);
        if (targetToHide != null) targetToHide.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (targetToShow != null) targetToShow.SetActive(false);
        if (targetToHide != null) targetToHide.SetActive(true);
    }
}
