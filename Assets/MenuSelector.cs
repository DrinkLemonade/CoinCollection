using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; //new input system
using UnityEditor.ShaderGraph;
using Unity.VisualScripting;

public class MenuSelector : MonoBehaviour
{
    [SerializeField]
    GameObject menuParent, selectorSword;
        
    GameObject selectedButton;

    List<Transform> buttonList;
    int selectorPositionInList = 0; //Lists thankfully(?) start at 0

    float buttonScaleWhenSelected = 2.25f;
    float baseButtonScale = 2f;

    [SerializeField]
    PlayerController playerController;


    //I'M GOING TO CRY
    //NONE OF THIS WAS NECESSARY
    //UNITY'S INPUT/EVENT SYSTEM HANDLES THIS ALREADY
    /*
    void OnEnable()
    {
        buttonList = new List<Transform>();
        foreach (Transform g in menuParent.transform.GetComponentsInChildren<Transform>())
        {
            if (g.gameObject.GetComponent<Button>() == null) continue;
            buttonList.Add(g); //Only add children with button components
            Debug.LogError(g.gameObject.name);
        }
        Debug.LogWarning("LIST Count: " + buttonList.Count);
        //baseButtonScale = buttonList[0].localScale.x; //Use button 1's scale as the default
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeSelf) return; //Don't run this if the object is inactive

        //Reset other button scales
        foreach (Transform g in buttonList)
        {
            float bbs = baseButtonScale;
            g.transform.localScale = new Vector3(bbs, bbs, bbs);
        }

        float moveY = playerController.controls.UI.Navigate.ReadValue<Vector2>().y;
        Debug.LogWarning("MOVE Y: " + moveY);

        if (moveY > -0.5f && moveY < 0.5f) moveY = 0;
        else if (moveY < 0.5f) moveY = -1f;
        else if (moveY > 0.5f) moveY = 1f;
        Debug.LogWarning("MOVE Y adjusted: " + moveY);

        selectorPositionInList += (int)moveY;
        if (selectorPositionInList > buttonList.Count - 1) selectorPositionInList = 0;
        if (selectorPositionInList < 0) selectorPositionInList = buttonList.Count - 1;
        Debug.LogWarning("SELECTOR POS:" + selectorPositionInList);

        Debug.LogWarning("FINDING BUTTON. LIST Count: " + buttonList.Count);
        selectedButton = buttonList[selectorPositionInList].gameObject;
        float bsws = buttonScaleWhenSelected;
        selectedButton.transform.localScale = new Vector3(bsws, bsws, bsws);
    }
    */
}
