using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Splines.ExtrusionShapes;

public class WallModifyPreview : IDynamicPreviewSystem
{
    private PlacementSystem system;
    private InputManager input;

    public List<GameObject> expanders = new();
    public GameObject selectedCursor;
    private Color selectedColor;
    private Vector3 latestPosition;
    private float width;
    private float height;
    private bool expand;
    private Wall selectedWall;


    public void StartPreview(Wall wall, PlacementSystem system, InputManager input, float width, float height)
    {
        this.system = system;
        this.input = input;
        this.width = width;
        this.height = height;
        selectedColor = Color.darkGreen;

        PrepareDynamicCursor(wall.points[0]);
        selectedWall = wall;
    }

    private void PrepareDynamicCursor(Vector3 originPos)
    {
        if (system.dynamicCursor.Splines.Count >= 0)
        {
            system.dynamicCursor.AddSpline();
        }

        system.expanderParent.transform.position = new Vector3(originPos.x, 0.01f, originPos.z);
        system.expanderParent.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void AddPoint(int index, Vector3 pos)
    {
        GameObject pointer = GameObject.Instantiate(system.pointerCursor, new Vector3(pos.x, pos.y + 0.01f, pos.z), Quaternion.Euler(0, 0, 0));
        pointer.transform.SetParent(system.expanderParent.transform);
        float pointerWidth = width > 0.5f ? width * 2f : 1;
        pointer.transform.localScale = new Vector3(pointerWidth, pointerWidth, pointerWidth);
        expanders.Insert(index, pointer);
        //Debug.Log(width);

        BezierKnot knot = new BezierKnot(new Vector3(pos.x, pos.y, pos.z));
        system.dynamicCursor[0].Insert(index, knot);

        UpdateDirection();  
        UpdatePreviewSpline(width, height);  
    }

    private void UpdateDirection()
    {
        if (system.dynamicCursor[0].Count >= 2)
        {
            for (int i = 0; i < system.dynamicCursor[0].Count - 1; i++)
            {
                BezierKnot kt = system.dynamicCursor[0][i];
                Vector3 vecA = (Vector3)(system.dynamicCursor[0][i + 1].Position - system.dynamicCursor[0][i].Position);
                kt.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, vecA, Vector3.up), 0);
                if (i > 0)
                {
                    Vector3 vecB = (Vector3)(system.dynamicCursor[0][i].Position - system.dynamicCursor[0][i - 1].Position);
                    kt.TangentIn = (vecB.normalized - vecA.normalized) * 0.1f;
                }
                kt.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                system.dynamicCursor[0][i] = kt;
            }

            BezierKnot ktL = system.dynamicCursor[0][^1];
            ktL.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, system.dynamicCursor[0][^1].Position - system.dynamicCursor[0][^2].Position, Vector3.up), 0);
            ktL.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
            system.dynamicCursor[0][^1] = ktL;
        }
    }

    private void UpdatePreviewSpline(float width, float height)
    {
        if (system.dynamicCursor[0].Count >= 2)
        {
            float uvOffset = 0;

            Mesh mesh = new Mesh();
            Mesh mesh2 = new Mesh();

            List<Vector3> currentVerts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> currentUVs = new List<Vector2>();

            for (int currentPointIndex = 1; currentPointIndex < system.dynamicCursor[0].Count; currentPointIndex++)
            {
                int vertOffset = currentPointIndex;

                Vector3 dir = (system.dynamicCursor[0][vertOffset - 1].Position - system.dynamicCursor[0][vertOffset].Position);
                Vector3 side = Vector3.Cross(dir.normalized, Vector3.up);

                Vector3 p1 = ((Vector3)system.dynamicCursor[0][vertOffset - 1].Position) - side * width;
                Vector3 p2 = p1 + new Vector3(0, height, 0);
                Vector3 p3 = ((Vector3)system.dynamicCursor[0][vertOffset - 1].Position) + side * width;
                Vector3 p4 = p3 + new Vector3(0, height, 0);
                Vector3 p5 = ((Vector3)system.dynamicCursor[0][vertOffset].Position) - side * width;
                Vector3 p6 = p5 + new Vector3(0, height, 0);
                Vector3 p7 = ((Vector3)system.dynamicCursor[0][vertOffset].Position) + side * width;
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

            int t25 = (8 * (system.dynamicCursor[0].Count - 2)) + 4;
            int t26 = (8 * (system.dynamicCursor[0].Count - 2)) + 6;
            int t27 = (8 * (system.dynamicCursor[0].Count - 2)) + 7;
            int t28 = (8 * (system.dynamicCursor[0].Count - 2)) + 7;
            int t29 = (8 * (system.dynamicCursor[0].Count - 2)) + 5;
            int t30 = (8 * (system.dynamicCursor[0].Count - 2)) + 4;

            tris.AddRange(new List<int> { t19, t20, t21, t22, t23, t24 });
            tris.AddRange(new List<int> { t25, t26, t27, t28, t29, t30 });

            mesh.SetVertices(currentVerts);
            mesh.SetTriangles(tris, 0);

            mesh.SetUVs(0, currentUVs);

            mesh2.SetVertices(currentVerts);
            mesh2.SetTriangles(tris, 0);

            mesh2.SetUVs(0, currentUVs);

            system.dynamicMesh.mesh = mesh;
            system.dynamicCollider.sharedMesh = mesh2;
        }
    }

    public int GetSplineIndex(Vector3 point)
    {
        SplineUtility.GetNearestPoint(system.dynamicCursor[0], point, out _, out float t1);

        if (system.dynamicCursor[0].Count > 0)
        {
            int start = 0; int end = system.dynamicCursor[0].Count - 1;

            while (end > start)
            {
                int mid = (start + end) / 2;
                float t2 = system.dynamicCursor[0].ConvertIndexUnit(mid, PathIndexUnit.Knot, PathIndexUnit.Normalized);
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

            if (t1 > system.dynamicCursor[0].ConvertIndexUnit(start, PathIndexUnit.Knot, PathIndexUnit.Normalized))
            {
                return start + 1;
            }
            else return start;
        }
        else return -1;
    }

    public void ApplyFeedback(bool validity)
    {
        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        system.dynamicRenderer.material.color = c;

        system.GetBuildToolsUI().canPlace = validity;
    }


    public bool CheckExpansionHandles()
    {
        bool ans = false;
        foreach (GameObject expander in expanders)
        {
            if (input.RayHitObject(expander))
            {
                ans = true;
                selectedCursor = expander;
                selectedCursor.GetComponent<Renderer>().material.color = selectedColor;
            }
        }
        return ans;
    }

    public bool CheckPreviewSplines()
    {
        if (input.RayHitObject(system.dynamicCollider.gameObject))
        {
            selectedCursor = system.dynamicCollider.gameObject;
            selectedCursor.GetComponent<Renderer>().material.color = selectedColor;
            return true;
        }
        return false;
    }

    public GameObject GetPreviewObject()
    {
        return system.dynamicCollider.gameObject;
    }

    public Vector3 GetPreviewPosition()
    {
        return latestPosition;
    }

    public Vector2Int GetPreviewSize()
    {
        return Vector2Int.one;
    }

    public void ModifyPointer(int index, Vector3 pos)
    {
        expanders[index].transform.position = pos;

        //LineRenderer line = system.expanderParent.GetComponent<LineRenderer>();
        BezierKnot kt = system.dynamicCursor[0][index];
        kt.Position = pos;
        system.dynamicCursor[0].SetKnot(index, kt);

        UpdateDirection();
        UpdatePreviewSpline(width, height); 
        latestPosition = pos;    
    }

    public void SetModifyState(bool state)
    {
        this.expand = state;
    }

    public bool GetModifyState()
    {
        return expand;
    }

    public void StopPreview()
    {
        expand = false;
        foreach (GameObject expander in expanders)
            GameObject.Destroy(expander);
        expanders.Clear();
        system.dynamicMesh.mesh = null;
        system.dynamicCollider.sharedMesh = null;
        system.dynamicCursor[0].Clear();
    }

    public void DeletePointer(int index)
    {
        GameObject.Destroy(expanders[index]);
        expanders.RemoveAt(index);

        system.dynamicCursor[0].RemoveAt(index);

        UpdateDirection();
    }

    public void ClearPointer()
    {
        foreach (GameObject expander in expanders)
            GameObject.Destroy(expander);
        expanders.Clear();
        system.dynamicCursor[0].Clear();
        system.dynamicMesh.mesh = null;
        system.dynamicCollider.sharedMesh = null;
    }

    public void Deselect()
    {
        foreach (GameObject expander in expanders)
        {
            expander.GetComponent<Renderer>().material.color = system.expandingCursor.GetComponent<Renderer>().sharedMaterial.color;
        }
    }
}