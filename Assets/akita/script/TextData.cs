using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TextDataSet
{
    public string question;
    public int replyCount;
    public bool isJump;
    public string[] reply;
    public Sprite[] sprite;
    public string[] jump;

    public TextDataSet(string _question)
    {
        question = _question;
        replyCount = 0;
        isJump = false;
    }

    public void SetReplyCount(int _count)
    {
        if (_count < 1) return;
        replyCount = _count;
        reply = new string[replyCount];
        sprite = new Sprite[replyCount];
    }

    public void SetReply(int _num, string _reply, string _spritePath)
    {
        if (_num < 0) return;
        if (_num >= replyCount) return;
        reply[_num] = _reply;
        sprite[_num] = MyFunctions.CreateSprite(_spritePath);

    }

    public void SetJump(int _num, string _filePath)
    {
        if (!isJump)
        {
            isJump = true;
            jump = new string[replyCount];
        }

        jump[_num] = _filePath;
    }
}

class CharaTextData
{
    Sprite icon;
    List<TextDataSet> dataList;
    int nowCount;
    int maxCount;
    bool isInvalid;

    public bool IsInvalid
    {
        get { return isInvalid; }
    }

    public CharaTextData(ExcelData _ed)
    {
        if (_ed.IsInvalid)
        {
            isInvalid = true;
            return;
        }
        nowCount = 0;
        maxCount = _ed.GetColumnCount() - 1;
        if (0 == maxCount)
        {
            isInvalid = true;
            return;
        }
        dataList = new List<TextDataSet>();
        
        icon = MyFunctions.CreateSprite(_ed.GetCell(0, 0));
        for (int col = 1; col < _ed.GetColumnCount(); ++col)
        {
            TextDataSet tds = new TextDataSet(_ed.GetCell(0, col));
            int count = (_ed.GetRowCount(col) - 1) / 3;
            for (int row = 1; row < _ed.GetRowCount(col); row += 3)
            {
                if (_ed.GetCell(row, col) != "") continue;
                count = (row - 1) / 3;
                break;
            }
            tds.SetReplyCount(count);
            for (int i = 0; i < count; ++i)
            {
                tds.SetReply(i, _ed.GetCell(i * 3 + 1, col), _ed.GetCell(i * 3 + 3, col));
                if (_ed.GetCell(i * 3 + 2, col) != "")
                    tds.SetJump(i, _ed.GetCell(i * 3 + 2, col));
            }
            dataList.Add(tds);
        }

        for (int i = 0; i < dataList.Count; ++i)
        {
            MyFunctions.LineFeed(ref dataList[i].question);
            for (int j = 0; j < dataList[i].replyCount; ++j)
                MyFunctions.LineFeed(ref dataList[i].reply[j]);
        }

        isInvalid = false;
    }

    public void Increment()
    {
        if (isInvalid) return;
        if (nowCount + 1 >= maxCount) return;
        ++nowCount;
    }

    public string GetQuestion()
    {
        if (isInvalid) return "";
        return dataList[nowCount].question;
    }

    public int GetChoiceCount()
    {
        if (isInvalid) return 0;
        return dataList[nowCount].replyCount;
    }

    public string GetChoice(int _num)
    {
        if (isInvalid) return "";
        if (_num < dataList[nowCount].replyCount) return dataList[nowCount].reply[_num];
        else return "";
    }

    public bool GetIsJump()
    {
        return dataList[nowCount].isJump;
    }

    public string GetJump(int _num)
    {
        if (isInvalid) return "";
        if (dataList[nowCount].isJump && _num < dataList[nowCount].replyCount) return dataList[nowCount].jump[_num];
        else return "";
    }

    public Sprite GetSprite(int _num)
    {
        if (isInvalid) return null;
        if (_num < dataList[nowCount].replyCount) return dataList[nowCount].sprite[_num];
        else return null;
    }

    public Sprite GetIcon()
    {
        return icon;
    }
}

class MyFunctions
{
    public static Sprite CreateSprite(string _path)
    {
        int dot = _path.LastIndexOf('.');
        return Resources.Load<Sprite>(_path.Substring(0, dot));
    }

    public static void LineFeed(ref string _str)
    {
        int count = _str.Length;
        for (int i = 0; i < count; ++i)
        {
            int pos = _str.IndexOf("\\n");
            if (pos == -1) break;
            _str = _str.Substring(0, pos) + '\n' + _str.Substring(pos + 2);
        }
    }

    public static string GetExtension(string _str)
    {
        int dot = _str.LastIndexOf('.');
        if (dot < 0) return "";
        return _str.Substring(dot + 1);
    }
}
