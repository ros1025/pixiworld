using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonePlacer : MonoBehaviour
{
 
    [SerializeField]
    public GridData zoneData = new();

    [SerializeField]
    public GameObject zoneIndicator;

    [SerializeField]
    private GameObject selectorObject;

    [SerializeField]
    public PlacementSystem placementSystem;

    [SerializeField]
    private Material selectorObjectMaterial;

    public void PlaceZones(Vector3 position, Vector3Int gridPos, int ID, Vector2Int size, float yOffset, int rotation)
    {
        GameObject zoneObject = Instantiate(zoneIndicator);
        zoneObject.transform.position = position;
        zoneObject.transform.position = new Vector3(position.x, position.y, position.z);
        zoneObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(zoneObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.3f, size.y - 0.1f);
        previewSelector.transform.position = new Vector3(position.x + 0.05f, position.y + 0.01f, position.z + 0.05f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        Zone zoneComponent = zoneObject.GetComponentInChildren<Zone>();
        zoneComponent.InstantiateNew(placementSystem, ID, size);
        zoneData.AddObjectAt(gridPos, zoneObject, size, ID, rotation);
        zoneObject.transform.SetParent(this.transform);
    }

    public void MoveZoneAt(GameObject prefab, Vector3Int gridPos, int ID, Vector3 position, Vector2Int size, int rotation)
    {
        if (!zoneData.HasKey(prefab))
            return;
        GameObject m_Object = prefab;
        m_Object.transform.position = position;
        m_Object.transform.rotation = Quaternion.Euler(0, rotation, 0);
        m_Object.transform.Find("Selector").localScale = new Vector3(size.x - 0.1f, 0.3f, size.y);
        m_Object.transform.Find("Selector").position = new Vector3(position.x, position.y + 0.01f, position.z);
        m_Object.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Renderer>().material = selectorObjectMaterial;
        Zone zoneComponent = m_Object.GetComponentInChildren<Zone>();
        zoneComponent.EditPosition(ID, size);
        zoneData.MoveObjectAt(gridPos, prefab, size, ID, rotation);
    }

    public void RemoveZoneAt(GameObject prefab)
    {
        if (zoneData.HasKey(prefab))
            return;

        Destroy(prefab);
        zoneData.RemoveObjectAt(prefab);
    }
}