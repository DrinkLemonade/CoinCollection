using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.VisualScripting;
using System;
//using UnityEditor.UIElements;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    PropsBuilder builder;

    public Texture2D mapTexture;

    private Color[,] colorArray;

    List<Vector3> _vertices = new List<Vector3>();
    List<int> _tris = new List<int>();
    List<Vector2> _uvs = new List<Vector2>();


    [NonSerialized]
    public GameObject myCube; //instance created each loop
    //public GameObject CubePrefab; //prefab
    private ProBuilderMesh[,] meshArray;

    ProBuilderMesh m_Mesh;

    private const int startX = 0;
    private const int startZ = 0;
    private const float tileSize = 1f; //in units

    [SerializeField]
    float lavaHeight = 4f;
    [SerializeField]
    GameObject lavaPrefab;

    [System.NonSerialized]
    public Mesh levelMesh;

    [SerializeField]
    MeshFilter levelMeshFilter;
    [SerializeField]
    MeshCollider levelMeshCollider;

    int _atlasGriddSize = 8; //8x8

    //Cache an array we use repeatedly
    Vector3[] vertexArray = new Vector3[]
    {
                new Vector3( tileSize / 2, 0, tileSize / 2),
                new Vector3( tileSize / 2, 0,  tileSize / 2),
                new Vector3( - (tileSize / 2), 0, tileSize / 2),
                new Vector3( - (tileSize / 2), 0,  tileSize / 2)
    };

    float[] _neighbourInfos = new float[4];

    int[] trisOrder = new int[6] { 0, 1, 2, 2, 1, 3 };

    [SerializeField]
    GameObject text3D;

    [SerializeField]
    GameObject ceiling;

    [SerializeField]
    bool debugMode = false;

    // Start is called before the first frame update
    void Start()
    {
        ceiling.SetActive(false);
        colorArray = Texture2DTo2DColorArray(mapTexture);
        GenerateFloor(colorArray);
        levelMeshFilter.sharedMesh = levelMesh;

        builder.CreateAllProps();
        ceiling.SetActive(true); //Don't let ceiling interfere with raycasts. TODO: lazy fix, just don't make it happen in the first place

    }

    /// <summary>
    /// Retourne un tableau 2D de couleur � partir d'un texture
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
                float height = ExtractHeight(colorArray[i, j]);  //Use blue component. Black (wall, 25) to whiteish (1, lowest floor)
                if (height > 0) CreateCube(i, j, height);
                else if (debugMode) CreateSpecialObject(i, j, height, lavaPrefab); //Debug

            }
        }
        levelMesh.SetVertices(_vertices);
        levelMesh.SetTriangles(_tris, 0);
        levelMesh.SetUVs(0, _uvs);
        levelMesh.RecalculateNormals();
        if (debugMode) Debug.Log(_tris.Count);

        levelMeshCollider.sharedMesh = levelMesh;
    }

    void CreateCube(int x, int z, float height)
    {
        int _vertsCount = _vertices.Count;

        //The way we do it with UVs allows us up to 64 textures but no blending between. We can use this for emissive or reflective surfaces via shader
        //We could use vertexcolor (1 solid color, soft blending)
        //Or a shader which would blend textures, but limit us to 4

        //Top face
        _vertices.Add(new Vector3(x + (tileSize / 2), height, z + (tileSize / 2)));
        _vertices.Add(new Vector3(x + (tileSize / 2), height, z - (tileSize / 2)));
        _vertices.Add(new Vector3(x - (tileSize / 2), height, z + (tileSize / 2)));
        _vertices.Add(new Vector3(x - (tileSize / 2), height, z - (tileSize / 2)));

        AddUvs(GetGroundTileTexture(height)); //Add green on the atlas. Lower left is 0,0, the texture is 8x8 
                                              //An atlas goes from 0 to 1, it's normalized. Each step is 0.125, so 1/8
        //i ==0 bottom  (from top view)
        //i ==1 left (from top view)
        //i ==2 top (from top view)
        //i ==3 right left (from top view)

        GetNeigbours(x, z, _neighbourInfos);

        int _createdFaceCount = 0;

        //Side faces creation
        for (int i = 0; i < 4; i++)
        {
            float _neighbourHeight = _neighbourInfos[i];
            if (height == _neighbourHeight) continue; //Don't create face if adjacent to another
            if (height < _neighbourHeight) continue;

            int _sideFaceSubdivCount = Mathf.Abs(Mathf.RoundToInt(height - _neighbourHeight));

            for (int j = 0; j < _sideFaceSubdivCount; j++)
            {
                //Adapt the height of the side acoording its neighbour
                vertexArray[1].y = height - j; //Used to not have -j
                vertexArray[3].y = height - j; //Idem

                vertexArray[0].y = height - (j + 1); //Used to be _neighbourHeight
                vertexArray[2].y = height - (j + 1); //Idem

                Quaternion rotation = Quaternion.Euler(new Vector3(0, i * 90, 0));
                Matrix4x4 m = Matrix4x4.Rotate(rotation);

                _createdFaceCount++;

                //Rotate face vertices
                for (int k = 0; k < vertexArray.Length; k++)
                {
                    _vertices.Add(m.MultiplyPoint3x4(vertexArray[k]) + new Vector3(x, 0, z)); //Rotation + Offset in the world
                }

                int _rand = UnityEngine.Random.Range(0, 8);
                int u = (j == 0 || j == _sideFaceSubdivCount - 1) ? 3 : 2;
                int v = (j == 0 || j == _sideFaceSubdivCount - 1) ? 7 : _rand;
                AddUvs(u, v); //Coordinates on the texture atlas.
            }
        }

        //Triangles creation
        for (int i = 0; i < _createdFaceCount + 1; i++)
        {
            for (int j = 0; j < trisOrder.Length; j++)
            {
                _tris.Add(_vertsCount + trisOrder[j] + (i * 4));
            }
        }
    }

    Vector2Int GetGroundTileTexture(float _tileHeight)
    {
        bool _wallBase = false;

        int _neighbourCount = 0;

        //Side faces creation
        for (int i = 0; i < 4; i++)
        {
            float _neighbourHeight = _neighbourInfos[i];
            if (_neighbourHeight > 0) _neighbourCount++;
            if (_tileHeight >= _neighbourHeight) continue; //Don't create face if adjacent to another

            _wallBase = true;
            break;
        }

        if (_wallBase) return new Vector2Int(2, 6);
        if (_neighbourCount != 4) return new Vector2Int(2, 7);

        int v = UnityEngine.Random.Range(4, 8);
        return new Vector2Int(1, v);
    }

    void GetNeigbours(int x, int z, float[] _result)
    {
        if (z + 1 < colorArray.GetLength(1) - 1) _result[0] = ExtractHeight(colorArray[x, z + 1]); //Top
        if (x + 1 < colorArray.GetLength(0) - 1) _result[1] = ExtractHeight(colorArray[x + 1, z]); //Right
        if (z - 1 > 0) _result[2] = ExtractHeight(colorArray[x, z - 1]); //Bottom
        if (x - 1 > 0) _result[3] = ExtractHeight(colorArray[x - 1, z]); //Left
    }
    float ExtractHeight(Color _color)
    {
        return (float)System.Math.Ceiling((1f - _color.b) * 25);
    }
    void AddUvs(float _xAtlasCellCoord, float _yAtlasCellCoord)
    {
        float _offset = +0.01f; //If UVs are right between 2 "tiles" on the atlas it can sample neighboring pixels, so let's offset them to the center
        float _cellSize = (1f / _atlasGriddSize);
        float _startX = (_cellSize * _xAtlasCellCoord);
        float _startY = _cellSize * _yAtlasCellCoord;
        _uvs.Add(new Vector2(_startX + _offset, _startY + _offset));
        _uvs.Add(new Vector2(_startX + _cellSize - _offset, _startY + _offset));
        _uvs.Add(new Vector2(_startX + _offset, _startY + _cellSize - _offset));
        _uvs.Add(new Vector2(_startX + _cellSize - _offset, _startY + _cellSize - _offset));
    }

    void AddUvs(Vector2Int _atlasCoord)
    {
        float _offset = +0.01f; //If UVs are right between 2 "tiles" on the atlas it can sample neighboring pixels, so let's offset them to the center
        float _cellSize = (1f / _atlasGriddSize);
        float _startX = (_cellSize * _atlasCoord.x);
        float _startY = _cellSize * _atlasCoord.y;
        _uvs.Add(new Vector2(_startX + _offset, _startY + _offset));
        _uvs.Add(new Vector2(_startX + _cellSize - _offset, _startY + _offset));
        _uvs.Add(new Vector2(_startX + _offset, _startY + _cellSize - _offset));
        _uvs.Add(new Vector2(_startX + _cellSize - _offset, _startY + _cellSize - _offset));
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
