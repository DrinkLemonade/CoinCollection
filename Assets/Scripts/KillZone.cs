using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //TODO: Find why stuff keeps entering killzones. Other killzones?
        if (other.CompareTag("Player"))
        {
            GameManager.i.TriggerGameOver();
        }
    }
}
