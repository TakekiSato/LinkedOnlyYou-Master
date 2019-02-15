using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneController : MonoBehaviour, ISceneManager
{

    public void Initialize()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TransScene();
        }
    }

    public void TransScene()
    {
        //遷移やり方
        Main main = Main.instance;
        main.GoNext(SceneName.ChatScene);
    }
}
