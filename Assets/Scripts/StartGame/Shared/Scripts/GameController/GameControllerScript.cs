using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
    [Header("패널들")]
    public GameObject FirstPanel;
    public GameObject SettingPanel;

    [Header("언어별 텍스트 박스 오브젝트 (RectTransform)")]
    public RectTransform Korean_AboveLine;
    public RectTransform Korean_Story;
    public RectTransform English_Above;
    public RectTransform English_Story;
    public RectTransform Japanese_Above;
    public RectTransform Japanese_Story;
    public RectTransform Chinese_Above;
    public RectTransform Chinese_Story;
    public RectTransform Kaza_Above;
    public RectTransform Kaza_Story;

    [Header("타이핑 제어")]
    public MonoBehaviour dialogueControllerToPause;

    [Header("텍스트 위치")]
    public Vector2 AboPo = new Vector2(-750f, 160f);
    public Vector2 StoPo = new Vector2(-250f, -20f);

    private string previousLang = "";

    private void Update()
    {
        // 1. 패널 열림 시 DialogueController 비활성화
        if (FirstPanel != null && FirstPanel.activeSelf && FirstPanel.transform.localPosition == Vector3.zero)
        {
            if (dialogueControllerToPause != null)
                dialogueControllerToPause.enabled = false;

            StopAllTypewriters();
            return;
        }
        else
        {
            if (dialogueControllerToPause != null)
                dialogueControllerToPause.enabled = true;

            ResumeAllTypewriters();
        }

        // 2. 언어 UI 항상 적용 (SettingPanel 열려 있을 때뿐 아니라 평소에도 적용)
        string currentLang = LanguageManager.GetLanguage()?.Trim().ToLower();

        if (currentLang != previousLang)
        {
            DisableAllLanguageTexts();
            ActivateLanguageByName(currentLang);
            previousLang = currentLang;
        }
    }

    private void ActivateLanguageByName(string lang)
    {
        switch (lang)
        {
            case "korean":
                ActivateLanguage(Korean_AboveLine, Korean_Story);
                break;
            case "english":
                ActivateLanguage(English_Above, English_Story);
                break;
            case "japanese":
                ActivateLanguage(Japanese_Above, Japanese_Story);
                break;
            case "chinese":
                ActivateLanguage(Chinese_Above, Chinese_Story);
                break;
            case "kazahustan":
            case "kaza":
                ActivateLanguage(Kaza_Above, Kaza_Story);
                break;
            default:
                Debug.LogWarning("[GameControllerScript] Unknown language: " + lang);
                break;
        }
    }

    private void ActivateLanguage(RectTransform above, RectTransform story)
    {
        if (above != null)
        {
            above.gameObject.SetActive(true);
            above.anchoredPosition = AboPo;
            EnableTypewriter(above);
        }

        if (story != null)
        {
            story.gameObject.SetActive(true);
            story.anchoredPosition = StoPo;
            EnableTypewriter(story);
        }
    }

    private void DisableAllLanguageTexts()
    {
        RectTransform[] all = {
            Korean_AboveLine, Korean_Story,
            English_Above, English_Story,
            Japanese_Above, Japanese_Story,
            Chinese_Above, Chinese_Story,
            Kaza_Above, Kaza_Story
        };

        foreach (var r in all)
            if (r != null) r.gameObject.SetActive(false);
    }

    private void EnableTypewriter(RectTransform obj)
    {
        var typewriter = obj.GetComponent<TypewriterEffect>();
        if (typewriter != null)
        {
            typewriter.StopTyping(); // 중단 후 리셋
            typewriter.enabled = false;
            typewriter.enabled = true;
        }
    }

    private void StopAllTypewriters()
    {
        foreach (var tw in FindObjectsOfType<TypewriterEffect>())
            tw.StopTyping();
    }

    private void ResumeAllTypewriters()
    {
        foreach (var tw in FindObjectsOfType<TypewriterEffect>())
        {
            tw.enabled = true;
        }
    }

    public void CarrotButtonClicked()
    {
        Debug.Log("🐰 Carrot button clicked!");

        if (FirstPanel == null)
        {
            Debug.LogError("❌ FirstPanel is NULL!");
            return;
        }

        FirstPanel.SetActive(true);
        FirstPanel.transform.localPosition = Vector3.zero;

        if (SettingPanel != null)
        {
            SettingPanel.SetActive(false);
        }
    }

    
    public void OnLanguageDropdownChanged(int index)
    {
        string selectedLang = ""; // 드롭다운 값에 따라

        switch (index)
        {
            case 0: selectedLang = "korean"; break;
            case 1: selectedLang = "english"; break;
            case 2: selectedLang = "japanese"; break;
            case 3: selectedLang = "chinese"; break;
            case 4: selectedLang = "kaza"; break;
        }

        LanguageManager.SetLanguage(selectedLang);
    }

    

}
