using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RanSceneController : MonoBehaviour, ISceneManager
{
    public void Initialize()
    {
        Debug.Log("RanScene_Initialize");
        //ansScene();
    }

    public void TransScene()
    {
        //Main.instance.GoNext((int)SceneName.TitleScene);
    }
}
