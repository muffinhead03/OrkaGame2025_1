using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreen : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private bool hidePanelOnStart = true;

    [Header("New Game Transition")]
    [SerializeField] private string newGameSceneName = "Stage1_1";
    [SerializeField] private Image transitionImage;  // ⬅ 알파를 줄 Image (초기 비활성화)
    [SerializeField] private float fadeDuration   = 2f;   // 알파 0→1
    [SerializeField] private float scaleDuration  = 3f;   // 스케일 1→1.3
    [SerializeField] private float targetScale    = 1.3f; // 최종 스케일
    [SerializeField] private bool useUnscaledTime = false; // 필요시 true로

    private bool isTransitioning = false;

    private void Start()
    {
        if (hidePanelOnStart && languagePanel != null)
            languagePanel.SetActive(false);

        // 트랜지션 이미지는 기본적으로 꺼 둠(요구사항)
        if (transitionImage != null)
            transitionImage.gameObject.SetActive(false);
    }

    // Start 버튼 (New Game)
    public void OnStartButtonClicked()
    {
        if (!isTransitioning)
            StartCoroutine(PlayNewGameTransitionAndLoad(newGameSceneName));
    }

    // ✅ Continue 버튼
    public void OnContinueButtonClicked()
    {
        if (CurrentGameStatus.HasSave)
            CurrentGameStatus.ContinueGame();
        else
            Debug.LogWarning("[MainScreen] 세이브가 없어 Continue 불가");
    }

    // Quit 버튼
    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Language 버튼
    public void OnLanguageButtonClicked()
    {
        if (languagePanel == null)
        {
            Debug.LogError("[MainScreen] languagePanel이 할당되지 않았습니다.");
            return;
        }
        languagePanel.SetActive(true);
        // languagePanel.SetActive(!languagePanel.activeSelf);
    }

    public void OnCloseLanguagePanel()
    {
        if (languagePanel != null)
            languagePanel.SetActive(false);
    }

    private System.Collections.IEnumerator PlayNewGameTransitionAndLoad(string sceneName)
    {
        isTransitioning = true;

        // 트랜지션 준비
        if (transitionImage != null)
        {
            var go = transitionImage.gameObject;
            var rt = transitionImage.rectTransform;

            go.SetActive(true);

            // 초기 상태 세팅: alpha 0, scale 1
            Color c = transitionImage.color;
            c.a = 0f;
            transitionImage.color = c;
            rt.localScale = Vector3.one;

            float t = 0f;
            float maxT = Mathf.Max(fadeDuration, scaleDuration);

            // 동시에 진행
            while (t < maxT)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt;

                // 알파
                if (fadeDuration > 0f)
                {
                    float a = Mathf.Clamp01(t / fadeDuration);
                    c.a = a;
                    transitionImage.color = c;
                }

                // 스케일
                if (scaleDuration > 0f)
                {
                    float s = Mathf.Lerp(1f, targetScale, Mathf.Clamp01(t / scaleDuration));
                    rt.localScale = new Vector3(s, s, 1f);
                }

                yield return null;
            }

            // 최종값 보정
            c.a = 1f; transitionImage.color = c;
            rt.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        // 모두 끝나면 씬 로드
        SceneManager.LoadScene(sceneName);
    }
}
