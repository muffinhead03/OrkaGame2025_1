using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SlidingPuzzleAfterDeathImageChangeCoffee : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("다국어 Sprite 설정")]
    public Sprite defaultSprite;
    public Sprite koreanSprite;
    public Sprite chineseSprite;
    public Sprite kazakhSprite;
    public Sprite japaneseSprite;
    public Sprite englishSprite;

    private Image uiImage;
    private RectTransform rectTransform;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        LanguageManager.Initialize();
        SetSprite(defaultSprite);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Sprite langSprite = GetLanguageSprite(LanguageManager.GetLanguage());
        SetSprite(langSprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetSprite(defaultSprite);
    }

    private Sprite GetLanguageSprite(string lang)
    {
        switch (lang)
        {
            case "korean":
                return koreanSprite;
            case "chinese":
                return chineseSprite;
            case "kazakh":
                return kazakhSprite;
            case "japanese":
                return japaneseSprite;
            default:
                return englishSprite;
        }
    }

    private void SetSprite(Sprite sprite)
    {
        uiImage.sprite = sprite;

        if (sprite != null)
        {
            // 원본 Sprite 크기에 맞게 UI 크기 조정
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.rect.width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprite.rect.height);
        }
    }
}
