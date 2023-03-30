using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem; //new input system
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //Layers
    //Our player's parent object (not children) is on the Agent layer (an active entity, not part of the level geometry)
    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1; //Ground raycasts probe all layers by default, but we'll change that in the editor, so we don't probe "ignore raycasts" and agents
    //"by using a mask we don't rely on a hard-coded layer name and are more flexible, which also makes experimentation easier."

    //Animation and model
    [SerializeField]
    PlayerAnimatorConfig animationConfig = default;
    Quaternion lastDirectionLooked; //Which direction we're looking
    //PlayerAnimator animator;
    [SerializeField]
    Animator unityAnimator;
    [SerializeField]
    GameObject playerModelObject;

    //Debug
    [SerializeField]
    bool debugging = false;
    
    //Controls and input
    public GameControls controls;
    [SerializeField]
    Transform playerInputSpace = default; //How we determine the relative direction the controls move player towards, e.g. the orbit camera's space. By default this is the player, so movement is always relative to self, not to camera or anything

    //Bodies
    private Rigidbody body, connectedBody, previousConnectedBody;
    public bool OnConnectedBody => connectedBody;
    Vector3 connectionWorldPosition, connectionLocalPosition; //An object animated by kinematics has no real velocity, so we figure out what it *should* be by keeping track of how much it's moved. We also keep track of where we're connected relative to the origin, so if we're on a rotating platform, we can orbit around the platform's origin. 

    //Movement from inputs
    private float movementX;
    private float movementY;

    //Speed and velocity
    Vector3 velocity, relativeVelocity, desiredVelocity, connectionVelocity; //Connection is used for moving platforms etc. Relative is used for animation
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

    //Ground detection and slopes
    int groundContactCount, steepContactCount;
    public bool OnGround => groundContactCount > 0; //shorthand way to define a single-statement readonly property. It's the same as: bool OnGround { get { return groundContactCount > 0; } }
    //Public because DecalShadow needs it
    public bool OnSteep => steepContactCount > 0;
    int stepsSinceLastGrounded, stepsSinceLastJump;
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;
    float minGroundDotProduct, minStairsDotProduct;
    [SerializeField]
    bool alwaysJumpStraightUp = true; //if not on steep ground
    Vector3 contactNormal, steepNormal; //anything too steep to count as ground, but isn't a wall, ceiling, or anything in between
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f; //Max speed at which we'll snap to slopes instead of flying off. "Note that setting both max speeds to the same value can produce inconsistent results due to precision limitations. It's better to make the max snap speed a bit higher or lower than the max speed."
    [SerializeField, Min(0f)]
    float probeDistance = 2.5f; //How far down we check for ground to snap down to, instead of flying off. Our little knight's center is 2 units off the floor, so check 0.5 units below its feet. "If too low, snapping can fail at steep angles or high velocities, while too high can lead to nonsensical snapping to ground far below."

    [SerializeField]
    AudioClip jumpAudio, landAudio;

    void Awake()
    {
        controls = new GameControls();
        body = GetComponent<Rigidbody>();
        OnValidate();
        UpdateAnimation();
        // Play("Idle_SwordShield");
        //animator.PlayIdle(animationConfig.IdleAnimationSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
        //Movement
        Vector2 movementVector = controls.Player.Move.ReadValue<Vector2>();
        //We clamp the vector, which is like Normalize but allows inputs between 0 and 1, e.g. gamepad stick
        //A magnitude of 1 is one "meter" per second, i.e. one Unit in Unity's measurement system
        movementVector = Vector2.ClampMagnitude(movementVector, 1f);
        movementX = movementVector.x;
        movementY = movementVector.y;

        //Input, instead of setting player's velocity directly (no inertia), sets the desired velocity (in meters per second).
        //But first, let's see if we're determining velocity relative to something, e.g. the camera
        if (playerInputSpace)
        {
            //Don't take the camera's vertical angle into account!
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            desiredVelocity = (forward * movementY + right * movementX) * maxSpeed;
        }
        else
        {
            desiredVelocity = new Vector3(movementX, 0f, movementY) * maxSpeed;
        }

        if (desiredVelocity.magnitude > 0.01f && OnGround && !GameManager.i.gameIsPaused) //Not sure why this happens during paused
        {
            playerModelObject.transform.rotation = Quaternion.Slerp(playerModelObject.transform.rotation, Quaternion.LookRotation(desiredVelocity), 0.15F);
        }

        //Jumping
        //If we don't invoke FixedUpdate next frame, normally the desire to jump would be forgotten.
        //We remember it using the OR operand, equivalent to x = x || y. Now it remains true once enabled, until explicitly made false.
        desiredJump |= controls.Player.Jump.triggered;
        desiredJumpRelease |= controls.Player.Jump.WasReleasedThisFrame();

        if (GameManager.i.gameIsPaused)
        {
            desiredJump = false;
            desiredJumpRelease = false;
        }
        if (desiredJump && debugging) Debug.Log("Jump desired!");
        if (desiredJumpRelease && debugging) Debug.Log("Jump release desired!");

        if (controls.UI.Unpause.WasReleasedThisFrame() && GameManager.i.gameIsPaused) //Pause Menu
        {
            GameManager.i.TogglePause();
        }
    }
    void FixedUpdate()
    {
        //"The FixedUpdate method gets invoked at the start of each physics simulation step. How often that happens depends on the time step, which is 0.02 -fifty times per second- by default, but you can change it via the Time project settings or via Time.fixedDeltaTime."
        //"Can we use Time.deltaTime in FixedUpdate? Yes. When FixedUpdate gets invoked Time.deltaTime is equal to Time.fixedDeltaTime."
        //"Depending on your frame rate FixedUpdate can get invoked zero, one, or multiple times per invocation of Update. Each frame a sequence of FixedUpdate invocations happen, then Update gets invoked, then the frame gets rendered. This can make the discrete nature of the physics simulation obvious when the physics time step is too large relative to the frame time."
        //"You can solve that by either decreasing the fixed time step or by enabling the Interpolate mode of a Rigidbody. Setting it to Interpolate makes it linearly interpolate between its last and current position, so it will lag a bit behind its actual position according to PhysX. The other option is Extrapolate, which interpolates to its guessed position according to its velocity, which is only really acceptable for objects that have a mostly constant velocity.
        //Note that increasing the time step means the sphere covers more distance per physics update, which can result in it tunneling through the walls when using discrete collision detection."

        UpdateState();
        AdjustVelocity();

        if (GameManager.i.gameIsPaused)
        {
            desiredJump = false;
            desiredJumpRelease = false;
        }
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
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        } //could do else if OnSteep jumpDirection = steepNormal, to allow jumping off steep surfaces
        else return;//can't jump

        //EXTREMELY smart maths from CatlikeCoding that determine the velocity needed to jump a certain height.
        //"Note that we most likely fall a bit short of the desired height due to the discrete nature of the physics simulation. The maximum would be reached somewhere in between time steps."
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        if (alwaysJumpStraightUp) jumpDirection = Vector3.up;
        else jumpDirection = (jumpDirection + Vector3.up).normalized; //jump is averaged between straight up, and away from ground's angle
        //we could also make it Vecor3.up for pure upwards jumping

        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;

        stepsSinceLastJump = 0;
        unityAnimator.SetTrigger("Jump");
        SoundPlayer.i.source.PlayOneShot(jumpAudio);
    }
    void JumpRelease()
    {
        if (!OnGround && !jumpReleaseUsed && velocity.y > 0)
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
        float minDot = GetMinDot(collision.gameObject.layer); //check if we're touching ground and not something else

        //"The amount of contact points can by found via the contactCount property of Collision. We can use that to loop through all points via the GetContact method, passing it an index. Then we can access the point's normal property."
        for (int i = 0; i < collision.contactCount; i++)
        {
            //We're using normal vectors to determine what's the ground. A normal vector points away from the center
            //Planes have only 1 normal vector, pointing straight up. (Spheres have many, pointing away)
            Vector3 normal = collision.GetContact(i).normal;

            if (normal.y >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                jumpReleaseUsed = false;
                connectedBody = collision.rigidbody; //What we're standing on - used by moving platforms
            }
            else if (normal.y > -0.01f) //If we find ourselves wedged inside a narrow space, with multiple steep contacts, then we might be able to move by pushing against those contact points.
            {
                steepContactCount += 1;
                steepNormal += normal;
                if (groundContactCount == 0) //Only accept a slope's rigidbody if there isn't a ground rigidbody to use
                {
                    connectedBody = collision.rigidbody;
                }
            }

            //else onGround |= normal.y >= minGroundDotProduct;
            //When a surface is horizontal the Y component of its normal vector is 1. For a perfectly vertical wall the Y component is zero.
        }
    }
    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad); //Dot product: Basically, the 2 vectors that make up a slope (flat ground + sloped ground) "cast a shadow" on each other, as if creating two right triangles. The length of the bottom sides of one such triangle is the result of the dot product. If both vectors are "unit length" (the same length? I'm not sure), the dot product is also the cosine of their angle (?) (I'm not good at geometry)
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;

        //Let's grab the current velocity from the RigidBody, so we know what we want to adjust to match the desired velocity.
        velocity = body.velocity;

        if (OnGround || SnapToGround() || CheckSteepContacts()) //SnapToGround will only get invoked when OnGround is false. Pretty cool.
        {
            if (stepsSinceLastGrounded > 1) DoLandingStuff();
            stepsSinceLastGrounded = 0;
            if (groundContactCount > 1) //"only bothering to normalize the contact normal if it's an aggregate, as it's already unit-length otherwise."
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up; //If jumping, don't fall back towards slopes!
        }

        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass) //Don't send yourself flying after, say, kicking a rock; only move alongside an object if it's big enough to move you, or using kinematics
            {
                UpdateConnectionState();
            }
        }
    }

    void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody) //If we're still standing on the same body
        {
            //Figure out connected body's velocity. Find where we are relative to its origin, and convert that into world space (absolute?), because we know the body's transform's current position relative to where it was placed. Then, substract its world position from that. If there is no rotation nothing changes, but if there is, we take orbit into account, if I understand correctly.
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
            connectionWorldPosition = body.position; //Since we're still standing on something, we know the connection point is where we are. That means we can figure out where we are *relative to the connected body's origin*... again, if I understand correctly.
            connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
        }
    }
    void AdjustVelocity()
    {
        //Calculate a new velocity relative to the ground, when moving down a slope.
        //When running down a slope, normally, the player will run off the slope and bounce awkwardly downwards, instead of sticking to the slope. We can use ProjectOnContactPlane to take the movement vector, and project it on the slope's normal.
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        //We want to accelerate to match the speed of whatever we're connected to, on top of accelerating toward a desired velocity relative to the connection velocity.
        relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

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

        FaceVelocityDirection();
    }
    void ClearState()
    {
        groundContactCount = steepContactCount = 0; //Today I learned you can do that
        contactNormal = steepNormal = connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
    }
    bool SnapToGround()
    {
        //Attempts to keep the player on the ground (to prevent e.g. flying off ramps). Returns whether attempt was successful

        //Abort if the player has been off the ground for more than 1 step, or 2 or fewer steps after a jump.
        //Why not 1 step? "Because of the collision data delay we're still considered grounded the step after the jump was initiated."
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }

        //Abort if we're moving off so fast, we should fly off instead of snapping to the ground.
        float speed = velocity.magnitude; //Length of the vector, i.e. not any direction info
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        //Abort if there's no ground below that we can snap to. The optional third paremeter here allows us to see what the ray hit!
        //"RaycastHit is a struct, thus a value type. We can define a variable via RaycastHit hit, then pass it as a third argument to Physics.Raycast. But it's an output argument, which means that it's passed by reference as if it were an object reference. This must be explicitly indicated by adding the out modifier to it. The method is responsible for assigning a value to it. Besides that, it's also possible to declare the variable used for the output argument inside the argument list, instead of on a separate line. That's what we do here."
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
        {
            //The hit data includes a normal vector, which we can use to check whether the surface we hit counts as ground.
            return false;
        }

        //If we did hit something, check that it counts as ground (e.g. not a slope that's too steep)
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        //If we haven't aborted at this point then we've just lost contact with the ground but are still above ground, so we snap to it.
        //The normal we found is going to become our contact normal.
        groundContactCount = 1;
        contactNormal = hit.normal;

        //Adjust velocity to align with the ground
        //"This works just like aligning the desired velocity, except that we have to keep the current speed and we'll calculate it explicitly instead of relying on ProjectOnContactPlane."
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f) //If we're already going downwards, don't realign 
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
            //Okay, I can't really visualize how that realigning part works. TODO: research
        }
        connectedBody = hit.rigidbody;
        return true; //NOTE: we'll still be ungrounded for like, a frame, so keep that in mind
    }
    float GetMinDot(int layer)
    {
        //Check if the layer is the stairs mask, and if so use that for minimum dot product, otherwise use normal ground
        return (stairsMask & (1 << layer)) == 0 ? //"The mask is a bit mask, with one bit per layer. If the stairs is the eleventh layer then it matches the eleventh bit. We can create a value with that single bit set by using 1 << layer, which applies the left-shift operator to the number 1 an amount of times equal to the layer index, which is ten. The result would be the binary number 10000000000." "That would work if the mask has only a single layer selected, but let's support a mask for any combination of layers. We do that by taking the boolean AND of the mask and layer bit. If the result is zero then the layer is not part of the mask."
            minGroundDotProduct : minStairsDotProduct;
    }
    bool CheckSteepContacts() //Used as an OnGround substitute when stuck in, say, a crevasse.
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    //New Input System: enable/disable controls
    protected void OnEnable()
    {
        controls.Player.Enable();
    }
    protected void OnDisable()
    {
        controls.Player.Disable();
    }

    void UpdateAnimation()
    {
        //unityAnimator.ResetTrigger("Fall");
        //unityAnimator.ResetTrigger("Land");

        //Si deplacement vecteur Z negatif etc passer un booleen?
        //Debug.Log("udpating animation! speed is: " + relativeVelocity.magnitude);
        Vector2 getAnimSpeed; //Don't take vertical (Y) velocity into account
        getAnimSpeed.x = relativeVelocity.x; //Use velocity related to connected body, not absolute, otherwise a platform moving down would trigger the falling animation
        getAnimSpeed.y = relativeVelocity.z;
        unityAnimator.SetFloat("speed", getAnimSpeed.magnitude);
        //unityAnimator.ResetTrigger("Jump");
        if (OnGround || OnSteep)
        {

        }
        else
        {
            unityAnimator.ResetTrigger("Land");
            if (velocity.y < 0) //Descending
            {
                unityAnimator.SetTrigger("Fall");
                unityAnimator.ResetTrigger("Jump");
                //Debug.Log("Falling!");
            }
        }
    }

    void DoLandingStuff() //Used when player hits the ground
    {
        unityAnimator.ResetTrigger("Jump");
        unityAnimator.ResetTrigger("Fall");
        unityAnimator.SetTrigger("Land");

        Debug.Log("Just landed!");
        SoundPlayer.i.source.PlayOneShot(landAudio);
    }

    void FaceVelocityDirection()
    {
        //Make rotation equal to the angle of our current velocity
        //TODO: See if I need to normalize...?
        //Quaternion newAngle = Quaternion.Euler(body.velocity.x, 0f, body.velocity.z);
        //Vector3 xyVelocity = new Vector3(velocity.x, 0f, velocity.z);
        //playerModelObject.GetComponent<Transform>().eulerAngles = xyVelocity;//Rotate(0f, , 0f);
        //= newAngle;
    }

}