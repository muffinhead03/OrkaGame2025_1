using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager2_7 : MonoBehaviour
{
    [Header("UI Components")]
    public LocalizeStringEvent aboveLineStringEvent;
    public LocalizeStringEvent storyLineStringEvent;
    public TMP_Text aboveLineText;
    public TMP_Text storyLineText;

    [Header("Audio")]
    public AudioSource bgmAudioSource;  // 배경음
    public AudioSource hukAudioSource;  // 헉 소리

    [Header("GameObject")]
    public GameObject gameOverImage;

    private string storyTableName = "Stage2_7StoryLine";
    private string aboveTableName = "Stage2_7AboveLine";

    private void Start()
    {
        gameOverImage.SetActive(false);

        aboveLineStringEvent.StringReference.SetReference(aboveTableName, "key1");
        storyLineStringEvent.StringReference.SetReference(storyTableName, "key2_7_1");

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
        StartCoroutine(TypeTextAndWaitForBGM(localizedText));
    }

    private IEnumerator TypeTextAndWaitForBGM(string fullText)
    {
        storyLineText.text = "";
        foreach (char c in fullText)
        {
            storyLineText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        // 대사가 다 나오고 나면 배경음이 재생되고 있을 동안 대기
        yield return new WaitUntil(() => !bgmAudioSource.isPlaying);

        // 배경음이 끝나면 GameOver 이미지와 헉 소리 출력
        gameOverImage.SetActive(true);
        hukAudioSource.Play();
    }
}
