using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WorldSaveData
{
    public List<ObjectSaveData> mapObjects;
    public List<ZoneSaveData> zones;
    public RoadMapSaveData roads;
    public WallMapSaveData mapWalls;

    public WorldSaveData(List<ObjectSaveData> mapObjects, List<ZoneSaveData> zones, RoadMapSaveData roads, WallMapSaveData mapWalls)
    {
        this.mapObjects = mapObjects;
        this.zones = zones;
        this.roads = roads;
        this.mapWalls = mapWalls;
    }

    public WorldSaveData()
    {

    }
}
