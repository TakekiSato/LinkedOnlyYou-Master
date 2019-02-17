using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger : MonoBehaviour
{
    [SerializeField]
    Text text;
    [SerializeField]
    Image icon;
    [SerializeField]
    Image window;

    public int lines;
    const float correction = 20.0f;
    const float fontWidth = 15.0f;

    public void ChangeText(string _text, float _fontSize = 0.1f, bool _isRight = false)
    {
        if (text == null) return;
        text.text = _text;
        
        int charaCount = 0;
        int start = 0;
        lines = 1;
        for (int i = 0; i < _text.Length; ++i)
        {
            int count = _text.IndexOf('\n', start);
            if (count == -1) break;
            ++lines;
            if (count - start > charaCount) charaCount = count - start;
            start = count + 1;
        }
        if (lines == 1) charaCount = _text.Length;
        text.rectTransform.sizeDelta = new Vector2(_fontSize * charaCount + fontWidth, _fontSize * lines + fontWidth);

        if (window == null) return;
        float scale = window.transform.localScale.x;
        window.rectTransform.sizeDelta = new Vector2(_fontSize * charaCount + correction + fontWidth, _fontSize * lines + fontWidth);
        int sign = _isRight ? 0 : 1;
        window.transform.localPosition = window.transform.localPosition - new Vector3(correction * scale * sign, 0, 0);

    }

    public void ChangeIcon(Sprite _sprite)
    {
        if (icon == null) return;
        icon.sprite = _sprite;
    }
}
