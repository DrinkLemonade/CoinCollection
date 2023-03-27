using UnityEngine;
using TMPro;
using UnityEngine.ProBuilder.Shapes;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StatusDisplay : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI coinCountText, timeLeftText;
    [SerializeField]
    GameObject coinIcon, hourglassIcon, blindIcon;
    [SerializeField]
    Image lightGaugeFillBar;

    void Update()
    {
        UpdateCoinCountDisplay();
        UpdateTimeLeftDisplay();
        UpdateLightGauge();
    }

    void UpdateCoinCountDisplay()
    {
        GameSession sess = GameManager.i.currentSession;
        int scoreDisplayed = sess.playerScore;
        Debug.Log("Sess Playerscore: " + scoreDisplayed);
        string text = "x " + scoreDisplayed.ToString().PadLeft(3, '0') + "<size=50%>/" + GameManager.i.sessionCoinsForVictory;
        coinCountText.SetText(text);
    }

    void UpdateTimeLeftDisplay()
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

        string text = "TIME: " + m + ":" + s + ":" + ms;
        timeLeftText.SetText(text);
    }

    void UpdateLightGauge()
    {
        GameSession sess = GameManager.i.currentSession;
        lightGaugeFillBar.fillAmount = Mathf.Clamp(sess.currentSecondsLeft / sess.startSecondsLeft, 0, 1f);
    }
}