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
