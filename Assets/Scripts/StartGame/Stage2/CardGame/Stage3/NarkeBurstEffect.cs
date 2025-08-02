using System.Collections;
using UnityEngine;

public class NarkeBurstEffect : MonoBehaviour
{
    public RectTransform narkeJumpScare; // NarkeJumpScare의 RectTransform

    private Vector2 startPos = new Vector2(-1896f, -288f);
    private Vector2 targetPos = new Vector2(-665f, -288f);
    private float moveDuration = 0.5f;

    public void OnCardNarkeBurstClicked()
    {
        if (ThirdCardGameEffectController.Instance.isEffectPlaying) return;

        ThirdCardGameEffectController.Instance.isEffectPlaying = true;
        StartCoroutine(MoveInAndOut());
    }

    private IEnumerator MoveInAndOut()
    {
        // 초기 위치 설정
        narkeJumpScare.anchoredPosition = startPos;

        // 앞으로 슬라이드
        yield return StartCoroutine(MoveUI(startPos, targetPos, moveDuration));

        // 뒤로 슬라이드
        yield return StartCoroutine(MoveUI(targetPos, startPos, moveDuration));

        // ❗️여기에서 효과 종료 처리
        ThirdCardGameEffectController.Instance.isEffectPlaying = false;
    }


    private IEnumerator MoveUI(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            narkeJumpScare.anchoredPosition = Vector2.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        narkeJumpScare.anchoredPosition = to; // 정확한 위치 보정
    }
}