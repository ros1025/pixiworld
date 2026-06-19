using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WorldSaveData
{
    public long lastSaveDate;
    public List<ObjectSaveData> mapObjects;
    public List<ZoneSaveData> zones;
    public List<Pool> pools;
    public RoadMapSaveData roads;
    public WallMapSaveData mapWalls;
    public List<Character> characters;

    public WorldSaveData(List<ObjectSaveData> mapObjects, List<ZoneSaveData> zones, RoadMapSaveData roads, WallMapSaveData mapWalls, List<Pool> pools, List<Character> characters)
    {
        lastSaveDate = System.DateTime.Now.ToBinary();
        this.mapObjects = mapObjects;
        this.pools = pools;
        this.zones = zones;
        this.roads = roads;
        this.mapWalls = mapWalls;
        this.characters = characters;
    }

    public WorldSaveData()
    {
        lastSaveDate = System.DateTime.Now.ToBinary();
    }
}
