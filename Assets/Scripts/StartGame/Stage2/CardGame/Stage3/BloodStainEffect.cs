using System.Collections;
using UnityEngine;

public class BloodStainEffect : MonoBehaviour
{
    public GameObject bloodStain1;
    public GameObject bloodStain2;

    public void OnCardBloddClicked()
    {
        if (ThirdCardGameEffectController.Instance.isEffectPlaying) return;
        ThirdCardGameEffectController.Instance.isEffectPlaying = true;
        StartCoroutine(PlayBloodStains());
    }

    private IEnumerator PlayBloodStains()
    {
        bool isB1Done = false;
        bool isB2Done = false;

        StartCoroutine(HandleBloodStain1(() => isB1Done = true));
        StartCoroutine(HandleBloodStain2(() => isB2Done = true));

        yield return new WaitUntil(() => isB1Done && isB2Done);

        ThirdCardGameEffectController.Instance.isEffectPlaying = false;
    }

    private IEnumerator HandleBloodStain1(System.Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);
        bloodStain1.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        bloodStain1.SetActive(false);
        onComplete?.Invoke();
    }

    private IEnumerator HandleBloodStain2(System.Action onComplete)
    {
        yield return new WaitForSeconds(0.7f);
        bloodStain2.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        bloodStain2.SetActive(false);
        onComplete?.Invoke();
    }
}