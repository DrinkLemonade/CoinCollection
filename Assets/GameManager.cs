using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    public KeyCode keyUp, keyDown, keyLeft, keyRight, keyJump, keyPause;
    //KeyCode[] controlScheme;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        i = this;
        SetDefaultControls();
    }

    void SetDefaultControls()
    {
        keyUp = KeyCode.Z;
        keyDown = KeyCode.S;
        keyLeft = KeyCode.Q;
        keyRight = KeyCode.D;
        keyJump = KeyCode.Space;
        keyPause = KeyCode.Return;
    }
}
