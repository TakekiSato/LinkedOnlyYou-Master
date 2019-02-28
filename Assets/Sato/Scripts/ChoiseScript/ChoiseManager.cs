using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiseManager : MonoBehaviour
{
    [SerializeField]
    Image choiseCircle;

    [SerializeField]
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
        //var parentobj = GameObject.Find("ParentObject");
        ////大神:シーンの親を作ったのでここにパブリックでシリアライズしたりすれば何でも取れる
        //var parent = parentobj.GetComponent<ParentObject>();
        //camera = parent.mainCamera;
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
                // 選択された時に後々実行される処理がここ

            }
        }
        else if (!outOfCircle)
        {
            outOfCircle = true;
            choiseCircle.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }
}
