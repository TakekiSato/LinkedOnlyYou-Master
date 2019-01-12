using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger : MonoBehaviour
{
    [SerializeField]
    Text text;

    public void func(string _text)
    {
        if (text == null) return;
        text.text = _text;
    }
}
