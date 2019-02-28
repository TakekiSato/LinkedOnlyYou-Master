using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoicePlayer : MonoBehaviour
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
        soundManager.PlayVoice(SoundManager.VOICE_LIST.COMMON_1);
    }

    void PlayVoiceQA()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.QUESTION_A);
    }

    void PlayVoiceQB()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.QUESTION_B);
    }

    void PlayVoiceRA()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.REPLY_A);
    }

    void PlayVoiceRI()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.REPLY_I);
    }

    void PlayVoiceRU()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.REPLY_U);
    }

    void PlayVoiceRE()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.REPLY_E);
    }

    void PlayVoiceC2()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.COMMON_2);
    }

    void PlayVoiceE1()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.ED_1);
    }

    void PlayVoiceE2()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.ED_2);
    }

    void PlayVoiceC3()
    {
        soundManager.PlayVoice(SoundManager.VOICE_LIST.COMMON_3);
    }
}
