using UnityEngine;

public class CardGame2PanelManager : MonoBehaviour
{
    private RectTransform slotTransform;
    private CardGame2Manager currentCard;

    private void Awake()
    {
        slotTransform = GetComponent<RectTransform>();
    }

    public bool TryPlaceCard(CardGame2Manager card)
    {
        if (currentCard == null || currentCard == card) // 같은 카드면 재배치 허용
        {
            PlaceCard(card);
            return true;
        }

        return false;
    }

    private void PlaceCard(CardGame2Manager card)
    {
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(transform);
        cardRect.localPosition = Vector3.zero;

        if (currentCard != null && currentCard != card)
        {
            currentCard.DetachFromSlot(); // 기존 카드가 있으면 분리
        }

        currentCard = card;
        card.SetCurrentSlot(this); // 카드에게 슬롯 전달
    }

    public void RemoveCard()
    {
        currentCard = null;
    }

    public CardGame2Manager GetCurrentCard()
    {
        return currentCard;
    }
}