using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaoruSceneController : MonoBehaviour, ISceneManager
{
    public void Initialize()
    {
        Debug.Log("KaoruScene_Initialize");
        TransScene();
    }

    public void TransScene()
    {
        Main.instance.GoNext((int)SceneName.TitleScene);
    }
}
