using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //new input system

public class PlayerController : MonoBehaviour
{
    //Components
    private Rigidbody rb;
    private Collider col;

    //Movement from inputs
    private float movementX;
    private float movementY;
    private float movementZ;

    //Speed and velocity
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 8f;
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 24f;
    Vector3 velocity;
    public float moveSpeed = 3;
    public float jumpSpeed = 6;

    //Jump flags, ground detection
    private bool jumpTriggered = false;
    private float distToGround;
    private bool canJump = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        distToGround = col.bounds.extents.y;
        Debug.Log("DistToGround is: " + distToGround);
    }
    private void OnMove(InputValue movementValue)
    {
        Debug.Log("Called OnMove");
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementVector = Vector2.ClampMagnitude(movementVector, 1f); //like Normalize but allows inputs between 0 and 1, e.g. gamepad stick
        //A magnitude of 1 is one "meter" per second, i.e. one Unit in Unity's measurement system

        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void OnJump(InputValue movementValue)
    {
        Debug.Log("Called OnJump. Is on ground: " + IsOnGround());
        if (IsOnGround()) jumpTriggered = false;
        if (!jumpTriggered && IsOnGround())
        {
            movementZ = jumpSpeed; //could add other stuff, potentially
            jumpTriggered = true;
        }

    }

    // Update is called once per frame
    void FixedUpdate() //More consistent for physics stuff than Update
    {
        //Input, instead of setting player's velocity directly (no inertia), sets the desired velocity (in meters per second).
        Vector3 desiredVelocity = new Vector3(movementX, movementZ, movementY) * maxSpeed;

        //Then, we change the current velocity until it matches the desired one.
        //To do this, we multiply max acceleration (in meters per one second) with how much time has passed between physics updates (in seconds), giving us the maximum change (extra meters) we can add to our velocity (meters per second)
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //Let's add this to velocity. However, if we add speed to reach a desired velocity, we risk going too fast.
        //And if we remove speed to reach a desired velocity, we risk slowing down too much.
        //So let's move velocity towards the desired amount, and if we're moving past that, set it to the desired amount instead.
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        //This is basically equivalent to doing:
        //"if velocity.x < desired.x then velocity.x = min between velocity+change OR desired
        //else if velocity.x > desired.x then velocity.x = max between velocity+change OR desired"

        //We now know how many meters we're moving every second (either to reach the desired amount, or we're already there)
        //Then we multiply that by how many seconds elapsed between physics updates, and that gives us meters to move
        Vector3 displacement = velocity * Time.deltaTime;
        transform.localPosition += displacement;

        //let's ditch the physics for a moment
        //#################
        //rb.AddForce(movement * moveSpeed);
        //if (jumpTriggered) movementZ = 0.0f;
    }

    bool IsOnGround()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

}
