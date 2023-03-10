using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Killzone entered!");
        if (other.CompareTag("Player"))
        {
            GameManager.i.TriggerGameOver();
        }
    }
}
