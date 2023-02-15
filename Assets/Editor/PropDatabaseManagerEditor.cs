using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScriptableObjectGenerator))]

public class PropDatabaseManagerEditor : Editor
{
    ScriptableObjectGenerator _target;

    private void OnEnable() //Called when a GameObject shown in inspector
    {
        _target = (ScriptableObjectGenerator)target; //By inspecting a GameObject in the editor, Editor's target property refers to the inspected object
    }
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI(); //Keep basic inspector information: Serialized variables in the object for example
        if (GUILayout.Button("Create ScriptableObjectGenerator to create a PropDatabase"))
        {
            _target.CreateScriptableObject();
        }
    }
}
