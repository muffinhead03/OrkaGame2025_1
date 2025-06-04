using System.Collections;
using TMPro;
using UnityEngine;

public class Typewriter : MonoBehaviour
{
    public TMP_Text targetText;
    public float typingSpeed = 0.04f;

    public IEnumerator Type(string fullText)
    {
        targetText.text = "";
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}