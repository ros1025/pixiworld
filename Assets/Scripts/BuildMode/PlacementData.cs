using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlacementData
{
    public GameObject prefab;
    public Vector3 occupiedPosition;
    public float rotation;
    public Vector2Int size;
    public int ID;
    //public int PlacedObjectIndex { get; private set; }

    public PlacementData(GameObject prefab, Vector3 occupiedPosition, float rotation, Vector2Int size, int iD)
    {
        this.prefab = prefab;
        this.occupiedPosition = occupiedPosition;
        this.size = size;
        this.rotation = rotation;
        ID = iD;
        //PlacedObjectIndex = placedObjectIndex;
    }
}
