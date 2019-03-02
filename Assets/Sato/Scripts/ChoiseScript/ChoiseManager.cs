using System.Collections;
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

    KaoruSceneController kaoruSceneController;
    YuSceneController yuSceneController;
    RanSceneController ranSceneController;

    SoundManager soundManager;
    

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
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        if(gameObject.name == "Choise1(Clone)")
        {
            if(name == ModelName.Kaoru)
            {
                kaoruSceneController = GameObject.Find("Kaoru(Clone)").GetComponent<KaoruSceneController>();
            }
            else if(name == ModelName.Yu)
            {
                yuSceneController = GameObject.Find("Yu(Clone)").GetComponent<YuSceneController>();
            }
            else if(name == ModelName.Ran)
            {
                ranSceneController = GameObject.Find("Ran(Clone)").GetComponent<RanSceneController>();
            }
        }
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
                // SE再生
                soundManager.PlayVoice(SoundManager.VOICE_LIST.SELECT_SE);
                // 次のアニメーションを再生
                // Choise1の時のみやる
                if(gameObject.name == "Choise1(Clone)")
                {
                    // Qustion A or B
                    if(name == ModelName.Kaoru)
                    {
                        if(kaoruSceneController.choiseAorB == 0)
                        {
                            // 次に再生するアニメーションの名前を強制的に書き換え
                            Debug.Log("質問A");
                            NextPlayAnime = AnimeName.QuestionA;
                        }
                        else if(kaoruSceneController.choiseAorB == 1)
                        {
                            Debug.Log("質問B");
                            NextPlayAnime = AnimeName.QuestionB;
                        }
                        else
                        {
                            Debug.Log("File Name Error!");
                        }
                    }
                    else if(name == ModelName.Yu)
                    {
                        if(yuSceneController.choiseAorB == 0)
                        {
                            NextPlayAnime = AnimeName.QuestionB;
                        }
                        else if(yuSceneController.choiseAorB == 1)
                        {
                            NextPlayAnime = AnimeName.QuestionA;
                        }
                        else
                        {
                            Debug.Log("File Name Error!");
                        }
                    }
                    else if(name == ModelName.Ran)
                    {
                        if(ranSceneController.choiseAorB == 0)
                        {
                            NextPlayAnime = AnimeName.QuestionB;
                        }
                        else if(ranSceneController.choiseAorB == 1)
                        {
                            NextPlayAnime = AnimeName.QuestionA;
                        }
                        else
                        {
                            Debug.Log("File Name Error!");
                        }
                    }
                }
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
