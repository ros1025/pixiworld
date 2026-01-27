using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Splines;

public class RoadIntersectionModifyPreview : IDynamicPreviewSystem
{
    private PlacementSystem system;
    private InputManager input;

    public GameObject cursor;
    private Color selectedColor;
    

    private Vector3 previewPos;

    private Intersection selectedIntersection;
    private List<(Spline, int, float)> selectedSplines;

    public void StartPreview(Vector3 point, Intersection intersection, List<Roads> roads, PlacementSystem system, InputManager input)
    {
        this.system = system;
        this.input = input;
        selectedIntersection = intersection;
        previewPos = point;
        selectedColor = Color.darkGreen;
        selectedSplines = new();

        foreach (Roads road in roads)
        {
            Spline spline = new();

            for (int i = 0; i < road.points.Count; i++)
            {
                BezierKnot knot = new();

                if (i == 0 && intersection.junctions.FindIndex(item => item.spline == road.road && item.knotIndex == 0) != -1)
                {
                    knot.Position = point;
                    selectedSplines.Add((spline, 0, road.width));
                }
                else if (i == road.points.Count - 1 && intersection.junctions.FindIndex(item => item.spline == road.road && item.knotIndex > 0) != -1)
                {
                    knot.Position = point;
                    selectedSplines.Add((spline, 1, road.width));
                }
                else knot.Position = road.points[i];

                spline.Add(knot);
            }

            system.dynamicCursor.AddSpline(spline);
        }

        AddPoint(0, point);
        UpdatePreviewSpline(); 
    }

    public void ModifyPointer(int index, Vector3 pos)
    {
        cursor.transform.position = pos;
        previewPos = pos;

        for (int i = 0; i < selectedSplines.Count; i++)
        {
            BezierKnot currentKnot = new();
            currentKnot.Position = pos;
            int id = selectedSplines[i].Item2 == 0 ? 0 : selectedSplines[i].Item1.Count - 1;
            selectedSplines[i].Item1[id] = currentKnot;
        }

        UpdateDirection();
        UpdatePreviewSpline(); 
        previewPos = pos;    
    }

    public void AddPoint(int index, Vector3 pos)
    {
        GameObject pointer = GameObject.Instantiate(system.pointerCursor, new Vector3(pos.x, pos.y + 0.01f, pos.z), Quaternion.Euler(0, 0, 0));
        pointer.transform.SetParent(system.expanderParent.transform);
        float pointerWidth = 1f;
        pointer.transform.localScale = new Vector3(pointerWidth, pointerWidth, pointerWidth);
        cursor = pointer;
        previewPos = pos;

        for (int i = 0; i < selectedSplines.Count; i++)
        {
            BezierKnot currentKnot = new();
            currentKnot.Position = pos;
            int id = selectedSplines[i].Item2 == 0 ? 0 : selectedSplines[i].Item1.Count - 1;
            selectedSplines[i].Item1[id] = currentKnot;
        }

        UpdateDirection();  
        UpdatePreviewSpline();
    }

    private void UpdateDirection()
    {
        foreach ((Spline, int, float) spline in selectedSplines)
        {
            if (spline.Item1.Count >= 2)
            {
                for (int i = 0; i < spline.Item1.Count - 1; i++)
                {
                    BezierKnot kt = spline.Item1[i];
                    Vector3 vecA = (Vector3)(spline.Item1[i + 1].Position - spline.Item1[i].Position);
                    kt.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, vecA, Vector3.up), 0);
                    if (i > 0)
                    {
                        Vector3 vecB = (Vector3)(spline.Item1[i].Position - spline.Item1[i - 1].Position);
                        kt.TangentIn = (vecB.normalized - vecA.normalized) * 0.1f;
                    }
                    kt.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                    spline.Item1[i] = kt;
                }

                BezierKnot ktL = spline.Item1[^1];
                ktL.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, spline.Item1[^1].Position - spline.Item1[^2].Position, Vector3.up), 0);
                ktL.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                spline.Item1[^1] = ktL;
            }
        }
    }

    private void UpdatePreviewSpline()
    {
        Mesh mesh = new Mesh();
        Mesh mesh2 = new Mesh();
        List<Vector3> currentVerts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> currentUVs = new List<Vector2>();
        int offset = 0;

        foreach ((Spline, int, float) group in selectedSplines)
        {
            if (group.Item1.Count >= 2)
            {
                for (int currentPointIndex = 1; currentPointIndex < group.Item1.Count; currentPointIndex++)
                {
                    int vertOffset = currentPointIndex;

                    Vector3 dir = (group.Item1[vertOffset - 1].Position - group.Item1[vertOffset].Position);
                    Vector3 side = Vector3.Cross(dir.normalized, Vector3.up);

                    Vector3 p1 = ((Vector3)group.Item1[vertOffset - 1].Position) - side * group.Item3;
                    Vector3 p2 = p1 + new Vector3(0, 0.1f, 0);
                    Vector3 p3 = ((Vector3)group.Item1[vertOffset - 1].Position) + side * group.Item3;
                    Vector3 p4 = p3 + new Vector3(0, 0.1f, 0);
                    Vector3 p5 = ((Vector3)group.Item1[vertOffset].Position) - side * group.Item3;
                    Vector3 p6 = p5 + new Vector3(0, 0.1f, 0);
                    Vector3 p7 = ((Vector3)group.Item1[vertOffset].Position) + side * group.Item3;
                    Vector3 p8 = p7 + new Vector3(0, 0.1f, 0);

                    //offset = 8 * calculateRes(currentSplineIndex);

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
                    offset += 8;

                    currentVerts.AddRange(new List<Vector3> { p1, p2, p3, p4, p5, p6, p7, p8 });
                    tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
                    tris.AddRange(new List<int> { t6, t5, t4, t3, t2, t1 });
                    tris.AddRange(new List<int> { t7, t8, t9, t10, t11, t12 });
                    tris.AddRange(new List<int> { t12, t11, t10, t9, t8, t7 });
                    tris.AddRange(new List<int> { t13, t14, t15, t16, t17, t18 });
                    tris.AddRange(new List<int> { t18, t17, t16, t15, t14, t13 });

                    float distance = Vector3.Distance(p1, p5);
                }

                int t19 = 0;
                int t20 = 1;
                int t21 = 3;
                int t22 = 3;
                int t23 = 2;
                int t24 = 0;

                int t25 = (8 * (group.Item1.Count - 2)) + 4;
                int t26 = (8 * (group.Item1.Count - 2)) + 6;
                int t27 = (8 * (group.Item1.Count - 2)) + 7;
                int t28 = (8 * (group.Item1.Count - 2)) + 7;
                int t29 = (8 * (group.Item1.Count - 2)) + 5;
                int t30 = (8 * (group.Item1.Count - 2)) + 4;

                tris.AddRange(new List<int> { t19, t20, t21, t22, t23, t24 });
                tris.AddRange(new List<int> { t25, t26, t27, t28, t29, t30 });
            }
        }

        mesh.SetVertices(currentVerts);
        mesh.SetTriangles(tris, 0);

        mesh.SetUVs(0, currentUVs);

        mesh2.SetVertices(currentVerts);
        mesh2.SetTriangles(tris, 0);

        mesh2.SetUVs(0, currentUVs);

        system.dynamicMesh.mesh = mesh;
        system.dynamicCollider.sharedMesh = mesh2;
    }

    public void DeletePointer(int index)
    {
        return;
    }

    public void ClearPointer()
    {
        StopPreview();
    }

    public bool CheckPreviewSplines()
    {
        return false;
    }

    public bool CheckExpansionHandles()
    {
        if (input.RayHitObject(cursor))
        {
            cursor.GetComponent<Renderer>().material.color = selectedColor;
            return true;
        }
        return false;    
    }

    public void SetModifyState(bool state)
    {
        return;
    }

    public bool GetModifyState()
    {
        return false;
    }

    public void StopPreview()
    {
        GameObject.Destroy(cursor);
        system.dynamicMesh.mesh = null;
        system.dynamicCollider.sharedMesh = null;

        foreach ((Spline, int, float) spline in selectedSplines)
        {
            system.dynamicCursor.RemoveSpline(spline.Item1);  
        }  
    }

    public void ApplyFeedback(bool validity)
    {
        Color c = validity ? Color.white : Color.red;

        c.a = 0.5f;
        system.dynamicRenderer.material.color = c;
    }

    public Vector3 GetPreviewPosition()
    {
        return previewPos;
    }

    public Vector2Int GetPreviewSize()
    {
        return Vector2Int.one;
    }

    public GameObject GetPreviewObject()
    {
        return cursor;
    }
    
    public void Deselect()
    {
        cursor.GetComponent<Renderer>().material.color = system.expandingCursor.GetComponent<Renderer>().sharedMaterial.color;
    }
}