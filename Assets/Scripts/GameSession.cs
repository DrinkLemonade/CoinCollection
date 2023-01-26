using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession
{
    GameManager myManager;
    public int playerScore;
    public int startSecondsLeft = 70;
    public float currentSecondsLeft;
    
    public GameSession(GameManager passManager) //constructor. automatically returns HangmanLogic instance
    {
        myManager = passManager;
        Debug.Log("Starting a Coin Challenge session...");
    }

    public void AddScore(int points)
    {
        Debug.Log("Added score: " + points);
        playerScore += points;    
    }
}
