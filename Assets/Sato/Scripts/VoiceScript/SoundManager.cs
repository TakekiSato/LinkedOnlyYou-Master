using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] voiceTable = new AudioClip[1];

    const int VOICE_MAX = 11;

    AudioSource[] audioSources = new AudioSource[VOICE_MAX];

    int nowVoiceIndex = 0;

    public enum VOICE_LIST
    {
        COMMON_1,
        QUESTION_A,
        REPLY_A,
        REPLY_I,
        QUESTION_B,
        REPLY_U,
        REPLY_E,
        COMMON_2,
        ED_1,
        ED_2,
        COMMON_3,
        SELECT_SE
    }

    void Awake()
    {
        for (int audioIndex = 0; audioIndex < VOICE_MAX; audioIndex++)
        {
            audioSources[audioIndex] = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayVoice(VOICE_LIST voiceID)
    {
        audioSources[nowVoiceIndex].PlayOneShot(voiceTable[(int)voiceID]);
        nowVoiceIndex++;
        if(nowVoiceIndex >= VOICE_MAX)
        {
            nowVoiceIndex = 0;
        }
    }
}
