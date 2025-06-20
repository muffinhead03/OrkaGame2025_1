using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitToScreen : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (!sr) return;

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        Vector2 spriteSize = sr.bounds.size;

        transform.localScale = new Vector3(
            screenWidth / spriteSize.x,
            screenHeight / spriteSize.y,
            1);
    }
}