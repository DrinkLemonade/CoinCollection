using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    [SerializeField]
    float intensity = 0;
    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientIntensity = intensity; // RenderSettings controls found in Lighting tab
        RenderSettings.reflectionIntensity = intensity;

        Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
        foreach (Light light in lights)
        {
            //light.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
