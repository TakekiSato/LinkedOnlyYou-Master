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
            TransScene();
        }
    }

    public void TransScene()
    { 
    }
}
