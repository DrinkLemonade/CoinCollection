using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    public GameSession currentSession;

    public bool gameIsPaused = false;

    [SerializeField]
    int coinPointPool = 100; //How many points can be collected by getting all the coins
    //Maybe I should put this in GameSession...? But everything in GameSession can't be modified from the editor, hm

    //public KeyCode keyUp, keyDown, keyLeft, keyRight, keyJump, keyPause;
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
        //SetDefaultControls();
    }

    /*void SetDefaultControls()
    {
        keyUp = KeyCode.Z;
        keyDown = KeyCode.S;
        keyLeft = KeyCode.Q;
        keyRight = KeyCode.D;
        keyJump = KeyCode.Space;
        keyPause = KeyCode.Return;
    }*/

    static void NewGame()
    {
        Debug.Log("------Starting a game...------");
        i.currentSession = new GameSession(i);
        i.currentSession.currentSecondsLeft = i.currentSession.startSecondsLeft;
        GUI.i.UpdateDisplay(i.currentSession);
    }

    public void TogglePause()
    {
        gameIsPaused = !gameIsPaused; //Toggle
        if (gameIsPaused)
        {
            Time.timeScale = 0;
            //Make menus appear and stuff
        }
        else
        {
            Time.timeScale = 1;
            //Make menus disappear
        }
    }
}
