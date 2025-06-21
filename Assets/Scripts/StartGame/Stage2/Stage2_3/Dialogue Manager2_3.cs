using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_3 : MonoBehaviour
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
    private string storyTableName = "Stage2_3";
    private string aboveTableName = "Stage2_2Above_Line";

    private string[] storyKeys = {
        "key2_3_1", "key2_3_2", "key2_3_3", "key2_3_4", "key2_3_5",
        "key2_3_6", "key2_3_7", "key2_3_8", "key2_3_9", "key2_3_10"
    };

    private int[] useKey1AboveIndexes = { 0, 2, 3, 5, 6, 7, 9 };

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
            SceneManager.LoadScene("Stage2_4");
            return;
        }

        nextButton.gameObject.SetActive(false);

        bool isCat = System.Array.Exists(useKey1AboveIndexes, n => n == i);
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
        StopAllCoroutines(); // 중복 방지
        StartCoroutine(TypeText(localizedText));
    }

    private IEnumerator TypeText(string fullText)
    {
        storyLineText.text = "";
        foreach (char c in fullText)
        {
            storyLineText.text += c;
            yield return new WaitForSeconds(0.03f); // 타자 속도 조절
        }

        yield return new WaitForSeconds(0.5f); // 다 나온 후 버튼 등장
        nextButton.gameObject.SetActive(true);
    }

    public void NextDialogue()
    {
        index++;
        ShowDialogue(index);
    }
}
