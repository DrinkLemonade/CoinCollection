using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Unity.VisualScripting.Member;

public class Coin : MonoBehaviour
{
    float rot = 0;
    private readonly float angleIncreasePerSecond = 90;

    [Range(0, 10)]
    public int pointsWorth = 0;

    [SerializeField]
    Transform myTransform;

    [SerializeField]
    ParticleSystem myParticleSystem;
    [SerializeField]
    Light myLight;

    [SerializeField]
    AudioClip myAudioClip;

    [SerializeField]
    Renderer myRenderer;

    void Start()
    {
        Color myColor = myRenderer.material.color;

        var colLife = myParticleSystem.colorOverLifetime;
        colLife.enabled = true;
        Gradient grad = new();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(myColor, 0.0f), new GradientColorKey(myColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
        colLife.color = grad;

        myLight.color = myColor;
    }

    public void CoinCollected()
    {
        GameManager.i.currentSession.AddScore(pointsWorth);
        //myAudioSource.PlayOneShot(myAudioSource.clip);
        SoundPlayer.i.source.PlayOneShot(myAudioClip);
        Destroy(gameObject);//.transform.parent.parent.gameObject); //This is SO stupid.
    }

    // Update is called once per frame
    void Update()
    {
        rot += angleIncreasePerSecond * Time.deltaTime;
        myTransform.localRotation = Quaternion.Euler(0f, rot, myTransform.localRotation.z);
    }
}
