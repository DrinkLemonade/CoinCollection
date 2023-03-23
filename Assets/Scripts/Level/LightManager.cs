using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LightManager : MonoBehaviour
{
    [SerializeField]
    float intensity = 5;
    [SerializeField]
    float maxIntensity = 20;

    [SerializeField]
    float flameLightRange = 0;
    [SerializeField]
    float flameLightRangeMax = 20;

    [SerializeField]
    bool affectAmbientLight = true;
    [SerializeField]
    bool affectPlayerLight = true;

    [SerializeField]
    Light playerLight;

    [SerializeField]
    Transform playerFlameTransform;
    [SerializeField]
    float flameMinScale = 0.1f;
    [SerializeField]
    float flameMaxScale = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        //sessionInfo = GameManager.i.currentSession;
        RenderSettings.ambientLight = Color.black;
        //SetAllLightsEnabledStatus(false);
    }

    // Update is called once per frame
    void Update()
    {
        float secsLeft = GameManager.i.currentSession.currentSecondsLeft;
        float secsMax = GameManager.i.currentSession.startSecondsLeft;
        float secsElapsed = secsMax - secsLeft;
        intensity = (secsElapsed * maxIntensity) / secsMax;

        //Exponential at the end
        if (secsLeft < 10) intensity *= ((10-secsLeft)*10);

        if (affectPlayerLight)
        {
            playerLight.intensity = intensity;
            //Debug.Log("Intensity: " + intensity);
        }
        if (affectAmbientLight)
        {
            //Divide intensity from 0-max to 0-1f, which ambient light uses
            intensity /= maxIntensity;
            RenderSettings.ambientIntensity = intensity; // RenderSettings controls found in Lighting tab
            RenderSettings.reflectionIntensity = intensity;
        }

        float flameIncrease = (secsElapsed * (flameMaxScale - flameMinScale)) / secsMax;
        float flameScale = flameMinScale + flameIncrease;
        playerFlameTransform.localScale = new Vector3(flameScale, flameScale, flameScale);

        float playerLightRange = (secsElapsed * (flameLightRangeMax - flameLightRange)) / secsMax;
        playerLight.range = playerLightRange;
    }

    void SetAllLightsEnabledStatus(bool choice)
    {
        Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
        foreach (Light light in lights)
        {
            light.enabled = choice;
        }
    }
}
