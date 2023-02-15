using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectGenerator : MonoBehaviour
{
    public void CreateScriptableObject()
    {
        PropDatabase _inst = (PropDatabase)ScriptableObject.CreateInstance("PropDatabase");
        string _fileName = "PropDatabase";

        string _path = "Assets/ScriptableObjects/" + _fileName + ".asset"; //Don't forget to add file extension, otherwise this could overwrite the folder with an empty version
        AssetDatabase.CreateAsset(_inst, _path);
        AssetDatabase.SaveAssets();
    }
}
