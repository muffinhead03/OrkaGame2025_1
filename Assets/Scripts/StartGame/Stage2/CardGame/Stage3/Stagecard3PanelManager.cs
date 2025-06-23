using UnityEngine;
using System.Collections.Generic;

public class Stagecard3PanelManager : MonoBehaviour
{
    public RectTransform panelRect;
    public float cardWidth = 550f;
    public float cardHeight = 550f;

    private List<Rect> slotAreas = new List<Rect>();
    private List<Vector2> slotCenters = new List<Vector2>();
    private bool[] slotOccupied = new bool[4];

    void Start()
    {
        float sectionWidth = panelRect.rect.width / 4f;
        float sectionHeight = panelRect.rect.height;

        for (int i = 0; i < 4; i++)
        {
            float x = sectionWidth * i;
            Rect slot = new Rect(x, 0, sectionWidth, sectionHeight);
            slotAreas.Add(slot);

            Vector2 center = new Vector2(
                x + sectionWidth / 2f - cardWidth / 2f,
                sectionHeight / 2f - cardHeight / 2f
            );
            slotCenters.Add(center);
        }
    }

    public bool TrySnapCard(Stagecard3Manager card, RectTransform cardRect)
    {
        Vector2 localPos = cardRect.anchoredPosition;

        for (int i = 0; i < 4; i++)
        {
            if (slotAreas[i].Contains(localPos) && !slotOccupied[i])
            {
                cardRect.anchoredPosition = slotCenters[i];
                slotOccupied[i] = true;
                return true;
            }
        }

        return false;
    }
}