using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_1 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;
    public LocalizeStringEvent storyLineStringEvent;
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;
    public Button nextButton;

    [Header("Character Image")]
    public GameObject ecoImage;

    private int index = 0;
    private string storyTableName = "Stage2_1";
    private string aboveTableName = "Stage2_1AboveLine";

    private string[] storyKeys = {
        "key2_1_1", "key2_1_2", "key2_1_3", "key2_1_4", "key2_1_5",
        "key2_1_6", "key2_1_7", "key2_1_8", "key2_1_9"
    };

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
            SceneManager.LoadScene("Stage2_2");
            return;
        }

        nextButton.gameObject.SetActive(false);

        // 항상 ecoImage만 표시
        ecoImage.SetActive(true);

        // 위 대사 설정 (즉시)
        aboveLineStringEvent.StringReference.SetReference(aboveTableName, "key1");
        aboveLineStringEvent.OnUpdateString.RemoveAllListeners();
        aboveLineStringEvent.OnUpdateString.AddListener(text => aboveLineText.text = text);
        aboveLineStringEvent.RefreshString();

        // 스토리 대사 설정 (타이핑)
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

    public void NextDialogue()
    {
        index++;
        ShowDialogue(index);
    }
}

