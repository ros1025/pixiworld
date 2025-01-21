using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZonesDatabaseSO", menuName = "Scriptable Objects/ZonesDatabaseSO")]
public class ZonesDatabaseSO : ScriptableObject
{
    public List<ZonesData> zonesData;
}