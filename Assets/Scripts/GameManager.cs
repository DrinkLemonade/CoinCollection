using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    public GameSession currentSession;

    //Used by GameSession's constructor
    [SerializeField]
    int sessionStartSeconds = 90; //Currently acts as the max, too
    [SerializeField]
    int sessionCoinPointPool = 100; //How many points can be collected by getting all the coins

    [SerializeField]
    GameObject pausePanel;

    [NonSerialized] //I knew that had to exist
    public bool gameIsPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        NewGame();
        pausePanel.SetActive(false); //May need to remove this if I want to keep the pause panel disabled by default.
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        pausePanel.SetActive(false);
    }

    public void TogglePause()
    {
        gameIsPaused = !gameIsPaused; //Toggle
        if (gameIsPaused)
        {
            Time.timeScale = 0;
            //Make menus appear and stuff
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            //Make menus disappear
            pausePanel.SetActive(false);
        }
    }
}
