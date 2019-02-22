using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YuSceneController : MonoBehaviour, ISceneManager
{
    public void Initialize()
    {
        Debug.Log("YuScene_Initialize");
        TransScene();
    }

    public void TransScene()
    {
        Main.instance.GoNext((int)SceneName.TitleScene);
    }
}
