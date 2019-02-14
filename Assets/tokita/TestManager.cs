using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InitModel : int
{
    Model_Ran,
    Model_Yu,
    Model_Kaoru,
    None = 99
}

public enum InitScene : int
{
    TitleScene,
    ChatScene,
    VideoScene,
    EndScene
}


public class TestManager : MonoBehaviour
{
    [SerializeField] InitModel initModel = InitModel.None;
    [SerializeField] InitScene initScene = InitScene.TitleScene;
    ModelLoad modelLoad;

    void Awake()
    {
        modelLoad = new ModelLoad();
        modelLoad.Load(initModel);

        DontDestroyOnLoad(gameObject);

        SceneLoad(initScene.ToString());
    }

    void SceneLoad(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
        InitializeScene();
    }

    void InitializeScene()
    {
        var scene = SceneManager.GetActiveScene().buildIndex;

        switch (scene)
        {
            case 0:
                TitleSceneController tsc = new TitleSceneController();
                break;
            case 1:
                ChatSceneController csc = new ChatSceneController();
                break;
            case 2:
                VideoSceneController vsc = new VideoSceneController();
                break;
            case 3:

                break;
        }
    }

}