using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewYOffset = 0.05f;

    [SerializeField]
    private GameObject cellIndicator;
    [NonSerialized]
    public GameObject previewObject;

    [SerializeField]
    public GameObject previewSelectorObject;
    [NonSerialized]
    public GameObject previewSelector;

    [SerializeField]
    private InputManager input;

    [SerializeField]
    private Material previewMaterialPrefab;
    private Material previewMaterialInstance;
    public Color selectedColor;

    [SerializeField]
    private PreviewTools buildToolsUI;

    [SerializeField]
    private GameObject expandingCursor;
    [SerializeField]
    private GameObject pointerCursor;
    [SerializeField]
    private GameObject expanderParent;
    [NonSerialized]
    public List<GameObject> expanders = new();
    [NonSerialized]
    public GameObject SelectedCursor;

    [NonSerialized]
    public Roads selectedRoad;
    [NonSerialized]
    public Wall selectedWall;
    private Renderer cellIndicatorRenderer;

    [NonSerialized] public Vector3 previewPos;
    [NonSerialized] public bool expand;
    [NonSerialized] public bool dynamic;
    [NonSerialized] public bool gridSnap;
    [NonSerialized] public Vector2Int previewSize;
    private Vector2Int minSize;

    private void Start()
    {
        previewMaterialInstance = new Material(previewMaterialPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();

        Button[] buttons = buildToolsUI.GetComponentsInChildren<Button>();
    }

    public void StartMovingObjectPreview(Vector3 gridPosition, int rotation, GameObject prefab, Vector2Int size)
    {
        previewObject = prefab;
        previewSelector = previewObject.transform.Find("Selector").gameObject;
        previewSize = size;
        previewPos = gridPosition;
        PreparePreview(prefab);
        PrepareCursor(size);
        MoveCursor(gridPosition);
        RotateCursor(rotation);
        RotatePreview(rotation);
        buildToolsUI.canRemove = true;
        buildToolsUI.isFurniture = true;
        buildToolsUI.isRoad = false;
        buildToolsUI.isWall = false;
        cellIndicator.SetActive(true);
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        previewObject = Instantiate(prefab);
        previewSelector = Instantiate(previewSelectorObject);
        previewSelector.transform.SetParent(previewObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSize = size;
        previewPos = new Vector3(cellIndicator.transform.position.x, 0, cellIndicator.transform.position.z);
        PreparePreview(previewObject);
        PrepareCursor(size);
        buildToolsUI.canRemove = false;
        buildToolsUI.isFurniture = true;
        buildToolsUI.isRoad = false;
        buildToolsUI.isWall = false;
        cellIndicator.SetActive(true);
        MovePreview(cellIndicator.transform.position);
    }

    public void StartCreatingZones(Vector3 originPos, Vector2Int size, Vector2Int minSize)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewSelector.transform.name = "Selector";
        previewPos = originPos;
        previewSize = size;
        this.minSize = minSize;
        PrepareExpandingCursor(originPos, size);
        buildToolsUI.canRemove = false;
        buildToolsUI.isFurniture = false;
        buildToolsUI.isRoad = false;
        buildToolsUI.isWall = false;
        MoveCursor(originPos);
        cellIndicator.SetActive(true);
    }

    public void StartMovingZones(Vector3 gridPosition, int rotation, GameObject prefab, Vector2Int size)
    {
        previewObject = prefab;
        previewSelector = prefab.transform.Find("Selector").gameObject;
        previewSelector.SetActive(true);
        previewSize = size;
        previewPos = gridPosition;
        PrepareExpandingCursor(gridPosition, size);
        MoveCursor(gridPosition);
        RotateCursor(rotation);
        RotatePreview(rotation);
        buildToolsUI.canRemove = true;
        buildToolsUI.isFurniture = false;
        buildToolsUI.isRoad = false;
        buildToolsUI.isWall = false;
        cellIndicator.SetActive(true);
    }

    public void StartCreatingRoads(Vector3 gridPosition)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = gridPosition;
        PrepareDynamicCursor(gridPosition);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
        buildToolsUI.canRemove = false;
        buildToolsUI.isFurniture = false;
        buildToolsUI.isRoad = true;
        buildToolsUI.isWall = false;
    }

    public void AddWalls()
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = Vector3.zero;
        PrepareDynamicCursor(Vector3.zero);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
        buildToolsUI.canRemove = false;
        buildToolsUI.isFurniture = false;
        buildToolsUI.isRoad = false;
        buildToolsUI.isWall = true;
    }

    public void ModifyWalls(Wall wall)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = Vector3.zero;
        PrepareDynamicCursor(Vector3.zero);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
        buildToolsUI.canRemove = true;
        buildToolsUI.isFurniture = false;
        buildToolsUI.isRoad = false;
        buildToolsUI.isWall = true;
        selectedWall = wall;
    }

    public void ModifyRoad(List<Vector3> points, Roads road, int cost, float length, int width)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = points[0];
        PrepareDynamicCursor(points[0]);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
        buildToolsUI.canRemove = true;
        buildToolsUI.isFurniture = false;
        buildToolsUI.isRoad = true;
        buildToolsUI.isWall = false;
        selectedRoad = road;
        for (int i = 0; i < points.Count; i++)
        {
            UpdatePointer(points[i], true, i, cost * Mathf.RoundToInt(length), Mathf.RoundToInt(length), width);
        }
    }

    private void PrepareCursor(Vector2Int size)
    {
        expand = false;
        dynamic = false;

        if (size.x > 0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, size.y, size.y);
            previewSelector.transform.localPosition = new Vector3(0.05f, 0f, 0.05f);
            previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.6f, size.y - 0.1f);
            //cellIndicatorRenderer.material.mainTextureScale = size;
        }

        previewSelector.transform.GetChild(0).GetComponent<Renderer>().material = previewMaterialInstance;
    }

    private void PrepareExpandingCursor(Vector3 originPos, Vector2Int size)
    {
        cellIndicator.transform.localScale = new Vector3(size.x, size.y, size.y);
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, 0.05f);
        previewSelector.transform.localScale = new Vector3(size.x - 0.1f, 0.6f, size.y - 0.1f);
        previewSelector.transform.GetChild(0).GetComponent<Renderer>().material = previewMaterialInstance;
        dynamic = false;

        expanderParent.transform.position = new Vector3(originPos.x, 0.01f, originPos.z);
        expanderParent.transform.rotation = Quaternion.Euler(0, 0, 0);

        GameObject horizontalCursor = GameObject.Instantiate(expandingCursor, new Vector3(originPos.x + size.x, 0.01f, originPos.z + (size.y/2)), Quaternion.Euler(0, 0, 90));
        horizontalCursor.transform.name = "horizontal"; expanders.Add(horizontalCursor);

        GameObject verticalCursor = GameObject.Instantiate(expandingCursor, new Vector3(originPos.x + (size.x/2), 0.01f, originPos.z + size.y), Quaternion.Euler(0, 90, 90));
        verticalCursor.transform.name = "vertical"; expanders.Add(verticalCursor);

        foreach (GameObject item in expanders) 
            item.transform.SetParent(expanderParent.transform);
    }

    private void PrepareDynamicCursor(Vector3 originPos)
    {
        cellIndicator.transform.localScale = Vector3.one;
        previewSelector.transform.localScale = new Vector3(1, 0.1f, 1);
        expanderParent.AddComponent<LineRenderer>();
        expanderParent.GetComponent<LineRenderer>().positionCount = 0;
        expanderParent.GetComponent<LineRenderer>().material = previewMaterialPrefab;

        expanderParent.transform.position = new Vector3(originPos.x, 0.01f, originPos.z);
        expanderParent.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void PreparePreview(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        /*
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterialInstance;
            }
            renderer.materials = materials;
        }
        */
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        if (previewObject != null)
            Destroy(previewObject);
        expand = false;
        Destroy(previewSelector);
        foreach (GameObject expander in expanders)
            GameObject.Destroy(expander);
        expanders.Clear();
        Destroy(expanderParent.GetComponent<LineRenderer>());
        previewObject = null;
        previewSelector = null;
    }

    public void StopMovingObject()
    {
        cellIndicator.SetActive(false);
        expand = false;
        foreach (GameObject expander in expanders)
            GameObject.Destroy(expander);
        expanders.Clear();
        previewObject = null;
        previewSelector = null;
    }

    public void deSelect()
    {
        foreach (GameObject expander in expanders)
        {
            expander.GetComponent<Renderer>().material.color = expandingCursor.GetComponent<Renderer>().sharedMaterial.color;
        }
    }

    public void UpdatePosition(Vector3 position, bool validity, Vector2Int size, int cost, int rotation)
    {
        cellIndicator.SetActive(true);
        if (previewObject != null)
        {
            MovePreview(position);
            RotatePreview(rotation);
            ApplyFeedbackToPreview(validity);

        }

        MoveCursor(position);
        RotateCursor(rotation);
        ApplyFeedbackToCursor(validity);
        buildToolsUI.AdjustLabels(cost, size);
        buildToolsUI.canPlace = validity;
        previewPos = position;
    }

    public void UpdateSize(Vector3 position)
    {
        Vector3 newSize = expanderParent.transform.InverseTransformPoint(position);
        Vector3 horizontalPos = expanderParent.transform.Find("horizontal").transform.localPosition;
        Vector3 verticalPos = expanderParent.transform.Find("vertical").transform.localPosition;

        if (SelectedCursor.name == "horizontal")
        {
            previewSize = new Vector2Int(Mathf.RoundToInt(newSize.x), previewSize.y);
            expanderParent.transform.Find("horizontal").transform.SetLocalPositionAndRotation(new Vector3(Mathf.RoundToInt(newSize.x), horizontalPos.y, horizontalPos.z), Quaternion.Euler(0, 0, 90));
            expanderParent.transform.Find("vertical").transform.SetLocalPositionAndRotation(new Vector3(Mathf.RoundToInt(newSize.x)/ 2, verticalPos.y, verticalPos.z), Quaternion.Euler(0, 90, 90));
        }
        else if (SelectedCursor.name == "vertical")
        {
            previewSize = new Vector2Int(previewSize.x, Mathf.RoundToInt(newSize.z));
            expanderParent.transform.Find("horizontal").transform.SetLocalPositionAndRotation(new Vector3(horizontalPos.x, horizontalPos.y, Mathf.RoundToInt(newSize.z) / 2), Quaternion.Euler(0, 0, 90));
            expanderParent.transform.Find("vertical").transform.SetLocalPositionAndRotation(new Vector3(verticalPos.x, verticalPos.y, Mathf.RoundToInt(newSize.z)), Quaternion.Euler(0, 90, 90));
        }

        cellIndicator.transform.localScale = new Vector3(previewSize.x, previewSize.y, previewSize.y);
        previewSelector.transform.localScale = new Vector3(previewSize.x - 0.1f, 0.6f, previewSize.y - 0.1f);
    }

    public void UpdatePointer(Vector3 position, bool validity, int index, int cost, int length, int width)
    {
        GameObject pointer = Instantiate(pointerCursor, new Vector3(position.x, position.y + 0.01f, position.z), Quaternion.Euler(0, 0, 0));
        pointer.transform.SetParent(expanderParent.transform);
        expanders.Add(pointer);

        cellIndicator.transform.localScale = new Vector3(previewSize.x, previewSize.y, previewSize.y);
        previewSelector.transform.localScale = new Vector3(previewSize.x - 0.1f, 0.6f, previewSize.y - 0.1f);

        LineRenderer line = expanderParent.GetComponent<LineRenderer>();
        line.positionCount = index + 1;
        line.SetPosition(index, new Vector3(position.x, position.y + 0.01f, position.z));

        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        line.material.color = c; cellIndicatorRenderer.material.color = c;

        ApplyFeedbackToCursor(validity);
        buildToolsUI.canPlace = validity;
        buildToolsUI.AdjustLabels(cost, new Vector2Int(length, width));
        previewPos = position;
    }

    public void MovePointer(Vector3 position, bool validity, int cost, int length, int width)
    {
        SelectedCursor.transform.position = position;

        cellIndicator.transform.localScale = new Vector3(previewSize.x, previewSize.y, previewSize.y);
        previewSelector.transform.localScale = new Vector3(previewSize.x - 0.1f, 0.6f, previewSize.y - 0.1f);

        LineRenderer line = expanderParent.GetComponent<LineRenderer>();
        int index = expanders.IndexOf(SelectedCursor);
        line.SetPosition(index, new Vector3(position.x, position.y + 0.01f, position.z));

        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        line.material.color = c; cellIndicatorRenderer.material.color = c;

        ApplyFeedbackToCursor(validity);
        buildToolsUI.canPlace = validity;
        buildToolsUI.AdjustLabels(cost, new Vector2Int(length, width));
        previewPos = position;
    }

    public void ClearPointer()
    {
        foreach (GameObject expander in expanders)
            GameObject.Destroy(expander);
        expanders.Clear();
        LineRenderer line = expanderParent.GetComponent<LineRenderer>();
        line.ResetBounds();
        line.positionCount = 0;
    }

    public bool CheckPreviewPositions()
    {
        if (input.RayHitObject(previewSelector.transform.GetChild(0).gameObject))
            return true;
        else
            return false;
    }

    public bool CheckExpansionHandle()
    {
        bool ans = false;
        foreach (GameObject expander in expanders)
        {
            if (input.RayHitObject(expander))
            {
                ans = true;
                SelectedCursor = expander;
                SelectedCursor.GetComponent<Renderer>().material.color = selectedColor;
            }
        }
        return ans;
    }

    private void ApplyFeedbackToPreview(bool validity)
    {
        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        previewMaterialInstance.color = c;
    }

    private void ApplyFeedbackToCursor(bool validity)
    {
        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        cellIndicatorRenderer.material.color = c; cellIndicatorRenderer.material.color = c;
        previewSelector.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = c;
    }

    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = new Vector3(position.x, position.y + previewYOffset, position.z);
        previewSelector.transform.position = new Vector3(position.x + 0.05f, position.y + previewYOffset, position.z + 0.05f);
        expanderParent.transform.position = new Vector3(position.x, position.y + previewYOffset * 2, position.z);
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = new Vector3(
            position.x,
            position.y + previewYOffset,
            position.z
        );
    }

    private void RotateCursor(int rotation)
    {
        cellIndicator.transform.rotation = Quaternion.Euler(0, rotation, 0);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        expanderParent.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private void RotatePreview(int rotation)
    {
        previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }
}