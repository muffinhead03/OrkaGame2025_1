using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_4 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;
    public LocalizeStringEvent storyLineStringEvent;
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;
    public Button nextButton;

    [Header("Character Images")]
    public GameObject catImage;
    public GameObject ecoImage;

    private int index = 0;
    private string storyTableName = "Stage2_4";
    private string aboveTableName = "Stage2_4AboveLine";

    private string[] storyKeys = {
        "Key2_4_1", "Key2_4_2", "Key2_4_3", "Key2_4_4", "Key2_4_5",
        "Key2_4_6", "Key2_4_7", "Key2_4_8", "Key2_4_9", "Key2_4_10", "Key2_4_11"
    };

    // Cat ´ë»ç ÀÎµ¦½º
    private int[] catIndexes = { 0, 3, 7, 9 };

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
            SceneManager.LoadScene("Card Game First Stage");
            return;
        }

        nextButton.gameObject.SetActive(false);

        bool isCat = System.Array.Exists(catIndexes, n => n == i);
        string aboveKey = isCat ? "Key1" : "Key2";

        aboveLineStringEvent.StringReference.SetReference(aboveTableName, aboveKey);
        storyLineStringEvent.StringReference.SetReference(storyTableName, storyKeys[i]);

        catImage.SetActive(isCat);
        ecoImage.SetActive(!isCat);

        aboveLineStringEvent.OnUpdateString.RemoveAllListeners();
        aboveLineStringEvent.OnUpdateString.AddListener(text => aboveLineText.text = text);

        storyLineStringEvent.OnUpdateString.RemoveAllListeners();
        storyLineStringEvent.OnUpdateString.AddListener(OnStoryLineReady);

        aboveLineStringEvent.RefreshString();
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

    public void NextDialogue()
    {
        index++;
        ShowDialogue(index);
    }
}
