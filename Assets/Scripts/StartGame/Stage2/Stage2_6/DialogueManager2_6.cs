using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_6 : MonoBehaviour
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

    [Header("Dark Mode & Select Buttons")]
    public GameObject darkmode;
    public GameObject lightBox1;
    public GameObject lightBox2;
    public GameObject darkBox1;
    public GameObject darkBox2;

    private string storyTableName = "Stage2_6StoryLine";
    private string aboveTableName = "Stage2_6AboveLine";

    private string[] storyKeys = {
        "key2_6_1", "key2_6_2", "key2_6_3", "key2_6_4", "key2_6_5"
    };

    private int index = 0;

    private void Start()
    {
        nextButton.onClick.AddListener(NextDialogue);
        lightBox1.SetActive(false);
        lightBox2.SetActive(false);
        darkBox1.SetActive(false);
        darkBox2.SetActive(false);
        darkmode.SetActive(false);
        ShowDialogue(index);
    }

    private void ShowDialogue(int i)
    {
        if (i >= storyKeys.Length)
        {
            ShowChoiceUI();
            return;
        }

        nextButton.gameObject.SetActive(false);

        bool isEco = (i == 0 || i == 2); // key2_6_1, key2_6_3
        string aboveKey = isEco ? "key1" : "key2";

        catImage.SetActive(!isEco);
        ecoImage.SetActive(isEco);

        aboveLineStringEvent.StringReference.SetReference(aboveTableName, aboveKey);
        storyLineStringEvent.StringReference.SetReference(storyTableName, storyKeys[i]);

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

    private void ShowChoiceUI()
    {
        darkmode.SetActive(true);
        lightBox1.SetActive(true);
        lightBox2.SetActive(true);
    }

    // UI Button Hover Events
    public void OnHoverLightBox1()
    {
        lightBox1.SetActive(false);
        darkBox1.SetActive(true);
    }

    public void OnExitDarkBox1()
    {
        darkBox1.SetActive(false);
        lightBox1.SetActive(true);
    }

    public void OnHoverLightBox2()
    {
        lightBox2.SetActive(false);
        darkBox2.SetActive(true);
    }

    public void OnExitDarkBox2()
    {
        darkBox2.SetActive(false);
        lightBox2.SetActive(true);
    }

    // Scene Change
    public void OnClickDarkBox1()
    {
        SceneManager.LoadScene("Stage2_7");
    }

    public void OnClickDarkBox2()
    {
        SceneManager.LoadScene("Stage2_8");
    }
}

