using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager1_3 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;
    public LocalizeStringEvent storyLineStringEvent;
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;
    public Button nextButton;

    [Header("Audio")]
    public AudioSource bgmAudioSource;
    public AudioSource doorAudioSource;
    public AudioSource fluteAudioSource;

    [Header("Fade")]
    public CanvasGroup blackFadeImage; // ✅ 검은 배경 패널 (CanvasGroup 필요)

    private string storyTableName = "Stage1_3StoryLine";
    private string aboveTableName = "Stage1_3AboveLine";

    private int dialogueStep = 0;

    private void Start()
    {
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnNextClicked);

        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
            bgmAudioSource.Play();

        if (blackFadeImage != null)
        {
            blackFadeImage.alpha = 0f;
            blackFadeImage.blocksRaycasts = false;
            blackFadeImage.interactable = false;
        }

        aboveLineStringEvent.StringReference.SetReference(aboveTableName, "key1");
        storyLineStringEvent.StringReference.SetReference(storyTableName, "key1_3_1");

        aboveLineStringEvent.OnUpdateString.RemoveAllListeners();
        aboveLineStringEvent.OnUpdateString.AddListener(text => aboveLineText.text = text);

        storyLineStringEvent.OnUpdateString.RemoveAllListeners();
        storyLineStringEvent.OnUpdateString.AddListener(OnFirstStoryReady);

        aboveLineStringEvent.RefreshString();
        storyLineStringEvent.RefreshString();
    }

    private void OnFirstStoryReady(string localizedText)
    {
        StopAllCoroutines();
        StartCoroutine(PlayFirstDialogue(localizedText));
    }

    private IEnumerator PlayFirstDialogue(string fullText)
    {
        storyLineText.text = "";
        foreach (char c in fullText)
        {
            storyLineText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.5f);

        if (doorAudioSource != null)
            doorAudioSource.Play();

        nextButton.gameObject.SetActive(true);
        nextButton.interactable = true;
    }

    public void OnNextClicked()
    {
        nextButton.gameObject.SetActive(false);

        if (dialogueStep == 0)
        {
            dialogueStep++;

            storyLineStringEvent.StringReference.SetReference(storyTableName, "key1_3_2");
            storyLineStringEvent.OnUpdateString.RemoveAllListeners();
            storyLineStringEvent.OnUpdateString.AddListener(OnSecondStoryReady);
            storyLineStringEvent.RefreshString();
        }
    }

    private void OnSecondStoryReady(string localizedText)
    {
        StopAllCoroutines();
        StartCoroutine(PlaySecondDialogueAndTransition(localizedText));
    }

    private IEnumerator PlaySecondDialogueAndTransition(string fullText)
    {
        storyLineText.text = "";
        foreach (char c in fullText)
        {
            storyLineText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(FadeOut());

        if (fluteAudioSource != null)
            fluteAudioSource.Play();

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("Stage2_1");
    }

    private IEnumerator FadeOut()
    {
        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            if (blackFadeImage != null)
                blackFadeImage.alpha = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }

        if (blackFadeImage != null)
            blackFadeImage.alpha = 1f;
    }
}
