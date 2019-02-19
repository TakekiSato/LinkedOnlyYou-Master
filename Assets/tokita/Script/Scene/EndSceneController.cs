using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneController : MonoBehaviour, ISceneManager
{
    public void Initialize()
    {

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TransScene();
        }
    }

    public void TransScene()
    {
        Main main = Main.instance;
        main.GoNext(SceneName.TitleScene);
    }
}
