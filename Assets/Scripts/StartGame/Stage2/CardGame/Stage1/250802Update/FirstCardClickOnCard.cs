using UnityEngine;
using UnityEngine.EventSystems;

public class FirstCardClickOnCard : MonoBehaviour, IPointerDownHandler
{
    public int cardIndex; // 0~5
    [SerializeField] private FirstCardClickManager clickManager;

    void Awake()
    {
        if (clickManager == null)
            clickManager = FindObjectOfType<FirstCardClickManager>(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickManager == null) return;
        clickManager.ShowByCardIndex(cardIndex);
    }
}