using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Coin : MonoBehaviour
{
    float rot = 0;
    private readonly float rotSpeed = 0.5f;

    [Range(0, 10)]
    public int pointsWorth = 0;

    [SerializeField]
    Transform myTransform;

    [SerializeField]
    ParticleSystem myParticleSystem;
    [SerializeField]
    Color myParticleColor;
    // Start is called before the first frame update
    void Start()
    {
        var col = myParticleSystem.colorOverLifetime;
        col.enabled = true;

        Gradient grad = new();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(myParticleColor, 0.0f), new GradientColorKey(myParticleColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        col.color = grad;
    }

    // Update is called once per frame
    void Update()
    {
        rot += rotSpeed;
        myTransform.localRotation = Quaternion.Euler(0f, rot, myTransform.localRotation.z);
    }
}
