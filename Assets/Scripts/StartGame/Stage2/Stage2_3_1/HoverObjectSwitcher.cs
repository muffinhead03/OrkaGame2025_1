using UnityEngine;
using UnityEngine.EventSystems;

public class HoverObjectSwitcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject defaultImage;
    public GameObject hoverImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        defaultImage.SetActive(false);
        hoverImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        defaultImage.SetActive(true);
        hoverImage.SetActive(false);
    }
}
