using System.Collections.Generic;
using System.Text;
using UnityEngine;

class ExcelData
{
    string path;
    bool isInvalid;
    List<List<string>> data;

    public bool IsInvalid
    {
        get { return isInvalid; }
    }

    public ExcelData(string _path, bool _isShiftJIS = false)
    {
        path = _path;
        if (!System.IO.File.Exists(path))
        {
            isInvalid = true;
            Debug.Log(path + " does not exist.");
            return;
        }
        System.IO.StreamReader sr;
        if (_isShiftJIS) sr = new System.IO.StreamReader(path, Encoding.GetEncoding("shift_jis"));
        else sr = new System.IO.StreamReader(path);
        data = new List<List<string>>();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            List<string> rowData = new List<string>(line.Split(','));
            data.Add(rowData);
        }
    }

    public string GetCell(int _row, int _column)
    {
        if (isInvalid)
        {
            Debug.Log("The file was not loaded correctly.");
            return "";
        }
        if (_column < 0 || data.Count <= _column)
        {
            Debug.Log("_column is out of range.");
            return "";
        }
        if (_row < 0 || data[_column].Count <= _row)
        {
            Debug.Log("_row is out of range.");
            return "";
        }
        return data[_column][_row];
    }

    public int GetColumnCount()
    {
        if (isInvalid)
        {
            Debug.Log("The file was not loaded correctly.");
            return 0;
        }
        return data.Count;
    }

    public int GetRowCount(int _column)
    {
        if (isInvalid)
        {
            Debug.Log("The file was not loaded correctly.");
            return 0;
        }
        if (_column < 0 || data.Count <= _column)
        {
            Debug.Log("_column is out of range.");
            return 0;
        }
        return data[_column].Count;
    }
}
