using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;
    [SerializeField, Min(0f)]
    float focusRadius = 1f; //When player moves a certain distance from center of camera, start moving camera
    Vector3 focusPoint; //Of course, now we need to track the point we're focusing on, we're no longer looking right at the focus object

    [SerializeField, Range(1f, 20f)]
    float distance = 5f;
    // Start is called before the first frame update
    void Start()
    {
        focusPoint = focus.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Happens after update?
    void LateUpdate()
    {
        UpdateFocusPoint(); //Did the player move?

        //We find position by moving it away from the focus's position in the opposite direction that the focus is looking
        Vector3 focusPoint = focus.position; //position instead of localPosition lets us focus correctly on child objects
        Vector3 lookDirection = transform.forward;
        transform.localPosition = focusPoint - lookDirection * distance;

        //Because the physics engine adjusts the sphere's position at a fixed interval, the camera will too, so it'll look weird
        //We can fix this by telling the player's rigidbody to interpolate its position!
    }
    void UpdateFocusPoint()
    {
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f) //If we have a focus radius, i.e. a distance from central point past which camera starts moving...
        {
            float distance = Vector3.Distance(targetPoint, focusPoint); //calculate distance between current focus point and target
            if (distance > focusRadius)
            {
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, focusRadius / distance); //interpolate using the radius divided by current distance as the interpolator (NOTE: see how lerp stuff works in more detail)
            }
        }
        else
        {
            focusPoint = targetPoint; //Just focus on the target object bro
        }
    }
}
