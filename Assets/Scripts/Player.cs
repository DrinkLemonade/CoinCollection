using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    int moveSpeed = 6;
    int jumpSpeed = 10;
    bool onGround = false;
    Rigidbody myRigidBody;
    enum State
    {
        IDLE,           // valeur 0
        ATTACK,     // valeur 1
        DIE             // valeur 2
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate() //runs in sync with the physics system
    {
        GetInput();
    }

    void Awake()
    {
        myRigidBody = GetComponent<Rigidbody>();
    }

    void GetInput()
    {
        int dirX = (Input.GetKey(GameManager.i.keyLeft) ? -1 : 0) + (Input.GetKey(GameManager.i.keyRight) ? 1 : 0);
        int dirZ = (Input.GetKey(GameManager.i.keyDown) ? -1 : 0) + (Input.GetKey(GameManager.i.keyUp) ? 1 : 0);
        MoveDirection(dirX, dirZ);

        bool jumpPressed = Input.GetKeyDown(GameManager.i.keyJump);
        bool jumpStaysPressed = Input.GetKey(GameManager.i.keyJump);
        bool jumpReleased = Input.GetKeyUp(GameManager.i.keyJump);
        if (jumpPressed && onGround) MoveJump();

        //for some reason it's working with X but not with keyJump?
        bool ascending = myRigidBody.velocity.y > 0;
        Debug.Log("Released: " + jumpReleased + ", ascending: " + ascending + ", not on ground: " + !onGround);
        //TODO: check that you can't StopJumpHere multiple times mid air
        if (!onGround && jumpReleased && ascending) StopJumpHere();
    }

    void MoveDirection(int moveX, int moveZ)
    {
        //moveX and Z can be -1, 0 or 1
        Vector3 dir = new Vector3(moveX, 0, moveZ);
        dir = Vector3.ClampMagnitude(dir, 1); //stops the ol' "moving diagonally is faster" problem
        transform.Translate(moveSpeed * Time.deltaTime * dir);
    }

    void MoveJump()
    {
        myRigidBody.AddForce(jumpSpeed * Time.deltaTime * Vector3.up, ForceMode.Impulse);
    }

    void StopJumpHere()
    {
        Debug.Log("Jump released");
        myRigidBody.AddForce(myRigidBody.velocity.y/2 * Time.deltaTime * Vector3.down, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
        Debug.Log("Le joueur entre en collision avec " + collision.transform.name);
        onGround = true;
    }
    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Le joueur n'est plus en collision avec " + collision.transform.name);
        onGround = false;
    }

}
