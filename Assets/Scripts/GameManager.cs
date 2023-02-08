using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    public GameSession currentSession;

    //Used by GameSession's constructor
    [SerializeField]
    int sessionStartSeconds = 90; //Currently acts as the max, too
    [SerializeField]
    int sessionCoinPointPool = 100; //How many points can be collected by getting all the coins

    [NonSerialized] //I knew that had to exist
    public bool gameIsPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        NewGame();
    }

    // Update is called once per frame
    void Update()
    {
        i.currentSession.currentSecondsLeft -= Time.deltaTime;
        if (i.currentSession.currentSecondsLeft <= 0) i.currentSession.currentSecondsLeft = 0;
    }

    private void Awake()
    {
        i = this;
    }

    static void NewGame()
    {
        Debug.Log("------Starting a game...------");
        i.currentSession = new GameSession(i, i.sessionStartSeconds);
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
