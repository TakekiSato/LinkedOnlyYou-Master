using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TitleSceneController : MonoBehaviour, ISceneManager
{
    public void Initialize()
    {
        Debug.Log("Initialize");
        VideoPlayer _videoPlayer = GameObject.FindWithTag("VideoPlane").GetComponent<VideoPlayer>();
        _videoPlayer.loopPointReached += EndVideo;
    }

    void EndVideo(VideoPlayer _p)
    {
        TransScene();
    }

    public void TransScene()
    { 
        Main.instance.GoNext((int)SceneName.ChatScene); //つまりこいつが呼べればシーン遷移できる
    }
}
