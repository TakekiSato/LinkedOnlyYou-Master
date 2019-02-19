using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RanSetup : MonoBehaviour, IModelInit
{
    public void Init()
    {
        Debug.Log("ここ生成時に一回だけ呼ばれます");
    }
}
