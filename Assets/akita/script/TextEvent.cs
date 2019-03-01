using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    const float noChoiceWaitTime = 1.5f;

    //時田
    float voiceDelayTime = 0.0f;
    bool isPlaying = false;

    string csvFileName;
    int soundCount;
    AudioSource speaker;

    void Start()
    {
        csvFileName = "text_data/Staff.csv";
        soundCount = 0;

        ExcelData ed = new ExcelData(Application.dataPath + "/akita/" + csvFileName, true);
        textBoxs = new Queue<GameObject>();
        choiseBoxes = new GameObject[maxChoiseCount];
        ctd = new CharaTextData(ed);
        if (ctd.IsInvalid) return;

        textBoxLInitPos = new Vector3(-12, 12, 10);
        textBoxRInitPos = new Vector3(+12, 12, 10);
        choiseBoxInitPosCenter = new Vector3(0, -10, 10);
        choiseBoxInitPosSide = new Vector3[2];
        choiseBoxInitPosSide[0] = new Vector3(-7, -9, 10);
        choiseBoxInitPosSide[1] = new Vector3(+7, -9, 10);

        speaker = gameObject.AddComponent<AudioSource>();

        UpdateTexts(true);
    }

    public void Choised(int _num)
    {
        SendChoisedText(_num);

        //選択SEここ!!!
        AudioClip selectSE = Resources.Load<AudioClip>("voice/SelectSE");
        speaker.PlayOneShot(selectSE);

        if (!ctd.GetIsJump())
        {
            UpdateTexts(false);
            return;
        }

        string extension = MyFunctions.GetExtension(ctd.GetJump(_num));
        if (extension == "csv")
        {
            csvFileName = ctd.GetJump(_num);
            soundCount = 0;
            ExcelData ed = new ExcelData(Application.dataPath + "/akita/" + csvFileName, true);
            ctd = new CharaTextData(ed);
            UpdateTexts(true);
        }
        else /*if (extension == "unity")*/
        {
            JumpNextStage(_num);
        }
    }

    void JumpNextStage(int _num)//現状、csvファイルを見ると引数は不必要かもしれない
    {
        string fileName = MyFunctions.RemoveExtension(ctd.GetJump(_num));
        StartCoroutine(TransitionNextScene(fileName));
    }

    void SendChoisedText(int _num)
    {
        GameObject obj;
        TextChanger tc;

        obj = Instantiate(textBoxRPrefab);
        obj.transform.parent = canvas.transform;
        obj.transform.localPosition = textBoxRInitPos;

        tc = obj.GetComponent<TextChanger>();
        tc.ChangeText(ctd.GetReply(_num), fontSize, true);
        EnterQueue(obj);
    }

    void UpdateTexts(bool _isFirst)
    {
        if (!_isFirst) ctd.Increment();

        GameObject obj;
        TextChanger tc;
        //ChoiseController cc;

        obj = Instantiate(textBoxLPrefab);
        obj.transform.parent = canvas.transform;
        obj.transform.localPosition = textBoxLInitPos;

        tc = obj.GetComponent<TextChanger>();
        tc.ChangeText(ctd.GetQuestion(), fontSize);
        tc.ChangeIcon(ctd.GetIcon());
        EnterQueue(obj);

        SoundedPartnerVoice();

        for (int i = 0; i < maxChoiseCount; ++i)
        {
            if (null != choiseBoxes[i]) Destroy(choiseBoxes[i]);
        }


        if (ctd.GetIsNextScene())
        {
            JumpNextStage(0);
            return;
        }

        if (ctd.GetReplyCount() == 0)
        {
            StartCoroutine(IntervalForNextText());
            return;
        }

        int startNum = (ctd.GetReplyCount() != 2) ? 1 : 0;

        StartCoroutine(BoxInstantiate(startNum, tc));
    }

    IEnumerator BoxInstantiate(int _startNum, TextChanger _tc)
    {
        ChoiseController cc;

        yield return new WaitForSeconds(voiceDelayTime + 0.5f);

        if (_startNum == 1)
        {
            choiseBoxes[0] = Instantiate(choiseBoxPrefab);
            choiseBoxes[0].transform.parent = canvas.transform;
            choiseBoxes[0].transform.localPosition = choiseBoxInitPosCenter;

            _tc = choiseBoxes[0].GetComponent<TextChanger>();
            _tc.ChangeIcon(ctd.GetSprite(0));
            _tc.ChangeText("");

            cc = choiseBoxes[0].GetComponent<ChoiseController>();
            cc.Init(0, this);
        }
        for (int i = _startNum; i < ctd.GetReplyCount(); ++i)
        {
            choiseBoxes[i] = Instantiate(choiseBoxPrefab);
            choiseBoxes[i].transform.parent = canvas.transform;
            choiseBoxes[i].transform.localPosition = choiseBoxInitPosSide[i - _startNum];

            _tc = choiseBoxes[i].GetComponent<TextChanger>();
            _tc.ChangeIcon(ctd.GetSprite(i));
            _tc.ChangeText("");

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
        //Debug.LogError("unko entry");
        yield return new WaitForSeconds(voiceDelayTime + 0.5f);
        UpdateTexts(false);
        yield break;
    }

    IEnumerator TransitionNextScene(string _sceneName)
    {
        yield return new WaitForSeconds(voiceDelayTime + 0.5f);
        
        Main.instance.select = Select_Branch(csvFileName);
        Main.instance.GoNextStr(_sceneName);
        Debug.Log(csvFileName);
        yield break;
    }

    void SoundedPartnerVoice()
    {
        // ここに音声再生を追加？
        string str = MyFunctions.GetOnlyFileName(csvFileName);
        AudioClip voice = Resources.Load<AudioClip>("voice/" + str + '/' + soundCount);
        if (voice != null)
        {
            voiceDelayTime = voice.length;
        }
        else
        {
            voiceDelayTime = noChoiceWaitTime;
        }
        Debug.Log("voice/" + str + '/' + soundCount);
        ++soundCount;
        if (voice == null) return;
        Debug.Log("FFFFFFFFFFF");
        speaker.PlayOneShot(voice);
    }

    int Select_Branch(string fileName)
    {
        //text_data/Kaoru_A_1.csv
        Debug.Log("名前：" + fileName + "：");
        if (   fileName == "text_data/Kaoru_A_1.csv"
            || fileName == "text_data/Kaoru_A_2.csv"
            || fileName == "text_data/Kaoru_C_1.csv"
            || fileName == "text_data/Kaoru_C_2.csv"
            || fileName == "text_data/Kaoru_D_1.csv"
            || fileName == "text_data/Kaoru_D_2.csv"
            || fileName == "text_data/Yu_A.csv"
            || fileName == "text_data/Yu_C.csv"
            || fileName == "text_data/Yu_D.csv"
            || fileName == "text_data/Ran_A_1.csv"
            || fileName == "text_data/Ran_A_2.csv"
            || fileName == "text_data/Ran_C_1.csv"
            || fileName == "text_data/Ran_C_2.csv"
            || fileName == "text_data/Ran_D_1.csv"
            || fileName == "text_data/Ran_D_2.csv"
            )
        {
            return 0;
        }
        else if (fileName == "text_data/Kaoru_B.csv"
            || fileName == "text_data/Kaoru_E.csv"
            || fileName == "text_data/Kaoru_F_1.csv"
            || fileName == "text_data/Kaoru_F_2.csv"
            || fileName == "text_data/Yu_B.csv"
            || fileName == "text_data/Yu_E.csv"
            || fileName == "text_data/Yu_F.csv"
            || fileName == "text_data/Ran_B_1.csv"
            || fileName == "text_data/Ran_B_2.csv"
            || fileName == "text_data/Ran_E_1.csv"
            || fileName == "text_data/Ran_E_2.csv"
            || fileName == "text_data/Ran_F_1.csv"
            || fileName == "text_data/Ran_F_2.csv"
            )
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
}
