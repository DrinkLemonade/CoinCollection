using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    public GameSession currentSession;

    public KeyCode keyUp, keyDown, keyLeft, keyRight, keyJump, keyPause;
    //KeyCode[] controlScheme;

    // Start is called before the first frame update
    void Start()
    {
        NewGame();
    }

    // Update is called once per frame
    void Update()
    {
        currentSession.currentSecondsLeft -= Time.deltaTime;
        if (currentSession.currentSecondsLeft <= 0) currentSession.currentSecondsLeft = 0;
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

    static void NewGame()
    {
        Debug.Log("------Starting a game...------");
        i.currentSession = new GameSession(i);
        i.currentSession.currentSecondsLeft = i.currentSession.startSecondsLeft;
        GUI.i.UpdateDisplay(i.currentSession);
    }
}
