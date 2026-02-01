using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonePlacer : MonoBehaviour
{
 
    [SerializeField]
    public List<ZoneSaveData> zoneData = new();

    [SerializeField]
    public GameObject zoneIndicator;

    [SerializeField]
    private GameObject selectorObject;

    [SerializeField]
    public PlacementSystem placementSystem;

    [SerializeField]
    private Material selectorObjectMaterial;

    public void PlaceZones(Vector3 position, Vector3 gridPos, long ID, Vector2Int size, float yOffset, float rotation)
    {
        GameObject zoneObject = Instantiate(zoneIndicator);
        zoneObject.transform.position = position;
        zoneObject.transform.position = new Vector3(position.x, position.y, position.z);
        zoneObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(zoneObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.3f, size.y - 0.1f);
        previewSelector.transform.position = new Vector3(position.x + 0.05f, position.y, position.z + 0.05f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        Zone zoneComponent = zoneObject.GetComponentInChildren<Zone>();
        zoneComponent.InstantiateNew(placementSystem, ID, size);
        zoneData.Add(new ZoneSaveData(zoneObject, gridPos, rotation, size, ID, zoneComponent.GetLevelSaveData()));
        zoneObject.transform.SetParent(this.transform);
    }

    public void PlaceZones(ZoneSaveData zone)
    {
        GameObject zoneObject = Instantiate(zoneIndicator);
        zoneObject.transform.position = zone.occupiedPosition;
        zoneObject.transform.position = new Vector3(zone.occupiedPosition.x, zone.occupiedPosition.y, zone.occupiedPosition.z);
        zoneObject.transform.rotation = Quaternion.Euler(0, zone.rotation, 0);
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(zoneObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localScale = new Vector3(zone.size.x - 0.1f, 0.3f, zone.size.y - 0.1f);
        previewSelector.transform.position = new Vector3(zone.occupiedPosition.x + 0.05f, zone.occupiedPosition.y, zone.occupiedPosition.z + 0.05f);
        previewSelector.transform.rotation = Quaternion.Euler(0, zone.rotation, 0);
        Zone zoneComponent = zoneObject.GetComponentInChildren<Zone>();
        zoneComponent.InstantiateNew(placementSystem, zone.ID, zone.size);
        zone.prefab = zoneObject;
        zoneData.Add(zone);
        zoneObject.transform.SetParent(this.transform);

        zoneComponent.LoadData(zone.levels, placementSystem);
        zone.levels = zoneComponent.GetLevelSaveData();
    }

    public void MoveZoneAt(GameObject prefab, Vector3 gridPos, long ID, Vector3 position, Vector2Int size, float rotation)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return;
        ZoneSaveData data = zoneData[index];
        GameObject m_Object = prefab;
        m_Object.transform.position = position;
        m_Object.transform.rotation = Quaternion.Euler(0, rotation, 0);
        m_Object.transform.Find("Selector").localScale = new Vector3(size.x - 0.1f, 0.3f, size.y - 0.1f);
        m_Object.transform.Find("Selector").position = new Vector3(position.x, position.y, position.z);
        m_Object.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Renderer>().material = selectorObjectMaterial;
        Zone zoneComponent = m_Object.GetComponentInChildren<Zone>();
        zoneComponent.EditPosition(ID, size);

        data.occupiedPosition = gridPos;
        data.rotation = rotation;
        data.size = size;
    }

    public void RemoveZoneAt(GameObject prefab)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return;
        else
        {
            Destroy(prefab);
            zoneData.RemoveAt(index);
        }
    }

    bool ObjectValidation(GameObject hitbox, GameObject previewSelector)
    {
        Collider collider1 = previewSelector.transform.GetChild(0).gameObject.GetComponent<Collider>();
        Collider collider2 = hitbox.transform.GetChild(0).gameObject.GetComponent<Collider>();
        Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (overlapsList.Contains(collider2))
        {
            return true;
        }
        return false;
    }

    private float GetAngle(List<Vector3> points, Vector3 cross)
    {
        float angle = 0;
        float angler = 0;
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
        {
            angler += Vector3.SignedAngle(points[(pointIndex + 2) % points.Count] - points[(pointIndex + 1) % points.Count], points[(pointIndex + 0) % points.Count] - points[(pointIndex + 1) % points.Count], cross);
        }
        if (Mathf.Abs(angler - (180 * (points.Count - 2))) < 0.1f)
            angle = angler;
        else if (Mathf.Abs(angler - (-(180 * (points.Count - 2)))) < 0.1f)
            angle = angler;
        else
        {
            float angleA = 0;
            float angleB = 0;
            for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
            {
                float localAngle = Vector3.SignedAngle(points[(pointIndex + 2) % points.Count] - points[(pointIndex + 1) % points.Count], points[(pointIndex + 0) % points.Count] - points[(pointIndex + 1) % points.Count], cross);
                if (localAngle < 0)
                {
                    angleA += 360 + localAngle;
                    angleB += localAngle;
                }
                else
                {
                    angleA += localAngle;
                    angleB += localAngle - 360;
                }
            }

            if (Mathf.Abs(angleA - (180 * (points.Count - 2))) < 0.1f)
            {
                angle = angleA;
            }
            else if (Mathf.Abs(angleB - (-(180 * (points.Count - 2)))) < 0.1f)
            {
                angle = angleB;
            }
        }

        return angle;
    }

    public bool CanPlaceObjectAt(List<Vector3> points)
    {
        float angle = GetAngle(points, Vector3.up);

        if (points.Count > 2 && Mathf.Abs(Mathf.Abs(angle) - ((points.Count - 2) * 180)) < 0.1f) 
        {
            Bounds boundBox = new();

            foreach (Vector3 point in points)
            {
                boundBox.Encapsulate(point);
            }
            Collider[] overlaps = Physics.OverlapBox(boundBox.center, boundBox.extents / 2f, Quaternion.identity, LayerMask.GetMask("Selector"));

            foreach (Collider overlap in overlaps)
            {
                if (zoneData.FindIndex(item => item.prefab.transform.Find("Selector").GetChild(0).GetComponent<Collider>() == overlap) != -1)
                {
                    Collider objCollider = zoneData.Find(item => item.prefab.transform.Find("Selector").GetChild(0).GetComponent<Collider>() == overlap).prefab.transform.Find("Selector").GetChild(0).GetComponent<Collider>();

                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector3 nearestPoint = objCollider.ClosestPoint(points[i]);
                        bool isInBound = true;

                        for (int j = 0; j < points.Count; j++)
                        {
                            Vector3 p1 = points[j];
                            Vector3 p2 = points[(j + 1) % points.Count];

                            Vector3 crossVector = Vector3.Cross((p2 - p1).normalized, Vector3.up).normalized;
                            if (angle > 0) crossVector *= -1;

                            if (Vector3.Angle(nearestPoint - p1, crossVector) > 90 && Vector3.Angle(nearestPoint - p2, crossVector) > 90)
                            {                
                                isInBound = false;

                                RaycastHit[] hits1 = Physics.RaycastAll(p1, (p2 - p1).normalized, Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
                                foreach (RaycastHit hit in hits1)
                                {
                                    if (hit.collider == objCollider)
                                    {
                                        return false;
                                    }
                                }

                                break;
                            }
                        }

                        if (isInBound) return false;
                    }
                }           
            }
            
            return true;
        }
        return false;
    }

    public bool CanPlaceObjectAt(GameObject previewSelector)
    {
        if (zoneData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true) != -1)
            return false;
        return true;
    }

    public bool CanPlaceObjectAt(GameObject cursor, Vector3 position, Vector2Int size, float rotation)
    {
        bool ans = true;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.name = "Intersector";
        previewSelector.transform.localScale = new Vector3(size.x, size.y, size.y);

        if (zoneData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true) != -1)
            ans = false;

        GameObject.Destroy(previewSelector);
        return ans;
    }

    public bool CanPlaceObjectAt(Vector3 p1, Vector3 p2, float width, float height)
    {
        RaycastHit[] hits = Physics.BoxCastAll(p1 + new Vector3(0, height / 2f, 0), new Vector3(width - 0.05f, height / 2f, width - 0.05f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
        List<RaycastHit> hitList = new(); hitList.AddRange(hits);

        if (zoneData.FindIndex(item => hitList.FindIndex(col => col.collider == item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) != -1) != -1)
        {
            return false;
        }

        return true;
    }

    public bool CanMoveObjectAt(GameObject selectedObject, GameObject previewSelector)
    {
        int index = zoneData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true);

        if (index != -1 && zoneData[index].prefab != selectedObject)
            return false;
        return true;
    }

    public GameObject GetObject(GameObject cursor, Vector3 position, Vector2Int size, float rotation)
    {
        GameObject m_Object = null;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.transform.localScale = new Vector3(size.x, size.y, size.y);

        int index = zoneData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true);
        if (index != -1)
            m_Object = zoneData[index].prefab;

        GameObject.Destroy(previewSelector);
        return m_Object;
    }

    internal bool HasKey(GameObject prefab)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return false;
        return true;
    }

    internal long GetObjectID(GameObject prefab)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return -1;
        return zoneData[index].ID;
    }

    internal Vector3 GetObjectCoordinate(GameObject prefab)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return new Vector3(0, 0, 0);
        return zoneData[index].occupiedPosition;
    }

    internal Vector2Int GetObjectSize(GameObject prefab)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return new Vector2Int(0, 0);
        return zoneData[index].size;
    }

    internal float GetObjectRotation(GameObject prefab)
    {
        int index = zoneData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return -1;
        return zoneData[index].rotation;
    }

    public void LoadData(List<ZoneSaveData> loadData)
    {
        for (int i = 0; i < zoneData.Count; i++)
        {
            if (!loadData.Contains(zoneData[i]))
            {
                RemoveZoneAt(zoneData[i].prefab);
            }
            else
            {
                ZoneSaveData saveData = loadData.Find(item => item == zoneData[i]);
                zoneData[i].prefab.GetComponentInChildren<Zone>().LoadData(saveData.levels, placementSystem);
            }
        }

        for (int i = 0; i < loadData.Count; i++)
        {
            if (!zoneData.Contains(loadData[i]))
            {
                PlaceZones(loadData[i]);
            }
        }
    }
}

[Serializable]
public class ZoneSaveData : PlacementData
{
    [SerializeField] public List<LevelSaveData> levels;
    public ZoneSaveData(GameObject prefab, Vector3 occupiedPosition, float rotation, Vector2Int size, long ID, List<LevelSaveData> levels) : base(prefab, occupiedPosition, rotation, size, ID)
    {
        this.levels = levels;
    }
}