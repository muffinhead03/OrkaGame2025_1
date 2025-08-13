using UnityEngine.EventSystems;

public class EmptyPuzzle2AfterDeath : SlidingPuzzle2AfterDeath
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