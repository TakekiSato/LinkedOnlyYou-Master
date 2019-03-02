using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RanSceneController : MonoBehaviour, ISceneManager
{
    public int choiseAorB;
    public void Initialize()
    {
        Debug.Log("RanScene_Initialize");
        //ansScene();
        //choiseAorB = Main.instance.select;
        choiseAorB = 0;
    }

    public void TransScene()
    {
        //Main.instance.GoNext((int)SceneName.TitleScene);
    }
}
