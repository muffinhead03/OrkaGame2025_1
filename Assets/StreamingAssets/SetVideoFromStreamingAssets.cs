using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class SetVideoFromStreamingAssets : MonoBehaviour
{
    public VideoPlayer vp;
    public string fileName = "bad_ending.mp4"; // 새 파일명

    void Awake()
    {
        if (!vp) vp = GetComponent<VideoPlayer>();
        vp.source = VideoSource.Url;
        vp.url = Path.Combine(Application.streamingAssetsPath, fileName);
        vp.Prepare();
    }

    void Start()
    {
        vp.prepareCompleted += _ => vp.Play();
    }
}