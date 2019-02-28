﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiseManager : MonoBehaviour
{
    [SerializeField]
    Image choiseCircle;

    Camera camera;

    Vector3 pos;
    Vector3 mPosCorrection;
    const float choisedTime = 3.0f;
    const float sensitivityRadius = 3.0f;
    const float increaseRadius = 2.0f;

    int num;
    TextEvent manager;
    bool outOfCircle;
    public float nowCountTime;

    string[] animeNames = 
    {
        "QuestionA",
        "Reply_a",
        "Reply_i",
        "QuestionB",
        "Reply_u",
        "Reply_e",
        "Common2",
        "ED1",
        "ED2",
        "Common3"
    };

    enum AnimeName
    {
        QuestionA,
        Reply_a,
        Reply_i,
        QuestionB,
        Reply_u,
        Reply_e,
        Common2,
        ED1,
        ED2,
        Common3
    }

    [SerializeField]
    AnimeName NextPlayAnime;

    public enum ModelName
    {
        Kaoru,
        Yu,
        Ran
    }

    public ModelName name;

    string[] modelNames = 
    {
        "KaoruModel",
        "YuModel",
        "RanModel"
    };
    GameObject model;

    public void Init(int _num, TextEvent _manager)
    {
        num = _num;
        manager = _manager;
    }

    void Start()
    {
        pos = transform.localPosition;
        mPosCorrection = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        outOfCircle = true;
        
        camera = GameObject.Find("[CameraRig]").GetComponentInChildren<Camera>();
        model = GameObject.Find(modelNames[(int)name]);
    }

    void Update()
    {
        Vector3 look = camera.transform.rotation * Vector3.forward;
        Vector3 d = transform.position - camera.transform.position;
        d = look - d.normalized;
        if (d.x * d.x + d.y * d.y + d.z * d.z <= 0.03f/*適当*/)
        {
            if (outOfCircle)
            {
                outOfCircle = false;
                nowCountTime = choisedTime;
                choiseCircle.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            float size = sensitivityRadius * 2 + increaseRadius * nowCountTime / choisedTime;
            choiseCircle.rectTransform.sizeDelta = new Vector2(size, size);

            nowCountTime -= Time.deltaTime;
            if (nowCountTime <= 0.0f)
            {
                // 次のアニメーションを再生
                model.GetComponent<Animator>().Play(animeNames[(int)NextPlayAnime]);
                if(gameObject.transform.parent.gameObject.name != "Canvas")
                {
                    Destroy(gameObject.transform.parent.gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (!outOfCircle)
        {
            outOfCircle = true;
            choiseCircle.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }
}