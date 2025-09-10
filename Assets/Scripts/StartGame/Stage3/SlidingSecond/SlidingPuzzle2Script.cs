using UnityEngine;
using UnityEngine.EventSystems;

public class SlidingPuzzle2Script : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("0=빈칸, 1~9=퍼즐")]
    public int puzzleNumber;

    [Tooltip("현재 위치 인덱스(0~9)")]
    public int currentPositionIndex;

    private RectTransform _rect;
    public RectTransform Rect => _rect ??= GetComponent<RectTransform>();

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // 패널로 인해 막혀 있거나, 일시정지면 클릭 무시
        if (SlidingGameManager2Script.Instance == null) return;
        if (!SlidingGameManager2Script.Instance.CanInteractPieces()) return;

        SlidingGameManager2Script.Instance.TryMovePuzzle(this);
    }

    public void SetPosition(int index, Vector2 anchoredPos)
    {
        currentPositionIndex = index;
        Rect.anchoredPosition = anchoredPos;
    }
}