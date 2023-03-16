using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameCollider : MonoBehaviour
{
    [SerializeField]
    TimePickupFlame flameController;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            flameController.FlameCollected();
        }
    }
}
