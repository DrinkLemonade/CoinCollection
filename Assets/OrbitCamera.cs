using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default; //What we focus on; we set this to the player in the editor. Because the physics engine adjusts the sphere's position at a fixed interval, the camera will too, so it'll look weird. We can fix this by telling the player's rigidbody to interpolate its position
    [SerializeField, Range(1f, 20f)]
    float distance = 5f; //How far from the focus the camera orbits

    [SerializeField, Min(0f)]
    float focusRadius = 5f; //When player moves a certain distance from center of camera, start moving camera
    Vector3 focusPoint, previousFocusPoint; //Of course, now we need to track the point we're focusing on, we're no longer looking right at the focus object
    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f; //Speed at which we center back on the player when the camera stops moving

    Camera regularCamera;
    Vector2 orbitAngles = new Vector2(45f, 0f);

    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f; //Don't allow camera rotation to get too weird

    [SerializeField, Min(0f)]
    float alignDelay = 5f; //Automatically align the camera's angle behind the player after this delay
    float lastManualRotationTime;

    // Start is called before the first frame update
    void Awake()
    {
        regularCamera = GetComponent<Camera>();
        focusPoint = focus.position; //Position instead of localPosition lets us focus correctly on child objects
        transform.localRotation = Quaternion.Euler(orbitAngles); //Make sure we respect the angle constraints on start
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Happens after update?
    void LateUpdate()
    {
        UpdateFocusPoint(); //Did the player move? If so, find new focus point

        //Find how we're rotating, and constrain the rotation if necessary
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles); 
        }
        else
        {
            lookRotation = transform.localRotation;
        }

        Vector3 lookDirection = lookRotation * Vector3.forward; //Then, adjust our direction
        Vector3 lookPosition = focusPoint - lookDirection * distance; //We find position by moving it away from the focus's position in the opposite direction that the focus is looking
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f) //If we have a focus radius, i.e. a distance from central point past which camera starts moving...
        {
            float distance = Vector3.Distance(targetPoint, focusPoint); //Calculate distance between current focus point and target
            float t = 1f; //Keep track of the elapsed time, so we can halve the distance every second. This will be our lerping speed
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime); //Unscaled lets us ignore pausing, slow-mo, etc
            }

            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance); //I *think* what this does is enforce we don't move past the focus
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t); //Interpolate between current focus point and target point, at the determined speed
            //NOTE: see how lerp stuff works in more detail)
        }
        else
        {
            focusPoint = targetPoint; //Just focus on the target object bro
        }
    }
    bool ManualRotation()
    {
        //Returns true if changes to rotation were made, false otherwise
        GameControls playerControls = focus.GetComponent<PlayerController>().controls;
        Vector2 input = playerControls.Player.Look.ReadValue<Vector2>();
        //Swap X and Y because it's inverted for some reason
        float temp;
        temp = input.x;
        input.x = input.y;
        input.y = temp;
        
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }
    void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false; //Don't start until the player hasn't touched manual rotation until (alignDelay) time
        }

        //Given the current focus point and the last one, we can deduce where the player is heading, and rotate in that direction
        Vector2 movement = new Vector2
        (
            focusPoint.x - previousFocusPoint.x,
            focusPoint.z - previousFocusPoint.z
        );
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f) //Player is barely moving and we don't need to rotate in that direction
        {
            return false;
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr)); //Pass the normalized movement vector to headingAngle
        orbitAngles.y = headingAngle; //New horizontal orbit angle

        return true;
    }
    static float GetAngle(Vector2 direction)
    {
        //Use the player's XZ movement vector to determine what angle the camera should point at in advance
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg; //Y component of the direction is the cosine of the angle we need (lol, math)
        return direction.x < 0f ? 360f - angle : angle; //If the X component is negative then we rotate counter-clockwise and must subtract the angle from 360
    }

    void OnValidate()
    {
        //Sanitize input in the inspector so max isn't inferior to min
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }
}
