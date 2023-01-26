using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.VisualScripting;

public class LevelGenerator : MonoBehaviour
{
    public Texture2D mapTexture;
    public GameObject myCube; //instance created each loop
    public GameObject CubePrefab; //prefab
    private Color[,] colorArray;
    private ProBuilderMesh[,] meshArray;

    ProBuilderMesh m_Mesh;

    private const int startX = 0;
    private const int startZ = 0;
    private const int tileSize = 1; //in units

    // Start is called before the first frame update
    void Start()
    {
        colorArray = Texture2DTo2DColorArray(mapTexture);
        GenerateFloor(colorArray);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Retourne un tableau 2D de couleur à partir d'un texture
    /// </summary>
    /// <param name="_texture"></param>
    /// <returns></returns>
    public static Color[,] Texture2DTo2DColorArray(Texture2D _texture)
    {
        Color[,] _colorArray = new Color[_texture.width, _texture.height];

        for (int x = 0; x < _texture.width; x++)
        {
            for (int y = 0; y < _texture.height; y++)
            {
                _colorArray[x, y] = _texture.GetPixel(x, y);
            }
        }

        return _colorArray;
    }

    void GenerateFloor(Color[,] colorArray)
    {
        //I could use GetLength(1) to get the other dimension of the array, but my texture is square, so it's the same as GetLength(0)
        int len = colorArray.GetLength(0);
        Debug.Log("Generating floor... We're reading an array of length: " + len);
        for (int i = 0; i < len; i++)
        {
            //Debug.Log("New row");
            for (int j = 0; j < len; j++)
            {
                if (colorArray[i,j] == Color.white)
                {
                    continue; //don't create anything
                }
                Debug.Log("Creating a cube at array position: " + i + "," + j);

                //meshArray[i,j] =
                CreatePlaneTile(i, 0, j); 
            }
        }
    }

    ProBuilderMesh CreatePlaneTile(int x, int y, int z)
    {
        m_Mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, 1, 1, 0, 0, Axis.Up);
        m_Mesh.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.defaultMaterial;
        m_Mesh.transform.localPosition = new Vector3(x, y, z); //incredibly inefficient, but it works!

        System.Random RNG = new System.Random();
        float colRand = (RNG.Next(10) + 90) / 10; //0.5 to 1
        colRand /= 10; //0.5 to 1
        m_Mesh.GetComponent<Renderer>().material.color = new Color(colRand, colRand, colRand, 1);
        m_Mesh.AddComponent<MeshCollider>();
        m_Mesh.transform.SetParent(this.gameObject.transform, false);

        return m_Mesh;
    }

    void CreateCubeOld(int i, int j)
    {
        GameObject tileInstance;

        //instantiate
        tileInstance = Instantiate(CubePrefab) as GameObject;//, location, Quaternion.identity) as GameObject;
        System.Random RNG = new System.Random();
        float colRand = (RNG.Next(50) + 50) / 10; //0.5 to 1
        Debug.Log("Colrand: " + colRand);
        colRand /= 10; //0.5 to 1
        Debug.Log("Colrand2: " + colRand);
        tileInstance.GetComponent<Renderer>().material.color = new Color(colRand, colRand, colRand, 1);
        //tileInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);

        //CubePrefab tileController = tileInstance.GetComponent<CubePrefab>();
        //tileController.gridX = j;
        //tileController.gridY = i;
        //tileGrid[j, i] = tileInstance; //stocker une struct avec les infos sur la tile ?

        int newX = startX + (j * tileSize);
        int newZ = startZ - (i * tileSize);

        tileInstance.transform.localPosition = new Vector3(newX, 0, newZ);
        //tileInstance.transform.localPosition = new Vector3(startX + (xOffset * xBetweenTiles) + (xOffset * tileSize) - ((26 / 4) * tileSize), startY + (yOffset * yBetweenTiles) + (yOffset * tileSize), 0);
        //tileInstance.transform.localScale = new Vector3(1, 1, 0); // change its local scale in x y z format

        //set tile size
        //Transform myRect = tileInstance.GetComponent<Transform>();
        //myRect.localScale = new Vector2(tileSize, tileSize);
    }
}
