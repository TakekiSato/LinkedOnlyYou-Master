﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiseMakingManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] choiseObjects;

    GameObject canvas;

    public enum Member
    {
        Kaoru,
        Yu,
        Ran
    }

    public Member member;

    Vector3[] memberUIPos =
    {
        new Vector3(0f, 0.6f, 0f),
        new Vector3(0f, 1f, -0.7f),
        new Vector3(0f, 0.6f, 0f)
    };

    void Start()
    {
        canvas = GameObject.Find("Canvas");
    }

    void Choise1(int num)
    {
        var choiseObj = Instantiate(choiseObjects[num]);
        choiseObj.transform.parent = canvas.transform;
        choiseObj.transform.position = memberUIPos[(int)member];
    }
}
