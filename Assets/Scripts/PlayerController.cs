using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //new input system

public class PlayerController : MonoBehaviour
{
    //Debug
    [SerializeField]
    bool debugging = false;
 
    //Controls
    public GameControls controls;

    //Components
    private Rigidbody body;

    //Movement from inputs
    private float movementX;
    private float movementY;

    //Speed and velocity
    Vector3 velocity, desiredVelocity;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 8f;
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 24f, maxAirAcceleration = 2.4f;
    [SerializeField, Range(0f, 10f)]
    private float jumpHeight = 6; //in Unity units (meters)

    //Jump flags
    bool desiredJump; //Set in Update and used in FixedUpdate
    bool desiredJumpRelease;
    bool jumpReleaseUsed = false;

    //Ground and slope detection
    int groundContactCount;
    bool OnGround => groundContactCount > 0; //shorthand way to define a single-statement readonly property
    //It's the same as: bool OnGround { get { return groundContactCount > 0; } }
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;
    float minGroundDotProduct;
    bool jumpAwayFromSlopes = true;
    Vector3 contactNormal;

    //Sphere mode - might use for stuff?
    bool sphereMode = true;

    void Awake()
    {
        controls = new GameControls();
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        Vector2 movementVector = controls.Player.Move.ReadValue<Vector2>();
        //We clamp the vector, which is like Normalize but allows inputs between 0 and 1, e.g. gamepad stick
        //A magnitude of 1 is one "meter" per second, i.e. one Unit in Unity's measurement system
        movementVector = Vector2.ClampMagnitude(movementVector, 1f);
        movementX = movementVector.x;
        movementY = movementVector.y;
        //Input, instead of setting player's velocity directly (no inertia), sets the desired velocity (in meters per second).
        desiredVelocity = new Vector3(movementX, 0f, movementY) * maxSpeed;

        //Jumping
        //If we don't invoke FixedUpdate next frame, normally the desire to jump would be forgotten.
        //We remember it using the OR operand, equivalent to x = x || y. Now it remains true once enabled, until explicitly made false.
        desiredJump |= controls.Player.Jump.triggered;
        desiredJumpRelease |= controls.Player.Jump.WasReleasedThisFrame();
        if (desiredJump && debugging) Debug.Log("Jump desired!");
        if (desiredJumpRelease && debugging) Debug.Log("Jump release desired!");
    }
    void FixedUpdate()
    {
        //"The FixedUpdate method gets invoked at the start of each physics simulation step. How often that happens depends on the time step, which is 0.02—fifty times per second—by default, but you can change it via the Time project settings or via Time.fixedDeltaTime."
        //"Can we use Time.deltaTime in FixedUpdate? Yes. When FixedUpdate gets invoked Time.deltaTime is equal to Time.fixedDeltaTime."
        //"Depending on your frame rate FixedUpdate can get invoked zero, one, or multiple times per invocation of Update. Each frame a sequence of FixedUpdate invocations happen, then Update gets invoked, then the frame gets rendered. This can make the discrete nature of the physics simulation obvious when the physics time step is too large relative to the frame time."
        //"You can solve that by either decreasing the fixed time step or by enabling the Interpolate mode of a Rigidbody. Setting it to Interpolate makes it linearly interpolate between its last and current position, so it will lag a bit behind its actual position according to PhysX. The other option is Extrapolate, which interpolates to its guessed position according to its velocity, which is only really acceptable for objects that have a mostly constant velocity.
        //Note that increasing the time step means the sphere covers more distance per physics update, which can result in it tunneling through the walls when using discrete collision detection."

        //Let's grab the current velocity from the RigidBody, so we know what we want to adjust to match the desired velocity.
        velocity = body.velocity;

        if (OnGround)
        {
            if (groundContactCount > 1) //"only bothering to normalize the contact normal if it's an aggregate, as it's already unit-length otherwise."
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up; //if jumping, don't fall back towards slopes!
        }
        AdjustVelocity();

        //Handle jump
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        if (desiredJumpRelease)
        {
            desiredJumpRelease = false;
            JumpRelease();
        }

        //We now know how many meters we're moving every second (either to reach the desired amount, or we're already there)
        //If we didn't use physics, we'd multiply that by how many seconds elapsed between physics updates, and that'd give us meters to move, and we could move the player by hand.
        //Instead, because it's a physics object, we give its RigidBody that velocity and let the physics system handle it.
        body.velocity = velocity;
        ClearState();
    }

    void Jump()
    {
        if (OnGround)
        {
            //EXTREMELY smart maths from CatlikeCoding that determine the velocity needed to jump a certain height.
            //"Note that we most likely fall a bit short of the desired height due to the discrete nature of the physics simulation. The maximum would be reached somewhere in between time steps."
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            velocity += contactNormal * jumpSpeed;
        }
    }
    void JumpRelease()
    {
        if (!OnGround && !jumpReleaseUsed)
        {
            jumpReleaseUsed = true;
            if (debugging) Debug.Log("Decreasing jump height...");
            velocity.y /= 2;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }
    void OnCollisionStay(Collision collision)
    {
        //Use Stay, not Exit, because exiting collision with e.g. a wall would make jumping impossible
        EvaluateCollision(collision);

        //"Each physics step begins with invoking all FixedUpdate methods, after which PhysX does its thing, and at the end the collision methods get invoked. So when FixedUpdate gets invoked onGround will have been set to true during the last step if there were any active collisions. All we have to do to keep onGround valid is to set it back to false at the end of FixedUpdate."
    }

    void EvaluateCollision(Collision collision)
    {
        //"The amount of contact points can by found via the contactCount property of Collision. We can use that to loop through all points via the GetContact method, passing it an index. Then we can access the point's normal property."
        for (int i = 0; i < collision.contactCount; i++)
        {
            //We're using normal vectors to determine what's the ground. A normal vector points away from the center
            //Planes have only 1 normal vector, pointing straight up. (Spheres have many, pointing away)
            Vector3 normal = collision.GetContact(i).normal;

            if (normal.y >= minGroundDotProduct)
            {
                groundContactCount += 1;
                if (jumpAwayFromSlopes) contactNormal += normal;
                else contactNormal += Vector3.up;
                jumpReleaseUsed = false;
            }
            
            //else onGround |= normal.y >= minGroundDotProduct;
            //When a surface is horizontal the Y component of its normal vector is 1. For a perfectly vertical wall the Y component is zero.
        }
    }
    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad); //Dot product: Basically, the 2 vectors that make up a slope (flat ground + sloped ground) "cast a shadow" on each other, as if creating two right triangles. The length of the bottom sides of one such triangle is the result of the dot product. If both vectors are "unit length" (the same length? I'm not sure), the dot product is also the cosine of their angle (?) (I'm not good at geometry)
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }
    void AdjustVelocity()
    {
        //Calculate a new velocity relative to the ground, when moving down a slope.
        //When running down a slope, normally, the player will run off the slope and bounce awkwardly downwards, instead of sticking to the slope. We can use ProjectOnContactPlane to take the movement vector, and project it on the slope's normal.
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        //Let's add this to velocity. However, if we add speed to reach a desired velocity, we risk going too fast.
        //And if we remove speed to reach a desired velocity, we risk slowing down too much.
        //So let's move velocity towards the desired amount, and if we're moving past that, set it to the desired amount instead.
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;


        //Let's add this to velocity. However, if we add speed to reach a desired velocity, we risk going too fast.
        //And if we remove speed to reach a desired velocity, we risk slowing down too much.
        //So let's move velocity towards the desired amount, and if we're moving past that, set it to the desired amount instead.
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
        //This is basically equivalent to doing:
        //"if velocity.x < desired.x then velocity.x = min between velocity+change OR desired
        //else if velocity.x > desired.x then velocity.x = max between velocity+change OR desired"

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    protected void OnEnable()
    {
        controls.Player.Enable();
    }

    protected void OnDisable()
    {
        controls.Player.Disable();
    }

}
