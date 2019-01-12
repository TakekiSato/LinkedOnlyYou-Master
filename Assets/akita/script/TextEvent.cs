using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextEvent : MonoBehaviour
{
    [SerializeField]
    GameObject canvas;

    [SerializeField]
    GameObject textBoxLPrefab;
    [SerializeField]
    GameObject textBoxRPrefab;
    [SerializeField]
    GameObject choiseBoxPrefab;


    CharaTextData ctd;
    CharaPoint charaPoint;
    Queue<GameObject> textBoxs;
    GameObject[] choiseBoxs;

    const int maxTextBoxs = 5;
    const int maxChoiseCount = 3;
    Vector3 textBoxLInitPos;
    Vector3 textBoxRInitPos;
    Vector3[] choiseBoxInitPos;
    const float betweenLines = 60;

    void Start()
    {
        ExcelData ed = new ExcelData(Application.dataPath + "/akita/text_data/text_data_sample.csv");
        textBoxs = new Queue<GameObject>();
        choiseBoxs = new GameObject[maxChoiseCount];
        ctd = new CharaTextData(ed);
        if (ctd.IsInvalid) return;
        
        charaPoint.Init();
        textBoxLInitPos = new Vector3(-Screen.width * 0.25f, Screen.height * 0.4f, 0);
        textBoxRInitPos = new Vector3(Screen.width * 0.25f, Screen.height * 0.4f, 0);
        choiseBoxInitPos = new Vector3[maxChoiseCount];
        choiseBoxInitPos[0] = new Vector3(-Screen.width * 0.25f, -Screen.height * 0.30f, 0);
        choiseBoxInitPos[1] = new Vector3( Screen.width * 0.00f, -Screen.height * 0.35f, 0);
        choiseBoxInitPos[2] = new Vector3(+Screen.width * 0.25f, -Screen.height * 0.30f, 0);

        UpdateTexts(true);
    }

    public void Choised(int _num)
    {
        charaPoint += ctd.GetCharaPoint(_num + 1);
        SendChoisedText(_num);
        UpdateTexts(false);
    }

    void SendChoisedText(int _num)
    {
        GameObject obj;
        TextChanger tc;

        obj = Instantiate(textBoxRPrefab);
        obj.transform.parent = canvas.transform;
        obj.transform.localPosition = textBoxRInitPos;

        tc = obj.GetComponent<TextChanger>();
        tc.func(ctd.GetChoice(_num + 1));

        EnterQueue(obj);
    }

    void UpdateTexts(bool _isFirst)
    {
        if (!_isFirst) ctd.Increment();

        GameObject obj;
        TextChanger tc;
        ChoiseController cc;

        obj = Instantiate(textBoxLPrefab);
        obj.transform.parent = canvas.transform;
        obj.transform.localPosition = textBoxLInitPos;

        tc = obj.GetComponent<TextChanger>();
        tc.func(ctd.GetQuestion());

        EnterQueue(obj);

        for (int i = 0; i < maxChoiseCount; ++i)
        {
            if (null != choiseBoxs[i]) Destroy(choiseBoxs[i]);
            choiseBoxs[i] = Instantiate(choiseBoxPrefab);
            choiseBoxs[i].transform.parent = canvas.transform;
            choiseBoxs[i].transform.localPosition = choiseBoxInitPos[i];

            tc = choiseBoxs[i].GetComponent<TextChanger>();
            tc.func(ctd.GetChoice(i + 1));

            cc = choiseBoxs[i].GetComponent<ChoiseController>();
            cc.Init(i, this);
        }
    }

    void EnterQueue(GameObject _obj)
    {
        textBoxs.Enqueue(_obj);
        if (textBoxs.Count > maxTextBoxs)
        {
            GameObject disObj = textBoxs.Dequeue();
            Destroy(disObj);
        }
        int ind = 0;
        foreach (GameObject tb in textBoxs)
        {
            Vector3 pos = tb.transform.localPosition;
            pos.y = textBoxLInitPos.y - betweenLines * ind;
            tb.transform.localPosition = pos;
            ++ind;
        }
    }
}
