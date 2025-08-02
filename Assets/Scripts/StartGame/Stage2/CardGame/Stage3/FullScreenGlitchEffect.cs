using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenGlitchEffect : MonoBehaviour
{
    public GameObject fullScreenGlitch;
    public float fadeDuration = 1.0f;

    private Image glitchImage;

    void Awake()
    {
        glitchImage = fullScreenGlitch.GetComponent<Image>();
        if (glitchImage != null)
        {
            var color = glitchImage.color;
            color.a = 0f;
            glitchImage.color = color;
        }
        fullScreenGlitch.SetActive(false);
    }

    public void OnCardFullScreenGlitchClicked()
    {
        if (ThirdCardGameEffectController.Instance.isEffectPlaying) return;

        ThirdCardGameEffectController.Instance.isEffectPlaying = true;
        StartCoroutine(FadeInAndDisable());
    }

    private IEnumerator FadeInAndDisable()
    {
        fullScreenGlitch.SetActive(true);

        float elapsed = 0f;
        Color color = glitchImage.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            color.a = alpha;
            glitchImage.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure fully visible
        color.a = 1f;
        glitchImage.color = color;

        // 바로 비활성화
        fullScreenGlitch.SetActive(false);

        // ✅ 효과 끝났으니 해제
        ThirdCardGameEffectController.Instance.isEffectPlaying = false;
    }
}