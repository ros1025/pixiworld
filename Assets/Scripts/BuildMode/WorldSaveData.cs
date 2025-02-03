using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WorldSaveData
{
    public long lastSaveDate;
    public List<ObjectSaveData> mapObjects;
    public List<ZoneSaveData> zones;
    public RoadMapSaveData roads;
    public WallMapSaveData mapWalls;

    public WorldSaveData(List<ObjectSaveData> mapObjects, List<ZoneSaveData> zones, RoadMapSaveData roads, WallMapSaveData mapWalls)
    {
        lastSaveDate = System.DateTime.Now.ToBinary();
        this.mapObjects = mapObjects;
        this.zones = zones;
        this.roads = roads;
        this.mapWalls = mapWalls;
    }

    public WorldSaveData()
    {
        lastSaveDate = System.DateTime.Now.ToBinary();
    }
}
