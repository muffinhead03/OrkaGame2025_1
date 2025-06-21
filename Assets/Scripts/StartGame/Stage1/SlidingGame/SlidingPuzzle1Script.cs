using UnityEngine;
using UnityEngine.EventSystems;

public class SlidingPuzzle1Script : MonoBehaviour, IPointerClickHandler
{
    public int puzzleNumber;  // 1 ~ 12 (EmptyPuzzle은 0)
    public int currentPositionIndex; // 1 ~ 13
    public Vector2[] positions; // 좌표들 미리 저장된 배열 (GameManager에서 초기화 가능)

    public void OnPointerClick(PointerEventData eventData)
    {
        SlidingGameManager1Script.Instance.TryMovePuzzle(this);
    }

    public void SetPosition(int index, Vector2 position)
    {
        currentPositionIndex = index;
        transform.localPosition = position;
    }
}
