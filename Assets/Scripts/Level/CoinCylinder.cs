using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCylinder : MonoBehaviour
{
    [SerializeField]
    Coin coinController;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered");
        if (other.CompareTag("Player"))
        {
            coinController.CoinCollected();
        }
    }
}
