using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    [Header("언어별 텍스트 TMP")]
    public TextMeshProUGUI koreanText;
    public TextMeshProUGUI englishText;
    public TextMeshProUGUI japaneseText;
    public TextMeshProUGUI chineseText;
    public TextMeshProUGUI kazaText;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.04f;

    private TextMeshProUGUI activeText;
    private string fullText;
    private Coroutine typingCoroutine;

    public bool IsComplete { get; private set; } = true;
    public Action onTypingComplete;

    private void Awake()
    {
        SetActiveTextByLanguage();
    }

    private void SetActiveTextByLanguage()
    {
        if (koreanText != null) koreanText.gameObject.SetActive(false);
        if (englishText != null) englishText.gameObject.SetActive(false);
        if (japaneseText != null) japaneseText.gameObject.SetActive(false);
        if (chineseText != null) chineseText.gameObject.SetActive(false);
        if (kazaText != null) kazaText.gameObject.SetActive(false);

        string lang = LanguageManager.GetLanguage()?.Trim().ToLower();

        switch (lang)
        {
            case "korean": activeText = koreanText; break;
            case "english": activeText = englishText ?? koreanText; break;
            case "japanese": activeText = japaneseText ?? koreanText; break;
            case "chinese": activeText = chineseText ?? koreanText; break;
            case "kazahustan":
            case "kaza": activeText = kazaText ?? koreanText; break;
            default: activeText = koreanText; break;
        }

        if (activeText != null)
            activeText.gameObject.SetActive(true);
    }

    public void SetText(string newText, float speed = -1f)
    {
        SetActiveTextByLanguage();
        fullText = newText;
        if (speed > 0f) typingSpeed = speed;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void StopTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (activeText != null)
            activeText.text = fullText;

        IsComplete = true;
        onTypingComplete?.Invoke();
    }

    private IEnumerator TypeText()
    {
        IsComplete = false;
        if (activeText == null)
        {
            Debug.LogWarning("[TypewriterEffect] 활성화된 텍스트 오브젝트가 없습니다.");
            yield break;
        }

        activeText.text = "";
        foreach (char c in fullText)
        {
            activeText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        IsComplete = true;
        onTypingComplete?.Invoke();
    }
}
