using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum ObjectTypes { Living_Room, Dining_Room, Bedroom, Bathroom, Kitchen, Baby, Toddler, Children, Preteen, Teen, Electronics, Lifestyle, Decorations, Lighting, Pool_Decorations, Vegetation, Outdoor_Decorations }

[CreateAssetMenu(fileName = "DoorDatabaseSO", menuName = "Scriptable Objects/DoorDatabaseSO")]
public class DoorDatabaseSO : ScriptableObject
{
    public List<DoorsData> doorsData;
}

[Serializable]
public class DoorsData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public int Length { get; private set; } = 1;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
}
