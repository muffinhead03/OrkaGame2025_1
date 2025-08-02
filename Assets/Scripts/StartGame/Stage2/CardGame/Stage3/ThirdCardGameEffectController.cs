using UnityEngine;

public class ThirdCardGameEffectController : MonoBehaviour
{
    public static ThirdCardGameEffectController Instance;

    public bool isEffectPlaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}