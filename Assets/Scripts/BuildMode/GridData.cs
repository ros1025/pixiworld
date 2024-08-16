using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    Dictionary<GameObject, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridPosition,
                            GameObject prefab,
                            Vector2Int objectSize,
                            int ID, int rotation)
    {
        PlacementData data = new PlacementData(prefab, gridPosition, rotation, objectSize, ID);
        placedObjects[prefab] = data;
    }

    public void MoveObjectAt(Vector3Int gridPosition,
                            GameObject prefab,
                            Vector2Int objectSize,
                            int ID, int rotation)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return;
        PlacementData data = new PlacementData(prefab, gridPosition, rotation, objectSize, ID);
        placedObjects[prefab] = data;
    }

    bool ObjectValidation(GameObject hitbox, GameObject previewSelector)
    {
        bool ans = false;
        Collider collider1 = previewSelector.transform.GetChild(0).gameObject.GetComponent<Collider>();
        Collider collider2 = hitbox.transform.GetChild(0).gameObject.GetComponent<Collider>();
        Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
        for (int i = 0; i < overlaps.Length; i++)
        {
            if (overlaps.GetValue(i) == collider2)
            {
                ans = true;
            }
        }

        //if (collider1.bounds.Intersects(collider2.bounds))
        //    ans = true;
        return ans;
    }

    public bool CanPlaceObjectAt(GameObject previewSelector)
    {
        bool ans = true;

        foreach (GameObject value in placedObjects.Keys)
        {
            GameObject selector = value.transform.Find("Selector").gameObject;
            if (ObjectValidation(selector, previewSelector) == true)
                ans = false;
        }
        return ans;
    }

    public bool CanPlaceObjectAt(GameObject cursor, Vector3 position, Vector2Int size, int rotation)
    {
        bool ans = true;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.name = "Intersector";
        previewSelector.transform.localScale = new Vector3Int(size.x, size.y, size.y);

        foreach (GameObject value in placedObjects.Keys)
        {
            GameObject selector = value.transform.Find("Selector").gameObject;
            if (ObjectValidation(selector, previewSelector) == true)
                ans = false;
        }
        GameObject.Destroy(previewSelector);
        return ans;
    }

    public bool CanPlaceObjectAt(Vector3 p1, Vector3 p2, float width, float height)
    {
        bool ans = true;
        RaycastHit[] hits = Physics.BoxCastAll(p1 + new Vector3(0, height / 2f, 0), new Vector3(width - 0.05f, height / 2f, width - 0.05f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));

        foreach (GameObject value in placedObjects.Keys)
        {
            Collider selector = value.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>();
            for (int i = 0; i < hits.Length; i++)
            {               
                if (hits[i].collider == selector)
                {
                    ans = false;
                    break;
                }
            }
        }
        return ans;
    }

    public bool CanMoveObjectAt(Vector3 originalPos, GameObject previewSelector)
    {
        bool ans = true;

        foreach (GameObject value in placedObjects.Keys)
        {
            GameObject selector = value.transform.Find("Selector").gameObject;
            if (placedObjects[value].occupiedPosition != originalPos)
                if (ObjectValidation(selector, previewSelector) == true)
                    ans = false;
        }
        return ans;
    }

    public GameObject GetObject(GameObject cursor, Vector3 position, Vector2Int size, int rotation)
    {
        GameObject m_Object = null;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.transform.localScale = new Vector3Int(size.x, size.y, size.y);

        foreach (GameObject value in placedObjects.Keys)
        {
            GameObject selector = value.transform.Find("Selector").gameObject;
            if (ObjectValidation(selector, previewSelector) == true)
                m_Object = value; 
        }
        GameObject.Destroy(previewSelector);
        return m_Object;
    }

    /*
    internal int GetRepresentationIndex(GameObject prefab)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return -1;
        return placedObjects[prefab].PlacedObjectIndex;
    }
    */

    internal bool HasKey(GameObject prefab)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return false;
        return true;
    }

    internal int GetObjectID(GameObject prefab)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return -1;
        return placedObjects[prefab].ID;
    }

    internal Vector3Int GetObjectCoordinate(GameObject prefab)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return new Vector3Int(0, 0, 0);
        return placedObjects[prefab].occupiedPosition;
    }

    internal Vector2Int GetObjectSize(GameObject prefab)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return new Vector2Int(0, 0);
        return placedObjects[prefab].size;
    }

    internal int GetObjectRotation(GameObject prefab)
    {
        if (placedObjects.ContainsKey(prefab) == false)
            return -1;
        return placedObjects[prefab].rotation;
    }

    internal void RemoveObjectAt(GameObject prefab)
    {
        placedObjects.Remove(prefab);
    }
}

public class PlacementData
{
    public GameObject prefab;
    public Vector3Int occupiedPosition;
    public int rotation { get; private set; }
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
