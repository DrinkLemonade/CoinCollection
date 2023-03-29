using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResumeButton : MonoBehaviour
{
    public Button primaryButton;
    //PlayerController playerController;
    // Start is called before the first frame update
    private void Start()
    {
        primaryButton.Select();
    }
    public void ClickUnpause()
    {
        GameManager.i.TogglePause();
        //playerController.Set
    }
}
