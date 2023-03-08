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
            GameManager.i.currentSession.AddScore(coinController.pointsWorth);
            Destroy(gameObject.transform.parent.parent.gameObject); //This is SO stupid.
        }
    }
}
