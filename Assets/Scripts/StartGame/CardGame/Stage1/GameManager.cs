using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("타이머 관련")] public TextMeshProUGUI timerText;
    private float remainingTime = 1200f; // 20분

    [Header("음악 관련")] public AudioClip introMusic; // 처음 1회용
    public AudioClip loopMusic; // 이후 반복 재생될 메인 브금
    private AudioSource audioSource;
    private bool introPlayed = false;

    [Header("Crazy Goat")] public GameObject crazyGoat;

    // 이전
// private SpriteRenderer goatRenderer;
    private Image goatImage; // UI Image 컴포넌트용으로 변경

    private bool isGoatActivated = false;

    [Header("화면 연출")] public Image blackScreen; // 🎯 UI 이미지 컴포넌트


    public bool isTimerPaused = false;
    private bool hasEnded = false;

    void Start()
    {
        // 염소 초기화
        goatImage = crazyGoat.GetComponent<Image>();
        if (goatImage != null)
        {
            Color initColor = goatImage.color;
            initColor.a = 0f;
            goatImage.color = initColor;
            crazyGoat.SetActive(false);
            Debug.Log("🐐 UI 염소 이미지 초기화 및 비활성화됨");
        }
        else
        {
            Debug.LogError("❌ Image 컴포넌트를 찾지 못했습니다!");
        }

        // AudioSource 생성 및 IntroMusic 재생
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (introMusic != null)
        {
            audioSource.clip = introMusic;
            audioSource.loop = false;
            audioSource.Play();
            Debug.Log($"🎵 IntroMusic 재생 시작 (길이: {introMusic.length:F2}초)");
        }
    }

    void UpdateGoatAlpha()
    {
        float timePassed = Time.timeSinceLevelLoad;

        // 30초(0.5분) 지나면 오브젝트 활성화
        if (!isGoatActivated && timePassed >= 30f)
        {
            crazyGoat.SetActive(true);
            isGoatActivated = true;
            Debug.Log("🐐 CrazyGoat 오브젝트 30초 후에 활성화됨!");
        }

        if (isGoatActivated)
        {
            if (timePassed < 150f) // 30초~150초 사이 점점 나타남 (2분)
            {
                float alpha = Mathf.InverseLerp(30f, 150f, timePassed); // 선형 보간
                SetGoatAlpha(alpha);
            }
            else
            {
                SetGoatAlpha(1f); // 2분 30초 이후 완전히 보이게
            }
        }
    }






    void Update()
    {
        // 타이머 동작
        if (!isTimerPaused && remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f) remainingTime = 0f;
            UpdateTimerUI();
        }

        // 타이머가 0되면 엔딩으로 이동 (옵션)
        if (!hasEnded && remainingTime <= 0f)
        {
            hasEnded = true;
            Debug.Log("⏰ 타이머 종료! FIrstSlidingGameDeathEnding 엔딩으로 이동");
            SceneManager.LoadScene("FIrstSlidingGameDeathEnding");
        }

        // 음악 전환 처리
        if (!introPlayed && !audioSource.isPlaying && loopMusic != null)
        {
            introPlayed = true;
            audioSource.clip = loopMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("🔁 LoopMusic 반복 재생 시작!");
        }

        // 염소 알파 처리
        UpdateGoatAlpha();



    }



    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        cg.gameObject.SetActive(true);

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
    }



    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }



    void SetGoatAlpha(float alpha)
    {
        if (goatImage != null)
        {
            Color c = goatImage.color;
            c.a = alpha;
            goatImage.color = c;

            Debug.Log($"[CrazyGoat UI] Alpha = {alpha:F2}");
        }
    }
}

