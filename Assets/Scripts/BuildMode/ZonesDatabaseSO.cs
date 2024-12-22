using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZonesDatabaseSO", menuName = "Scriptable Objects/ZonesDatabaseSO")]
public class ZonesDatabaseSO : ScriptableObject
{
    public List<ZonesData> zonesData;
}

[CreateAssetMenu(fileName = "ZonesData", menuName = "Scriptable Objects/ZonesData")]
public class ZonesData: ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public Zone zoneType { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
}