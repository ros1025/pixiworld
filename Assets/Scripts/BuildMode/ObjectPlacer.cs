using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    public GridData furnitureData = new();

    [SerializeField]
    public GameObject zoneIndicator;

    [SerializeField]
    private GameObject selectorObject;

    [SerializeField]
    public PlacementSystem placementSystem;

    [SerializeField]
    private Material selectorObjectMaterial;

    public void PlaceObject(GameObject prefab, Vector3Int gridPos, Vector3 position, Vector2Int size, int ID, int rotation)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        newObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.position = new Vector3(position.x + 0.05f, position.y + 0.01f, position.z + 0.05f);
        previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.3f, size.y - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        furnitureData.AddObjectAt(gridPos, newObject, size, ID, rotation);
        newObject.transform.SetParent(this.transform);
    }

    internal void MoveObjectAt(GameObject prefab, Vector3Int gridPos, Vector3 position, Vector2Int size, int ID, int rotation, Renderer[] renderers)
    {
        if (!furnitureData.HasKey(prefab))
            return;
        GameObject m_Object = prefab;
        m_Object.transform.position = position;
        m_Object.transform.rotation = Quaternion.Euler(0, rotation, 0);
        Renderer[] m_Renderers = m_Object.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            m_Renderers[i].materials = materials;
        }
        Vector3 scale = m_Object.transform.Find("Selector").localScale;
        scale.y = 0.3f;
        m_Object.transform.Find("Selector").localScale = scale;
        m_Object.transform.Find("Selector").transform.position = new Vector3(position.x, position.y + 0.01f, position.z);
        m_Object.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Renderer>().material = selectorObjectMaterial;
        furnitureData.MoveObjectAt(gridPos, prefab, size, ID, rotation);
    }

    internal void RemoveObjectAt(GameObject prefab)
    {
        if (!furnitureData.HasKey(prefab))
            return;
        Destroy(prefab);
        furnitureData.RemoveObjectAt(prefab);
    }
}
