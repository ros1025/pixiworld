using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;

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

    [SerializeField]
    private SplineContainer dynamicCursor;
    [SerializeField]
    private MeshFilter dynamicMesh;
    [SerializeField]
    private  MeshRenderer dynamicRenderer;
    [SerializeField]
    private MeshCollider dynamicCollider;

    public List<MatData> materials;

    private Vector2Int minSize;

    private void Start()
    {
        previewMaterialInstance = new Material(previewMaterialPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
        //materials = new();
    }

    public void StartMovingObjectPreview(Vector3 gridPosition, float rotation, GameObject prefab, Vector2Int size, List<MatData> materials)
    {
        previewObject = prefab;
        previewSelector = previewObject.transform.Find("Selector").gameObject;
        previewSize = size;
        previewPos = gridPosition;
        this.materials.Clear();
        for (int i = 0; i < materials.Count; i++)
        {
            this.materials.Add(new MatData(materials[i]));
        }

        int index = 0;
        for (int i = 0; i < previewObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            if (previewObject.GetComponentsInChildren<Renderer>()[i] != previewSelector.GetComponentInChildren<Renderer>())
            {
                /*
                List<Material> matList = materials.GetRange(index, previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length);
                Material[] mats = new Material[previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length];
                for (int j = 0; j < previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    mats[j] = matList[j];
                }
                previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials = mats;
                index += previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length;
                */
                for (int j = 0; j < previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                    index++;
                }
            }
        }

        PreparePreview(prefab);
        PrepareCursor(size);
        MoveCursor(gridPosition);
        RotateCursor(rotation);
        RotatePreview(rotation);
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
        cellIndicator.SetActive(true);

        if (materials != null && materials.Count > 0)
        {
            materials.Clear();
        }
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform.parent.gameObject != previewSelector)
            {
                for (int j = 0; j < renderer.sharedMaterials.Length; j++)
                {
                    renderer.materials[j] = Instantiate(renderer.sharedMaterials[j]);
                    materials.Add(new MatData(renderer.sharedMaterials[j].color));
                }
            }
        }
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
        MoveCursor(originPos);
        cellIndicator.SetActive(true);
    }

    public void StartMovingZones(Vector3 gridPosition, float rotation, GameObject prefab, Vector2Int size)
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
        cellIndicator.SetActive(true);
    }

    public void StartCreatingRoads(Vector3 gridPosition)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = gridPosition;
        PrepareDynamicCursor(gridPosition);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
    }

    public void AddWalls()
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = Vector3.zero;
        PrepareDynamicCursor(Vector3.zero);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
    }

    public void ModifyWalls(Wall wall)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = Vector3.zero;
        PrepareDynamicCursor(Vector3.zero);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
        selectedWall = wall;
    }

    public void ModifyRoad(List<Vector3> points, Roads road, int cost, float length, float width)
    {
        previewSelector = Instantiate(previewSelectorObject);
        previewPos = points[0];
        PrepareDynamicCursor(points[0]);
        MoveCursor(Vector3.zero);
        RotateCursor(0);
        selectedRoad = road;
        for (int i = 0; i < points.Count; i++)
        {
            UpdatePointer(points[i], true, i, cost * Mathf.RoundToInt(length), Mathf.RoundToInt(length), width, 0.1f);
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
        //expanderParent.AddComponent<LineRenderer>();
        //expanderParent.GetComponent<LineRenderer>().positionCount = 0;
        //expanderParent.GetComponent<LineRenderer>().material = previewMaterialPrefab;

        if (dynamicCursor.Splines.Count >= 0)
        {
            dynamicCursor.AddSpline();
        }

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
        dynamicMesh.mesh = null;
        dynamicCollider.sharedMesh = null;
        dynamicCursor[0].Clear();
        materials.Clear();
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
        materials.Clear();
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

    public void UpdatePosition(Vector3 position, bool validity, Vector2Int size, int cost, float rotation)
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

    public void UpdatePointer(Vector3 position, bool validity, int index, int cost, float length, float width, float height)
    {
        GameObject pointer = Instantiate(pointerCursor, new Vector3(position.x, position.y + 0.01f, position.z), Quaternion.Euler(0, 0, 0));
        pointer.transform.SetParent(expanderParent.transform);
        float pointerWidth = width > 0.5f ? width * 2f : 1;
        pointer.transform.localScale = new Vector3(pointerWidth, pointerWidth, pointerWidth);
        expanders.Insert(index, pointer);
        //Debug.Log(width);

        cellIndicator.transform.localScale = new Vector3(previewSize.x, previewSize.y, previewSize.y);
        previewSelector.transform.localScale = new Vector3(previewSize.x - 0.1f, 0.6f, previewSize.y - 0.1f);

        BezierKnot knot = new BezierKnot(new Vector3(position.x, position.y, position.z));
        dynamicCursor[0].Insert(index, knot);

        UpdateDirection();

        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        dynamicRenderer.material.color = c;

        ApplyFeedbackToCursor(validity);
        buildToolsUI.canPlace = validity;
        buildToolsUI.AdjustLabels(cost, new Vector2Int(Mathf.RoundToInt(length), Mathf.RoundToInt(width) > 0 ? Mathf.RoundToInt(width) : 1));
        previewPos = position;

        UpdatePreviewSpline(width, height);
    }

    public void RemovePointer(int index, bool validity, int cost, float length, float width, float height)
    {
        Destroy(expanders[index]);
        expanders.RemoveAt(index);

        dynamicCursor[0].RemoveAt(index);

        UpdateDirection();

        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        dynamicRenderer.material.color = c;

        ApplyFeedbackToCursor(validity);
        buildToolsUI.canPlace = validity;
        buildToolsUI.AdjustLabels(cost, new Vector2Int(Mathf.RoundToInt(length), Mathf.RoundToInt(width) > 0 ? Mathf.RoundToInt(width) : 1));
        UpdatePreviewSpline(width, height);

    }

    public void MovePointer(Vector3 position, bool validity, int cost, float length, float width, float height)
    {
        SelectedCursor.transform.position = position;

        cellIndicator.transform.localScale = new Vector3(previewSize.x, previewSize.y, previewSize.y);
        previewSelector.transform.localScale = new Vector3(previewSize.x - 0.1f, 0.6f, previewSize.y - 0.1f);

        //LineRenderer line = expanderParent.GetComponent<LineRenderer>();
        int index = expanders.IndexOf(SelectedCursor);
        BezierKnot kt = dynamicCursor[0][index];
        kt.Position = position;
        dynamicCursor[0].SetKnot(index, kt);

        UpdateDirection();

        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        dynamicRenderer.material.color = c;

        ApplyFeedbackToCursor(validity);
        buildToolsUI.canPlace = validity;
        buildToolsUI.AdjustLabels(cost, new Vector2Int(Mathf.RoundToInt(length), Mathf.RoundToInt(width) > 0 ? Mathf.RoundToInt(width) : 1));
        UpdatePreviewSpline(width, height); 
        previewPos = position;
    }

    public void ClearPointer()
    {
        foreach (GameObject expander in expanders)
            GameObject.Destroy(expander);
        expanders.Clear();
        dynamicCursor[0].Clear();
        dynamicMesh.mesh = null;
        dynamicCollider.sharedMesh = null;
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

    public bool CheckPreviewSpline()
    {
        if (input.RayHitObject(dynamicCollider.gameObject))
        {
            SelectedCursor = dynamicCollider.gameObject;
            SelectedCursor.GetComponent<Renderer>().material.color = selectedColor;
            return true;
        }
        return false;
    }

    public int GetSplineIndex(Vector3 point)
    {
        SplineUtility.GetNearestPoint(dynamicCursor[0], point, out _, out float t1);

        if (dynamicCursor[0].Count > 0)
        {
            int start = 0; int end = dynamicCursor[0].Count - 1;

            while (end > start)
            {
                int mid = (start + end) / 2;
                float t2 = dynamicCursor[0].ConvertIndexUnit(mid, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                if (t1 > t2)
                {
                    start = mid + 1;
                }
                else if (t1 < t2)
                {
                    end = mid - 1;
                }
                else return mid;
            }

            if (t1 > dynamicCursor[0].ConvertIndexUnit(start, PathIndexUnit.Knot, PathIndexUnit.Normalized))
            {
                return start + 1;
            }
            else return start;
        }
        else return -1;
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
        previewSelector.transform.position = new Vector3(position.x + 0.05f, position.y, position.z + 0.05f);
        //previewSelector.transform.localPosition = new Vector3(0.05f, 0f, 0.05f);
        expanderParent.transform.position = new Vector3(position.x, position.y + previewYOffset * 2, position.z);
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = new Vector3(
            position.x,
            position.y,
            position.z
        );
    }

    public void RefreshColors()
    {
        int index = 0;
        for (int i = 0; i < previewObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            if (previewObject.GetComponentsInChildren<Renderer>()[i] != previewSelector.GetComponentInChildren<Renderer>())
            {
                for (int j = 0; j < previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    previewObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                    index++;
                }
            }
        }
    }

    private void RotateCursor(float rotation)
    {
        cellIndicator.transform.rotation = Quaternion.Euler(0, rotation, 0);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        expanderParent.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private void RotatePreview(float rotation)
    {
        previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private void UpdateDirection()
    {
        if (dynamicCursor[0].Count >= 2)
        {
            for (int i = 0; i < dynamicCursor[0].Count - 1; i++)
            {
                BezierKnot kt = dynamicCursor[0][i];
                Vector3 vecA = (Vector3)(dynamicCursor[0][i + 1].Position - dynamicCursor[0][i].Position);
                kt.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, vecA, Vector3.up), 0);
                if (i > 0)
                {
                    Vector3 vecB = (Vector3)(dynamicCursor[0][i].Position - dynamicCursor[0][i - 1].Position);
                    kt.TangentIn = (vecB.normalized - vecA.normalized) * 0.1f;
                }
                kt.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                dynamicCursor[0][i] = kt;
            }

            BezierKnot ktL = dynamicCursor[0][^1];
            ktL.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, dynamicCursor[0][^1].Position - dynamicCursor[0][^2].Position, Vector3.up), 0);
            ktL.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
            dynamicCursor[0][^1] = ktL;
        }
    }

    private void UpdatePreviewSpline(float width, float height)
    {
        if (dynamicCursor[0].Count >= 2)
        {
            float uvOffset = 0;

            Mesh mesh = new Mesh();
            Mesh mesh2 = new Mesh();

            List<Vector3> currentVerts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> currentUVs = new List<Vector2>();

            for (int currentPointIndex = 1; currentPointIndex < dynamicCursor[0].Count; currentPointIndex++)
            {
                int vertOffset = currentPointIndex;

                Vector3 dir = (dynamicCursor[0][vertOffset - 1].Position - dynamicCursor[0][vertOffset].Position);
                Vector3 side = Vector3.Cross(dir.normalized, Vector3.up);

                Vector3 p1 = ((Vector3)dynamicCursor[0][vertOffset - 1].Position) - side * width;
                Vector3 p2 = p1 + new Vector3(0, height, 0);
                Vector3 p3 = ((Vector3)dynamicCursor[0][vertOffset - 1].Position) + side * width;
                Vector3 p4 = p3 + new Vector3(0, height, 0);
                Vector3 p5 = ((Vector3)dynamicCursor[0][vertOffset].Position) - side * width;
                Vector3 p6 = p5 + new Vector3(0, height, 0);
                Vector3 p7 = ((Vector3)dynamicCursor[0][vertOffset].Position) + side * width;
                Vector3 p8 = p7 + new Vector3(0, height, 0);

                //offset = 8 * calculateRes(currentSplineIndex);
                int offset = 8 * (currentPointIndex - 1);

                int t1 = offset + 0;
                int t2 = offset + 4;
                int t3 = offset + 5;
                int t4 = offset + 5;
                int t5 = offset + 1;
                int t6 = offset + 0;

                int t7 = offset + 2;
                int t8 = offset + 3;
                int t9 = offset + 7;
                int t10 = offset + 7;
                int t11 = offset + 6;
                int t12 = offset + 2;

                int t13 = offset + 5;
                int t14 = offset + 7;
                int t15 = offset + 3;
                int t16 = offset + 3;
                int t17 = offset + 1;
                int t18 = offset + 5;

                currentVerts.AddRange(new List<Vector3> { p1, p2, p3, p4, p5, p6, p7, p8 });
                tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
                tris.AddRange(new List<int> { t6, t5, t4, t3, t2, t1 });
                tris.AddRange(new List<int> { t7, t8, t9, t10, t11, t12 });
                tris.AddRange(new List<int> { t12, t11, t10, t9, t8, t7 });
                tris.AddRange(new List<int> { t13, t14, t15, t16, t17, t18 });
                tris.AddRange(new List<int> { t18, t17, t16, t15, t14, t13 });

                float distance = Vector3.Distance(p1, p5);
                float uvDistance = uvOffset + distance;
                currentUVs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvOffset, 0), new Vector2(uvOffset, 1),
                    new Vector2(uvDistance, 0), new Vector2(uvDistance, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1)});

                uvOffset += distance;
            }

            int t19 = 0;
            int t20 = 1;
            int t21 = 3;
            int t22 = 3;
            int t23 = 2;
            int t24 = 0;

            int t25 = (8 * (dynamicCursor[0].Count - 2)) + 4;
            int t26 = (8 * (dynamicCursor[0].Count - 2)) + 6;
            int t27 = (8 * (dynamicCursor[0].Count - 2)) + 7;
            int t28 = (8 * (dynamicCursor[0].Count - 2)) + 7;
            int t29 = (8 * (dynamicCursor[0].Count - 2)) + 5;
            int t30 = (8 * (dynamicCursor[0].Count - 2)) + 4;

            tris.AddRange(new List<int> { t19, t20, t21, t22, t23, t24 });
            tris.AddRange(new List<int> { t25, t26, t27, t28, t29, t30 });

            mesh.SetVertices(currentVerts);
            mesh.SetTriangles(tris, 0);

            mesh.SetUVs(0, currentUVs);

            mesh2.SetVertices(currentVerts);
            mesh2.SetTriangles(tris, 0);

            mesh2.SetUVs(0, currentUVs);

            dynamicMesh.mesh = mesh;
            dynamicCollider.sharedMesh = mesh2;
        }
    }
}