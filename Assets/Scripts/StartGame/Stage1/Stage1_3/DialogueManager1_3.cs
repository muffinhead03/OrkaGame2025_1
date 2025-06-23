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
    public FadeController fadeController;  // Animator 대신 FadeController 사용

    private string storyTableName = "Stage1_3StoryLine";
    private string aboveTableName = "Stage1_3AboveLine";

    private int dialogueStep = 0;

    private void Start()
    {
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnNextClicked);

        // 시작 시 BGM 재생
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
            bgmAudioSource.Play();

        // 초기 대사 출력
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

        if (fluteAudioSource != null)
            fluteAudioSource.Play();

        // 페이드아웃 시작 후 자동 씬 이동
        if (fadeController != null)
            fadeController.StartFadeOut("Stage2_1");
    }
}

