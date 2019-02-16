using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiseController : MonoBehaviour
{
    [SerializeField]
    Image choiseCircle;
    
    Vector3 pos;
    Vector3 mPosCorrection;
    const float choisedTime = 5.0f;
    const float sensitivityRadius = 3.0f;
    const float increaseRadius = 2.0f;

    int num;
    TextEvent manager;
    bool outOfCircle;
    public float nowCountTime;

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
    }

    void Update()
    {
        Vector3 look = Camera.main.transform.rotation * Vector3.forward;
        Vector3 d = transform.position - Camera.main.transform.position;
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
                manager.Choised(num);
            }
        }
        else if (!outOfCircle)
        {
            outOfCircle = true;
            choiseCircle.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }
}
