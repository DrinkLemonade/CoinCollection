using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePickupFlame : MonoBehaviour
{
    [SerializeField]
    float timeSecondsAdded = 10f;
    [SerializeField]
    AudioClip myAudioClip;
    public void FlameCollected()
    {
        GameManager.i.currentSession.AddTime(timeSecondsAdded);
        //myAudioSource.PlayOneShot(myAudioSource.clip);
        SoundPlayer.i.source.PlayOneShot(myAudioClip);
        Destroy(gameObject);//.transform.parent.parent.gameObject); //This is SO stupid.
    }
}
