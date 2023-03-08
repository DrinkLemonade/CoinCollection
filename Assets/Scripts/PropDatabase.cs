using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropDatabase : ScriptableObject
{
    public List<DatabaseEntry> dataList;
    Dictionary<Color, DatabaseEntry> dictionary; //Associate database entry to color

    [System.Serializable] //SerializeField but for classes, shows in inspector
    public class DatabaseEntry
    {
        public Color entryColor;
        public GameObject entryPrefab;
        public float verticalAdjust;
    }

    public void InitDatabase()
    {
        Debug.Log("Creating prop DB...");
        dictionary = new Dictionary<Color, DatabaseEntry>();
        foreach (DatabaseEntry entry in dataList)
        {
            dictionary.Add(entry.entryColor, entry);
        }
    }

    public GameObject GetPrefabFromId(Color id)
    {
        DatabaseEntry ret;
        Debug.Log("dict: " + dictionary + "color: " + id);
        dictionary.TryGetValue(id, out ret);
        return ret.entryPrefab;
    }

    public float GetVerticalAdjustFromId(Color id)
    {
        DatabaseEntry ret;
        dictionary.TryGetValue(id, out ret);
        return ret.verticalAdjust;
    }
}
