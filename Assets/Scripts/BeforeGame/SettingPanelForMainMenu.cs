using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPanelForMainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;

    [Header("Hold Options")]
    public float unitsPerSecond = 7f;

    [Tooltip("길게 누른 것으로 판정되는 시간(초)")]
    public float longPressThreshold = 0.5f;

    public Vector3 settingPanelOrigin = new Vector3(-3100, 727, 0);
    public Vector3 center = Vector3.zero;

    [Header("Display (TMP)")]
    public TextMeshProUGUI bgmValueText;  // "BGM: 50"
    public TextMeshProUGUI sfxValueText;  // "SFX: 50"

    [Header("Audio Groups")]
    public AudioSource[] backgroundMusicSources;
    public AudioSource[] soundEffectSources;

    // 내부(베이스 볼륨)
    private float[] bgmBaseVolumes;
    private float[] sfxBaseVolumes;

    // -------- BGM 상태 --------
    private bool holdingBgm = false;
    private int dirBgm = 0;              
    private float bgmHoldStartTime = 0f;   
    private bool bgmThreshold = false;    
    private float bgmAccum = 0f;           

    // -------- SFX 상태 --------
    private bool holdingSfx = false;
    private int dirSfx = 0;
    private float sfxHoldStartTime = 0f;
    private bool sfxThreshold = false;
    private float sfxAccum = 0f;
// ===== 패널 열기 =====
    public void OnOpenSetting()
    {
        if (!settingPanel) return;

        // 패널 활성화 & 위치 중앙으로
        settingPanel.SetActive(true);
        settingPanel.transform.localPosition = center;

        // 최신 볼륨 값 UI/오디오에 반영
        ApplyVolumes();
        RefreshTexts();

        // 가장 위로 가져오기(선택)
        settingPanel.transform.SetAsLastSibling();
    }

    void Awake()
    {
        // 베이스 볼륨 백업
        if (backgroundMusicSources != null && backgroundMusicSources.Length > 0)
        {
            bgmBaseVolumes = new float[backgroundMusicSources.Length];
            for (int i = 0; i < backgroundMusicSources.Length; i++)
                bgmBaseVolumes[i] = backgroundMusicSources[i] ? backgroundMusicSources[i].volume : 1f;
        }
        if (soundEffectSources != null && soundEffectSources.Length > 0)
        {
            sfxBaseVolumes = new float[soundEffectSources.Length];
            for (int i = 0; i < soundEffectSources.Length; i++)
                sfxBaseVolumes[i] = soundEffectSources[i] ? soundEffectSources[i].volume : 1f;
        }

        // 범위 보정
        MusicVolumeManager.SetBackgroundVolume(MusicVolumeManager.currentBackgroundVolume);
        MusicVolumeManager.SetSoundEffectVolume(MusicVolumeManager.currentSoundEffectVolume);

        ApplyVolumes();
        RefreshTexts();
    }

    void Update()
    {
        bool changed = false;
        float dt = Time.unscaledDeltaTime;

        // ---------- BGM ----------
        if (holdingBgm)
        {
            float elapsed = Time.unscaledTime - bgmHoldStartTime;

            if (!bgmThreshold && elapsed >= longPressThreshold)
            {
                bgmThreshold = true;
                bgmAccum = 0f;
            }

            if (bgmThreshold)
            {
                bgmAccum += unitsPerSecond * dt;
                int steps = Mathf.FloorToInt(bgmAccum);
                if (steps > 0)
                {
                    int v = MusicVolumeManager.currentBackgroundVolume + dirBgm * steps;
                    v = Mathf.Clamp(v, 0, 100);
                    if (v != MusicVolumeManager.currentBackgroundVolume)
                    {
                        MusicVolumeManager.SetBackgroundVolume(v);
                        changed = true;
                    }
                    bgmAccum -= steps;
                }
            }
        }

        // ---------- SFX ----------
        if (holdingSfx)
        {
            float elapsed = Time.unscaledTime - sfxHoldStartTime;

            if (!sfxThreshold && elapsed >= longPressThreshold)
            {
                sfxThreshold = true;
                sfxAccum = 0f;
            }

            if (sfxThreshold)
            {
                sfxAccum += unitsPerSecond * dt;
                int steps = Mathf.FloorToInt(sfxAccum);
                if (steps > 0)
                {
                    int v = MusicVolumeManager.currentSoundEffectVolume + dirSfx * steps;
                    v = Mathf.Clamp(v, 0, 100);
                    if (v != MusicVolumeManager.currentSoundEffectVolume)
                    {
                        MusicVolumeManager.SetSoundEffectVolume(v);
                        changed = true;
                    }
                    sfxAccum -= steps;
                }
            }
        }

        if (changed)
        {
            ApplyVolumes();
            RefreshTexts();
        }
    }

    // ====== BGM: 포인터 다운/업 ======
    public void OnBgmArrowLeftDown()  { StartHoldBgm(-1); }
    public void OnBgmArrowRightDown() { StartHoldBgm(+1); }
    public void OnBgmArrowLeftUp()    { EndHoldBgm();     }
    public void OnBgmArrowRightUp()   { EndHoldBgm();     }

    private void StartHoldBgm(int dir)
    {
        holdingBgm = true;
        dirBgm = dir;
        bgmHoldStartTime = Time.unscaledTime;
        bgmThreshold = false;
        bgmAccum = 0f;
    }

    private void EndHoldBgm()
    {
        if (holdingBgm)
        {
            float elapsed = Time.unscaledTime - bgmHoldStartTime;

            if (!bgmThreshold && elapsed < longPressThreshold)
            {
                int v = MusicVolumeManager.currentBackgroundVolume + dirBgm * 1;
                v = Mathf.Clamp(v, 0, 100);
                MusicVolumeManager.SetBackgroundVolume(v);
                ApplyVolumes();
                RefreshTexts();
            }
        }

        holdingBgm = false;
        bgmThreshold = false;
        bgmAccum = 0f;
        dirBgm = 0;
    }

    // ====== SFX: 포인터 다운/업 ======
    public void OnSfxArrowLeftDown()  { StartHoldSfx(-1); }
    public void OnSfxArrowRightDown() { StartHoldSfx(+1); }
    public void OnSfxArrowLeftUp()    { EndHoldSfx();     }
    public void OnSfxArrowRightUp()   { EndHoldSfx();     }

    private void StartHoldSfx(int dir)
    {
        holdingSfx = true;
        dirSfx = dir;
        sfxHoldStartTime = Time.unscaledTime;
        sfxThreshold = false;
        sfxAccum = 0f;
    }

    private void EndHoldSfx()
    {
        if (holdingSfx)
        {
            float elapsed = Time.unscaledTime - sfxHoldStartTime;

            if (!sfxThreshold && elapsed < longPressThreshold)
            {
                int v = MusicVolumeManager.currentSoundEffectVolume + dirSfx * 1;
                v = Mathf.Clamp(v, 0, 100);
                MusicVolumeManager.SetSoundEffectVolume(v);
                ApplyVolumes();
                RefreshTexts();
            }
        }

        holdingSfx = false;
        sfxThreshold = false;
        sfxAccum = 0f;
        dirSfx = 0;
    }

    // ===== 볼륨 적용/표시 =====
    private void ApplyVolumes()
    {
        float b = MusicVolumeManager.BackgroundVolume01;
        float s = MusicVolumeManager.SoundEffectVolume01;

        if (backgroundMusicSources != null)
        {
            for (int i = 0; i < backgroundMusicSources.Length; i++)
            {
                var src = backgroundMusicSources[i];
                if (!src) continue;
                float baseVol = (bgmBaseVolumes != null && i < bgmBaseVolumes.Length) ? bgmBaseVolumes[i] : 1f;
                src.volume = baseVol * b;
            }
        }
        if (soundEffectSources != null)
        {
            for (int i = 0; i < soundEffectSources.Length; i++)
            {
                var src = soundEffectSources[i];
                if (!src) continue;
                float baseVol = (sfxBaseVolumes != null && i < sfxBaseVolumes.Length) ? sfxBaseVolumes[i] : 1f;
                src.volume = baseVol * s;
            }
        }
    }

    private void RefreshTexts()
    {
        if (bgmValueText)
            bgmValueText.text = MusicVolumeManager.currentBackgroundVolume.ToString();

        if (sfxValueText)
            sfxValueText.text = MusicVolumeManager.currentSoundEffectVolume.ToString();
    }

    // ===== 패널 닫기 =====
    public void OnCloseSetting()
    {
        if (settingPanel)
        {
            settingPanel.transform.localPosition = settingPanelOrigin;
            settingPanel.SetActive(false);
        }
    }
}
