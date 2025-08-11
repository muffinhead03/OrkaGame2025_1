using UnityEngine;

public static class GameAudioSettings
{
    // 0~100 (초기 50)
    public static int CurrentBackgroundMusic = 50;
    public static int CurrentSoundEffect     = 50;

    // 0.0~1.0
    public static float Bgm01 => Mathf.Clamp01(CurrentBackgroundMusic / 100f);
    public static float Sfx01 => Mathf.Clamp01(CurrentSoundEffect     / 100f);
}