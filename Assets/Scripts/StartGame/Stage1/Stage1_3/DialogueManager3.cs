using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialogueManager3 : MonoBehaviour
{
    public TMP_Text storyText;
    public GameObject nextButton;
    private Typewriter typewriter;


    public LocalizedString[] dialogueLines;

    [Header("Audio")]
    public AudioClip slightlyDoorOpen;
    public AudioClip clearPanflute;
    public AudioClip glitchEffect;

    public AudioSource bgmSource;

    [Header("Fade")]
    public GameObject whiteScreen;

    private int dialogueIndex = 0;
    private float originalBGMVolume;

    void Start()
    {
        if (whiteScreen != null)
        {
            CanvasGroup cg = whiteScreen.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.gameObject.SetActive(true);
            }
        }

        if (bgmSource != null)
            originalBGMVolume = bgmSource.volume;

        nextButton.SetActive(false); // 처음엔 숨김
        StartCoroutine(StartDialogue());
    }

    IEnumerator StartDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            Debug.Log("▶ 첫 대사 출력 시작");
            yield return StartCoroutine(TypeLocalized(dialogueLines[0]));
            Debug.Log("✅ 첫 대사 출력 완료 — 버튼 보이기");
            nextButton.SetActive(true);
        }
        else
        {
            Debug.LogError("dialogueLines 배열이 비어 있습니다!");
        }
    }


    public void OnNextClicked()
    {
        nextButton.SetActive(false);

        switch (dialogueIndex)
        {
            case 0:
                StartCoroutine(HandleStep1());
                break;
            case 1:
                StartCoroutine(HandleStep2());
                break;
            default:
                Debug.Log("모든 대사가 끝났습니다.");
                break;
        }
    }

    IEnumerator HandleStep1()
    {
        dialogueIndex = 1;

        if (bgmSource != null)
            StartCoroutine(FadeAudio(bgmSource, 0f, 1f));

        if (slightlyDoorOpen != null)
            AudioSource.PlayClipAtPoint(slightlyDoorOpen, Camera.main.transform.position);

        yield return new WaitForSeconds(slightlyDoorOpen.length);

        if (bgmSource != null)
            StartCoroutine(FadeAudio(bgmSource, originalBGMVolume, 1f));

        yield return StartCoroutine(TypeLocalized(dialogueLines[1]));
        nextButton.SetActive(true);
    }

    IEnumerator HandleStep2()
    {
        dialogueIndex = 2;

        if (clearPanflute != null)
            AudioSource.PlayClipAtPoint(clearPanflute, Camera.main.transform.position);

        yield return new WaitForSeconds(0.5f);

        if (whiteScreen != null)
            yield return StartCoroutine(BlinkWhiteScreen());

        yield return new WaitForSeconds(1.5f);

        if (glitchEffect != null)
            AudioSource.PlayClipAtPoint(glitchEffect, Camera.main.transform.position);

        // 마지막 연출이라면 필요 시 씬 전환, 추가 연출 등 가능
    }

    void Awake()
    {
        if (typewriter == null)
        {
            typewriter = GetComponent<Typewriter>(); // 자동 연결 시도
        }
    }
    IEnumerator TypeLocalized(LocalizedString loc)
    {
        var op = loc.GetLocalizedStringAsync();
        yield return op;

        string localizedText = op.Result;

        if (string.IsNullOrEmpty(localizedText))
        {
            Debug.LogWarning("Localized text is null or empty.");
            storyText.text = "[텍스트 없음]";
            yield break;
        }

        if (typewriter != null)
        {
            yield return StartCoroutine(typewriter.Type(localizedText));
        }
        else
        {
            storyText.text = localizedText;
        }
    }


    IEnumerator FadeInWhiteScreen()
    {
        CanvasGroup cg = whiteScreen.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogError("WhiteScreen에 CanvasGroup이 필요합니다!");
            yield break;
        }

        cg.alpha = 0;
        cg.gameObject.SetActive(true);

        for (float a = 0f; a <= 1f; a += 0.02f)
        {
            cg.alpha = a;
            yield return new WaitForSeconds(0.02f);
        }

        cg.alpha = 1f;
    }

    IEnumerator BlinkWhiteScreen()
    {
        CanvasGroup cg = whiteScreen.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        cg.alpha = 1f;
        yield return new WaitForSeconds(0.2f);
        cg.alpha = 0f;
        yield return new WaitForSeconds(0.2f);
        cg.alpha = 1f;
    }

    IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        source.volume = targetVolume;
    }
}
