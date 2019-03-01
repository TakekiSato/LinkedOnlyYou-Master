using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatVoiceManager : MonoBehaviour
{
    public AudioClip[] voiceTable = new AudioClip[1];

    const int VOICE_MAX = 11;

    AudioSource[] audioSources = new AudioSource[VOICE_MAX];

    int nowVoiceIndex = 0;

    public enum VOICE_LIST
    {
        これから
            , ああああ
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
        if (nowVoiceIndex >= VOICE_MAX)
        {
            nowVoiceIndex = 0;
        }
    }
}
