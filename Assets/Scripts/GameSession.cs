using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession
{
    GameManager myManager;
    public int playerScore;
    public int startSecondsLeft;
    public float currentSecondsLeft;
    
    public GameSession(GameManager passManager, int managerStartSeconds) //constructor. automatically returns HangmanLogic instance
    {
        myManager = passManager;
        startSecondsLeft = managerStartSeconds;
        currentSecondsLeft = startSecondsLeft;
    }

    public void AddScore(int points)
    {
        playerScore += points;    
    }
}
