using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject charaModel = Instantiate(Resources.Load("ran_kyoutuu 1_honban model", typeof(GameObject))) as GameObject;
        GameObject roomModel = Instantiate(Resources.Load("room_ran", typeof(GameObject))) as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
