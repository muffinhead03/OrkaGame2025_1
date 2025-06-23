using UnityEngine;
using TMPro;
using System.Collections;

public class Typewriter : MonoBehaviour
{
    public TMP_Text targetText;
    public float typingSpeed = 0.04f;

    private string fullText = "";
    private Coroutine typingCoroutine;

    public IEnumerator Type(string text)
    {
        fullText = text;
        targetText.text = "";

        typingCoroutine = StartCoroutine(TypeRoutine());
        yield return typingCoroutine;
    }

    private IEnumerator TypeRoutine()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            targetText.text += fullText[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    // 버튼 누르면 즉시 전체 텍스트 출력
    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            targetText.text = fullText;
        }
    }
}