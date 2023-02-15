using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.VisualScripting;
using System;
using UnityEditor.UIElements;

public class LevelGenAlt : MonoBehaviour
{
    public Texture2D mapTexture;

    Color c_coinGold = new Color32(255,201,14,255);
    Color c_coinSilver = new Color32(153, 217, 234, 255);
    Color c_coinCopper = new Color32(185, 122, 87, 255);
    Color c_coinPinata = new Color32(34, 177, 76, 255);
    Color c_torch = Color.red;
    Color c_nothing = Color.white;

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
    GameObject pf_torchPrefab; //prefab
    [SerializeField]
    GameObject pf_copperCoinPrefab;
    [SerializeField]
    GameObject pf_silverCoinPrefab;
    [SerializeField]
    GameObject pf_goldCoinPrefab;
    [SerializeField]
    GameObject pf_coinPinataPrefab;


    List<Vector3> _vertices = new List<Vector3>();
    List<int> _tris = new List<int>();
    List<Vector2> _uvs = new List<Vector2>();

    Mesh levelMesh;
    [SerializeField]
    MeshFilter levelMeshFilter;

    [SerializeField]
    GameObject text3D;

    // Start is called before the first frame update
    void Start()
    {
        colorArray = Texture2DTo2DColorArray(mapTexture);
        GenerateFloor(colorArray);
        levelMeshFilter.sharedMesh = levelMesh;
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

        levelMesh = new Mesh();
        levelMesh.name = "Level Mesh";

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


                if (col == c_nothing)
                {
                    continue; //don't create anything
                }
                else
                {
                    float height = (float)System.Math.Ceiling((1f - colorArray[i, j].b) * 25); //Use blue component. Black (wall, 25) to whiteish (1, lowest floor)

                    GameObject prefab;
                    if (col == c_coinGold) prefab = pf_goldCoinPrefab;
                    else if (col == c_coinSilver) prefab = pf_silverCoinPrefab;
                    else if (col == c_coinCopper) prefab = pf_copperCoinPrefab;
                    else if (col == c_torch) prefab = pf_torchPrefab;
                    else
                    {
                        //CreatePBCube(i, j, height);
                        continue;
                    }
                    //Debug.Log("Cube and torch");
                    float altHeight = (float)System.Math.Ceiling((1f - colorArray[i-1, j].b) * 25); //create block, same height as block to the left

                    //CreateSpecialObject(i, j, altHeight + 0.7f, prefab);
                    continue;
                }
            }
        }
        CreateCube(0, 0, 10);
        levelMesh.SetVertices(_vertices);
        levelMesh.SetTriangles(_tris,0);
        Debug.Log(_tris.Count);
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

    public static Vector3 DirectionFromAngleXY(float _angleInDegrees)
    {
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad), 0);
    }

    public static Vector3 DirectionFromAngleXZ(float _angleInDegrees)
    {
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    }

    void CreateCube(int x, int z, float height)
    {
        //Top face
        _vertices.Add(new Vector3(x - (tileSize / 2), height, z - (tileSize / 2)));
        _vertices.Add(new Vector3(x + (tileSize / 2), height, z - (tileSize / 2)));
        _vertices.Add(new Vector3(x - (tileSize / 2), height, z + (tileSize / 2)));
        _vertices.Add(new Vector3(x + (tileSize / 2), height, z + (tileSize / 2)));

        Vector3[] vertexArray = new Vector3[]
        {
            new Vector3(x + (tileSize / 2), height, z + (tileSize / 2)),
            new Vector3(x + (tileSize / 2), 0, z + (tileSize / 2)),
            new Vector3(x - (tileSize / 2), height, z + (tileSize / 2)),
            new Vector3(x - (tileSize / 2), 0, z + (tileSize / 2))
        };

        Vector3 dirTemp = new Vector3(0,0,1); //Forward?

        for(int i = 0; i < 4; i++)
        {
            float angle = i * 90;
            Vector3 dir = DirectionFromAngleXZ(angle);
            Quaternion rotation = Quaternion.LookRotation(dirTemp, dir);
            Matrix4x4 m = Matrix4x4.Rotate(rotation);
            Vector3 finalPos = m.MultiplyPoint3x4(dir);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = finalPos;
            Debug.Log(finalPos + ", dir:" + dir + ", angle:" + angle);
        }

        //Order is important
        //Tri 1

        for (int i = 0; i < 2; i++)
        {
            _tris.Add(0 + i*4);
            _tris.Add(2 + i*4);
            _tris.Add(1 + i*4);

            _tris.Add(2 + i * 4);
            _tris.Add(3 + i * 4);
            _tris.Add(1 + i * 4);
        }


            //TODO: UV, il faudra ajouter un meshrenderer et meshfilter sur levelgen, quand on déclare le mesh. MeshFilter.sharedMesh = le mesh créé, puis lui fournir les infos qu'on a calculé (triangles, etc) avec Mesh.SetVertices, SetTriangles et SetUVs. Sans UV il prend la couleur du 1er pixel sur le material






            /*Vector3 size = new Vector3(tileSize, height, tileSize);
            m_Mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            m_Mesh.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.defaultMaterial;
            m_Mesh.transform.localPosition = new Vector3(x, 0 + (height/2), z); //incredibly inefficient, but it works!
            //Positions will range from -12.75 to 12.75. -13 will probably be lava or whatever

            System.Random RNG = new System.Random();
            float colRand = (RNG.Next(10) + 90) / 10; //0.5 to 1
            colRand /= 10; //0.5 to 1
            m_Mesh.GetComponent<Renderer>().material.color = new Color(colRand, colRand, colRand, 1);
            m_Mesh.AddComponent<MeshCollider>();
            m_Mesh.transform.SetParent(this.gameObject.transform, false);*/
        }

    ProBuilderMesh CreatePBCube(int x, int z, float height)
    {
        Vector3 size = new Vector3(tileSize, height, tileSize);
        m_Mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
        m_Mesh.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.defaultMaterial;
        m_Mesh.transform.localPosition = new Vector3(x, 0 + (height / 2), z); //incredibly inefficient, but it works!
        //Positions will range from -12.75 to 12.75. -13 will probably be lava or whatever

        System.Random RNG = new System.Random();
        float colRand = (RNG.Next(10) + 90) / 10; //0.5 to 1
        colRand /= 10; //0.5 to 1
        m_Mesh.GetComponent<Renderer>().material.color = new Color(colRand, colRand, colRand, 1);
        m_Mesh.AddComponent<MeshCollider>();
        m_Mesh.transform.SetParent(this.gameObject.transform, false);

        return m_Mesh;
    }

    GameObject CreateSpecialObject(int x, int z, float y, GameObject prefab)
    {
        //instantiate
        GameObject prefabInstance = Instantiate(prefab);
        prefabInstance.transform.localPosition = new Vector3(x, y, z);
        prefabInstance.transform.SetParent(this.gameObject.transform, false);
        return prefabInstance;
    }
}
