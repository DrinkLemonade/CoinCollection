using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer i;
    public AudioSource source;
    private void Awake()
    {
        i = this;
    }
}
