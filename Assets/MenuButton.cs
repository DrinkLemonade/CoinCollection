using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuButton : MonoBehaviour
{
    [SerializeField]
    Button buttonComponent;

    [SerializeField]
    public Component optionBasicScript;

    [SerializeField]
    public TextMeshProUGUI textDisplay;

    //Button.ButtonClickedEvent onClick;
    // Start is called before the first frame update
    public void ClickOption()
    {
    //    GameManager.i.TogglePause();
    }
}
