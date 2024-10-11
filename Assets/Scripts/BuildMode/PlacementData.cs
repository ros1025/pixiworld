using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlacementData
{
    public GameObject prefab { get; private set; }
    public Vector3Int occupiedPosition;
    public int rotation;
    public Vector2Int size { get; private set; }
    public int ID { get; private set; }
    //public int PlacedObjectIndex { get; private set; }

    public PlacementData(GameObject prefab, Vector3Int occupiedPosition, int rotation, Vector2Int size, int iD)
    {
        this.prefab = prefab;
        this.occupiedPosition = occupiedPosition;
        this.size = size;
        this.rotation = rotation;
        ID = iD;
        //PlacedObjectIndex = placedObjectIndex;
    }
}
