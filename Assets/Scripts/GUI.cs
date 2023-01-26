using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI : MonoBehaviour
{
    public static GUI i;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        i = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateDisplay(GameSession currentSession)
    {

    }
}
