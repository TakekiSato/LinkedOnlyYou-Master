using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InitModel : int
{
    Model_Ran,
    Model_Yu,
    Model_Kaoru,
    None = 99
}

public class VideoSceneController : MonoBehaviour, ISceneManager
{

    [SerializeField] InitModel initModel = InitModel.None;
    ModelLoad modelLoad;

    void Awake()
    {
        Debug.Log("Awake");
    }

    public void Initialize()
    {
        Debug.Log("Initialize");
        modelLoad = new ModelLoad();
        StartCoroutine(modelLoad.Load(initModel));
    }

    void Start()
    {
        Debug.Log("Start");
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
        Main.instance.GoNext(SceneName.EndScene);
    }
}
