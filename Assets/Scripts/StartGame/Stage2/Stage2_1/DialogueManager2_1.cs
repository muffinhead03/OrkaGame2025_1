using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueManagerStage2_1 : MonoBehaviour
{
    [Header("언어 오브젝트")]
    public RectTransform Korean_Above, Korean_Story;
    public RectTransform English_Above, English_Story;
    public RectTransform Japanese_Above, Japanese_Story;
    public RectTransform Chinese_Above, Chinese_Story;
    public RectTransform Kaza_Above, Kaza_Story;

    [Header("UI 요소")]
    public GameObject nextButton;

    [Header("타이핑 및 텍스트 처리")]
    public TypewriterEffect typewriterEffect;
    public LanguageCollector2_1 languageCollector;

    [Header("캐릭터 이미지 관리")]
    public Image characterImage;
    public Sprite[] echoSprites; // arc_echo_default, arc_echo_3eyeclosed 등 Inspector에서 등록

    [Header("오디오")]
    public AudioSource[] audioSources; // 여러 사운드를 동시에 재생할 용도 (복수 AudioSource를 Array로)

    [System.Serializable]
    public class DialogueLineInfo
    {
        public string characterSprite;
        public AudioClip[] audioClips; // 한 대사(한 행)에 여러 사운드를 등록할 수 있음
        public string 기타;
    }
    public List<DialogueLineInfo> dialogueInfoList = new List<DialogueLineInfo>();

    private string[] currentLines;
    private int lineIndex = 0;

    private void Start()
    {
        SetupLanguageUI();
        currentLines = languageCollector.GetLines();
        StartCoroutine(ShowCurrentLine());
    }

    public void OnNextButtonClicked()
    {
        nextButton.SetActive(false);
        lineIndex++;
        currentLines = languageCollector.GetLines();
        if (lineIndex >= currentLines.Length)
            return;
        StartCoroutine(ShowCurrentLine());
    }

    private IEnumerator ShowCurrentLine()
    {
        SetCharacterImage(lineIndex);
        PlayAllAudioClips(lineIndex);
        typewriterEffect.SetText(currentLines[lineIndex]);
        yield return new WaitUntil(() => typewriterEffect.IsComplete);
        nextButton.SetActive(true);
    }

    private void SetCharacterImage(int idx)
    {
        if (dialogueInfoList == null || idx >= dialogueInfoList.Count) return;
        var info = dialogueInfoList[idx];
        if (!string.IsNullOrEmpty(info.characterSprite))
        {
            foreach (var sprite in echoSprites)
            {
                if (sprite.name == info.characterSprite)
                {
                    characterImage.sprite = sprite;
                    break;
                }
            }
        }
    }

    // ★ 한 대사(행)마다 여러 소리가 동시에 나오도록
    private void PlayAllAudioClips(int idx)
    {
        if (dialogueInfoList == null || idx >= dialogueInfoList.Count) return;
        var info = dialogueInfoList[idx];

        // 이전에 재생 중인 모든 소리 정지
        foreach (var source in audioSources)
            source.Stop();

        if (info.audioClips != null && info.audioClips.Length > 0)
        {
            int cnt = Mathf.Min(audioSources.Length, info.audioClips.Length);
            for (int i = 0; i < cnt; i++)
            {
                audioSources[i].clip = info.audioClips[i];
                audioSources[i].Play();
            }
        }
    }

    // 언어 UI 세팅 등 이하 동일
    private void SetupLanguageUI()
    {
        string lang = LanguageManager.GetLanguage();
        DisableAllLangObjects();
        switch (lang)
        {
            case "korean":
                SetActiveAndPosition(Korean_Above, Korean_Story);
                break;
            case "english":
                SetActiveAndPosition(English_Above, English_Story);
                break;
            case "japanese":
                SetActiveAndPosition(Japanese_Above, Japanese_Story);
                break;
            case "chinese":
                SetActiveAndPosition(Chinese_Above, Chinese_Story);
                break;
            case "kazahustan":
            case "kaza":
                SetActiveAndPosition(Kaza_Above, Kaza_Story);
                break;
        }
    }
    private void SetActiveAndPosition(RectTransform above, RectTransform story)
    {
        above.gameObject.SetActive(true);
        story.gameObject.SetActive(true);
    }
    private void DisableAllLangObjects()
    {
        Korean_Above?.gameObject.SetActive(false);
        Korean_Story?.gameObject.SetActive(false);
        English_Above?.gameObject.SetActive(false);
        English_Story?.gameObject.SetActive(false);
        Japanese_Above?.gameObject.SetActive(false);
        Japanese_Story?.gameObject.SetActive(false);
        Chinese_Above?.gameObject.SetActive(false);
        Chinese_Story?.gameObject.SetActive(false);
        Kaza_Above?.gameObject.SetActive(false);
        Kaza_Story?.gameObject.SetActive(false);
    }
}

