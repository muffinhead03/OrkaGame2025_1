using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_2 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;
    public LocalizeStringEvent storyLineStringEvent;
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;
    public Button nextButton;

    [Header("Character Images")]
    public GameObject narkeImage;
    public GameObject echoImage;

    private int index = 0;
    private string storyTableName = "Stage2_2SL";
    private string aboveTableName = "Stage2_2AL";

    private string[] storyKeys = {
        "key2_2_1", "key2_2_2", "key2_2_3", "key2_2_4", "key2_2_5",
        "key2_2_6", "key2_2_7", "key2_2_8", "key2_2_9", "key2_2_10"
    };

    private int[] narkeIndexes = { 0, 2, 4, 6, 7, 9 };

    private void Start()
    {
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(NextDialogue);
        ShowDialogue(index);
    }

    private void ShowDialogue(int i)
    {
        if (i >= storyKeys.Length)
        {
            SceneManager.LoadScene("Stage2_3");
            return;
        }

        nextButton.gameObject.SetActive(false);

        bool isNarke = System.Array.Exists(narkeIndexes, n => n == i);
        string aboveKey = (i % 2 == 0) ? "key1" : "key2";

        aboveLineStringEvent.StringReference.SetReference(aboveTableName, aboveKey);
        aboveLineStringEvent.OnUpdateString.RemoveAllListeners();
        aboveLineStringEvent.OnUpdateString.AddListener(text => aboveLineText.text = text);
        aboveLineStringEvent.RefreshString();

        storyLineStringEvent.StringReference.SetReference(storyTableName, storyKeys[i]);
        storyLineStringEvent.OnUpdateString.RemoveAllListeners();
        storyLineStringEvent.OnUpdateString.AddListener(OnStoryLineReady);
        storyLineStringEvent.RefreshString();

        narkeImage.SetActive(isNarke);
        echoImage.SetActive(!isNarke);
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

    public void NextDialogue()
    {
        index++;
        ShowDialogue(index);
    }
}
