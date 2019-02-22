using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISceneManager
{
    /// <summary>
    ///sceneのロードが完了した時点でstartよりも早いタイミングで呼ばれる
    /// </summary>
    void Initialize();

    /// <summary>
    /// シーン遷移するときはこれの中に処理を書く
    /// </summary>
    void TransScene();
}
