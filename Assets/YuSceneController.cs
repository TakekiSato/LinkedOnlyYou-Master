using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YuSceneController : MonoBehaviour, ISceneManager
{
    string a;

    public void Initialize()
    {
        Debug.Log("YuScene_Initialize");
        //TransScene();
        a = Main.instance.select;
    }

    public void TransScene()
    {
        Main.instance.GoNext((int)SceneName.TitleScene);
    }


}
