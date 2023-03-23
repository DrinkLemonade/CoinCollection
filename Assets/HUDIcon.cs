using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDIcon : MonoBehaviour
{
    [SerializeField]
    CanvasRenderer myRend;
    [SerializeField]
    RectTransform myRT;
    [SerializeField]
    Color defaultColor, glowColor;
    [SerializeField]
    float baseScale, growMaxScale;

    [SerializeField]
    float amplitude, frequency, speed = 1f; //Amplitude of the sinewave

    private Vector3 startPosition; // Starting position of the object
    private float time = 0f; // Time elapsed since start

    // Update is called once per frame
    void Update()
    {
        //float changeScale = Mathf.Sin(Time.deltaTime); //Between 0 and 1
        //Debug.Log(changeScale);
        //changeScale *= (growMaxScale - changeScale); //If we grow from 75 to 100, the change is between 0 and 25
        float scale = baseScale + (GetSineWave());
        myRT.sizeDelta = new Vector2(scale, scale);
        myRend.SetColor(defaultColor);

        //localScale = new Vector3(scale,scale,scale);

    }

    private float GetSineWave()
    {
        time += Time.deltaTime * speed; //Update the time elapsed

        //Calculate the new position based on the sinewave equation
        return amplitude * Mathf.Sin(2f * Mathf.PI * frequency * time);
    }
}
