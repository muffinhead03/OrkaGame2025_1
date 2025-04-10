using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;

public class SettingsController : MonoBehaviour
{
    public GameObject mainSettingMenu;
    
    [Header("패널들")]
    public GameObject volumePanel;
    public GameObject languagePanel;
    public GameObject difficultyPanel;

    [Header("슬라이더")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Audio")]
    public AudioMixer mixer; // SFX용
    private const float volumeMax = 100f;
    
    void Start()
    {
        // ✅ 메인 버튼 메뉴 활성화 (버튼 3개 보이게)
        if (mainSettingMenu != null)
            mainSettingMenu.SetActive(true);

        // ❌ 각 설정 패널은 초기에는 숨김
        if (volumePanel != null)
            volumePanel.SetActive(false);

        if (languagePanel != null)
            languagePanel.SetActive(false);

        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);

        // 🎵 BGM 및 SFX 볼륨 불러오기
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 75f);

        bgmSlider.value = bgm;
        sfxSlider.value = sfx;

        if (BGMManager.Instance != null)
            BGMManager.Instance.SetVolume(bgm / volumeMax);

        mixer.SetFloat("SFXVolume", Mathf.Log10(sfx / volumeMax) * 20);

        // 🌍 언어 설정 불러오기
        int langIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langIndex];
    }

    // ▼ 패널 열기
    public void OpenVolumePanel() => TogglePanel(volumePanel);
    public void OpenLanguagePanel() => TogglePanel(languagePanel);
    public void OpenDifficultyPanel() => TogglePanel(difficultyPanel);

    public void CloseAllPanels()
    {
        mainSettingMenu.SetActive(false);
        volumePanel.SetActive(false);
        languagePanel.SetActive(false);
        difficultyPanel.SetActive(false);
    }

    void TogglePanel(GameObject panel)
    {
        CloseAllPanels();
        panel.SetActive(true);
    }

    // ▼ 슬라이더 볼륨 제어
    public void SetBGMVolume(float val)
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.SetVolume(val / volumeMax);
        PlayerPrefs.SetFloat("BGMVolume", val);
    }

    public void SetSFXVolume(float val)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(val / volumeMax) * 20);
        PlayerPrefs.SetFloat("SFXVolume", val);
    }

    // ▼ 언어 선택 버튼 (0~3)
    public void SetLanguage(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt("LanguageIndex", index);
    }

    // ▼ 각 패널 닫기 버튼 전용
    public void CloseVolumePanel()
    {
        volumePanel.SetActive(false);
        mainSettingMenu.SetActive(true);  // 🔥 돌아가기
    }

    public void CloseLanguagePanel()
    {
        languagePanel.SetActive(false);
        mainSettingMenu.SetActive(true);  // 🔥 돌아가기
    }

    public void CloseDifficultyPanel()
    {
        difficultyPanel.SetActive(false);
        mainSettingMenu.SetActive(true);  // 🔥 돌아가기
    }

}
