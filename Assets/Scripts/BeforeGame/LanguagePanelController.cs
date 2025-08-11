using UnityEngine;
using TMPro;

public class LanguagePanelController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject languagePanel; // LanguagePanel GameObject
    [SerializeField] private TextMeshProUGUI currentLanguageText; // 현재 언어 표시용 TMP

    [Header("언어 리스트")]
    private readonly string[] languages = { "korean", "english", "chinese", "japanese", "kazakh" };

    private int currentIndex = 0;

    private void Start()
    {
        // LanguageManager에서 현재 언어 읽기
        string currentLang = LanguageManager.GetLanguage();
        currentIndex = System.Array.IndexOf(languages, currentLang);
        if (currentIndex == -1) currentIndex = 0; // 못 찾으면 0(한국어)로

        UpdateLanguageDisplay();
    }

    /// <summary>
    /// LanguagePanel 닫기 버튼
    /// </summary>
    public void OnTurnOffButton()
    {
        if (languagePanel != null)
            languagePanel.SetActive(false);
    }

    /// <summary>
    /// 왼쪽 화살표: 이전 언어로 변경
    /// </summary>
    public void OnArrowLeft()
    {
        currentIndex = (currentIndex - 1 + languages.Length) % languages.Length;
        ApplyLanguageChange();
    }

    /// <summary>
    /// 오른쪽 화살표: 다음 언어로 변경
    /// </summary>
    public void OnArrowRight()
    {
        currentIndex = (currentIndex + 1) % languages.Length;
        ApplyLanguageChange();
    }

    /// <summary>
    /// 실제 언어 변경 처리
    /// </summary>
    private void ApplyLanguageChange()
    {
        LanguageManager.SetLanguage(languages[currentIndex]);
        UpdateLanguageDisplay();
    }

    /// <summary>
    /// TMP에 현재 언어 표시
    /// </summary>
    private void UpdateLanguageDisplay()
    {
        if (currentLanguageText != null)
            currentLanguageText.text = char.ToUpper(languages[currentIndex][0]) + languages[currentIndex].Substring(1);
    }

}