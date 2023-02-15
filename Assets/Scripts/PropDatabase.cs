using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropDatabase : ScriptableObject
{
    public List<DatabaseEntry> dataList;

    [System.Serializable] //SerializeField but for classes, shows in inspector
    public class DatabaseEntry
    {
        public int entryId;
        public GameObject entryPrefab;
    }
}
