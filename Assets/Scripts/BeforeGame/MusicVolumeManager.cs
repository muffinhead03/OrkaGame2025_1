using UnityEngine;

public class MusicVolumeManager : MonoBehaviour
{
    // 0 ~ 100 범위, 0 = 무음, 100 = 최대
    public static int currentBackgroundVolume = 50;
    public static int currentSoundEffectVolume = 50;

    // 현재 값을 0.0 ~ 1.0 범위(float)로 변환해서 반환
    public static float BackgroundVolume01 => Mathf.Clamp01(currentBackgroundVolume / 100f);
    public static float SoundEffectVolume01 => Mathf.Clamp01(currentSoundEffectVolume / 100f);

    /// <summary>
    /// 배경음악 볼륨을 변경 (0 ~ 100 범위 자동 제한)
    /// </summary>
    public static void SetBackgroundVolume(int value)
    {
        currentBackgroundVolume = Mathf.Clamp(value, 0, 100);
    }

    /// <summary>
    /// 사운드 효과 볼륨을 변경 (0 ~ 100 범위 자동 제한)
    /// </summary>
    public static void SetSoundEffectVolume(int value)
    {
        currentSoundEffectVolume = Mathf.Clamp(value, 0, 100);
    }
}