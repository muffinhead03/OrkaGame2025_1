using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using System.Collections;

public class DiologueManager2 : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text abovelineText;
    public TMP_Text storyText;
    public GameObject nextButton;
    public Image fadeImage;

    [Header("Characters")]
    public GameObject echoImage;
    public GameObject fatherImage;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip mainBGM;

    [Header("Sound Effects")]
    public AudioClip roughlyHittingWall;
    public AudioClip kickEcho;
    public AudioClip echoBreathingSmall;
    public AudioClip doorSlam;
    public AudioClip lockingSound;
    private bool hasPlayedSlamAndLock = false;

    [Header("Typewriter")]
    public Typewriter typewriter;

    private LocalizedString[] storyLines;

    private string[] abovelines = new string[]
    {
        "에코", "아빠", "에코", "아빠", "아빠", "에코",
        "아빠", "에코", "에코", "에코", "아빠", "아빠",
        "에코", "에코", "에코", "에코", "에코", "에코"
    };

    private int dialogueIndex = 0;

    void Start()
    {
        SetBlackScreenFullyOpaque();

        if (bgmSource != null && mainBGM != null)
        {
            bgmSource.clip = mainBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        storyLines = new LocalizedString[18];
        for (int i = 0; i < 18; i++)
        {
            storyLines[i] = new LocalizedString { TableReference = "Stage1_2", TableEntryReference = $"key{i + 1}" };
        }

        StartCoroutine(StartupSequence());
    }

    void SetBlackScreenFullyOpaque()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }
    }

    IEnumerator StartupSequence()
    {
        yield return StartCoroutine(TypeLocalizedWithSpeaker(dialogueIndex));
        nextButton.SetActive(true);
    }

    public void OnNextClicked()
    {
        nextButton.SetActive(false);

        switch (dialogueIndex)
        {
            case 5:
                StartCoroutine(HandleWallHitThenNext());
                break;
            case 8:
                StartCoroutine(HandleKickThenNext());
                break;
            case 9:
                StartCoroutine(HandleBreathingThenNext());
                break;
            case 11:
                if (!hasPlayedSlamAndLock)
                {
                    hasPlayedSlamAndLock = true;
                    StartCoroutine(HandleSlamAndLockThenNext());
                }
                break;
            default:
                dialogueIndex++;

                if (dialogueIndex < storyLines.Length)
                {
                    StartCoroutine(TypeLocalizedWithSpeaker(dialogueIndex));
                }
                else
                {
                    SceneManager.LoadScene("SlidingPuzzle");
                }
                break;
        }
    }

    IEnumerator TypeLocalizedWithSpeaker(int index)
    {
        string speaker = abovelines[index];
        abovelineText.text = speaker;

        echoImage.SetActive(speaker == "에코");
        fatherImage.SetActive(speaker == "아빠");

        var op = storyLines[index].GetLocalizedStringAsync();
        yield return op;

        yield return StartCoroutine(typewriter.Type(op.Result));
        nextButton.SetActive(true);
    }

    IEnumerator FadeBGMVolume(float targetVolume, float duration)
    {
        if (bgmSource == null) yield break;

        float startVolume = bgmSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    IEnumerator PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) yield break;

        yield return StartCoroutine(FadeBGMVolume(0f, 0.5f));
        sfxSource.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);
        yield return StartCoroutine(FadeBGMVolume(1f, 0.5f));
    }

    IEnumerator HandleWallHitThenNext()
    {
        dialogueIndex++;

        yield return PlaySFX(roughlyHittingWall);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeLocalizedWithSpeaker(dialogueIndex));
        nextButton.SetActive(true);
    }

    IEnumerator HandleKickThenNext()
    {
        dialogueIndex++;

        yield return PlaySFX(kickEcho);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeLocalizedWithSpeaker(dialogueIndex));
        nextButton.SetActive(true);
    }

    IEnumerator HandleBreathingThenNext()
    {
        dialogueIndex++;

        yield return PlaySFX(echoBreathingSmall);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeLocalizedWithSpeaker(dialogueIndex));
        nextButton.SetActive(true);
    }

    IEnumerator HandleSlamAndLockThenNext()
    {
        dialogueIndex++;

        if (doorSlam != null)
            yield return PlaySFX(doorSlam);

        yield return new WaitForSeconds(0.5f);

        if (lockingSound != null)
            yield return PlaySFX(lockingSound);

        if (dialogueIndex < storyLines.Length)
        {
            yield return StartCoroutine(TypeLocalizedWithSpeaker(dialogueIndex));
            nextButton.SetActive(true);
        }
    }
}
