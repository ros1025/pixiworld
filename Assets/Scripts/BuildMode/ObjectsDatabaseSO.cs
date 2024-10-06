using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum ObjectTypes { Living_Room, Dining_Room, Bedroom, Bathroom, Kitchen, Baby, Toddler, Children, Preteen, Teen, Electronics, Lifestyle, Decorations, Lighting, Pool_Decorations, Vegetation, Outdoor_Decorations }

[CreateAssetMenu(fileName = "ObjectsDatabaseSO", menuName = "Scriptable Objects/ObjectsDatabaseSO")]
public class ObjectsDatabaseSO : ScriptableObject
{
    public List<ObjectData> objectsData;
}

[Serializable]
public class ObjectData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
    [field: SerializeField]
    public List<int> objectTypeId { get; private set; }
}
