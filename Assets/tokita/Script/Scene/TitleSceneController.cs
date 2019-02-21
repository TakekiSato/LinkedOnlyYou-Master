using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneController : MonoBehaviour, ISceneManager
{
    void Awake()
    {
        Debug.Log("Awake");
    }

    public void Initialize()
    {
        Debug.Log("Initialize");
    }

    void Start()
    {
        Debug.Log("Start");
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
        Main.instance.GoNext(SceneName.ChatScene);
    }
}
