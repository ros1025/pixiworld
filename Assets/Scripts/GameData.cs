using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Dictionary<PlacementData, List<PlacementData>> zones;

    public GameData()
    {
        zones = new();
    }
}
