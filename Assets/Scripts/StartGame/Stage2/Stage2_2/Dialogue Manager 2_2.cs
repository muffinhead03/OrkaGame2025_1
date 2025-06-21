using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DialogueManager2_2 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;   // Stage2_2Above용
    public LocalizeStringEvent storyLineStringEvent;   // Stage2_2용
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;
    public Button nextButton;

    [Header("Character Images")]
    public GameObject narkeImage;
    public GameObject echoImage;

    private int index = 0;
    private string storyTableName = "Stage2_2";
    private string aboveTableName = "Stage2_2Above";

    private string[] storyKeys = {
        "key2_2_1", "key2_2_2", "key2_2_3", "key2_2_4", "key2_2_5",
        "key2_2_6", "key2_2_7", "key2_2_8", "key2_2_9", "key2_2_10"
    };

    // 위 대사의 화자가 나르케인 경우
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

        // 위 대사 설정
        aboveLineStringEvent.StringReference.SetReference(
            aboveTableName,
            isNarke ? "key2_2_above_narke" : "key2_2_above_echo"
        );

        // 스토리 대사 설정
        storyLineStringEvent.StringReference.SetReference(
            storyTableName,
            storyKeys[i]
        );

        // 캐릭터 이미지 표시
        narkeImage.SetActive(isNarke);
        echoImage.SetActive(!isNarke);

        // 위 대사 바인딩
        aboveLineStringEvent.OnUpdateString.RemoveAllListeners();
        aboveLineStringEvent.OnUpdateString.AddListener(text => aboveLineText.text = text);

        // 스토리 대사 바인딩
        storyLineStringEvent.OnUpdateString.RemoveAllListeners();
        storyLineStringEvent.OnUpdateString.AddListener(OnStoryLineReady);

        // 텍스트 갱신
        aboveLineStringEvent.RefreshString();
        storyLineStringEvent.RefreshString();
    }

    private void OnStoryLineReady(string localizedText)
    {
        storyLineText.text = localizedText;
        StartCoroutine(ShowNextButtonDelayed());
    }

    private IEnumerator ShowNextButtonDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        nextButton.gameObject.SetActive(true);
    }

    public void NextDialogue()
    {
        index++;
        ShowDialogue(index);
    }
}