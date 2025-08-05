using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CarrotShaker : MonoBehaviour
{
    public bool isDialoguePlaying = true;
    private bool isShaking = false;

    public void EndDialogue() => isDialoguePlaying = false;

    public void ShakeCarrot() // 👉 이걸 Button.OnClick()에 연결하세요
    {
        if (isDialoguePlaying || isShaking) return;
        StartCoroutine(ShakeThenLoadScene());
    }

    IEnumerator ShakeThenLoadScene()
    {
        isShaking = true;

        yield return ShakeZ(20f, 0.5f);
        yield return ShakeZ(-40f, 1.0f);
        yield return ShakeZ(40f, 1.0f);
        yield return ShakeZ(-40f, 1.0f);

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        isShaking = false;

        SceneManager.LoadScene("Stage2_3");
    }

    IEnumerator ShakeZ(float angleDelta, float duration)
    {
        float startZ = transform.eulerAngles.z;
        float endZ = startZ + angleDelta;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float z = Mathf.Lerp(startZ, endZ, elapsed / duration);
            transform.rotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, endZ);
    }
}
