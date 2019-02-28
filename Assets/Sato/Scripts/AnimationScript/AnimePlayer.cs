using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimePlayer : MonoBehaviour
{
    GameObject model;

    AnimationManager animeManager;
    // Start is called before the first frame update
    void Start()
    {
        model = GameObject.Find("KaoruModel");
        animeManager = model.GetComponent<AnimationManager>();
    }
}
