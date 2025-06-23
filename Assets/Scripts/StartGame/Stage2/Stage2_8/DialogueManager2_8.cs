using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_8 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;
    public LocalizeStringEvent storyLineStringEvent;
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;
    public Button nextButton;

    private string aboveTableName = "Stage2_8AboveLine";
    private string storyTableName = "Stage2_8StoryLine";

    private string[] storyKeys = { "key2_8_1", "key2_8_2" };
    private int index = 0;

    private void Start()
    {
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(NextDialogue);

        aboveLineStringEvent.StringReference.SetReference(aboveTableName, "key1");
        aboveLineStringEvent.OnUpdateString.RemoveAllListeners();
        aboveLineStringEvent.OnUpdateString.AddListener(text => aboveLineText.text = text);
        aboveLineStringEvent.RefreshString();

        ShowDialogue(index);
    }

    private void ShowDialogue(int i)
    {
        if (i >= storyKeys.Length)
            return;

        nextButton.gameObject.SetActive(false);

        storyLineStringEvent.StringReference.SetReference(storyTableName, storyKeys[i]);
        storyLineStringEvent.OnUpdateString.RemoveAllListeners();
        storyLineStringEvent.OnUpdateString.AddListener(OnStoryLineReady);
        storyLineStringEvent.RefreshString();
    }

    private void OnStoryLineReady(string localizedText)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(localizedText));
    }

    private IEnumerator TypeText(string fullText)
    {
        storyLineText.text = "";
        foreach (char c in fullText)
        {
            storyLineText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.5f);
        nextButton.gameObject.SetActive(true);
    }

    private void NextDialogue()
    {
        index++;
        if (index >= storyKeys.Length)
        {
            SceneManager.LoadScene("CardGameThirdStage");
        }
        else
        {
            ShowDialogue(index);
        }
    }
}
