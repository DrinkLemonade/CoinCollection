using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DecalShadow : MonoBehaviour
{
    public float raycastLength = 2.2f;
    //float raycastVerticalOffset = 2f;
    public float upDecalFromFloor = 0.075f;
    public GameObject parentObject;
    public PlayerController playerController;
    public SpriteRenderer renderer;
    [SerializeField]
    float decalStartFade = 1f; //When player is that many units above ground, start fading out
    [SerializeField]

    private void Update()
    {
        Vector3 parentPos = parentObject.transform.position;
        renderer.color = new Color(0f, 0f, 0f, 0f);
        transform.position = parentObject.transform.position;

        if (!(playerController.OnGround || playerController.OnSteep || playerController.OnConnectedBody))
        {
            Ray ray = new Ray(parentObject.transform.position, Vector3.down * raycastLength);// * raycastLength);
            if (Physics.Raycast(ray, out var hit))
            {
                transform.position = hit.point + (Vector3.up * upDecalFromFloor);
                transform.LookAt(transform.position + hit.normal);

                Debug.DrawLine(parentObject.transform.position, hit.point, Color.yellow, 2);

                float dist = hit.distance;
                if (dist > decalStartFade) renderer.color = new Color(0f, 0f, 0f, 1f); //Solid black
                else if (dist < 0.1f) renderer.color = new Color(0f, 0f, 0f, 0f); //Compleyely invisible
                else renderer.color = new Color(0f, 0f, 0f, dist / (decalStartFade)); //Transparent
            }
            //else renderer.color = new Color(0f, 0f, 0f, 0f);
        }
        //else renderer.color = new Color(0f, 0f, 0f, 0f);
    }   
}