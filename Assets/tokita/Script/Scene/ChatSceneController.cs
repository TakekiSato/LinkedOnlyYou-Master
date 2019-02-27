using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatSceneController : MonoBehaviour, ISceneManager
{
    public void Initialize()
    {
        Debug.Log("Initialize");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Main.instance.select = SelectCharacter.Kaoru;　//選択されたキャラこうやって渡す
            TransScene();
        }
    }

    public void TransScene()
    { 
        Main.instance.GoNext((int)Main.instance.select); //上で選択されたシーンへ遷移する
    }
}
