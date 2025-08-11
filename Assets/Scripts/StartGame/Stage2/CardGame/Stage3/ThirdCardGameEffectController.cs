using UnityEngine;
using UnityEngine.SceneManagement;

public class ThirdCardGameEffectController : MonoBehaviour
{
    public static ThirdCardGameEffectController Instance;

    [Tooltip("이 버튼으로 이동할 다음 씬 이름")]
    [SerializeField] private string nextSceneName = "Stage3_1";

    public bool isEffectPlaying = false; // 이펙트 중엔 클릭 무시하고 싶을 때 사용

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 필요하면 다음 줄 주석 해제해서 씬 넘어가도 안 없어지게
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 버튼 OnClick에 연결할 함수
    public void OnClickGoToNext()
    {
        if (isEffectPlaying) return; // 이펙트 중이면 무시
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("[ThirdCardGameEffectController] nextSceneName 비어 있음");
            return;
        }
        SceneManager.LoadScene(nextSceneName);
    }

    // (옵션) 이펙트 상태 토글용 헬퍼
    public void BeginEffect() => isEffectPlaying = true;
    public void EndEffect()   => isEffectPlaying = false;
}