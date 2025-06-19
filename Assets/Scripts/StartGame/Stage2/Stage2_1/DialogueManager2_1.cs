using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DialogueManager2_1 : MonoBehaviour
{
    [Header("Components")]
    public LocalizeStringEvent localizedStringEvent;
    public Typewriter typeWriter;
    public TMP_Text targetText;
    public Button nextButton;

    [Header("Sound Effects")]
    public AudioSource birdAudio;
    public AudioSource waterAudio;

    private int index = 0;
    private string tableName = "Stage2_1";
    private string[] keySuffixes = {
        "key2_1_1",
        "key2_1_2",
        "key2_1_3",
        "key2_1_4",
        "key2_1_5",
        "key2_1_6",
        "key2_1_7",
        "key2_1_8",
        "key2_1_9"
    };

    private void Start()
    {
        nextButton.interactable = false;
        nextButton.onClick.AddListener(NextDialogue);
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        if (birdAudio) birdAudio.Play();
        yield return new WaitForSeconds(0.5f);

        if (waterAudio) waterAudio.Play();
        yield return new WaitForSeconds(0.5f);

        ShowDialogue(index);
    }

    private void ShowDialogue(int i)
    {
        if (i >= keySuffixes.Length)
        {
            SceneManager.LoadScene("Stage2_1");
            return;
        }

        nextButton.interactable = false;

        string key = keySuffixes[i];
        localizedStringEvent.StringReference.SetReference(tableName, key);
        localizedStringEvent.OnUpdateString.RemoveAllListeners();
        localizedStringEvent.OnUpdateString.AddListener(OnLocalizedStringReady);
        localizedStringEvent.RefreshString(); // 강제로 갱신
    }

    private void OnLocalizedStringReady(string localizedText)
    {
        StartCoroutine(StartTypingCoroutine(localizedText));
    }

    private IEnumerator StartTypingCoroutine(string fullText)
    {
        yield return StartCoroutine(typeWriter.Type(fullText));
        nextButton.interactable = true;
    }

    private void NextDialogue()
    {
        index++;
        ShowDialogue(index);
    }
}
