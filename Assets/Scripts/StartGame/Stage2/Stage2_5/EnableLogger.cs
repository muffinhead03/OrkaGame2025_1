using UnityEngine;

public class EnableLogger : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log($"[EnableLogger] {name} ENABLED at {Time.frameCount}\n{System.Environment.StackTrace}");
    }
    void OnDisable()
    {
        Debug.Log($"[EnableLogger] {name} DISABLED at {Time.frameCount}");
    }
}