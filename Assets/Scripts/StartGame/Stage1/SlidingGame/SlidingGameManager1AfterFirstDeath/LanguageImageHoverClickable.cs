using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 기본 이미지는 공통으로 표시.
/// 호버 시 언어별 스프라이트로 교체하고(주사기 전용처럼) 가로로만 넓어지게 할 수 있다.
/// Canvas + EventSystem + GraphicRaycaster 전제.
/// </summary>
[RequireComponent(typeof(Image))]
public class LanguageHoverImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("기본(공통) 이미지")]
    public Sprite baseSprite;

    [Header("언어별(호버 시 교체할) 이미지")]
    public Sprite hoverKorean;
    public Sprite hoverEnglish;
    public Sprite hoverChinese;
    public Sprite hoverJapanese;
    public Sprite hoverKazakh;

    [Header("호버 스케일(일반 확대/축소)")]
    public bool scaleOnHover = false;     // 주사기 전용에선 보통 false 권장
    public float hoverScale = 1.06f;
    public float scaleLerpSpeed = 12f;

    [Header("가로만 넓히기(주사기 전용 권장)")]
    public bool widenOnHover = true;      // 주사기 오브젝트에서 켜두세요
    [Tooltip("계산된 가로폭에 추가로 곱해줄 배율(1 = 그대로, 1.2 = 20% 더 넓게)")]
    public float widthExtraMultiplier = 1.0f;
    [Tooltip("가로폭 보간 속도")]
    public float widthLerpSpeed = 12f;

    private Image baseImage;
    private RectTransform rt;
    private LayoutElement layoutElement;

    private Vector3 originalScale;
    private bool isHovering;
    private Sprite cachedBase;       // 복구용

    // 너비 보간용
    private float baseWidth;           // 레이아웃이 없을 때 기준 sizeDelta.x
    private float basePreferredWidth;  // 레이아웃이 있을 때 기준 preferredWidth
    private float currentWidth;        // 보간 중 현재 값

    private void Awake()
    {
        baseImage = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();

        originalScale = transform.localScale;

        if (baseSprite != null) baseImage.sprite = baseSprite;
        cachedBase = baseImage.sprite;

        // 가로 확장에 방해되는 옵션 방지
        baseImage.type = Image.Type.Simple;
        baseImage.preserveAspect = false;

        // 알파 0 방지
        var c = baseImage.color;
        if (c.a <= 0f) { c.a = 1f; baseImage.color = c; }
        baseImage.enabled = true;
        baseImage.raycastTarget = true;

        // 기준 너비 기록
        if (layoutElement != null)
        {
            basePreferredWidth = layoutElement.preferredWidth > 0 ? layoutElement.preferredWidth : rt.rect.width;
            currentWidth = basePreferredWidth;
        }
        else
        {
            // sizeDelta.x가 0일 수 있으니, rect.width 기준으로 초기화
            baseWidth = rt.sizeDelta.x != 0 ? rt.sizeDelta.x : rt.rect.width;
            currentWidth = baseWidth;
        }
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Update()
    {
        // 일반 스케일 효과(선택)
        if (scaleOnHover)
        {
            var targetScale = isHovering ? originalScale * hoverScale : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleLerpSpeed);
        }

        // 가로만 넓히기
        if (widenOnHover)
        {
            float targetW = GetTargetWidth(isHovering);
            currentWidth = Mathf.Lerp(currentWidth, targetW, Time.unscaledDeltaTime * widthLerpSpeed);

            if (layoutElement != null)
            {
                layoutElement.preferredWidth = currentWidth;
            }
            else
            {
                var sd = rt.sizeDelta;
                // sizeDelta.y는 그대로 두고 x만 변경
                sd.x = currentWidth;
                rt.sizeDelta = sd;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        cachedBase = baseImage.sprite; // 외부에서 바뀌었을 수도 있으니 캐시

        // 언어별 이미지로 교체
        baseImage.sprite = GetLanguageSprite(LanguageManager.GetLanguage()) ?? baseSprite ?? cachedBase;
        baseImage.enabled = true;
        // (가로폭 목표치는 Update에서 계산/보간됨)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // 공통 이미지로 복구
        baseImage.sprite = baseSprite != null ? baseSprite : cachedBase;
        baseImage.enabled = true;
        // (가로폭은 Update에서 기준값으로 되돌아가도록 보간)
    }

    private void OnLanguageChanged(string _)
    {
        // 호버 중이면 즉시 언어 스프라이트로 갱신
        if (isHovering)
            baseImage.sprite = GetLanguageSprite(LanguageManager.GetLanguage()) ?? baseSprite ?? cachedBase;
        // 폭은 Update에서 자동 반영
    }

    private Sprite GetLanguageSprite(string lang)
    {
        switch (lang)
        {
            case "korean":   return hoverKorean;
            case "english":  return hoverEnglish;
            case "chinese":  return hoverChinese;
            case "japanese": return hoverJapanese;
            case "kazakh":   return hoverKazakh;
            default:         return hoverEnglish ?? hoverKorean ?? hoverChinese ?? hoverJapanese ?? hoverKazakh;
        }
    }

    /// <summary>
    /// 현재 높이를 기준으로, 언어 스프라이트의 가로/세로 비율에 맞춰
    /// 목표 가로폭을 계산한다. (widenOnHover 전용)
    /// </summary>
    private float GetTargetWidth(bool hovering)
    {
        // 기준 높이 구하기 (레이아웃이 있으면 preferredHeight 우선)
        float height;
        if (layoutElement != null)
        {
            height = layoutElement.preferredHeight > 0 ? layoutElement.preferredHeight : rt.rect.height;
        }
        else
        {
            // sizeDelta.y가 0인 경우 rect.height 사용
            height = rt.sizeDelta.y != 0 ? rt.sizeDelta.y : rt.rect.height;
        }

        if (!hovering || baseImage.sprite == null)
        {
            // 비호버 → 기준 너비로 복귀
            return layoutElement != null ? basePreferredWidth : baseWidth;
        }

        // 언어 스프라이트의 픽셀 기준 가로/세로
        var spr = baseImage.sprite;
        float sprW = spr.rect.width / spr.pixelsPerUnit;
        float sprH = spr.rect.height / spr.pixelsPerUnit;
        float aspect = sprH > 0 ? (sprW / sprH) : 1f;

        // 현재 높이를 유지하면서 가로폭만 aspect에 맞게 확장
        float targetWidth = height * aspect;

        // 추가 배율 적용(주사기 오브젝트라면 1.1~1.3 정도 권장)
        targetWidth *= Mathf.Max(0.01f, widthExtraMultiplier);

        // 레이아웃 유무에 따라 최소 기준과 비교(너무 작게 줄지 않도록)
        float minBase = (layoutElement != null) ? basePreferredWidth : baseWidth;
        return Mathf.Max(minBase, targetWidth);
    }
}
