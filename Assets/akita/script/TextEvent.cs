﻿using System.Collections;
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
    Queue<GameObject> textBoxs;
    Queue<GameObject> icons;
    GameObject[] choiseBoxes;

    const int maxTextBoxs = 5;
    const int maxChoiseCount = 3;
    Vector3 textBoxLInitPos;
    Vector3 textBoxRInitPos;
    Vector3 choiseBoxInitPosCenter;
    Vector3[] choiseBoxInitPosSide;
    const float fontSize = 40.0f;
    const float betweenLines = 0.5f;
    const float correByFont = 0.05f;
    const float noChoiceWaitTime = 3.0f;

    void Start()
    {
        ExcelData ed = new ExcelData(Application.dataPath + "/akita/text_data/Staff.csv", true);
        textBoxs = new Queue<GameObject>();
        choiseBoxes = new GameObject[maxChoiseCount];
        ctd = new CharaTextData(ed);
        if (ctd.IsInvalid) return;
        
        textBoxLInitPos = new Vector3(-12, 12, 10);
        textBoxRInitPos = new Vector3(+12, 12, 10);
        choiseBoxInitPosCenter = new Vector3(0, -8, 10);
        choiseBoxInitPosSide = new Vector3[2];
        choiseBoxInitPosSide[0] = new Vector3(-7, -7, 10);
        choiseBoxInitPosSide[1] = new Vector3(+7, -7, 10);

        UpdateTexts(true);
    }

    public void Choised(int _num)
    {
        SendChoisedText(_num);
        
        if (ctd.GetIsJump())
        {
            //拡張子確認後テキストデータ更新かシーン遷移か選べばいいかと
            ExcelData ed = new ExcelData(Application.dataPath + "/akita/" + ctd.GetJump(_num), true);
            ctd = new CharaTextData(ed);
            UpdateTexts(true);
        }
        else
        {
            UpdateTexts(false);
        }
    }

    void SendChoisedText(int _num)
    {
        GameObject obj;
        TextChanger tc;

        obj = Instantiate(textBoxRPrefab);
        obj.transform.parent = canvas.transform;
        obj.transform.localPosition = textBoxRInitPos;

        tc = obj.GetComponent<TextChanger>();
        tc.ChangeText(ctd.GetChoice(_num), fontSize, true);
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
        tc.ChangeText(ctd.GetQuestion(), fontSize);
        tc.ChangeIcon(ctd.GetIcon());

        EnterQueue(obj);
        for (int i = 0; i < maxChoiseCount; ++i)
        {
            if (null != choiseBoxes[i]) Destroy(choiseBoxes[i]);
        }


        if (ctd.GetChoiceCount() == 0)
        {
            StartCoroutine(IntervalForNextText());
            return;
        }

        int startNum = (ctd.GetChoiceCount() != 2) ? 1 : 0;
        if (startNum == 1)
        {
            choiseBoxes[0] = Instantiate(choiseBoxPrefab);
            choiseBoxes[0].transform.parent = canvas.transform;
            choiseBoxes[0].transform.localPosition = choiseBoxInitPosCenter;

            tc = choiseBoxes[0].GetComponent<TextChanger>();
            tc.ChangeIcon(ctd.GetSprite(0));
            tc.ChangeText("");

            cc = choiseBoxes[0].GetComponent<ChoiseController>();
            cc.Init(0, this);
        }
        for (int i = startNum; i < ctd.GetChoiceCount(); ++i)
        {
            choiseBoxes[i] = Instantiate(choiseBoxPrefab);
            choiseBoxes[i].transform.parent = canvas.transform;
            choiseBoxes[i].transform.localPosition = choiseBoxInitPosSide[i - startNum];

            tc = choiseBoxes[i].GetComponent<TextChanger>();
            tc.ChangeIcon(ctd.GetSprite(i));
            tc.ChangeText("");

            cc = choiseBoxes[i].GetComponent<ChoiseController>();
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
        float prevY = textBoxLInitPos.y;
        foreach (GameObject tb in textBoxs)
        {
            Vector3 pos = tb.transform.localPosition;
            pos.y = prevY;
            prevY -= (betweenLines + fontSize * correByFont * tb.GetComponent<TextChanger>().lines);
            tb.transform.localPosition = pos;
            ++ind;
        }
    }

    IEnumerator IntervalForNextText()
    {
        yield return new WaitForSeconds(noChoiceWaitTime);
        UpdateTexts(false);
        yield break;
    }
}