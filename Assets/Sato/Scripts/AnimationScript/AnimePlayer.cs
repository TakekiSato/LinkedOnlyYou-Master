using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimePlayer : MonoBehaviour
{
    [SerializeField]
    GameObject model;

    AnimationManager animeManager;
    // Start is called before the first frame update
    void Start()
    {
        animeManager = model.GetComponent<AnimationManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("pushed");
            animeManager.PlayAnime("Reply-a");
        }
    }
}
