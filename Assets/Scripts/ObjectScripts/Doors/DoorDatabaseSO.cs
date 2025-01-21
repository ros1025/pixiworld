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
