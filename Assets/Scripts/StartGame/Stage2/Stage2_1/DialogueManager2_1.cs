using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public AudioSource grassAudio;

    private bool isTyping = false;
    private int index = 0;
    private string tableName = "Stage2_1";

    private string[] keySuffixes = {
        "key2_1_1", "key2_1_2", "key2_1_3", "key2_1_4",
        "key2_1_5", "key2_1_6", "key2_1_7", "key2_1_8", "key2_1_9"
    };

    private void Start()
    {
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(NextDialogue);
        
        
        localizedStringEvent.OnUpdateString.RemoveAllListeners();
        localizedStringEvent.OnUpdateString.AddListener(OnLocalizedStringReady);
        StartCoroutine(HandleDialogue(index));
    }

    private IEnumerator HandleDialogue(int i)
    {
        nextButton.gameObject.SetActive(false);
        targetText.text = "";

        if (i >= keySuffixes.Length)
        {
            SceneManager.LoadScene("Stage2_2");
            yield break;
        }

        // 특별한 사운드 시퀀스 처리
        if (i == 0)
        {
            if (birdAudio) birdAudio.Play();
            yield return new WaitForSeconds(0.5f);
            if (waterAudio) waterAudio.Play();
            yield return new WaitForSeconds(0.5f);
        }
        else if (i == 3) // 4번째 대사(key2_1_4) 전에 풀숲 소리
        {
            if (grassAudio) grassAudio.Play();
            yield return new WaitForSeconds(0.5f);
        }

        ShowDialogue(i);
    }

    private void ShowDialogue(int i)
    {
        string key = keySuffixes[i];
        Debug.Log($"[SetReference] Trying to use key: {key}"); // 추가
        localizedStringEvent.StringReference.SetReference(tableName, key);
        localizedStringEvent.RefreshString();
    }



    private void OnLocalizedStringReady(string localizedText)
    {
        if (isTyping) return; // 중복 방지
        Debug.Log("[Localized] " + localizedText);
        StartCoroutine(StartTypingCoroutine(localizedText));
    }



    private IEnumerator StartTypingCoroutine(string fullText)
    {
        isTyping = true;
        yield return StartCoroutine(typeWriter.Type(fullText));
        yield return new WaitForSeconds(0.5f);
        nextButton.gameObject.SetActive(true);
        isTyping = false;
    }

    private void NextDialogue()
    {
        index++;
        StartCoroutine(HandleDialogue(index));
    }

}
