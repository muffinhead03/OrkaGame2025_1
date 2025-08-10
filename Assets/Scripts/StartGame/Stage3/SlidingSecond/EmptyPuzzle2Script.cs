using UnityEngine.EventSystems;

public class EmptyPuzzle2Script : SlidingPuzzle2Script
{
    private void Reset()
    {
        puzzleNumber = 0; // 빈칸
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // 빈칸 클릭은 무시
    }
}