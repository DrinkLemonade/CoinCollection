using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class CoinPinata : MonoBehaviour
{
    [SerializeField]
    int pinataPoints = 10;
    [SerializeField]
    GameObject goldCoinPrefab, silverCoinPrefab, copperCoinPrefab;

    [SerializeField]
    float rotationSpeed = 100f, orbitSpeed = 50f, maxOrbitDistance = 10f;

    int goldCoins, silverCoins;
    List<GameObject> coinList;

    // Start is called before the first frame update
    void Start()
    {
        coinList = new List<GameObject>();
        int pointsLeft = pinataPoints;

        int maxGoldCoins = (pointsLeft / 2) / 5; //Gold coins can compose up to 50% of the pinata's value, tops
        goldCoins = Random.Range(0,maxGoldCoins+1);
        pointsLeft -= goldCoins*5;
        CreateCoinRing(goldCoins, goldCoinPrefab);

        int maxSilverCoins = (int)Mathf.Floor((pointsLeft / 2) / 2); //Silver: Up to 50% of remaining value, rounded down
        silverCoins = Random.Range(0, maxSilverCoins+1);
        if (goldCoins > 0 && silverCoins == 1) silverCoins = 0; //A lonely silver coin just doesn't look right
        pointsLeft -= silverCoins*2;
        CreateCoinRing(silverCoins, silverCoinPrefab);

        CreateCoinRing(pointsLeft, copperCoinPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject coin in coinList)
        {
            //coin.transform.RotateAround(Vector3.zero, Vector3.one, 1f);//gameObject.transform.position, Vector3.up, 2 * Time.deltaTime);
            if (coin != null)
            {
                //transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                coin.transform.RotateAround(transform.position, Vector3.up, orbitSpeed * Time.deltaTime);

                //fix possible changes in distance
                float currentOrbitingObjectDistance = Vector3.Distance(transform.position, coin.transform.position);
                Vector3 towardsTarget = coin.transform.position - transform.position;

                //VERY ugly way to do this. TODO: Fix
                float coinTypeOrbitDistance;
                bool lonelySilverCoin = (coin.transform.localScale.x == 1.25 && silverCoins == 1);
                if (coin.transform.localScale.x == 1.5 || lonelySilverCoin) coinTypeOrbitDistance = 0;
                else if (coin.transform.localScale.x == 1.25) coinTypeOrbitDistance = maxOrbitDistance / 2;
                else coinTypeOrbitDistance = maxOrbitDistance;
                 
                coin.transform.position += (coinTypeOrbitDistance - currentOrbitingObjectDistance) * towardsTarget.normalized; //Use coin size to adjust orbit distance
            }
        }
    }

    GameObject CreateCoin(GameObject prefab, float adjust)
    {
        GameObject prefabInstance = Instantiate(prefab);
        Vector3 pos = transform.localPosition;
        pos.z += maxOrbitDistance * prefabInstance.transform.localScale.x;
        prefabInstance.transform.localPosition = pos;// gameObject.transform.localPosition;
        prefabInstance.transform.SetParent(gameObject.transform, true);
        prefabInstance.transform.RotateAround(transform.position, Vector3.up, adjust);// (orbitSpeed * Time.deltaTime) * adjust);
        //Debug.Log("Pinata coin created: " + prefabInstance);
        return prefabInstance;
    }

    void CreateCoinRing(int number, GameObject prefab)
    {
        if (number == 0) return;
        for (int i = 0; i < number; i++)
        {
            float angleAdjust = (360 / number) * (i + 1); //May be necessary if I want to use several gold coins.
            coinList.Add(CreateCoin(prefab, angleAdjust));
        }
    }
}
