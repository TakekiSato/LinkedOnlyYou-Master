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
        K_COMMON_1,
        K_QUESTION_A,
        K_REPLY_A,
        K_REPLY_I,
        K_QUESTION_B,
        K_REPLY_U,
        K_REPLY_E,
        K_COMMON_2,
        K_ED_1,
        K_ED_2,
        K_COMMON_3
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
