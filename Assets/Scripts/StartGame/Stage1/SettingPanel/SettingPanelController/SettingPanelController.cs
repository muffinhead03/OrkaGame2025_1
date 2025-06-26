using UnityEngine;
using TMPro;

public class SettingPanelController : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject firstPanel;

    public Vector3 settingPanelOrigin = new Vector3(-3100, 727, 0);
    public Vector3 center = Vector3.zero;

    public TextMeshProUGUI whatLanguage;    // 현재 언어 표시용 TMP

    private Vector2[] laPosition = new Vector2[]
    {
        new Vector2(23, -275),
        new Vector2(700, -1000)
    };

    private readonly string[] laKind = { "Korean", "English", "Chinese", "Japanese", "Kazahustan" };
    private int currentIndex = 0;

    private void Start()
    {
        // LanguageManager에서 현재 언어 읽기
        string currentLang = LanguageManager.GetLanguage();
        currentIndex = System.Array.IndexOf(laKind, currentLang);
        if (currentIndex == -1) currentIndex = 0;

        UpdateLanguageDisplay();
    }

    public void OnArrowLeft()
    {
        currentIndex = (currentIndex - 1 + laKind.Length) % laKind.Length;
        LanguageManager.SetLanguage(laKind[currentIndex]);
        UpdateLanguageDisplay();
    }

    public void OnArrowRight()
    {
        currentIndex = (currentIndex + 1) % laKind.Length;
        LanguageManager.SetLanguage(laKind[currentIndex]);
        UpdateLanguageDisplay();
    }

    private void UpdateLanguageDisplay()
    {
        // 모든 언어 텍스트 비활성화
        

        // LanguageManager에서 현재 언어 "읽기"
// 기존에는 languageTexts[i]를 다루는 시각 요소도 같이 보여주고 있었지만
// now, 단순히 글자 "Korean" 같은 것만 출력하면 됩니다.

// 이 부분만 정리하면 됨:
        if (whatLanguage != null)
        {
            whatLanguage.text = $" {LanguageManager.GetLanguage()}";
        }

    }

    public void OnCloseSetting()
    {
        settingPanel.transform.localPosition = settingPanelOrigin;
        settingPanel.SetActive(false);

        firstPanel.SetActive(true);
        firstPanel.transform.localPosition = center;
    }
}
