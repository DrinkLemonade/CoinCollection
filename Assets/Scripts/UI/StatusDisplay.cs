using UnityEngine;
using TMPro;
using UnityEngine.ProBuilder.Shapes;
using System;
using UnityEngine.UI;

public class StatusDisplay : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI display;

    void Update()
    {
        GameSession sess = GameManager.i.currentSession;

        float seconds = sess.currentSecondsLeft % 60;
        float minutes = (float)Math.Floor(sess.currentSecondsLeft / 60);
        float milliseconds = (seconds - (float)Math.Floor(seconds)) * 100;

        seconds = (float)Math.Floor(seconds);
        milliseconds = (float)Math.Floor(milliseconds);

        string s = seconds.ToString();
        string m = minutes.ToString();
        string ms = milliseconds.ToString();

        if (ms.Length > 2) ms = ms.Substring(0, 2);

        s = s.PadLeft(2, '0');
        m = m.PadLeft(2, '0');
        ms = ms.PadLeft(2, '0');

        int scoreDisplayed = sess.playerScore;
        string text = "    x " + scoreDisplayed.ToString().PadLeft(7, '0') + "\nTIME: " + m + ":" + s + ":" + ms;
        display.SetText(text);
    }
}