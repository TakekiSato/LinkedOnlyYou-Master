using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaoruVoicePlayer : MonoBehaviour
{
    [SerializeField]
    GameObject SMObj;

    SoundManager soundManager;

    private void Awake()
    {
        soundManager = SMObj.GetComponent<SoundManager>();
    }

    void PlayVoiceC1()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.K_COMMON_1);
    }

    void PlayVoiceQA()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.K_QUESTION_A);
    }

    void PlayVoiceQB()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.K_QUESTION_B);
    }

    void PlayVoiceRA()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.K_REPLY_A);
    }
}
