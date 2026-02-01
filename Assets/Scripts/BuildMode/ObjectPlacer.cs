using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    public List<ObjectSaveData> furnitureData = new();

    [SerializeField]
    public GameObject zoneIndicator;

    [SerializeField]
    private GameObject selectorObject;

    [SerializeField]
    public PlacementSystem placementSystem;

    [SerializeField]
    private Material selectorObjectMaterial;

    public void PlaceObject(GameObject prefab, Vector3 gridPos, Vector3 position, Vector2Int size, long ID, float rotation, List<MatData> materials)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        newObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        int index = 0;
        for (int i = 0; i < newObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            /*
            List<Material> matList = materials.GetRange(index, newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length);
            Material[] mats = new Material[newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length];
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                mats[j] = matList[j];
            }
            newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials = mats;
            index += newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length;
            */
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                newObject.GetComponentsInChildren<Renderer>()[i].materials[j] = Instantiate(newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j]);
                newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                index++;
            }
        }

        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.position = new Vector3(position.x + 0.05f, position.y, position.z + 0.05f);
        previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.3f, size.y - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        furnitureData.Add(new ObjectSaveData(newObject, gridPos, rotation, size, ID, materials));
        newObject.transform.SetParent(this.transform);
    }

    public void PlaceObject(ObjectSaveData objectData)
    {
        GameObject newObject;
        newObject = Instantiate(placementSystem.GetObjectPrefab(objectData.ID));

        newObject.transform.position = transform.TransformPoint(objectData.occupiedPosition);
        newObject.transform.rotation = Quaternion.Euler(0, objectData.rotation, 0);
        int index = 0;
        for (int i = 0; i < newObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            /*
            List<Material> matList = materials.GetRange(index, newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length);
            Material[] mats = new Material[newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length];
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                mats[j] = matList[j];
            }
            newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials = mats;
            index += newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length;
            */
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                newObject.GetComponentsInChildren<Renderer>()[i].materials[j] = Instantiate(newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j]);
                newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = objectData.materials[index].color;
                index++;
            }
        }

        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.position = new Vector3(objectData.occupiedPosition.x + 0.05f, objectData.occupiedPosition.y, objectData.occupiedPosition.z + 0.05f);
        previewSelector.transform.localScale = new Vector3(objectData.size.x - 0.1f, 0.3f, objectData.size.y - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, objectData.rotation, 0);
        objectData.prefab = newObject;
        furnitureData.Add(objectData);
        newObject.transform.SetParent(this.transform);
    }

    internal void MoveObjectAt(GameObject prefab, Vector3 gridPos, Vector3 position, Vector2Int size, long ID, float rotation, List<MatData> materials)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return;
        ObjectSaveData data = furnitureData[index];
        GameObject m_Object = prefab;
        m_Object.transform.position = position;
        m_Object.transform.rotation = Quaternion.Euler(0, rotation, 0);

        Vector3 scale = m_Object.transform.Find("Selector").localScale;
        scale.y = 0.3f;
        m_Object.transform.Find("Selector").localScale = scale;
        m_Object.transform.Find("Selector").transform.position = new Vector3(position.x, position.y, position.z);
        m_Object.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Renderer>().material = selectorObjectMaterial;

        int matIndex = 0;
        for (int i = 0; i < m_Object.GetComponentsInChildren<Renderer>().Length; i++)
        {
            if (m_Object.GetComponentsInChildren<Renderer>()[i] != m_Object.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Renderer>())
            {
                /*
                List<Material> matList = materials.GetRange(matIndex, m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length);
                Material[] mats = new Material[m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length];
                for (int j = 0; j < m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    mats[j] = matList[j];
                }
                m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials = mats;
                matIndex += m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length;
                */
                for (int j = 0; j < m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    m_Object.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[matIndex].color;
                    matIndex++;
                }
            }
        }

        data.occupiedPosition = gridPos;
        data.rotation = rotation;
        data.materials = materials;
    }

    internal void RemoveObjectAt(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return;
        else
        {
            Destroy(prefab);
            furnitureData.RemoveAt(index);
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

    public bool CanPlaceObjectAt(GameObject previewSelector)
    {
        if (furnitureData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true) != -1)
            return false;
        return true;
    }

    public bool CanPlaceObjectAt(GameObject cursor, Vector3 position, Vector2Int size, float rotation)
    {
        bool ans = true;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.name = "Intersector";
        previewSelector.transform.localScale = new Vector3Int(size.x, size.y, size.y);

        if (furnitureData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true) != -1)
            ans = false;

        GameObject.Destroy(previewSelector);
        return ans;
    }

    public bool CanPlaceObjectAt(Vector3 p1, Vector3 p2, float width, float height)
    {
        RaycastHit[] hits = Physics.BoxCastAll(p1 + new Vector3(0, height / 2f, 0), new Vector3(width - 0.05f, height / 2f, width - 0.05f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
        List<RaycastHit> hitList = new(); hitList.AddRange(hits);

        if (furnitureData.FindIndex(item => hitList.FindIndex(col => col.collider == item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) != -1) != -1)
        {
            return false;
        }

        return true;
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
                if (furnitureData.FindIndex(item => item.prefab.transform.Find("Selector").GetChild(0).GetComponent<Collider>() == overlap) != -1)
                {
                    Collider objCollider = furnitureData.Find(item => item.prefab.transform.Find("Selector").GetChild(0).GetComponent<Collider>() == overlap).prefab.transform.Find("Selector").GetChild(0).GetComponent<Collider>();

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

    public bool CanMoveObjectAt(GameObject selectedObject, GameObject previewSelector)
    {
        int index = furnitureData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true);

        if (index != -1 && furnitureData[index].prefab != selectedObject)
            return false;
        return true;
    }

    public GameObject GetObject(GameObject cursor, Vector3 position, Vector2Int size, float rotation)
    {
        GameObject m_Object = null;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.transform.localScale = new Vector3Int(size.x, size.y, size.y);

        int index = furnitureData.FindIndex(item => ObjectValidation(item.prefab.transform.Find("Selector").gameObject, previewSelector) == true);
        if (index != -1)
            m_Object = furnitureData[index].prefab;

        GameObject.Destroy(previewSelector);
        return m_Object;
    }

    internal bool HasKey(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return false;
        return true;
    }

    internal long GetObjectID(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return -1;
        return furnitureData[index].ID;
    }

    internal Vector3 GetObjectCoordinate(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return new Vector3Int(0, 0, 0);
        return furnitureData[index].occupiedPosition;
    }

    internal Vector2Int GetObjectSize(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return new Vector2Int(0, 0);
        return furnitureData[index].size;
    }

    internal float GetObjectRotation(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return -1;
        return furnitureData[index].rotation;
    }

    internal List<MatData> GetObjectRenderers(GameObject prefab)
    {
        int index = furnitureData.FindIndex(item => item.prefab == prefab);
        if (index == -1)
            return null;
        return furnitureData[index].materials;
    }

    public void LoadData(List<ObjectSaveData> loadData)
    {
        foreach (ObjectSaveData obj in furnitureData)
        {
            if (!loadData.Contains(obj))
            {
                RemoveObjectAt(obj.prefab);
            }
        }

        foreach (ObjectSaveData obj in loadData)
        {
            if (!furnitureData.Contains(obj))
            {
                PlaceObject(obj);
            }
        }
    }

    public void SetPlacementSystem(PlacementSystem system)
    {
        placementSystem = system;
    }
}

[Serializable]
public class ObjectSaveData : PlacementData
{
    public List<MatData> materials;
    public ObjectSaveData(GameObject prefab, Vector3 occupiedPosition, float rotation, Vector2Int size, long ID, List<MatData> materials) : base(prefab, occupiedPosition, rotation, size, ID)
    {
        this.materials = materials;
    }
}