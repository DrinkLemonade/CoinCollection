using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.VisualScripting;
using System;

public class LevelGenerator : MonoBehaviour
{
    public Texture2D mapTexture;

    [NonSerialized]
    public GameObject myCube; //instance created each loop
    //public GameObject CubePrefab; //prefab
    private Color[,] colorArray;
    private ProBuilderMesh[,] meshArray;

    ProBuilderMesh m_Mesh;

    private const int startX = 0;
    private const int startZ = 0;
    private const float tileSize = 1f; //in units

    [SerializeField]
    GameObject TorchPrefab; //prefab

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
                Color col = colorArray[i, j];
                Debug.Log("Creating a cube at array position: " + i + "," + j + ", COLOR R IS: " + col.r);

                if (col == Color.white)
                {
                    continue; //don't create anything
                }
                else if (col == Color.red) //255 or 1
                {
                    Debug.Log("Cube and torch");
                    float altHeight = (float)System.Math.Ceiling((1f - colorArray[i-1, j].b) * 25); //create block, same height as block to the left
                    CreateCube(i, j, altHeight);
                    CreateTorch(i, j, altHeight + 0.7f);//add torch height
                    continue;
                }

                float height = (float)System.Math.Ceiling((1f - colorArray[i, j].b) * 25); //Use blue component. Black (wall, 25) to whiteish (1, lowest floor)

                //meshArray[i,j] =
                //CreatePlaneTile(i, 0, j);
                CreateCube(i, j, height);
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

    ProBuilderMesh CreateCube(int x, int z, float height)
    {
        Vector3 size = new Vector3(tileSize, height, tileSize);
        m_Mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
        m_Mesh.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.defaultMaterial;
        m_Mesh.transform.localPosition = new Vector3(x, 0 + (height/2), z); //incredibly inefficient, but it works!
        //Positions will range from -12.75 to 12.75. -13 will probably be lava or whatever

        System.Random RNG = new System.Random();
        float colRand = (RNG.Next(10) + 90) / 10; //0.5 to 1
        colRand /= 10; //0.5 to 1
        m_Mesh.GetComponent<Renderer>().material.color = new Color(colRand, colRand, colRand, 1);
        m_Mesh.AddComponent<MeshCollider>();
        m_Mesh.transform.SetParent(this.gameObject.transform, false);

        return m_Mesh;
    }

    void CreateTorch(int x, int z, float y)
    {
        //instantiate
        GameObject torchInstance = Instantiate(TorchPrefab);
        torchInstance.transform.localPosition = new Vector3(x, y, z);
        torchInstance.transform.SetParent(this.gameObject.transform, false);
    }
}
