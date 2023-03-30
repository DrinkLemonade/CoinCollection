using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    public GameSession currentSession;

    //Used by GameSession's constructor
    [SerializeField]
    int sessionStartSeconds = 90; //Currently acts as the max, too
    [SerializeField]
    int sessionCoinPointPool = 100; //How many points can be collected by getting all the coins
    public int sessionCoinsForVictory = 100; //Used by status display

    [SerializeField]
    GameObject pausePanel, gameOverPanel;

    [NonSerialized] //I knew that had to exist
    public bool gameIsPaused = false;
    bool gameIsOver = false;

    [SerializeField]
    PlayerInput input;
    [SerializeField]
    GameObject player;
    PlayerController getControls;

    [SerializeField]
    bool debugMode = false;

    // Start is called before the first frame update
    void Start()
    {
        NewSession();
        pausePanel.SetActive(false); //May need to remove this if I want to keep the pause panel disabled by default.
        gameOverPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameIsOver)
        {
            if (i.currentSession.currentSecondsLeft > 0)
            {
                i.currentSession.currentSecondsLeft -= Time.deltaTime;
            }
            else
            {
                i.currentSession.currentSecondsLeft = 0;
                if (!debugMode) TriggerGameOver();
            }
        }

        PlayerController getControls = player.GetComponent<PlayerController>();
        bool pausing = (!gameIsPaused && getControls.controls.Player.Menu.WasReleasedThisFrame());
        bool unpausing = (gameIsPaused && getControls.controls.UI.Unpause.WasReleasedThisFrame());
        if (pausing || unpausing) //Pause Menu
        {
            TogglePause();
        }
    }

    private void Awake()
    {
        i = this;
    }

    static void NewSession()
    {
        Debug.Log("------Starting a game...------");
        i.currentSession = new GameSession(i, i.sessionStartSeconds);
    }

    public void RestartSession()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        Debug.Log("-----GAME OVER-----");
        gameIsOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void QuitToMenu()
    {
        //Do nothing for now
    }

    public void TogglePause()
    {
        PlayerController getControls = player.GetComponent<PlayerController>();

        gameIsPaused = !gameIsPaused; //Toggle
        if (gameIsOver) gameIsPaused = false; //Don't allow displaying the pause menu during game over
        if (gameIsPaused)
        {
            getControls.controls.Player.Disable();
            //input.SwitchCurrentActionMap("UI");
            Time.timeScale = 0;
            pausePanel.SetActive(true);
        }
        else
        {
            getControls.controls.Player.Enable();
            //input.SwitchCurrentActionMap("Player");
            Time.timeScale = 1;
            pausePanel.SetActive(false);
        }
    }
}
