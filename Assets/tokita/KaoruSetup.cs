using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaoruSetup : MonoBehaviour, IModelInit
{
    public void Init()
    {
        Debug.Log("ここ生成時に一回だけ呼ばれます");
    }
}
