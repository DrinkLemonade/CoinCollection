//Goal: Instantiates the desired amount of buttons and automatically spaces them out on screen

//From Unity's scripting API:

// To use this example, attach this script to an empty GameObject.
// Create three buttons (Create>UI>Button). Next, select your
// empty GameObject in the Hierarchy and click and drag each of your
// Buttons from the Hierarchy to the Your First Button, Your Second Button
// and Your Third Button fields in the Inspector.
// Click each Button in Play Mode to output their message to the console.
// Note that click means press down and then release.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuParent : MonoBehaviour
{
    [SerializeField]
    List<string> buttonList;
    [SerializeField]
    GameObject buttonPrefab;

    [SerializeField]
    Component menuLogicScript;

    void Start()
    {
        foreach(string str in buttonList)
        {
            GameObject inst = Instantiate(buttonPrefab);
            inst.transform.SetParent(transform); //Button's parent is MenuOptionParent game object
            inst.GetComponent<MenuButton>().textDisplay.text = str; //Set button's text

        }
    }

    private void OnGUI()
    {
        foreach (string str in buttonList)
        {
            //GUILayout.Button(str);
        }
    }

    void TaskOnClick()
    {
        //Output this to console when Button1 or Button3 is clicked
        Debug.Log("You have clicked the button!");
    }

    void TaskWithParameters(string message)
    {
        //Output this to console when the Button2 is clicked
        Debug.Log(message);
    }

    void ButtonClicked(int buttonNo)
    {
        //Output this to console when the Button3 is clicked
        Debug.Log("Button clicked = " + buttonNo);
    }
}