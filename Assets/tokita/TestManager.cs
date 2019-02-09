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


public class TestManager : MonoBehaviour
{
    [SerializeField] InitModel initModel = InitModel.None;
    ModelLoad modelLoad;

    void Start()
    {
        modelLoad = new ModelLoad();
        modelLoad.Load(initModel);

        DontDestroyOnLoad(gameObject);
    }

}