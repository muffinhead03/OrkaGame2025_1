using UnityEngine;

public class CardGame1PanelManager : MonoBehaviour
{
    private RectTransform slotTransform;
    private CardGame1Manager currentCard;

    private void Awake()
    {
        slotTransform = GetComponent<RectTransform>();
    }

    public bool TryPlaceCard(CardGame1Manager card)
    {
        if (currentCard == null || currentCard == card) // 수정: 같은 카드면 재배치 허용
        {
            PlaceCard(card);
            return true;
        }

        return false;
    }

    private void PlaceCard(CardGame1Manager card)
    {
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(transform);
        cardRect.localPosition = Vector3.zero;

        if (currentCard != null && currentCard != card)
        {
            currentCard.DetachFromSlot(); // 카드가 교체될 경우 원래 카드에 알림
        }

        currentCard = card;
        card.SetCurrentSlot(this); // 카드에게 현재 슬롯 전달
    }

    public void RemoveCard()
    {
        currentCard = null;
    }

    public CardGame1Manager GetCurrentCard()
    {
        return currentCard;
    }
}
