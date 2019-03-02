using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiseMakingManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] choiseObjects;

    GameObject canvas;

    [SerializeField]
    GameObject fadeoutPanel;

    public enum Member
    {
        Kaoru,
        Yu,
        Ran
    }

    public Member member;

    Vector3[] memberUIPos =
    {
        new Vector3(0f, 0.6f, 0f),
        new Vector3(0f, 1f, -0.7f),
        new Vector3(1.3f, 0.9f, 1f)
    };

    void Start()
    {
        canvas = GameObject.Find("Canvas");
    }

    void Choise1(int num)
    {
        var choiseObj = Instantiate(choiseObjects[num]);
        choiseObj.transform.parent = canvas.transform;
        choiseObj.transform.position = memberUIPos[(int)member];
    }

    void FadeOut()
    {
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        //var panel = Instantiate(fadeoutPanel);
        //panel.transform.parent = canvas.transform;
        StartCoroutine(fadeoutCoroutine(fadeoutPanel, 3.0f));
    }

    IEnumerator fadeoutCoroutine(GameObject fadePanel, float fadeTime)
    {
        var fadeImage = fadePanel.GetComponent<Image>();
        
        Color color = fadeImage.color;

        for(float t = 0; t < fadeTime; t++)
        {
            color.a += t / fadeTime;
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1;
        fadeImage.color = color;
        yield return null;

        yield return new WaitForSeconds(1f);
        if(Input.GetKey(KeyCode.J))
        {
            Main.instance.GoNext((int)SceneName.TitleScene);
        }
    }
}
