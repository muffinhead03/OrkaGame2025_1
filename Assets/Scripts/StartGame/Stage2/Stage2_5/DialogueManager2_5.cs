using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_5 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent AboveLine;
    public LocalizeStringEvent StoryLine;
    public TMP_Text AboveLineText;
    public TMP_Text StoryLineText;
    public Button Button;

    [Header("Character Images")]
    public GameObject Eco;
    public GameObject Cat;

    [Header("Audio")]
    public AudioSource YellingSound;

    private int index = 0;

    private string[] storyKeys = {
        "key2_5_1", "key2_5_2", "key2_5_3", "key2_5_4", "key2_5_5"
    };

    private void Start()
    {
        Button.onClick.AddListener(OnClickNext);
        Button.gameObject.SetActive(false);
        ShowDialogue(index);
    }

    void ShowDialogue(int i)
    {
        if (i >= storyKeys.Length)
        {
            SceneManager.LoadScene("Stage2_6");
            return;
        }

        // 화자 지정
        bool isEco = (i == 0 || i == 1 || i == 4); // 0,1,4번 대사는 Eco
        string aboveKey = isEco ? "key1" : "key2";

        AboveLine.StringReference.SetReference("Stage2_5AboveLine", aboveKey);
        StoryLine.StringReference.SetReference("Stage2_5StoryLine", storyKeys[i]);

        Eco.SetActive(isEco);
        Cat.SetActive(!isEco);

        AboveLine.OnUpdateString.RemoveAllListeners();
        AboveLine.OnUpdateString.AddListener(text => AboveLineText.text = text);

        StoryLine.OnUpdateString.RemoveAllListeners();
        StoryLine.OnUpdateString.AddListener(OnStoryReady);

        AboveLine.RefreshString();
        StoryLine.RefreshString();

        Button.gameObject.SetActive(false);
    }

    void OnStoryReady(string text)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string fullText)
    {
        StoryLineText.text = "";
        foreach (char c in fullText)
        {
            StoryLineText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        if (index == 0) // key2_5_1 이후
        {
            yield return new WaitForSeconds(1f);
            YellingSound.Play();
            yield return new WaitUntil(() => !YellingSound.isPlaying);
            yield return new WaitForSeconds(0.5f);
            index++;
            ShowDialogue(index);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            Button.gameObject.SetActive(true);
        }
    }

    void OnClickNext()
    {
        index++;
        ShowDialogue(index);
    }
}
