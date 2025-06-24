using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text storyText;
    public GameObject nextButton;
    public Image fadeImage;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip mainBGM;
    public AudioClip lightSFX;
    public AudioClip knockingDoor;
    public AudioClip roughlyOpen;

    
    [Header("Dialogue")]
    public LocalizedString[] dialogueLines = new LocalizedString[4];
    public Typewriter typewriter;

    private int dialogueIndex = 0;

    void Start()
    {
        SetBlackScreenFullyOpaque();

        // 🎵 메인 브금 시작 (loop)
        if (bgmSource != null && mainBGM != null)
        {
            bgmSource.clip = mainBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        if (sfxSource == null)
            Debug.LogError("❌ sfxSource가 연결되어 있지 않습니다!");
        if (lightSFX == null)
            Debug.LogError("❌ lightSFX 클립이 지정되지 않았습니다!");

        StartCoroutine(StartupSequence());
    }
    
    void Awake()
    {
        dialogueLines[0] = new LocalizedString { TableReference = "Stage1_1", TableEntryReference = "1" };
        dialogueLines[1] = new LocalizedString { TableReference = "Stage1_1", TableEntryReference = "2" };
        dialogueLines[2] = new LocalizedString { TableReference = "Stage1_1", TableEntryReference = "3" };
        dialogueLines[3] = new LocalizedString { TableReference = "Stage1_1", TableEntryReference = "4" };
    }

    void SetBlackScreenFullyOpaque()
    {
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
    }

    IEnumerator StartupSequence()
    {
        yield return StartCoroutine(TypeLocalized(dialogueLines[0]));
        nextButton.SetActive(true);
    }

    public void OnNextClicked()
    {
        Debug.Log("🔘 버튼 클릭됨!");
        nextButton.SetActive(false);

        if (dialogueIndex == 0)
        {
            Debug.Log("➡ Step1 진입");
            LogLocalizedStrings();
            StartCoroutine(HandleAfterStep1());
        }
        else if (dialogueIndex == 1)
        {
            Debug.Log("➡ Step2 진입");
            LogLocalizedStrings();
            StartCoroutine(HandleAfterStep2());
        }
        else if (dialogueIndex == 2)
        {
            Debug.Log("➡ Step3 진입");
            LogLocalizedStrings();
            StartCoroutine(HandleAfterStep3());
        }
        else if (dialogueIndex == 3)
        {
            Debug.Log("➡ Step4 진입: 마지막 처리");
            LogLocalizedStrings();
            StartCoroutine(HandleFinalStep());
        }

    }

    IEnumerator HandleAfterStep1()
    {
        // 이미 bgm 재생 중이므로 이 부분은 제거 가능
        // yield return new WaitForSeconds(2f); → 필요 없다면 제거 가능

        yield return StartCoroutine(FadeOutBlack());

        dialogueIndex = 1;
        yield return StartCoroutine(TypeLocalized(dialogueLines[1]));
        nextButton.SetActive(true);
    }

    IEnumerator HandleAfterStep2()
    {
        dialogueIndex = 2;

        // 🎵 mainBGM 일시 멈춤
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Pause();

        // 🔊 lightSFX 재생
        sfxSource.clip = lightSFX;
        sfxSource.loop = false;
        sfxSource.Play();

        yield return new WaitForSeconds(4f);
        sfxSource.Stop();
        sfxSource.clip = null;

        // 🎵 mainBGM 재개
        if (bgmSource != null && mainBGM != null)
            bgmSource.UnPause();

        yield return StartCoroutine(TypeLocalized(dialogueLines[2]));
        nextButton.SetActive(true);
    }


    IEnumerator HandleAfterStep3()
    {
        dialogueIndex = 3;
        yield return StartCoroutine(TypeLocalized(dialogueLines[3]));
        nextButton.SetActive(true);
    }

    IEnumerator TypeLocalized(LocalizedString loc)
    {
        var op = loc.GetLocalizedStringAsync();
        yield return op;
        yield return StartCoroutine(typewriter.Type(op.Result));
    }

    IEnumerator FadeOutBlack()
    {
        Color c = fadeImage.color;
        for (float f = 1f; f >= 0; f -= 0.02f)
        {
            c.a = f;
            fadeImage.color = c;
            yield return new WaitForSeconds(0.02f);
        }
        c.a = 0;
        fadeImage.color = c;
    }

    IEnumerator FadeInBlack()
    {
        Color c = fadeImage.color;
        for (float f = 0f; f <= 1f; f += 0.02f)
        {
            c.a = f;
            fadeImage.color = c;
            yield return new WaitForSeconds(0.02f);
        }
        c.a = 1;
        fadeImage.color = c;
    }
    
    IEnumerator HandleFinalStep()
    {
        dialogueIndex = 4; // 이건 더 이상 필요 없지만 혹시 모르니 유지

        // 1. 메인 브금 끄기
        if (bgmSource != null)
            bgmSource.Stop();

        // 2. 0.5초 대기 후 노킹
        yield return new WaitForSeconds(0.5f);
        if (knockingDoor != null)
            sfxSource.PlayOneShot(knockingDoor);

        // 3. 또 0.5초 대기
        yield return new WaitForSeconds(0.5f);
        if (roughlyOpen != null)
            sfxSource.PlayOneShot(roughlyOpen);

        // 4. 효과음 길이만큼 대기 후 씬 전환
        float delay = roughlyOpen != null ? roughlyOpen.length : 1f;
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("Stage1_2");


    }
    
    void LogLocalizedStrings()
    {
        foreach (var line in dialogueLines)
        {
            var op = line.GetLocalizedStringAsync();
            op.Completed += handle =>
            {
                Debug.Log($"🔤 Key: {line.TableEntryReference}, Text: {handle.Result}");
            };
        }
    }


}
