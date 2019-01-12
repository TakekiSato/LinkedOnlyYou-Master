using System.Collections;
using System.Collections.Generic;

struct CharaPoint
{
    public int A;
    public int B;
    public int C;

    public void Init()
    {
        A = 0; B = 0; C = 0;
    }

    static public CharaPoint Zero()
    {
        CharaPoint ans;
        ans.A = 0; ans.B = 0; ans.C = 0;
        return ans;
    }

    static public CharaPoint operator+ (CharaPoint obj1, CharaPoint obj2)
    {
        CharaPoint ans;
        ans.A = obj1.A + obj2.A;
        ans.B = obj1.B + obj2.B;
        ans.C = obj1.C + obj2.C;
        return ans;
    }
}

class ChoiceCharaPointSet
{
    public CharaPoint cp;
    public string text;

    public ChoiceCharaPointSet(string _text, int _pA, int _pB, int _pC)
    {
        text = _text;
        cp.A = _pA;
        cp.B = _pB;
        cp.C = _pC;
    }
}

class CharaTextData
{
    List<string> question;
    List<ChoiceCharaPointSet> ccpList1;
    List<ChoiceCharaPointSet> ccpList2;
    List<ChoiceCharaPointSet> ccpList3;
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
        maxCount = _ed.GetColumnCount();
        if (0 == maxCount)
        {
            isInvalid = true;
            return;
        }

        ccpList1 = new List<ChoiceCharaPointSet>();
        ccpList2 = new List<ChoiceCharaPointSet>();
        ccpList3 = new List<ChoiceCharaPointSet>();
        question = new List<string>();

        for (int i = 0; i < maxCount; ++i)
        {
            question.Add(_ed.GetCell(0, i));
            ccpList1.Add(new ChoiceCharaPointSet(
                _ed.GetCell(1, i), int.Parse(_ed.GetCell(2, i)), int.Parse(_ed.GetCell(3, i)), int.Parse(_ed.GetCell(4, i))));
            ccpList2.Add(new ChoiceCharaPointSet(
                _ed.GetCell(5, i), int.Parse(_ed.GetCell(6, i)), int.Parse(_ed.GetCell(7, i)), int.Parse(_ed.GetCell(8, i))));
            ccpList3.Add(new ChoiceCharaPointSet(
                _ed.GetCell(9, i), int.Parse(_ed.GetCell(10, i)), int.Parse(_ed.GetCell(11, i)), int.Parse(_ed.GetCell(12, i))));
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
        return question[nowCount];
    }

    public string GetChoice(int _num)
    {
        if (isInvalid) return "";
        switch (_num)
        {
            case 1: return ccpList1[nowCount].text;
            case 2: return ccpList2[nowCount].text;
            case 3: return ccpList3[nowCount].text;
        }
        return "";
    }

    public CharaPoint GetCharaPoint(int _num)
    {
        if (isInvalid) return CharaPoint.Zero();
        switch (_num)
        {
            case 1: return ccpList1[nowCount].cp;
            case 2: return ccpList2[nowCount].cp;
            case 3: return ccpList3[nowCount].cp;
        }
        return CharaPoint.Zero();
    }
}
