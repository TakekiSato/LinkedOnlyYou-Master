using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaoruSceneController : MonoBehaviour, ISceneManager
{
    public int choiseAorB;
    public void Initialize()
    {
        Debug.Log("KaoruScene_Initialize");
        //TransScene();
        choiseAorB = Main.instance.select;
    }

    public void TransScene()
    {
        Main.instance.GoNext((int)SceneName.TitleScene);
    }
}
