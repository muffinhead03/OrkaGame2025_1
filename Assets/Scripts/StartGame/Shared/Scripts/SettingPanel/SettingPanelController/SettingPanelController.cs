using UnityEngine;
using TMPro;

public class SettingPanelController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;
    public GameObject firstPanel;

    public Vector3 settingPanelOrigin = new Vector3(-3100, 727, 0);
    public Vector3 center = Vector3.zero;

    [Header("Display (TMP)")]
    public TextMeshProUGUI bgmValueText;  // "BGM: 50" 같은 표시
    public TextMeshProUGUI sfxValueText;  // "SFX: 50" 같은 표시

    [Header("Audio Groups (배열에 AudioSource들 넣기)")]
    public AudioSource[] backgroundMusicSources;
    public AudioSource[] soundEffectSources;

    [Header("Hold Adjust Options")]
    [Tooltip("마우스를 누른 채 유지 시 초당 변화량")]
    public float unitsPerSecond = 7f;

    // 내부 상태 (홀드 여부)
    private bool holdBgmLeft,  holdBgmRight;
    private bool holdSfxLeft,  holdSfxRight;

    // 각 소스의 '기본(베이스) 볼륨' 저장해서 스케일링
    private float[] bgmBaseVolumes;
    private float[] sfxBaseVolumes;

    void Awake()
    {
        // 기본값 보정(최초 실행 시 50으로)
        GameAudioSettings.CurrentBackgroundMusic = Mathf.Clamp(GameAudioSettings.CurrentBackgroundMusic, 0, 100);
        GameAudioSettings.CurrentSoundEffect     = Mathf.Clamp(GameAudioSettings.CurrentSoundEffect,     0, 100);

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

        // 첫 반영
        ApplyVolumes();
        RefreshTexts();
    }

    void Update()
    {
        float delta = unitsPerSecond * Time.unscaledDeltaTime; // 일시정지 무시

        bool changed = false;

        // BGM 조절
        if (holdBgmLeft || holdBgmRight)
        {
            int v = GameAudioSettings.CurrentBackgroundMusic;
            if (holdBgmLeft)  v -= Mathf.CeilToInt(delta);
            if (holdBgmRight) v += Mathf.CeilToInt(delta);
            int clamped = Mathf.Clamp(v, 0, 100);
            if (clamped != GameAudioSettings.CurrentBackgroundMusic)
            {
                GameAudioSettings.CurrentBackgroundMusic = clamped;
                changed = true;
            }
        }

        // SFX 조절
        if (holdSfxLeft || holdSfxRight)
        {
            int v = GameAudioSettings.CurrentSoundEffect;
            if (holdSfxLeft)  v -= Mathf.CeilToInt(delta);
            if (holdSfxRight) v += Mathf.CeilToInt(delta);
            int clamped = Mathf.Clamp(v, 0, 100);
            if (clamped != GameAudioSettings.CurrentSoundEffect)
            {
                GameAudioSettings.CurrentSoundEffect = clamped;
                changed = true;
            }
        }

        if (changed)
        {
            ApplyVolumes();
            RefreshTexts();
        }
    }

    // ===== UI Hook: 버튼/이벤트 트리거에서 호출 =====
    // --- BGM ---
    public void OnBgmArrowLeftDown()  { holdBgmLeft  = true;  }
    public void OnBgmArrowLeftUp()    { holdBgmLeft  = false; }
    public void OnBgmArrowRightDown() { holdBgmRight = true;  }
    public void OnBgmArrowRightUp()   { holdBgmRight = false; }

    // --- SFX ---
    public void OnSfxArrowLeftDown()  { holdSfxLeft  = true;  }
    public void OnSfxArrowLeftUp()    { holdSfxLeft  = false; }
    public void OnSfxArrowRightDown() { holdSfxRight = true;  }
    public void OnSfxArrowRightUp()   { holdSfxRight = false; }

    // 단발 클릭(+/- 1씩)도 원하면 아래 두 함수 쓰면 됨
    public void NudgeBgm(int delta)
    {
        GameAudioSettings.CurrentBackgroundMusic = Mathf.Clamp(GameAudioSettings.CurrentBackgroundMusic + delta, 0, 100);
        ApplyVolumes(); RefreshTexts();
    }
    public void NudgeSfx(int delta)
    {
        GameAudioSettings.CurrentSoundEffect = Mathf.Clamp(GameAudioSettings.CurrentSoundEffect + delta, 0, 100);
        ApplyVolumes(); RefreshTexts();
    }

    // ===== 볼륨 적용/표시 =====
    private void ApplyVolumes()
    {
        float b = GameAudioSettings.Bgm01;
        float s = GameAudioSettings.Sfx01;

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
            bgmValueText.text = $"BGM: {GameAudioSettings.CurrentBackgroundMusic:0}";
        if (sfxValueText)
            sfxValueText.text = $"SFX: {GameAudioSettings.CurrentSoundEffect:0}";
    }

    // ===== 패널 닫기 =====
    public void OnCloseSetting()
    {
        if (settingPanel)
        {
            settingPanel.transform.localPosition = settingPanelOrigin;
            settingPanel.SetActive(false);
        }
        if (firstPanel)
        {
            firstPanel.SetActive(true);
            firstPanel.transform.localPosition = center;
        }
    }
}
