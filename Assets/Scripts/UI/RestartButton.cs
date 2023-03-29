using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    public Button primaryButton;
    private void Start()
    {
        primaryButton.Select();    
    }

    public void ClickRestart()
    {
        GameManager.i.RestartSession();
    }
}
