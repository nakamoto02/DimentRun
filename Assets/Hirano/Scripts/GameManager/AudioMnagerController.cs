using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMnagerController : SingletonMonoBehaviour<AudioMnagerController>
{
    public static AudioSource BGM1;
    public static AudioSource BGM2;
    public static AudioSource SE1;
    public static AudioSource SE2;
    // Use this for initialization
    void Start ()
    {
        AudioSource[] audiosorce = GetComponents<AudioSource>();
        BGM1 = audiosorce[0];
        BGM2 = audiosorce[1];
        SE1 = audiosorce[2];
        SE2 = audiosorce[3];
    }

    public void BGMFadeOut()
    {
        BGM1.volume -= 0.3f * Time.deltaTime;
    }
}
