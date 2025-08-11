using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class HoverObjectSwitcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Refs")]
    public GameObject defaultImage;
    public GameObject hoverImage;

    // 내부 상태(선택)
    private bool isHovering = false;

    void Awake()     => ResetState();
    void OnEnable()  => ResetState();
    void Start()     => ResetState();
    void OnDisable() => ResetState(); // 호버 중 비활성 → 다시 켜질 때 기본 상태로

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        SetActiveSafe(defaultImage, false);
        SetActiveSafe(hoverImage, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        ResetState();
    }

    private void ResetState()
    {
        // 항상 기본만 켜고 호버는 끈다
        SetActiveSafe(defaultImage, true);
        SetActiveSafe(hoverImage, false);
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

#if UNITY_EDITOR
    // 에디터에서 드롭만 해도 기본/호버가 동시에 켜져 있던 걸 정리
    void OnValidate()
    {
        if (!Application.isPlaying) ResetState();
    }
#endif
}