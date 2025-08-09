using UnityEngine;
using UnityEngine.EventSystems;

public class FirstCardClickHandler : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("0~5: 카드 인덱스")]
    public int cardIndex;

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