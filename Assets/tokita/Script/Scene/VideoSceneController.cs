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

    public void Initialize()
    {
        modelLoad = new ModelLoad();
        StartCoroutine(modelLoad.Load(initModel));
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
        main.GoNext(SceneName.EndScene);
    }
}
