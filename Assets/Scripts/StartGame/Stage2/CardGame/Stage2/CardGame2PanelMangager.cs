using UnityEngine;

public class CardGame2PanelManager : MonoBehaviour
{
    private RectTransform slotTransform;
    private CardGame2Card currentCard;

    private void Awake()
    {
        slotTransform = GetComponent<RectTransform>();
    }

    public bool TryPlaceCard(CardGame2Card card)
    {
        if (currentCard == null || currentCard == card)
        {
            PlaceCard(card);
            return true;
        }

        return false;
    }

    private void PlaceCard(CardGame2Card card)
    {
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(transform);
        cardRect.localPosition = Vector3.zero;

        if (currentCard != null && currentCard != card)
        {
            currentCard.DetachFromSlot();
        }

        currentCard = card;
        card.SetCurrentSlot(this);
    }

    public void RemoveCard()
    {
        currentCard = null;
    }

    public CardGame2Card GetCurrentCard()
    {
        return currentCard;
    }
}