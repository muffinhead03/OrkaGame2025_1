using UnityEngine;
using UnityEngine.UI;

public class PuzzleTile : MonoBehaviour
{
    [Header("Tile Data")]
    public int tileIndex; // 0 = 빈칸, 1~12 = 퍼즐 번호
    public int currentSlot; // 현재 위치 인덱스 (0~12)
    public PuzzleManager puzzleManager;

    private Button tileButton;

    void Awake()
    {
        // 버튼 컴포넌트를 안전하게 캐싱
        tileButton = GetComponent<Button>();

        if (tileButton != null)
        {
            tileButton.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogWarning($"❗ {gameObject.name}에 Button 컴포넌트가 없습니다.");
        }
    }

    void OnClicked()
    {
        // 빈칸은 클릭해도 아무 반응 없음
        if (tileIndex != 0 && puzzleManager != null)
        {
            puzzleManager.OnTileClicked(this);
        }
    }
}