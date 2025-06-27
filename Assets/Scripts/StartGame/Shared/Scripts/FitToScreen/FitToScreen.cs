using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitToScreen : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (!sr || sr.sprite == null)
        {
            Debug.LogWarning("SpriteRenderer 또는 sprite가 없습니다.");
            return;
        }

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        Vector2 spriteSize = sr.bounds.size;
        if (spriteSize.x == 0 || spriteSize.y == 0)
        {
            Debug.LogWarning($"spriteSize가 0입니다: {spriteSize}");
            return;
        }

        transform.localScale = new Vector3(
            screenWidth / spriteSize.x,
            screenHeight / spriteSize.y,
            1);
    }
}