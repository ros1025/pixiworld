using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoadsDatabaseSO", menuName = "Scriptable Objects/RoadsDatabaseSO")]
public class RoadsDatabaseSO : ScriptableObject
{
    public List<RoadsData> roadsData;
}
