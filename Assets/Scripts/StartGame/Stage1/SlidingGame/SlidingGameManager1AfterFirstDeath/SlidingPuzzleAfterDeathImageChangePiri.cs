using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SlidingPuzzleAfterDeathImageChangePiri : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Piri 이미지")]
    public Sprite defaultSprite;
    public Sprite hoverSprite;

    [Header("다국어 텍스트 이미지")]
    public Sprite koreanTextSprite;
    public Sprite chineseTextSprite;
    public Sprite kazakhTextSprite;
    public Sprite japaneseTextSprite;
    public Sprite englishTextSprite;

    private Image piriImage;
    private GameObject piriTextObj;
    private Image piriTextImage;

    void Awake()
    {
        piriImage = GetComponent<Image>();

        // 자식 오브젝트로부터 PiriText 찾기
        piriTextObj = transform.Find("PiriText")?.gameObject;

        if (piriTextObj == null)
        {
            Debug.LogError("PiriText 오브젝트가 Piri의 자식으로 존재해야 합니다.");
            return;
        }

        piriTextImage = piriTextObj.GetComponent<Image>();
        if (piriTextImage == null)
        {
            Debug.LogError("PiriText 오브젝트에 Image 컴포넌트가 필요합니다.");
            return;
        }

        piriTextObj.SetActive(false); // 시작 시 숨김
    }

    void Start()
    {
        LanguageManager.Initialize();
        SetPiriSprite(defaultSprite);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetPiriSprite(hoverSprite);

        Sprite langSprite = GetLanguageTextSprite(LanguageManager.GetLanguage());
        if (langSprite != null)
        {
            piriTextImage.sprite = koreanTextSprite; // 무조건 한글 이미지 테스트용
            piriTextImage.SetNativeSize();
            piriTextObj.SetActive(true);

        }
        Debug.Log($"[PiriText] Language Sprite Set: {langSprite?.name}");

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetPiriSprite(defaultSprite);
        piriTextObj.SetActive(false);
    }

    private void SetPiriSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            piriImage.sprite = sprite;
            piriImage.SetNativeSize(); // 원본 크기 유지
        }
    }

    private Sprite GetLanguageTextSprite(string lang)
    {
        switch (lang.ToLower())
        {
            case "korean": return koreanTextSprite;
            case "chinese": return chineseTextSprite;
            case "kazakh": return kazakhTextSprite;
            case "japanese": return japaneseTextSprite;
            default: return englishTextSprite;
        }
    }
}
