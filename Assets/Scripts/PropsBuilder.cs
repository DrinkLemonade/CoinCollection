using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsBuilder : MonoBehaviour
{
    [SerializeField]
    PropDatabase db;
    [SerializeField]
    Texture2D propTexture;
    public void CreateAllProps()
    {
        db.InitDatabase();
        for (int x = 0; x < propTexture.width; x++)
        {
            for (int y = 0; y < propTexture.height; y++)
            {
                Color col;
                col = propTexture.GetPixel(x, y); //Not optimized but.
                if (col == Color.white) continue;
                if (col == Color.black) continue;
                col.a = 0f;
                if (col == Color.clear) continue; //Not to be confused with black, since we're turning black to transparent
                Debug.Log("COLOR AT POS " + x + "," + y + " : R<" + col.r + ">, G<" + col.g + ">, B<" + col.b + ">");

                /*Bruteforce fix. TODO: Fix this
                if (col.r > 0 && col.r < 1) col.r = 0.4f;
                if (col.g > 0 && col.g < 1) col.g = 0.4f;
                if (col.b > 0 && col.b < 1) col.b = 0.4f;*/

                CreateProp(col, x, y, 25);
            }
        }
    }

    void CreateProp(Color color, int x, int z, int height)
    {
        GameObject prefab = db.GetPrefabFromId(color);
        Debug.Log("DATABASE: " + db);
        if (prefab == null)
        {
            Debug.Log("Null prefab with ID " + color);
            return;
        }
        Debug.Log("Creating prop " + prefab.name);
        GameObject inst = Instantiate(prefab);
        inst.transform.position = new Vector3(x, height, z);


        float newY = 12;
        Ray ray = new Ray(inst.transform.position, Vector3.down * 25);// * raycastLength);
        if (Physics.Raycast(ray, out var hit))
        {
            newY = hit.point.y;// + (Vector3.up * upPrefabFromFloor)).y;
            //transform.LookAt(transform.position + hit.normal);
        }

        inst.transform.position = new Vector3(x, newY + db.GetVerticalAdjustFromId(color), z);
        inst.transform.SetParent(this.transform);  
    }
}
