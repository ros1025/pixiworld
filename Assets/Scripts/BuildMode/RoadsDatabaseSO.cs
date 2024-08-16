using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoadsDatabaseSO : ScriptableObject
{
    public List<RoadsData> roadsData;
}

[Serializable]
public class RoadsData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    public int width { get; private set; } = 1;
    [field: SerializeField]
    public bool TwoWay { get; private set; }
    [field: SerializeField]
    public Texture2D tex { get; private set; }
}
