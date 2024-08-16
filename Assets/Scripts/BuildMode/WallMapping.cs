using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WallMapping : MonoBehaviour
{
    private List<Vector3> m_vertsP1;
    private List<Vector3> m_vertsP2;
    private int times;
    public List<Wall> walls;
    public List<Room> rooms;
    public List<Intersection> intersections;
    [SerializeField] private GameObject wallParent;
    [SerializeField] private GameObject floorParent;
    [SerializeField] private GameObject ceilParent;
    [SerializeField] private GameObject gizmoPointer;
    [SerializeField] private SplineSampler m_SplineSampler;
    [SerializeField] private SplineContainer m_SplineContainer;
    [SerializeField] private Material defaultWallMaterial;
    [SerializeField] private Material defaultFloorMaterial;

    public void MakeWalls()
    {
        //GetVerts();
        BuildMesh();
    }

    private void GetVerts()
    {
        m_vertsP1 = new List<Vector3>();
        m_vertsP2 = new List<Vector3>();

        Vector3 p1;
        Vector3 p2;
        for (int j = 0; j < walls.Count; j++)
        {
            //int resolution = (int)(walls[j].wall.GetLength() * 2);
            int resolution = walls[j].resolution;
            float step = 1f / (float)resolution;
            for (int i = 0; i < resolution; i++)
            {
                float t = step * i;

                m_SplineSampler.SampleSplineWidth(j, t, 0.04f, out p1, out p2);
                m_vertsP1.Add(p1);
                m_vertsP2.Add(p2);
            }

            m_SplineSampler.SampleSplineWidth(j, 1f, 0.04f, out p1, out p2);
            m_vertsP1.Add(p1);
            m_vertsP2.Add(p2);
        }
    }

    private void GetVerts(int index, out List<Vector3> vertsP1, out List<Vector3> vertsP2)
    {
        vertsP1 = new List<Vector3>();
        vertsP2 = new List<Vector3>();

        Vector3 p1;
        Vector3 p2;
        //int resolution = (int)(walls[j].wall.GetLength() * 2);
        int resolution = walls[index].resolution;
        float step = 1f / (float)resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = step * i;

            m_SplineSampler.SampleSplineWidth(index, t, 0.04f, out p1, out p2);
            vertsP1.Add(p1);
            vertsP2.Add(p2);
        }

        m_SplineSampler.SampleSplineWidth(index, 1f, 0.04f, out p1, out p2);
        vertsP1.Add(p1);
        vertsP2.Add(p2);
    }

    private int calculateRes(int index)
    {
        int res = 0;
        for (int i = 0; i < index; i++)
        {
            res += walls[i].resolution;
        }
        return res;
    }

    private bool HitsContainCollider(List<RaycastHit> hitList, RaycastHit hit)
    {
        foreach (RaycastHit testHit in hitList)
        {
            if (testHit.collider == hit.collider)
            {
                return true;
            }
        }
        return false;
    }

    //remove points from wall after the intersection and place them into a new wall
    private void FilterPoints(Spline wall, Spline wall2, List<Vector3> points, float ratio, out List<Vector3> points2)
    {
        List<BezierKnot> knotList = new(); List<BezierKnot> knotList2 = new();
        List<Vector3> removePoints = new();
        points2 = new();
        for (int i = 0; i < points.Count; i++)
        {
            if (Vector3.Distance(points[i], SplineUtility.EvaluatePosition(wall, ratio)) < 0.5f)
            {
                removePoints.Add(points[i]);
            }
            else
            {
                int j = wall.IndexOf(wall[i]);
                float knotT = wall.ConvertIndexUnit(j, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                if (knotT > ratio)
                {
                    knotList.Add(wall[i]);
                }
                else
                {
                    knotList2.Add(wall[i]);
                    removePoints.Add(points[i]);
                    points2.Add(points[i]);
                }
            }
            
        }
        foreach (Vector3 point in removePoints) { points.Remove(point); }
        wall.Clear(); wall2.Clear();
        foreach (BezierKnot knot in knotList) { wall.Add(knot); }
        foreach (BezierKnot knot in knotList2) { wall2.Add(knot); }
    }

    private float EvaluateT(Spline e, Vector3 pos)
    {
        m_SplineSampler.SampleSplinePoint(e, pos, (int)(e.GetLength() * 2), out Vector3 nearestPoint, out float knotT);
        return knotT;
    }

    private void BuildWall(int currentSplineIndex)
    {
        int offset = 0; float uvOffset = 0;
        GetVerts(currentSplineIndex, out List<Vector3> m_vertsP1, out List<Vector3> m_vertsP2);

        Mesh wall = new Mesh();
        wall.subMeshCount = 3;

        List<Vector3> currentVerts = new List<Vector3>();
        List<int> trisA = new List<int>();
        List<int> trisB = new List<int>();
        List<int> trisS = new List<int>();
        List<Vector2> currentUVs = new List<Vector2>();

        Mesh c = new Mesh();
        List<int> trisC = new List<int>();

        int resolution = ((int)(walls[currentSplineIndex].resolution));
        //int splineOffset = resolution * currentSplineIndex;
        //int splineOffset = calculateRes(currentSplineIndex);
        //splineOffset += currentSplineIndex;

        for (int currentPointIndex = 1; currentPointIndex <= resolution; currentPointIndex++)
        {
            int vertOffset = currentPointIndex;
            Vector3 p1 = m_vertsP1[vertOffset - 1];
            Vector3 p2 = p1 + new Vector3(0, 2, 0);
            Vector3 p3 = m_vertsP2[vertOffset - 1];
            Vector3 p4 = p3 + new Vector3(0, 2, 0);
            Vector3 p5 = m_vertsP1[vertOffset];
            Vector3 p6 = p5 + new Vector3(0, 2, 0);
            Vector3 p7 = m_vertsP2[vertOffset];
            Vector3 p8 = p7 + new Vector3(0, 2, 0);

            offset = 0;
            //offset = 8 * calculateRes(currentSplineIndex);
            offset += 8 * (currentPointIndex - 1);

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

            int t13 = offset + 1;
            int t14 = offset + 5;
            int t15 = offset + 7;
            int t16 = offset + 7;
            int t17 = offset + 3;
            int t18 = offset + 1;

            currentVerts.AddRange(new List<Vector3> { p1, p2, p3, p4, p5, p6, p7, p8 });
            trisA.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
            trisA.AddRange(new List<int> { t6, t5, t4, t3, t2, t1 });
            trisB.AddRange(new List<int> { t7, t8, t9, t10, t11, t12 });
            trisB.AddRange(new List<int> { t12, t11, t10, t9, t8, t7 });
            trisS.AddRange(new List<int> { t13, t14, t15, t16, t17, t18 });

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

        int t25 = (8 * (resolution - 1)) + 4;
        int t26 = (8 * (resolution - 1)) + 6;
        int t27 = (8 * (resolution - 1)) + 7;
        int t28 = (8 * (resolution - 1)) + 7;
        int t29 = (8 * (resolution - 1)) + 5;
        int t30 = (8 * (resolution - 1)) + 4;

        trisS.AddRange(new List<int> { t19, t20, t21, t22, t23, t24 });
        trisS.AddRange(new List<int> { t25, t26, t27, t28, t29, t30 });

        for (int i = 0; i < trisA.Count; i++)
        {
            trisC.Add(trisA[i] /*- (8 * calculateRes(currentSplineIndex))*/);
        }
        for (int i = 0; i < trisB.Count; i++)
        {
            trisC.Add(trisB[i] /*- (8 * calculateRes(currentSplineIndex))*/);
        }
        for (int i = 0; i < trisS.Count; i++)
        {
            trisC.Add(trisS[i] /*- (8 * calculateRes(currentSplineIndex))*/);
        }

        //verts.AddRange(currentVerts);
        wall.SetVertices(currentVerts);
        wall.SetTriangles(trisS, 0);
        wall.SetTriangles(trisA, 1);
        wall.SetTriangles(trisB, 2);

        //tris.Add(trisA);
        //tris.Add(trisB);
        //tris[0].AddRange(trisS);

        c.SetVertices(currentVerts);
        c.SetTriangles(trisC, 0);
        walls[currentSplineIndex].collider.sharedMesh = c;

        wall.SetUVs(0, currentUVs);
        walls[currentSplineIndex].mesh.mesh = wall;
    }

    private void BuildIntersection(int i)
    {
        int offset = 0; float uvOffset = 0;

        Intersection intersection = intersections[i];
        Vector3 center = new Vector3();
        List<Intersection.JunctionEdge> junctionEdges = new List<Intersection.JunctionEdge>();
        List<int> splines = new List<int>();
        List<int> hand = new List<int>();

        foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
        {
            int splineIndex = junction.GetSplineIndex(m_SplineContainer);
            float t = junction.knotIndex == 0 ? 0f : 1f;
            m_SplineSampler.SampleSplineWidth(splineIndex, t, 0.04f, out Vector3 p1, out Vector3 p2);
            //if knot index is 0 we are facing away from the junction, otherwise we are facing the junction
            if (junction.knotIndex == 0)
            {
                junctionEdges.Add(new Intersection.JunctionEdge(p1, p2));
                splines.Add(splineIndex);
                hand.Add(1);
            }
            else
            {
                junctionEdges.Add(new Intersection.JunctionEdge(p2, p1));
                splines.Add(splineIndex);
                hand.Add(2);
            }
            center += p1;
            center += p2;
        }

        center /= (junctionEdges.Count * 2);

        splines.Sort((x, y) => {
            Vector3 xDir = junctionEdges[splines.IndexOf(x)].center - center;
            Vector3 yDir = junctionEdges[splines.IndexOf(y)].center - center;

            float angleA = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
            float angleB = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

            if (angleA > angleB)
            {
                return -1;
            }
            else if (angleA < angleB)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        hand.Sort((x, y) => {
            Vector3 xDir = junctionEdges[hand.IndexOf(x)].center - center;
            Vector3 yDir = junctionEdges[hand.IndexOf(y)].center - center;

            float angleA = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
            float angleB = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

            if (angleA > angleB)
            {
                return -1;
            }
            else if (angleA < angleB)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        junctionEdges.Sort((x, y) => {
            Vector3 xDir = x.center - center;
            Vector3 yDir = y.center - center;

            float angleA = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
            float angleB = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

            if (angleA > angleB)
            {
                return -1;
            }
            else if (angleA < angleB)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        Mesh mesh = new Mesh();
        Mesh col = new Mesh();
        MeshCollider collider = intersection.collider;
        List<Vector3> vertices = new();
        List<List<int>> trisA = new();
        List<int> trisS = new();
        List<int> trisC = new();
        List<Vector2> uvs = new();
        offset = 0; uvOffset = 0;

        mesh.subMeshCount = 1 + junctionEdges.Count;
        MeshRenderer renderer = collider.gameObject.GetComponent<MeshRenderer>();
        Material[] newMaterials = new Material[1 + junctionEdges.Count];
        newMaterials[0] = defaultWallMaterial;

        for (int j = 0; j < junctionEdges.Count; j++)
        {
            if (junctionEdges.Count > 1)
            {
                Vector3 a = junctionEdges[j].right;
                Vector3 b = junctionEdges[j].left;
                Vector3 c = (j < junctionEdges.Count - 1) ? junctionEdges[j + 1].right : junctionEdges[0].right;

                vertices.AddRange(new List<Vector3> { a, b, c });
                vertices.AddRange(new List<Vector3> { a + new Vector3(0, 2, 0), b + new Vector3(0, 2, 0), c + new Vector3(0, 2, 0) });

                offset = j * 6;
                trisS.AddRange(new List<int> { offset + 0, offset + 1, offset + 2 });
                trisS.AddRange(new List<int> { offset + 3, offset + 4, offset + 5 });
                trisS.AddRange(new List<int> { offset + 0, offset + 1, offset + 4, offset + 4, offset + 3, offset + 0 });
                trisS.AddRange(new List<int> { offset + 0, offset + 3, offset + 4, offset + 4, offset + 1, offset + 0 });
                trisA.Add(new List<int> { offset + 1, offset + 2, offset + 5, offset + 5, offset + 4, offset + 1, offset + 1, offset + 4, offset + 5, offset + 5, offset + 2, offset + 1 });
                trisC.AddRange(new List<int> { offset + 1, offset + 2, offset + 5, offset + 5, offset + 4, offset + 1, offset + 1, offset + 4, offset + 5, offset + 5, offset + 2, offset + 1 });

                float distanceA = Vector3.Distance(a, b) / 4f;
                float distanceB = Vector3.Distance(b, c) / 4f;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset + distanceA, 0), new Vector2(uvOffset + distanceA + distanceB, 0),
                    new Vector2(uvOffset, 1), new Vector2(uvOffset + distanceA, 1), new Vector2(uvOffset + distanceA + distanceB, 1)});

                uvOffset += distanceA + distanceB;

                newMaterials[j + 1] = walls[j].renderer.materials[hand[j]];
            }
            else
            {
                Vector3 a = junctionEdges[j].right;
                Vector3 b = junctionEdges[j].left;

                vertices.AddRange(new List<Vector3> { a, b, a + new Vector3(0, 2, 0), b + new Vector3(0, 2, 0) });

                offset = j * 4;
                trisA.Add(new List<int> { offset + 0, offset + 2, offset + 3, offset + 3, offset + 1, offset + 0 });
                trisC.AddRange(new List<int> { offset + 0, offset + 2, offset + 3, offset + 3, offset + 1, offset + 0 });

                float distanceA = Vector3.Distance(a, b);
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset + distanceA, 0),
                    new Vector2(uvOffset, 1), new Vector2(uvOffset + distanceA, 1)});

                uvOffset += distanceA;

                newMaterials[j + 1] = walls[j].renderer.materials[hand[j]];
            }
        }

        renderer.sharedMaterials = newMaterials;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(trisS, 0);
        for (int m = 1; m <= trisA.Count; m++)
        {
            mesh.SetTriangles(trisA[m - 1], m);
        }
        mesh.SetUVs(0, uvs);
        collider.gameObject.GetComponent<MeshFilter>().mesh = mesh;

        col.SetVertices(vertices);
        trisC.AddRange(trisS);
        col.SetTriangles(trisC, 0);
        col.SetUVs(0, uvs);
        collider.sharedMesh = col;
    }

    private void BuildRoom(int i)
    {
        List<Vector3> verts = new(); List<Vector3> vertsB = new();
        List<Vector3> verts2 = new(); List<Vector3> verts2B = new();
        List<int> tris = new(); List<int> trisB = new();
        List<int> tris2 = new(); List<int> tris2B = new();
        List<Vector2> uvs = new(); List<Vector2> uvs2 = new();
        Mesh mesh = new Mesh(); Mesh ceilingMesh = new Mesh();
        Mesh colliderMesh = new Mesh();
        int offset = 0;

        float angle = 0;

        float angler = 0;
        for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
        {
            angler += Vector3.SignedAngle(rooms[i].points[(pointIndex + 2) % rooms[i].points.Count] - rooms[i].points[(pointIndex + 1) % rooms[i].points.Count], rooms[i].points[(pointIndex + 0) % rooms[i].points.Count] - rooms[i].points[(pointIndex + 1) % rooms[i].points.Count], Vector3.up);
        }
        if (Mathf.Abs(angler - (180 * (rooms[i].points.Count - 2))) < 0.1f)
            angle = angler;
        else if (Mathf.Abs(angler - (-(180 * (rooms[i].points.Count - 2)))) < 0.1f)
            angle = angler;
        else
        {
            float angleA = 0;
            float angleB = 0;
            for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
            {
                float localAngle = Vector3.SignedAngle(rooms[i].points[(pointIndex + 2) % rooms[i].points.Count] - rooms[i].points[(pointIndex + 1) % rooms[i].points.Count], rooms[i].points[(pointIndex + 0) % rooms[i].points.Count] - rooms[i].points[(pointIndex + 1) % rooms[i].points.Count], Vector3.up);
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

            if (Mathf.Abs(angleA - (180 * (rooms[i].points.Count - 2))) < 0.1f)
            {
                angle = angleA;
            }
            else if (Mathf.Abs(angleB - (-(180 * (rooms[i].points.Count - 2)))) < 0.1f)
            {
                angle = angleB;
            }
        }

        List<List<Vector3>> pointsList = new();
        List<List<Vector3>> pointsList2 = new();

        for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
        {
            Vector3 p1x = rooms[i].points[pointIndex];
            Vector3 p2x = rooms[i].points[(pointIndex + 1) % rooms[i].points.Count];
            List<Vector3> localPointList = new();

            int w1i = walls.FindIndex(item => item.points.FindIndex(data => Vector3.Distance(data, p1x) < 0.2f) != -1 && item.points.FindIndex(data => Vector3.Distance(data, p2x) < 0.2f) != -1);

            for (int k = 0; k <= walls[w1i].resolution; k++)
            {
                float step = (float)k / walls[w1i].resolution;
                localPointList.Add(SplineUtility.EvaluatePosition(walls[w1i].wall, step));
            }

            if (Vector3.Distance(localPointList[0], p1x) > 0.2f)
            {
                localPointList.Reverse();
            }

            pointsList.Add(localPointList);
        }

        for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
        {
            Vector3 p1x = rooms[i].points[pointIndex];
            Vector3 p2x = rooms[i].points[(pointIndex + 1) % rooms[i].points.Count];
            List<Vector3> localPointList = new();

            int w1i = walls.FindIndex(item => item.points.FindIndex(data => Vector3.Distance(data, p1x) < 0.2f) != -1 && item.points.FindIndex(data => Vector3.Distance(data, p2x) < 0.2f) != -1);

            for (int k = 0; k <= walls[w1i].resolution; k++)
            {
                float step = (float)k / walls[w1i].resolution;
                localPointList.Add((Vector3)SplineUtility.EvaluatePosition(walls[w1i].wall, step) + new Vector3(0, 2f, 0));
            }

            if (Vector3.Distance(localPointList[0], p1x + new Vector3(0, 2f, 0)) > 0.2f)
            {
                localPointList.Reverse();
            }

            pointsList2.Add(localPointList);
        }

        
        times = 0;
        BuildRoomPoints(rooms[i], 0, 0.02f, pointsList, angle, verts, tris, uvs, vertsB, trisB);

        times = 0;
        BuildRoomPoints(rooms[i], 0, -0.02f, pointsList2, angle, verts2, tris2, uvs2, verts2B, tris2B);
        

        /*
        for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
        {
            Vector3 p1x = transform.TransformPoint(rooms[i].points[pointIndex]);
            Vector3 p4x = p1x + new Vector3(0, 0.02f, 0);
            int pivot = pointIndex;

            for (int subPointIndex = 1; subPointIndex < rooms[i].points.Count; subPointIndex++)
            {
                Vector3 p2x = transform.TransformPoint(rooms[i].points[(pivot + subPointIndex) % rooms[i].points.Count]);
                Vector3 p3x = transform.TransformPoint(rooms[i].points[(pivot + subPointIndex + 1) % rooms[i].points.Count]);
                Vector3 p5x = p2x + new Vector3(0, 0.02f, 0);
                Vector3 p6x = p3x + new Vector3(0, 0.02f, 0);

                RaycastHit[] hits1 = Physics.RaycastAll(p1x, p3x - p1x, Vector3.Distance(p1x, p3x), LayerMask.GetMask("Selector"));
                RaycastHit[] hits2 = Physics.RaycastAll(p3x, p1x - p3x, Vector3.Distance(p1x, p3x), LayerMask.GetMask("Selector"));
                List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2); List<RaycastHit> hitList = new();
                bool canDraw = true;
                foreach (RaycastHit hit in hits)
                {
                    if (!HitsContainCollider(hitList, hit) && !(hit.point == Vector3.zero && p1x != Vector3.zero))
                    {
                        hitList.Add(hit);
                    }
                }

                foreach (RaycastHit hit in hitList)
                {                   
                    if (walls.FindIndex(item => item.collider == hit.collider) != -1)
                    {
                        Wall wall = walls.Find(item => item.collider == hit.collider);
                        m_SplineSampler.SampleSplinePoint(wall.wall, transform.InverseTransformPoint(hit.point), wall.resolution, out Vector3 point, out float t);
                        if (Vector3.Distance(p1x, transform.TransformPoint(point)) >= 0.5f && Vector3.Distance(p3x, transform.TransformPoint(point)) >= 0.5f && Mathf.Abs(Vector3.Angle((p3x - p1x).normalized, (wall.points[0] - wall.points[^1]).normalized)) < 0.01f)
                        {
                            if (Vector3.Distance(p1x, transform.TransformPoint(point)) < Vector3.Distance(p3x, transform.TransformPoint(point)))
                            {
                                p1x = transform.TransformPoint(point);
                                p4x = p1x + new Vector3(0, 0.02f, 0);
                            }
                            else
                            {
                                p3x = transform.TransformPoint(point);
                                p6x = p3x + new Vector3(0, 0.02f, 0);
                            }
                            canDraw = false;
                            break;
                        }
                    }
                }

                //Debug.Log($"{i} : {pivot}+{subPointIndex + pivot} {canDraw}");
                if (canDraw && Mathf.Sign(angle) == Mathf.Sign(Vector3.SignedAngle(p2x - p1x, p3x - p1x, Vector3.up)))
                {
                    verts.AddRange(new List<Vector3> { p1x, p2x, p3x, p4x, p5x, p6x });
                    vertsB.AddRange(new List<Vector3> { p1x, p2x, p3x, p4x, p5x, p6x });

                    int t1 = offset + 0; int t2 = offset + 1; int t3 = offset + 2;
                    int t4 = offset + 3; int t5 = offset + 4; int t6 = offset + 5;
                    tris.AddRange(new List<int> { t1, t2, t3, t3, t2, t1 });
                    trisB.AddRange(new List<int> { t1, t2, t3, t3, t2, t1 });
                    tris.AddRange(new List<int> { t4, t5, t6, t6, t5, t4 });
                    trisB.AddRange(new List<int> { t4, t5, t6, t6, t5, t4 });
                    tris.AddRange(new List<int> { t1, t4, t5, t5, t2, t1, t2, t5, t6, t6, t3, t2, t3, t6, t4, t4, t1, t3 });
                    trisB.AddRange(new List<int> { t1, t4, t5, t5, t2, t1, t2, t5, t6, t6, t3, t2, t3, t6, t4, t4, t1, t3 });

                    uvs.AddRange(new List<Vector2> { new Vector2(p1x.x - rooms[i].points[0].x, p1x.z - rooms[i].points[0].z), new Vector2(p2x.x - rooms[i].points[0].x, p2x.z - rooms[i].points[0].z),
                new Vector2(p3x.x - rooms[i].points[0].x, p3x.z - rooms[i].points[0].z), new Vector2(p4x.x - rooms[i].points[0].x, p4x.z - rooms[i].points[0].z),
                new Vector2(p5x.x - rooms[i].points[0].x, p5x.z - rooms[i].points[0].z), new Vector2(p6x.x - rooms[i].points[0].x, p6x.z - rooms[i].points[0].z)});
                    offset += 6; pointIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        offset = 0;
        for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
        {
            Vector3 p1x = transform.TransformPoint(rooms[i].points[pointIndex]) + new Vector3(0, 2f, 0);
            Vector3 p4x = p1x - new Vector3(0, 0.02f, 0);
            int pivot = pointIndex;

            for (int subPointIndex = 1; subPointIndex < rooms[i].points.Count; subPointIndex++)
            {
                Vector3 p2x = transform.TransformPoint(rooms[i].points[(pivot + subPointIndex) % rooms[i].points.Count]) + new Vector3(0, 2f, 0);
                Vector3 p3x = transform.TransformPoint(rooms[i].points[(pivot + subPointIndex + 1) % rooms[i].points.Count]) + new Vector3(0, 2f, 0);
                Vector3 p5x = p2x - new Vector3(0, 0.02f, 0);
                Vector3 p6x = p3x - new Vector3(0, 0.02f, 0);

                RaycastHit[] hits1 = Physics.RaycastAll(p1x, p3x - p1x, Vector3.Distance(p1x, p3x), LayerMask.GetMask("Selector"));
                RaycastHit[] hits2 = Physics.RaycastAll(p3x, p1x - p3x, Vector3.Distance(p1x, p3x), LayerMask.GetMask("Selector"));
                List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2); List<RaycastHit> hitList = new();
                bool canDraw = true;
                foreach (RaycastHit hit in hits)
                {
                    if (!HitsContainCollider(hitList, hit) && !(hit.point == Vector3.zero && p1x != Vector3.zero))
                    {
                        hitList.Add(hit);
                    }
                }

                foreach (RaycastHit hit in hitList)
                {
                    if (walls.FindIndex(item => item.collider == hit.collider) != -1)
                    {
                        Wall wall = walls.Find(item => item.collider == hit.collider);
                        //m_SplineSampler.SampleSplinePoint(wall.wall, transform.InverseTransformPoint(hit.point), wall.resolution, out Vector3 point, out float t);
                        if (Vector3.Distance(p1x, hit.point) >= 0.5f && Vector3.Distance(p3x, hit.point) >= 0.5f && Mathf.Abs(Vector3.Angle((p3x - p1x).normalized, (wall.points[0] - wall.points[^1]).normalized)) > 0.01f)
                        {
                            if (Vector3.Distance(p1x, hit.point) < Vector3.Distance(p3x, hit.point))
                            {
                                p1x = hit.point;
                                p4x = p1x + new Vector3(0, 0.02f, 0);
                            }
                            else
                            {
                                p3x = hit.point;
                                p6x = p3x + new Vector3(0, 0.02f, 0);
                            }
                            canDraw = false;
                            break;
                        }
                    }
                }

                //Debug.Log($"{i} : {pivot}+{subPointIndex + pivot} {canDraw}");
                if (canDraw && Mathf.Sign(angle) == Mathf.Sign(Vector3.SignedAngle(p2x - p1x, p3x - p1x, Vector3.up)))
                {
                    verts2.AddRange(new List<Vector3> { p1x, p2x, p3x, p4x, p5x, p6x });
                    verts2B.AddRange(new List<Vector3> { p1x, p2x, p3x, p4x, p5x, p6x });

                    int t1 = offset + 0; int t2 = offset + 1; int t3 = offset + 2;
                    int t4 = offset + 3; int t5 = offset + 4; int t6 = offset + 5;
                    tris2.AddRange(new List<int> { t1, t2, t3, t3, t2, t1 });
                    tris2B.AddRange(new List<int> { t1, t2, t3, t3, t2, t1 });
                    tris2.AddRange(new List<int> { t4, t5, t6, t6, t5, t4 });
                    tris2B.AddRange(new List<int> { t4, t5, t6, t6, t5, t4 });
                    tris2.AddRange(new List<int> { t1, t4, t5, t5, t2, t1, t2, t5, t6, t6, t3, t2, t3, t6, t4, t4, t1, t3 });
                    tris2B.AddRange(new List<int> { t1, t4, t5, t5, t2, t1, t2, t5, t6, t6, t3, t2, t3, t6, t4, t4, t1, t3 });

                    uvs2.AddRange(new List<Vector2> { new Vector2(p1x.x - rooms[i].points[0].x, p1x.z - rooms[i].points[0].z + 2f), new Vector2(p2x.x - rooms[i].points[0].x, p2x.z - rooms[i].points[0].z + 2f),
                new Vector2(p3x.x - rooms[i].points[0].x, p3x.z - rooms[i].points[0].z + 2f), new Vector2(p4x.x - rooms[i].points[0].x, p4x.z - rooms[i].points[0].z + 2f),
                new Vector2(p5x.x - rooms[i].points[0].x, p5x.z - rooms[i].points[0].z + 2f), new Vector2(p6x.x - rooms[i].points[0].x, p6x.z - rooms[i].points[0].z + 2f)});
                    offset += 6; pointIndex++;
                }
                else
                {
                    break;
                }
            }
        }
        */

        mesh.subMeshCount = 1;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);

        ceilingMesh.subMeshCount = 1;
        ceilingMesh.SetVertices(verts2);
        ceilingMesh.SetTriangles(tris2, 0);
        ceilingMesh.SetUVs(0, uvs2);

        colliderMesh.SetVertices(vertsB);
        colliderMesh.SetTriangles(trisB, 0);

        rooms[i].mesh.mesh = mesh;
        rooms[i].ceilingMesh.mesh = ceilingMesh;
        rooms[i].collider.sharedMesh = colliderMesh;
    }

    private void BuildMesh()
    {
        for (int currentSplineIndex = 0; currentSplineIndex < walls.Count; currentSplineIndex++)
        {
            BuildWall(currentSplineIndex);
        }        

        for (int i = 0; i < intersections.Count; i++)
        {
            BuildIntersection(i);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            BuildRoom(i);
        }
    }

    private void BuildRoomPoints(Room room, int offset, float height, List<List<Vector3>> points, float angle, List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Vector3> vertsB, List<int> trisB)
    {
        List<List<Vector3>> alteredPoints = new();
        bool repeat = true;

        for (int i = 0; i < points.Count; i++)
        {
            List<Vector3> internalAlteredPoints = new();

            List<Vector3> vertsP1a = points[i];
            List<Vector3> vertsP2a = points[(i + 1) % points.Count];
            List<Vector3> vertsP3a = points[(i + 2) % points.Count];

            float ratioA = 0.5f;
            float ratioB = 0.5f;

            //Debug.Log($"{endPointA} {endPointB}");

            if (vertsP1a.Count > 1 && vertsP2a.Count > 1 && vertsP3a.Count > 1)
            {
                if (Vector3.Distance(vertsP1a[0], vertsP1a[^1]) < 1f)
                {
                    ratioA = (Vector3.Distance(vertsP1a[0], vertsP1a[^1]) / 2);
                }
                else if (Vector3.Distance(vertsP3a[0], vertsP3a[^1]) < 1f)
                {
                    ratioB = (Vector3.Distance(vertsP3a[0], vertsP3a[^1]) / 2);
                }

                if (Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), transform.TransformPoint(vertsP1a[^2]) - transform.TransformPoint(vertsP1a[^1]), Vector3.up)) > 0 && Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0]), transform.TransformPoint(vertsP2a[^2]) - transform.TransformPoint(vertsP2a[^1]), Vector3.up)) > 0)
                {
                    Vector3 endPointA = new();
                    Debug.Log(Vector3.Lerp(Vector3.Cross((transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0])).normalized, Vector3.up).normalized, Vector3.Cross((transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2])).normalized, Vector3.up).normalized, 0.5f));

                    if (Vector3.Distance(vertsP2a[0], vertsP2a[^1]) > 1f)
                    {
                        if (angle > 0)
                        {
                            endPointA = transform.TransformPoint(vertsP2a[0]) + (Vector3.Lerp(-Vector3.Cross((transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0])).normalized, Vector3.up).normalized, -Vector3.Cross((transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (ratioA / (Mathf.Sin(Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), transform.TransformPoint(vertsP1a[^2]) - transform.TransformPoint(vertsP1a[^1]), Vector3.up) / 2 * (Mathf.PI / 180))))));
                        }
                        else
                        {
                            endPointA = transform.TransformPoint(vertsP2a[0]) + (Vector3.Lerp(Vector3.Cross((transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0])).normalized, Vector3.up).normalized, Vector3.Cross((transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (ratioA / (Mathf.Sin(Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), transform.TransformPoint(vertsP1a[^2]) - transform.TransformPoint(vertsP1a[^1]), Vector3.up) / 2 * (Mathf.PI / 180))))));
                        }
                    }
                    else
                    {
                        if (angle > 0)
                        {
                            endPointA = transform.TransformPoint(vertsP2a[0]) + (Vector3.Lerp(-Vector3.Cross((transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0])).normalized, Vector3.up).normalized, -Vector3.Cross((transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (Mathf.Sqrt(Mathf.Pow(ratioA, 2) + Mathf.Pow(Vector3.Distance(vertsP2a[0], vertsP2a[^1]) / 2, 2))));
                        }
                        else
                        {
                            endPointA = transform.TransformPoint(vertsP2a[0]) + (Vector3.Lerp(Vector3.Cross((transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0])).normalized, Vector3.up).normalized, Vector3.Cross((transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (Mathf.Sqrt(Mathf.Pow(ratioA, 2) + Mathf.Pow(Vector3.Distance(vertsP2a[0], vertsP2a[^1]) / 2, 2))));
                        }
                    }


                    Vector3 endPointB = new();
                    if (Vector3.Distance(vertsP2a[0], vertsP2a[^1]) > 1f)
                    {
                        if (angle > 0)
                        {
                            endPointB = transform.TransformPoint(vertsP3a[0]) + (Vector3.Lerp(-Vector3.Cross((transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0])).normalized, Vector3.up).normalized, -Vector3.Cross((transform.TransformPoint(vertsP2a[^1]) - transform.TransformPoint(vertsP2a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (ratioB / (Mathf.Sin(Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0]), transform.TransformPoint(vertsP2a[^2]) - transform.TransformPoint(vertsP2a[^1]), Vector3.up) / 2 * (Mathf.PI / 180))))));
                        }
                        else
                        {
                            endPointB = transform.TransformPoint(vertsP3a[0]) + (Vector3.Lerp(Vector3.Cross((transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0])).normalized, Vector3.up).normalized, Vector3.Cross((transform.TransformPoint(vertsP2a[^1]) - transform.TransformPoint(vertsP2a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (ratioB / (Mathf.Sin(Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0]), transform.TransformPoint(vertsP2a[^2]) - transform.TransformPoint(vertsP2a[^1]), Vector3.up) / 2 * (Mathf.PI / 180))))));
                        }
                    }
                    else
                    {
                        if (angle > 0)
                        {
                            endPointB = transform.TransformPoint(vertsP3a[0]) + (Vector3.Lerp(-Vector3.Cross((transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0])).normalized, Vector3.up).normalized, -Vector3.Cross((transform.TransformPoint(vertsP2a[^1]) - transform.TransformPoint(vertsP2a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (Mathf.Sqrt(Mathf.Pow(ratioB, 2) + Mathf.Pow(Vector3.Distance(vertsP2a[0], vertsP2a[^1]) / 2, 2))));
                        }
                        else
                        {
                            endPointB = transform.TransformPoint(vertsP3a[0]) + (Vector3.Lerp(Vector3.Cross((transform.TransformPoint(vertsP3a[1]) - transform.TransformPoint(vertsP3a[0])).normalized, Vector3.up).normalized, Vector3.Cross((transform.TransformPoint(vertsP2a[^1]) - transform.TransformPoint(vertsP2a[^2])).normalized, Vector3.up).normalized, 0.5f).normalized
                                * (Mathf.Sqrt(Mathf.Pow(ratioB, 2) + Mathf.Pow(Vector3.Distance(vertsP2a[0], vertsP2a[^1]) / 2, 2))));
                        }
                    }

                    Vector3 p1 = transform.TransformPoint(vertsP1a[^2]);
                    Vector3 p2 = p1 + new Vector3(0, height, 0);
                    Vector3 p3 = transform.TransformPoint(vertsP2a[1]);
                    Vector3 p4 = p3 + new Vector3(0, height, 0);
                    Vector3 p5 = new();
                    if (angle > 0)
                    {
                        if (Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), endPointA - (p3 - (Vector3.Cross(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), Vector3.up).normalized * 0.5f)), Vector3.up)) < 90)
                            p5 = endPointA;
                        else
                            p5 = p3 - (Vector3.Cross(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), Vector3.up).normalized * ratioA);
                    }
                    else
                    {
                        if (Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), endPointA - (p3 + (Vector3.Cross(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), Vector3.up).normalized * 0.5f)), Vector3.up)) < 90)
                            p5 = endPointA;
                        else
                            p5 = p3 + (Vector3.Cross(transform.TransformPoint(vertsP2a[1]) - transform.TransformPoint(vertsP2a[0]), Vector3.up).normalized * ratioA);
                    }
                    Vector3 p6 = p5 + new Vector3(0, height, 0);
                    Vector3 p7 = transform.TransformPoint(vertsP1a[^1]);
                    Vector3 p8 = p7 + new Vector3(0, height, 0);
                    Vector3 p9 = transform.TransformPoint(vertsP2a[0]);
                    Vector3 p10 = p7 + new Vector3(0, height, 0);
                    Vector3 p11 = new();
                    if (angle > 0)
                    {
                        if (Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2]), (p1 - (Vector3.Cross(transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2]), Vector3.up).normalized * 0.5f)) - endPointA, Vector3.up)) < 90)
                            p11 = endPointA;
                        else
                            p11 = p1 - (Vector3.Cross(transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2]), Vector3.up).normalized * ratioA);
                    }
                    else
                    {
                        if (Mathf.Abs(Vector3.SignedAngle(transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2]), (p1 + (Vector3.Cross(transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2]), Vector3.up).normalized * 0.5f)) - endPointA, Vector3.up)) < 90)
                            p11 = endPointA;
                        else
                            p11 = p1 + (Vector3.Cross(transform.TransformPoint(vertsP1a[^1]) - transform.TransformPoint(vertsP1a[^2]), Vector3.up).normalized * ratioA);
                    }
                    Vector3 p12 = p11 + new Vector3(0, height, 0);
                    Vector3 p13 = endPointA;
                    Vector3 p14 = p13 + new Vector3(0, height, 0);

                    if (internalAlteredPoints.FindIndex(coordinate => Vector3.Distance(coordinate, endPointA) < 0.05f) == -1)
                        internalAlteredPoints.Add(endPointA);

                    int t1 = offset + 0;
                    int t2 = offset + 10;
                    int t3 = offset + 6;
                    int t4 = offset + 6;
                    int t5 = offset + 10;
                    int t6 = offset + 12;
                    int t7 = offset + 12;
                    int t8 = offset + 6;
                    int t9 = offset + 8;
                    int t10 = offset + 8;
                    int t11 = offset + 12;
                    int t12 = offset + 4;
                    int t13 = offset + 4;
                    int t14 = offset + 8;
                    int t15 = offset + 2;

                    int t16 = offset + 1;
                    int t17 = offset + 11;
                    int t18 = offset + 7;
                    int t19 = offset + 7;
                    int t20 = offset + 11;
                    int t21 = offset + 13;
                    int t22 = offset + 13;
                    int t23 = offset + 7;
                    int t24 = offset + 9;
                    int t25 = offset + 9;
                    int t26 = offset + 13;
                    int t27 = offset + 5;
                    int t28 = offset + 5;
                    int t29 = offset + 9;
                    int t30 = offset + 3;


                    verts.AddRange(new List<Vector3> { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14 });
                    vertsB.AddRange(new List<Vector3> { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14 });
                    tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15 });
                    trisB.AddRange(new List<int> { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15 });
                    tris.AddRange(new List<int> { t16, t17, t18, t19, t20, t21, t22, t23, t24, t25, t26, t27, t28, t29, t30 });
                    trisB.AddRange(new List<int> { t16, t17, t18, t19, t20, t21, t22, t23, t24, t25, t26, t27, t28, t29, t30 });
                    tris.AddRange(new List<int> { t1, t16, t18, t18, t3, t1, t3, t18, t24, t24, t9, t3, t9, t24, t30, t30, t15, t9, t5, t20, t21, t21, t6, t5, t6, t21, t27, t27, t12, t6 });
                    trisB.AddRange(new List<int> { t1, t16, t18, t18, t3, t1, t3, t18, t24, t24, t9, t3, t9, t24, t30, t30, t15, t9, t5, t20, t21, t21, t6, t5, t6, t21, t27, t27, t12, t6 });

                    offset += 14;

                    uvs.AddRange(new List<Vector2> { new Vector2(p1.x - transform.TransformPoint(room.points[0]).x, p1.z - transform.TransformPoint(room.points[0]).z), new Vector2(p2.x - transform.TransformPoint(room.points[0]).x, p2.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p3.x - transform.TransformPoint(room.points[0]).x, p3.z - transform.TransformPoint(room.points[0]).z), new Vector2(p4.x - transform.TransformPoint(room.points[0]).x, p4.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p5.x - transform.TransformPoint(room.points[0]).x, p5.z - transform.TransformPoint(room.points[0]).z), new Vector2(p6.x - transform.TransformPoint(room.points[0]).x, p6.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p7.x - transform.TransformPoint(room.points[0]).x, p7.z - transform.TransformPoint(room.points[0]).z), new Vector2(p8.x - transform.TransformPoint(room.points[0]).x, p8.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p9.x - transform.TransformPoint(room.points[0]).x, p9.z - transform.TransformPoint(room.points[0]).z), new Vector2(p10.x - transform.TransformPoint(room.points[0]).x, p10.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p11.x - transform.TransformPoint(room.points[0]).x, p11.z - transform.TransformPoint(room.points[0]).z), new Vector2(p12.x - transform.TransformPoint(room.points[0]).x, p12.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p13.x - transform.TransformPoint(room.points[0]).x, p13.z - transform.TransformPoint(room.points[0]).z), new Vector2(p14.x - transform.TransformPoint(room.points[0]).x, p14.z - transform.TransformPoint(room.points[0]).z)});


                    //(Vector3.Cross(p3t - p1t, Vector3.up).normalized * 0.5f)
                    for (int k = 1; k < vertsP2a.Count - 2; k++)
                    {
                        Vector3 p1t = transform.TransformPoint(vertsP2a[k]);
                        Vector3 p2t = p1t + new Vector3(0, height, 0);
                        Vector3 p3t = transform.TransformPoint(vertsP2a[k + 1]);
                        Vector3 p4t = p3t + new Vector3(0, height, 0);

                        Vector3 altPoint1 = new();
                        if (angle > 0)
                        {
                            altPoint1 = p3t - (Vector3.Cross((transform.TransformPoint(vertsP2a[k + 2]) - transform.TransformPoint(vertsP2a[k + 1])).normalized, Vector3.up).normalized * (ratioA < ratioB ? ratioA : ratioB));
                        }
                        else
                        {
                            altPoint1 = p3t + (Vector3.Cross((transform.TransformPoint(vertsP2a[k + 2]) - transform.TransformPoint(vertsP2a[k + 1])).normalized, Vector3.up).normalized * (ratioA < ratioB ? ratioA : ratioB));
                        }
                        Vector3 altPoint2 = new();
                        if (angle > 0)
                        {
                            altPoint2 = p1t - (Vector3.Cross((transform.TransformPoint(vertsP2a[k + 1]) - transform.TransformPoint(vertsP2a[k])).normalized, Vector3.up).normalized * (ratioA < ratioB ? ratioA : ratioB));
                        }
                        else
                        {
                            altPoint2 = p1t + (Vector3.Cross((transform.TransformPoint(vertsP2a[k + 1]) - transform.TransformPoint(vertsP2a[k])).normalized, Vector3.up).normalized * (ratioA < ratioB ? ratioA : ratioB));
                        }

                        Vector3 p5t = new();
                        Vector3 nearestPoint = GetNearestPoint(endPointA, endPointB, altPoint1);

                        if (Vector3.Distance(nearestPoint, endPointA) < 0.1f)
                            p5t = endPointA;
                        else if (Vector3.Distance(nearestPoint, endPointB) < 0.1f)
                            p5t = endPointB;
                        else
                            p5t = altPoint1;

                        Vector3 p6t = p5t + new Vector3(0, height, 0);
                        Vector3 p7t = new();
                        nearestPoint = GetNearestPoint(endPointA, endPointB, altPoint2);

                        if (Vector3.Distance(nearestPoint, endPointA) < 0.1f)
                            p7t = endPointA;
                        else if (Vector3.Distance(nearestPoint, endPointB) < 0.1f)
                            p7t = endPointB;
                        else
                            p7t = altPoint2;

                        Vector3 p8t = p7t + new Vector3(0, height, 0);

                        //Debug.Log($"{p7t} {p5t}");

                        if (internalAlteredPoints.FindIndex(coordinate => Vector3.Distance(coordinate, p7t) < 0.05f) == -1)
                            internalAlteredPoints.Add(p7t);
                        if (internalAlteredPoints.FindIndex(coordinate => Vector3.Distance(coordinate, p5t) < 0.05f) == -1)
                            internalAlteredPoints.Add(p5t);

                        int t1x = offset + 0;
                        int t2x = offset + 2;
                        int t3x = offset + 4;
                        int t4x = offset + 4;
                        int t5x = offset + 6;
                        int t6x = offset + 0;

                        int t7x = offset + 1;
                        int t8x = offset + 3;
                        int t9x = offset + 5;
                        int t10x = offset + 5;
                        int t11x = offset + 7;
                        int t12x = offset + 1;

                        verts.AddRange(new List<Vector3> { p1t, p2t, p3t, p4t, p5t, p6t, p7t, p8t });
                        vertsB.AddRange(new List<Vector3> { p1t, p2t, p3t, p4t, p5t, p6t, p7t, p8t });
                        tris.AddRange(new List<int> { t1x, t2x, t3x, t4x, t5x, t6x });
                        trisB.AddRange(new List<int> { t1x, t2x, t3x, t4x, t5x, t6x });
                        tris.AddRange(new List<int> { t7x, t8x, t9x, t10x, t11x, t12x });
                        trisB.AddRange(new List<int> { t7x, t8x, t9x, t10x, t11x, t12x });
                        tris.AddRange(new List<int> { t1x, t7x, t8x, t8x, t2x, t1x, t3x, t9x, t11x, t11x, t5x, t3x });
                        trisB.AddRange(new List<int> { t1x, t7x, t8x, t8x, t2x, t1x, t3x, t9x, t11x, t11x, t5x, t3x });

                        offset += 8;

                        uvs.AddRange(new List<Vector2> { new Vector2(p1t.x - transform.TransformPoint(room.points[0]).x, p1t.z - transform.TransformPoint(room.points[0]).z), new Vector2(p2t.x - transform.TransformPoint(room.points[0]).x, p2t.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p3t.x - transform.TransformPoint(room.points[0]).x, p3t.z - transform.TransformPoint(room.points[0]).z), new Vector2(p4t.x - transform.TransformPoint(room.points[0]).x, p4t.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p5t.x - transform.TransformPoint(room.points[0]).x, p5t.z - transform.TransformPoint(room.points[0]).z), new Vector2(p6t.x - transform.TransformPoint(room.points[0]).x, p6t.z - transform.TransformPoint(room.points[0]).z),
                    new Vector2(p7t.x - transform.TransformPoint(room.points[0]).x, p7t.z - transform.TransformPoint(room.points[0]).z), new Vector2(p8t.x - transform.TransformPoint(room.points[0]).x, p8t.z - transform.TransformPoint(room.points[0]).z)});
                    }

                    if (internalAlteredPoints.FindIndex(coordinate => Vector3.Distance(coordinate, endPointB) < 0.05f) == -1)
                        internalAlteredPoints.Add(endPointB);

                    if (Vector3.Distance(vertsP1a[0], vertsP1a[^1]) > 1f && Vector3.Distance(vertsP2a[0], vertsP2a[^1]) > 1f && Vector3.Distance(vertsP3a[0], vertsP3a[^1]) > 1f)
                    {
                        alteredPoints.Add(internalAlteredPoints);
                    }
                    else repeat = false;

                    //DrawPoints(internalAlteredPoints);
                }
            }
        }
        
        if (alteredPoints.Count > 0 && repeat)
            BuildRoomPoints(room, offset, height, alteredPoints, angle, verts, tris, uvs, vertsB, trisB);
    }

    private void FilterIntersections(Spline s, Spline r, float sT)
    {
        List<Spline> containerSplines = new();
        Spline combinedSpline = new Spline();
        foreach (BezierKnot k in r)
        {
            combinedSpline.Add(k);
        }
        foreach (BezierKnot k in s)
        {
            combinedSpline.Add(k);
        }
        foreach (Spline spline in m_SplineContainer.Splines) { containerSplines.Add(spline); }
        foreach (Intersection intersect in intersections)
        {
            for (int a = 0; a < intersect.junctions.Count; a++)
            {
                if (intersect.junctions[a].spline == s)
                {
                    int indexR = combinedSpline.IndexOf(intersect.junctions[a].knot);
                    float rT = combinedSpline.ConvertIndexUnit(indexR, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                    if (rT < sT)
                    {
                        Intersection.JunctionInfo jinfo = new Intersection.JunctionInfo(r, r[intersect.junctions[a].knotIndex]);
                        intersect.junctions[a] = jinfo;
                    }
                    else
                    {
                        Intersection.JunctionInfo jinfo = new Intersection.JunctionInfo(s, s[intersect.junctions[a].knotIndex]);
                        intersect.junctions[a] = jinfo;
                    }
                }
            }
        }
    }

    private bool SplineInIntersection(Intersection i, Spline s, BezierKnot k)
    {
        foreach (Intersection.JunctionInfo junction in i.junctions)
        {
            if (junction.spline == s)
            {
                if (Vector3.Distance(junction.knot.Position, k.Position) < 0.5f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool HasRoomWithPoints(List<BezierKnot> knots)
    {
        bool result = false;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].points.Count == knots.Count)
            {
                bool innerResult = true;
                for (int j = 0; j < knots.Count; j++)
                {
                    if (rooms[i].points.FindIndex(data => Vector3.Distance(data, knots[j].Position) < 0.5f) == -1)
                    {
                        innerResult = false;
                        break;
                    }
                }

                if (innerResult)
                {
                    result = innerResult;
                    break;
                }
            }
        }

        return result;
    }

    private List<List<int>> GetDuplicatePoints(List<BezierKnot> points)
    {
        List<List<int>> d1 = new();
        List<int> skipIndex = new();

        for (int i = 0; i < points.Count - 1; i++)
        {
            if (!skipIndex.Contains(i))
            {
                List<int> internalList = new();
                List<BezierKnot> duplicates = points.FindAll(data => Vector3.Distance(data.Position, points[i].Position) < 0.5f);
                if (duplicates.Count > 1)
                {
                    for (int j = 0; j < duplicates.Count; j++)
                    {
                        //Debug.Log($"{points.IndexOf(duplicates[j])} in {(Vector3)duplicates[j].Position}");
                        internalList.Add(points.IndexOf(duplicates[j]));
                        skipIndex.Add(j);
                    }
                    d1.Add(internalList);
                }
            }
        }

        return d1;
    }

    private List<BezierKnot> RemoveDuplicatePoints(List<BezierKnot> points)
    {
        List<BezierKnot> removePoints = new();
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                if (Vector3.Distance(points[i].Position, points[j].Position) < 0.5f)
                {
                    removePoints.Add(points[j]);
                }
            }
        }

        for (int i = 0; i < removePoints.Count; i++)
        {
            points.Remove(removePoints[i]);
        }

        return points;
    }

    private void SortPoints(List<Vector3> sort)
    {
        for (int j = 0; j < sort.Count - 1; j++)
        {
            List<Wall> searchWalls = new();
            //Vector3 posA = sort[j];

            for (int k = j + 1; k < sort.Count; k++)
            {
                bool matchFound = false;
                for (int i = 0; i < walls.Count; i++)
                {
                    if (walls[i].points.FindIndex(data => Vector3.Distance(sort[k - 1], data) < 0.5f) != -1)
                    {
                        searchWalls.Add(walls[i]);
                    }
                }

                foreach (Wall wall in searchWalls)
                {
                    m_SplineSampler.SampleSplinePoint(wall.wall, sort[k - 1], wall.resolution, out Vector3 hitPos, out float t);
                    if (Vector3.Distance(hitPos, sort[k - 1]) < 0.5f)
                    {
                        float knotF = SplineUtility.ConvertIndexUnit(wall.wall, t, PathIndexUnit.Knot);
                        int knot = Mathf.RoundToInt(knotF);
                        if (knot == 0)
                        {
                            if (Vector3.Distance((Vector3)wall.wall[knot + 1].Position, sort[k]) < 0.5f)
                            {
                                Vector3 temp = sort[k];
                                sort[k] = sort[j + 1];
                                sort[j + 1] = sort[k];
                                matchFound = true;
                                break;
                            }
                        }
                        else
                        {
                            if (Vector3.Distance((Vector3)wall.wall[knot - 1].Position, sort[k]) < 0.5f)
                            {
                                Vector3 temp = sort[k];
                                sort[k] = sort[j + 1];
                                sort[j + 1] = sort[k];
                                matchFound = true;
                                break;
                            }
                        }
                    }
                }

                if (matchFound)
                {
                    break;
                }
            }
        }
    }

    private bool IsRoomOverlap(List<BezierKnot> knots)
    {
        bool status = false;

        return status;
    }

    private bool IsRoomMeshContinuous(List<BezierKnot> points)
    {
        float angle = 0;

        float angler = 0;
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
        {
            angler += Vector3.SignedAngle(points[(pointIndex + 2) % points.Count].Position - points[(pointIndex + 1) % points.Count].Position, points[(pointIndex + 0) % points.Count].Position - points[(pointIndex + 1) % points.Count].Position, Vector3.up);
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
                float localAngle = Vector3.SignedAngle(points[(pointIndex + 2) % points.Count].Position - points[(pointIndex + 1) % points.Count].Position, points[(pointIndex + 0) % points.Count].Position - points[(pointIndex + 1) % points.Count].Position, Vector3.up);
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

        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
        {
            BezierKnot pointA = points[pointIndex];
            BezierKnot pointB = points[(pointIndex + 1) % points.Count];
            BezierKnot pointC = points[(pointIndex + 2) % points.Count];
            Spline spline = walls.Find(item => item.points.FindIndex(data => Vector3.Distance(data, pointA.Position) < 0.2f) != -1 && item.points.FindIndex(data => Vector3.Distance(data, pointB.Position) < 0.2f) != -1).wall;
            
            if (intersections.FindIndex(item => item.junctions.FindIndex(data => data.knot.Equals(pointB) && data.spline == spline) != -1) != -1)
            {
                Intersection intersection = intersections.Find(item => item.junctions.FindIndex(data => data.knot.Equals(pointB) && data.spline == spline) != -1);
                if (intersection.junctions.Count > 2)
                {
                    foreach (Intersection.JunctionInfo junction in intersection.junctions)
                    {
                        if (junction.spline != spline)
                        {
                            int otherPointIndex = junction.knotIndex == 0 ? 1 : 0;
                            if ((Vector3)junction.spline[otherPointIndex].Position != (Vector3)pointC.Position)
                            {
                                float measureAngle = Vector3.SignedAngle(junction.spline[otherPointIndex].Position - pointB.Position, pointC.Position - pointB.Position, Vector3.up);

                                if (Mathf.Abs(measureAngle) != 180)
                                {
                                    if (Mathf.Sign(angle) < 0 && measureAngle > 0)
                                        return false;
                                    else if (Mathf.Sign(angle) > 0 && measureAngle < 0)
                                        return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private static Vector3 GetNearestPoint(Vector3 start, Vector3 end, Vector3 point)
    {
        var wander = point - start;
        var span = end - start;

        // Compute how far along the line is the closest approach to our point.
        float t = Vector3.Dot(wander, span) / span.sqrMagnitude;

        // Restrict this point to within the line segment from start to end.
        t = Mathf.Clamp01(t);

        Vector3 nearest = start + t * span;
        return nearest;
    }


    private bool IsRoomContinuous(List<BezierKnot> knots)
    {
        bool continuous = true;
        
        for (int j = 1; j <= knots.Count; j++)
        {
            List<Wall> searchWalls = new();

            Vector3 posA = knots[j - 1].Position;
            Vector3 posB = j < knots.Count ? knots[j].Position : knots[0].Position;
            for (int i = 0; i < walls.Count; i++)
            {
                if (walls[i].points.FindIndex(data => Vector3.Distance(posA, data) < 0.5f) != -1)
                {
                    searchWalls.Add(walls[i]);
                }
            }
            bool linked = false;

            foreach (Wall wall in searchWalls)
            {
                m_SplineSampler.SampleSplinePoint(wall.wall, posA, wall.resolution, out Vector3 hitPos, out float t);
                if (Vector3.Distance(hitPos, posA) < 0.5f)
                {
                    float knotF = SplineUtility.ConvertIndexUnit(wall.wall, t, PathIndexUnit.Knot);
                    int knot = Mathf.RoundToInt(knotF);
                    if (knot == 0)
                    {
                        if (Vector3.Distance((Vector3)wall.wall[knot + 1].Position, posB) < 0.5f)
                        {
                            linked = true;
                            break;
                        }
                    }
                    else
                    {
                        if (Vector3.Distance((Vector3)wall.wall[knot - 1].Position, posB) < 0.5f)
                        {
                            linked = true;
                            break;
                        }
                    }
                }
            }

            if (linked == false)
            {
                continuous = false;
                break;
            }
        }

        return continuous;
    }

    private void CleanRooms()
    {
        List<Room> deleteRooms = new();
        
        for (int i = 0; i < rooms.Count; i++)
        {
            bool isContinuous = IsRoomContinuous(rooms[i].knotList);            

            if (!isContinuous)
            {
                deleteRooms.Add(rooms[i]);
            }
            else
            {
                if (!IsRoomMeshContinuous(rooms[i].knotList))
                {
                    deleteRooms.Add(rooms[i]);
                }
            }
        }

        for (int i = 0; i < deleteRooms.Count; i++)
        {
            Destroy(deleteRooms[i].collider.gameObject);
            Destroy(deleteRooms[i].ceilingMesh.gameObject);
            rooms.Remove(deleteRooms[i]);
        }
    }

    private void CleanIntersections()
    {
        for (int i = 0; i < intersections.Count; i++)
        {
            if (intersections[i].junctions.Count <= 1)
            {
                Destroy(intersections[i].collider.gameObject);
                intersections.Remove(intersections[i]);
            }
        }
    }

    private bool EditKnotsInIntersection(Spline mainSpline, BezierKnot mainKnot, Vector3 position, out List<Spline> intersectList)
    {
        intersectList = new();
        bool isEdit = false;
        foreach (Intersection intersection in intersections)
        {
            if (intersection.junctions.FindIndex(item => item.spline == mainSpline && item.knot.Equals(mainKnot)) != -1)
            {
                for (int i = 0; i < intersection.junctions.Count; i++)
                {
                    if (i != intersection.junctions.FindIndex(item => item.spline == mainSpline && item.knot.Equals(mainKnot)))
                    {
                        BezierKnot newKnot = intersection.junctions[i].knot;
                        newKnot.Position = position;
                        int otherPointIndex = intersection.junctions[i].knotIndex == 0 ? 1 : 0;
                        if (otherPointIndex == 1)
                            Quaternion.Euler(0, Vector3.SignedAngle((Vector3)intersection.junctions[i].spline[otherPointIndex].Position - position, Vector3.forward, Vector3.down), 0);
                        else
                            Quaternion.Euler(0, Vector3.SignedAngle(position - (Vector3)intersection.junctions[i].spline[otherPointIndex].Position, Vector3.forward, Vector3.down), 0);
                        intersection.junctions[i].spline[intersection.junctions[i].knotIndex] = newKnot;
                        Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(intersection.junctions[i].spline, newKnot);
                        intersection.junctions[i] = newInfo;
                        intersectList.Add(intersection.junctions[i].spline);

                        if (walls.FindIndex(item => item.wall == intersection.junctions[i].spline) != -1)
                        {
                            Wall wall = walls.Find(item => item.wall == intersection.junctions[i].spline);
                            wall.points[intersection.junctions[i].knotIndex] = position;
                        }
                    }
                }

                isEdit = true;
            }            
        }
        return isEdit;
    }

    private void CreateRoom(Spline spline, BezierKnot knot, int direction, List<BezierKnot> knotList)
    {
        knotList.Add(knot);
        List<List<int>> duples = GetDuplicatePoints(knotList);
        if (duples.Count > 0)
        {
            if (knotList.Count >= 3)
            {
                for (int i = 0; i < duples.Count; i++)
                {
                    for (int j = 1; j < duples[i].Count; j++)
                    {
                        List<BezierKnot> newKnotList = knotList.GetRange(duples[i][j-1], duples[i][j] - duples[i][j-1]);
                        //newKnotList = RemoveDuplicatePoints(newKnotList);
                        //SortPoints(newKnotList);

                        if (!HasRoomWithPoints(newKnotList) && newKnotList.Count >= 3 && IsRoomContinuous(newKnotList) && IsRoomMeshContinuous(newKnotList))
                        {
                            MakeRoom(newKnotList);
                        }
                    }
                }
                return;
            }
        }

        if (spline.IndexOf(knot) - 1 >= 0 && direction == 0)
        {
            List<BezierKnot> newKnotList = new();
            for (int i = 0; i < knotList.Count; i++)
            {
                newKnotList.Add(knotList[i]);
            }
            CreateRoom(spline, spline[spline.IndexOf(knot) - 1], direction, newKnotList);
        }
        else if (spline.IndexOf(knot) + 1 < spline.Count && direction == 1)
        {
            List<BezierKnot> newKnotList = new();
            for (int i = 0; i < knotList.Count; i++)
            {
                newKnotList.Add(knotList[i]);
            }
            CreateRoom(spline, spline[spline.IndexOf(knot) + 1], direction, newKnotList);
        }

        List<Intersection> nextJunctions = new();
        for (int i = 0; i < intersections.Count; i++)
        {
            if (SplineInIntersection(intersections[i], spline, knot))
            {
                //Debug.Log($"Current spline in intersection with {i} at {knot.Position}");
                nextJunctions.Add(intersections[i]);
            }
        }

        foreach (Intersection intersection in nextJunctions)
        {
            foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
            {
                if (junction.spline != spline)
                {
                    int splineIndex = junction.GetSplineIndex(m_SplineContainer);
                    BezierKnot nextKnot = junction.knotIndex == 0 ? m_SplineContainer[splineIndex].Next(junction.knotIndex) : m_SplineContainer[splineIndex].Previous(junction.knotIndex);
                    int dir = junction.knotIndex == 0 ? 1 : 0;
                    //Debug.Log($"{(Vector3)knot.Position} to {(Vector3)nextKnot.Position} at {splineIndex}");

                    List<BezierKnot> newKnotList = new();
                    for (int i = 0; i < knotList.Count; i++)
                    {
                        newKnotList.Add(knotList[i]);
                    }
                    CreateRoom(junction.spline, nextKnot, dir, newKnotList);
                }
            }
        }
    }

    public void AddWalls(List<Vector3> points)
    {      
        for (int i = 1; i < points.Count; i++)
        {
            Spline spline = new Spline();
            List<Vector3> splinePoints = new();
            //List<int> singleKnot = new();

            Vector3 p1 = points[i - 1];
            Vector3 p2 = points[i];

            BezierKnot k1 = new BezierKnot();
            k1.Position = transform.InverseTransformPoint(p1);
            k1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0);
            k1.TangentIn = new Unity.Mathematics.float3(0, 0, 0.1f);
            k1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

            BezierKnot k2 = new BezierKnot();
            k2.Position = transform.InverseTransformPoint(p2);
            k2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0);
            k2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
            k2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

            spline.Add(k1); spline.Add(k2);
            splinePoints.Add(transform.InverseTransformPoint(p1)); splinePoints.Add(transform.InverseTransformPoint(p2)); //gets only the two points

            List<List<Spline>> wallsList = new();
            List<List<BezierKnot>> knotsList = new();
            List<int> hitRemoveList = new();
            List<RaycastHit> hitList = new();

            //Uses a boxcast to determine intersection points
            RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(0.04f, 0.04f, 0.04f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
            RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(0.04f, 0.04f, 0.04f), p1 - p2, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
            List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2);
            foreach (RaycastHit hit in hits)
            {
                if (!HitsContainCollider(hitList, hit) && !(hit.point == Vector3.zero && points[i] != Vector3.zero))
                {
                    hitList.Add(hit);
                    hitList.Sort((a, b) =>
                    {
                        if (a.point == Vector3.zero) { a.point = p1; }
                        float tThis = EvaluateT(spline, a.point);
                        float tComp = EvaluateT(spline, b.point);

                        if (tThis < tComp)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    });
                }
            }

            foreach (RaycastHit hit in hitList)
            {
                for (int j = 0; j < intersections.Count; j++)
                {
                    if (hit.collider == intersections[j].collider)
                    {
                        Spline spline2 = new Spline();
                        List<Vector3> spline2Points = new();

                        Vector3 center = Vector3.zero;
                        foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                        {
                            center += (Vector3)junction.knot.Position;
                        }

                        center /= intersections[j].junctions.Count;

                        //determine the spline in which it is intersecting
                        Unity.Mathematics.float3 intersectingSplinePointf3;
                        SplineUtility.GetNearestPoint(spline, center, out intersectingSplinePointf3, out float t1, (int)(spline.GetLength() * 2));
                        Vector3 intersectingSplinePoint = intersectingSplinePointf3;

                        //determine the direction to move the splines in
                        Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);

                        //add new splines and split the roads (plan)
                        BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint;
                        knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                        knot1.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint;
                        knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                        knot2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint) < 1 && EvaluateT(spline, intersectingSplinePoint) > 0
                            && Vector3.Distance(splinePoints[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(splinePoints[^1], intersectingSplinePoint) >= 0.5f)
                        {
                            FilterPoints(spline, spline2, splinePoints, t1, out spline2Points);
                            spline.Insert(0, knot1); splinePoints.Insert(0, intersectingSplinePoint);

                            spline2.Add(knot2); spline2Points.Add(intersectingSplinePoint);

                            MakeSpline(spline2, spline2Points);
                            for (int filterWallList = 0; filterWallList < wallsList.Count; filterWallList++)
                            {
                                for (int filterWall = 0; filterWall < wallsList[filterWallList].Count; filterWall++)
                                {
                                    if (wallsList[filterWallList][filterWall] == spline)
                                    {
                                        wallsList[filterWallList][filterWall] = spline2;
                                    }
                                }
                            }
                            intersections[j].AddJunction(spline, spline[0], 0.5f);
                            intersections[j].AddJunction(spline2, spline2[^1], 0.5f);
                        }
                        else if (Vector3.Distance(splinePoints[0], intersectingSplinePoint) < 0.5f)
                        {
                            spline.SetKnot(0, knot1);
                            intersections[j].AddJunction(spline, spline[0], 0.5f);
                        }
                        else
                        {
                            spline.SetKnot(spline.Count - 1, knot2);
                            intersections[j].AddJunction(spline, spline[^1], 0.5f);
                        }

                        for (int k = 0; k < intersections[j].junctions.Count; k++)
                        {
                            for (int w = 0; w < walls.Count; w++)
                            {
                                if (walls[w].wall == intersections[j].junctions[k].spline && intersections[j].junctions[k].spline != spline)
                                {
                                    BezierKnot modifyKnot = intersections[j].junctions[k].knot;
                                    modifyKnot.Position = intersectingSplinePoint;
                                    walls[w].wall.SetKnot(intersections[j].junctions[k].knotIndex, modifyKnot);
                                    intersections[j].junctions[k] = new Intersection.JunctionInfo(intersections[j].junctions[k].spline, modifyKnot);
                                }
                            }
                        }

                        for (int k = 0; k < hitList.Count; k++)
                        {
                            Vector3 compA = new Vector3(hitList[k].point.x, 0, hitList[k].point.z);
                            Vector3 compB = new Vector3(intersectingSplinePoint.x, 0, intersectingSplinePoint.z);
                            if (Vector3.Distance(compA, compB) < 1f && !hitList[k].Equals(hit))
                            {
                                hitRemoveList.Add(k);
                            }
                        }
                    }
                }
            }

            foreach (RaycastHit hit in hitList)
            {
                for (int j = 0; j < walls.Count; j++)
                {
                    if (hit.collider == walls[j].collider && !hitRemoveList.Contains(hitList.IndexOf(hit)))
                    {
                        Spline spline2 = new Spline();
                        List<Vector3> spline2Points = new();
                        Spline spline3 = new Spline();
                        List<Vector3> spline3Points = new();

                        List<Spline> internalWallList = new();
                        List<BezierKnot> internalKnotList = new();

                        Unity.Mathematics.float3 thisSplinePoint = new(); Unity.Mathematics.float3 otherSplinePoint = new();
                        SplineUtility.GetNearestPoint(spline, hit.point, out thisSplinePoint, out float t1, (int)((spline.GetLength() * 2)));
                        SplineUtility.GetNearestPoint(walls[j].wall, hit.point, out otherSplinePoint, out float t2, walls[j].resolution);
                        SplineUtility.GetNearestPoint(spline, otherSplinePoint, out thisSplinePoint, out t1, (int)((spline.GetLength() * 2)));
                        SplineUtility.GetNearestPoint(walls[j].wall, thisSplinePoint, out otherSplinePoint, out t2, walls[j].resolution);
                        SplineUtility.GetNearestPoint(spline, otherSplinePoint, out thisSplinePoint, out t1, (int)((spline.GetLength() * 2)));
                        Vector3 intersectingSplinePoint = new Vector3(Mathf.Round(thisSplinePoint.x * 10) / 10, Mathf.Round(thisSplinePoint.y * 10) / 10, Mathf.Round(thisSplinePoint.z * 10) / 10); //used to prioritise the existing road coordinates in the creation of an intersection

                        Vector3 dir1 = SplineUtility.EvaluateTangent(spline, EvaluateT(spline, intersectingSplinePoint));
                        Vector3 dir2 = SplineUtility.EvaluateTangent(walls[j].wall, EvaluateT(walls[j].wall, intersectingSplinePoint));

                        BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint;
                        knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                        knot1.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint;
                        knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                        knot2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        BezierKnot knot3 = new BezierKnot(); knot3.Position = intersectingSplinePoint;
                        knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                        knot3.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                        knot3.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        BezierKnot knot4 = new BezierKnot(); knot4.Position = intersectingSplinePoint;
                        knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                        knot4.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                        knot4.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        //insert incoming spline
                        if (Vector3.Distance(splinePoints[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(splinePoints[^1], intersectingSplinePoint) >= 0.5f)
                        {
                            FilterPoints(spline, spline2, splinePoints, t1, out spline2Points);
                            spline.Insert(0, knot1); splinePoints.Insert(0, intersectingSplinePoint);

                            spline2.Add(knot2); spline2Points.Add(intersectingSplinePoint);

                            MakeSpline(spline2, spline2Points);
                            for (int filterWallList = 0; filterWallList < wallsList.Count; filterWallList++)
                            {
                                for (int filterWall = 0; filterWall < wallsList[filterWallList].Count; filterWall++)
                                {
                                    if (wallsList[filterWallList][filterWall] == spline)
                                    {
                                        wallsList[filterWallList][filterWall] = spline2;
                                    }
                                }
                            }
                            internalWallList.Add(spline); internalWallList.Add(spline2);
                            internalKnotList.Add(spline[0]); internalKnotList.Add(spline2[^1]);
                            //CreateRoom(spline2, spline2[spline2.Count - 1], 0, new List<BezierKnot>());
                        }
                        else if (Vector3.Distance(splinePoints[0], intersectingSplinePoint) < 0.5f)
                        {
                            spline.SetKnot(0, knot1);
                            internalWallList.Add(spline); internalKnotList.Add(spline[0]);
                            //singleKnot.Remove(0);
                        }
                        else if (Vector3.Distance(splinePoints[^1], intersectingSplinePoint) < 0.5f)
                        {
                            spline.SetKnot(spline.Count - 1, knot2);
                            internalWallList.Add(spline); internalKnotList.Add(spline[^1]);
                            //singleKnot.Remove(1);
                        }

                        //insert overlapping spline
                        if (EvaluateT(walls[j].wall, intersectingSplinePoint) > 0 && EvaluateT(walls[j].wall, intersectingSplinePoint) < 1 && Vector3.Distance(walls[j].points[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(walls[j].points[^1], intersectingSplinePoint) >= 0.5f)
                        {
                            FilterPoints(walls[j].wall, spline3, walls[j].points, t2, out spline3Points);
                            walls[j].wall.Insert(0, knot3); walls[j].points.Insert(0, intersectingSplinePoint);

                            spline3.Add(knot4); spline3Points.Add(intersectingSplinePoint);
                            walls[j].resolution = ((int)(walls[j].wall.GetLength() * 2));

                            MakeSpline(spline3, spline3Points);
                            FilterIntersections(walls[j].wall, spline3, t2);
                            internalWallList.Add(walls[j].wall); internalWallList.Add(spline3);
                            internalKnotList.Add(walls[j].wall[0]); internalKnotList.Add(spline3[^1]);
                        }
                        else if (Vector3.Distance(walls[j].points[0], intersectingSplinePoint) < 0.5f)
                        {
                            walls[j].wall.SetKnot(0, knot3);
                            internalWallList.Add(walls[j].wall); internalKnotList.Add(walls[j].wall[0]);
                        }
                        else if (Vector3.Distance(walls[j].points[^1], intersectingSplinePoint) < 0.5f)
                        {
                            walls[j].wall.SetKnot(walls[j].wall.Count - 1, knot4);
                            internalWallList.Add(walls[j].wall); internalKnotList.Add(walls[j].wall[^1]);
                        }

                        if (internalWallList.Count > 0)
                        {
                            wallsList.Add(internalWallList);
                            knotsList.Add(internalKnotList);
                        }
                    }
                }
            }

            MakeSpline(spline, splinePoints);
            for (int k = 0; k < wallsList.Count; k++)
            {
                MakeIntersection(wallsList[k], knotsList[k]);
            }

            CreateRoom(spline, spline[0], 1, new List<BezierKnot>());
            CleanRooms();
            CleanIntersections();
            //MakeWalls();
        }
    }

    public void ModifyWalls(Wall wall, List<Vector3> points)
    {
        Spline spline = wall.wall;
        List<Vector3> pointsList = points;
        List<Spline> intersectList = new();

        Vector3 p1 = points[0];
        Vector3 p2 = points[1];

        BezierKnot k1 = new BezierKnot();
        k1.Position = transform.InverseTransformPoint(p1);
        k1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0);
        k1.TangentIn = new Unity.Mathematics.float3(0, 0, 0.1f);
        k1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

        BezierKnot k2 = new BezierKnot();
        k2.Position = transform.InverseTransformPoint(p2);
        k2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0);
        k2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
        k2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

        bool isIntersect1 = EditKnotsInIntersection(spline, spline[0], transform.InverseTransformPoint(p1), out List<Spline> newIntersectList);
        intersectList.AddRange(newIntersectList);

        bool isIntersect2 = EditKnotsInIntersection(spline, spline[1], transform.InverseTransformPoint(p2), out newIntersectList);
        intersectList.AddRange(newIntersectList);

        spline.SetKnot(0, k1);
        spline.SetKnot(spline.Count - 1, k2);
        wall.wall = spline;

        List<List<Spline>> wallsList = new();
        List<List<BezierKnot>> knotsList = new();
        List<int> hitRemoveList = new();
        List<RaycastHit> hitList = new();
        List<Intersection> removeIntersectionList = new();

        //Uses a boxcast to determine intersection points
        RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(0.04f, 0.04f, 0.04f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
        RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(0.04f, 0.04f, 0.04f), p1 - p2, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
        List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2);
        foreach (RaycastHit hit in hits)
        {
            if (!HitsContainCollider(hitList, hit) && !(hit.point == Vector3.zero && p1 != Vector3.zero))
            {
                hitList.Add(hit);
                hitList.Sort((a, b) =>
                {
                    if (a.point == Vector3.zero) { a.point = p1; }
                    float tThis = EvaluateT(spline, a.point);
                    float tComp = EvaluateT(spline, b.point);

                    if (tThis < tComp)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                });
            }
        }

        foreach (RaycastHit hit in hitList)
        {
            for (int j = 0; j < intersections.Count; j++)
            {
                if (hit.collider == intersections[j].collider && intersections[j].junctions.FindIndex(item => item.spline == spline) == -1)
                {
                    Spline spline2 = new Spline();
                    List<Vector3> spline2Points = new();

                    Vector3 center = Vector3.zero;
                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {
                        center += (Vector3)junction.knot.Position;
                    }

                    center /= intersections[j].junctions.Count;

                    //determine the spline in which it is intersecting
                    Unity.Mathematics.float3 intersectingSplinePointf3;
                    SplineUtility.GetNearestPoint(spline, center, out intersectingSplinePointf3, out float t1, (int)(spline.GetLength() * 2));
                    Vector3 intersectingSplinePoint = intersectingSplinePointf3;

                    //determine the direction to move the splines in
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);

                    //add new splines and split the roads (plan)
                    BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint;
                    knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                    knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                    knot1.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                    BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint;
                    knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                    knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                    knot2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                    //insert incoming spline
                    if (EvaluateT(spline, intersectingSplinePoint) < 1 && EvaluateT(spline, intersectingSplinePoint) > 0
                        && Vector3.Distance(pointsList[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(pointsList[^1], intersectingSplinePoint) >= 0.5f)
                    {
                        FilterPoints(spline, spline2, pointsList, t1, out spline2Points);
                        spline.Insert(0, knot1); pointsList.Insert(0, intersectingSplinePoint);

                        spline2.Add(knot2); spline2Points.Add(intersectingSplinePoint);

                        MakeSpline(spline2, spline2Points);
                        FilterIntersections(spline, spline2, t1);
                        intersections[j].AddJunction(spline, spline[0], 0.5f);
                        intersections[j].AddJunction(spline2, spline2[^1], 0.5f);
                    }
                    else if (Vector3.Distance(pointsList[0], intersectingSplinePoint) < 0.5f)
                    {
                        spline.SetKnot(0, knot1);
                        if (!isIntersect1)
                        {
                            intersections[j].AddJunction(spline, spline[0], 0.5f);
                        }
                        else
                        {
                            for (int k = 0; k < intersections.Count; k++)
                            {
                                if (intersections[k].junctions.FindIndex(item => item.spline == spline && item.knot.Equals(spline[0])) != -1)
                                {
                                    for (int i = 0; i < intersections[k].junctions.Count; i++)
                                    {
                                        BezierKnot newKnot = intersections[k].junctions[i].knot;
                                        newKnot.Position = p1;
                                        int otherPointIndex = intersections[k].junctions[i].knotIndex == 0 ? 1 : 0;
                                        if (otherPointIndex == 1)
                                            Quaternion.Euler(0, Vector3.SignedAngle((Vector3)intersections[k].junctions[i].spline[otherPointIndex].Position - p1, Vector3.forward, Vector3.down), 0);
                                        else
                                            Quaternion.Euler(0, Vector3.SignedAngle(p1 - (Vector3)intersections[k].junctions[i].spline[otherPointIndex].Position, Vector3.forward, Vector3.down), 0);
                                        intersections[j].AddJunction(intersections[k].junctions[i].spline, newKnot, 0.5f);
                                    }

                                    removeIntersectionList.Add(intersections[k]);
                                }
                            }
                        }
                    }
                    else
                    {
                        spline.SetKnot(spline.Count - 1, knot2);
                        if (!isIntersect2)
                        {
                            intersections[j].AddJunction(spline, spline[^1], 0.5f);
                        }
                        else
                        {
                            for (int k = 0; k < intersections.Count; k++)
                            {
                                if (intersections[k].junctions.FindIndex(item => item.spline == spline && item.knot.Equals(spline[1])) != -1)
                                {
                                    for (int i = 0; i < intersections[k].junctions.Count; i++)
                                    {
                                        BezierKnot newKnot = intersections[k].junctions[i].knot;
                                        newKnot.Position = p2;
                                        int otherPointIndex = intersections[k].junctions[i].knotIndex == 0 ? 1 : 0;
                                        if (otherPointIndex == 1)
                                            Quaternion.Euler(0, Vector3.SignedAngle((Vector3)intersections[k].junctions[i].spline[otherPointIndex].Position - p2, Vector3.forward, Vector3.down), 0);
                                        else
                                            Quaternion.Euler(0, Vector3.SignedAngle(p2 - (Vector3)intersections[k].junctions[i].spline[otherPointIndex].Position, Vector3.forward, Vector3.down), 0);
                                        intersections[j].AddJunction(intersections[k].junctions[i].spline, newKnot, 0.5f);
                                    }

                                    removeIntersectionList.Add(intersections[k]);
                                }
                            }
                        }
                    }

                    for (int k = 0; k < intersections[j].junctions.Count; k++)
                    {
                        for (int w = 0; w < walls.Count; w++)
                        {
                            if (walls[w].wall == intersections[j].junctions[k].spline && intersections[j].junctions[k].spline != spline)
                            {
                                BezierKnot modifyKnot = intersections[j].junctions[k].knot;
                                modifyKnot.Position = intersectingSplinePoint;
                                walls[w].wall.SetKnot(intersections[j].junctions[k].knotIndex, modifyKnot);
                                intersections[j].junctions[k] = new Intersection.JunctionInfo(intersections[j].junctions[k].spline, modifyKnot);
                            }
                        }
                    }

                    for (int k = 0; k < hitList.Count; k++)
                    {
                        Vector3 compA = new Vector3(hitList[k].point.x, 0, hitList[k].point.z);
                        Vector3 compB = new Vector3(intersectingSplinePoint.x, 0, intersectingSplinePoint.z);
                        if (Vector3.Distance(compA, compB) < 1f && !hitList[k].Equals(hit))
                        {
                            hitRemoveList.Add(k);
                        }
                    }
                }
            }
        }

        foreach (RaycastHit hit in hitList)
        {
            for (int j = 0; j < walls.Count; j++)
            {
                if (hit.collider == walls[j].collider && !hitRemoveList.Contains(hitList.IndexOf(hit)) && walls[j] != wall && !intersectList.Contains(wall.wall))
                {
                    Spline spline2 = new Spline();
                    List<Vector3> spline2Points = new();
                    Spline spline3 = new Spline();
                    List<Vector3> spline3Points = new();

                    List<Spline> internalWallList = new();
                    List<BezierKnot> internalKnotList = new();

                    Unity.Mathematics.float3 thisSplinePoint = new(); Unity.Mathematics.float3 otherSplinePoint = new();
                    SplineUtility.GetNearestPoint(spline, hit.point, out thisSplinePoint, out float t1, (int)((spline.GetLength() * 2)));
                    SplineUtility.GetNearestPoint(walls[j].wall, hit.point, out otherSplinePoint, out float t2, walls[j].resolution);
                    SplineUtility.GetNearestPoint(spline, otherSplinePoint, out thisSplinePoint, out t1, (int)((spline.GetLength() * 2)));
                    SplineUtility.GetNearestPoint(walls[j].wall, thisSplinePoint, out otherSplinePoint, out t2, walls[j].resolution);
                    SplineUtility.GetNearestPoint(spline, otherSplinePoint, out thisSplinePoint, out t1, (int)((spline.GetLength() * 2)));
                    Vector3 intersectingSplinePoint = thisSplinePoint; //used to prioritise the existing road coordinates in the creation of an intersection

                    Vector3 dir1 = SplineUtility.EvaluateTangent(spline, EvaluateT(spline, intersectingSplinePoint));
                    Vector3 dir2 = SplineUtility.EvaluateTangent(walls[j].wall, EvaluateT(walls[j].wall, intersectingSplinePoint));

                    BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint;
                    knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                    knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                    knot1.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                    BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint;
                    knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                    knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                    knot2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                    BezierKnot knot3 = new BezierKnot(); knot3.Position = intersectingSplinePoint;
                    knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                    knot3.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                    knot3.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                    BezierKnot knot4 = new BezierKnot(); knot4.Position = intersectingSplinePoint;
                    knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                    knot4.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                    knot4.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                    //insert incoming spline
                    if (Vector3.Distance(pointsList[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(pointsList[^1], intersectingSplinePoint) >= 0.5f)
                    {
                        FilterPoints(spline, spline2, pointsList, t1, out spline2Points);
                        spline.Insert(0, knot1); pointsList.Insert(0, intersectingSplinePoint);
                        wall.resolution = ((int)(spline.GetLength() * 2));

                        spline2.Add(knot2); spline2Points.Add(intersectingSplinePoint);

                        MakeSpline(spline2, spline2Points);
                        FilterIntersections(spline, spline2, t1);
                        internalWallList.Add(spline); internalWallList.Add(spline2);
                        internalKnotList.Add(spline[0]); internalKnotList.Add(spline2[^1]);
                        //CreateRoom(spline2, spline2[spline2.Count - 1], 0, new List<BezierKnot>());
                    }
                    else if (Vector3.Distance(pointsList[0], intersectingSplinePoint) < 0.5f)
                    {
                        spline.SetKnot(0, knot1);
                        if (!isIntersect1)
                        {
                            internalWallList.Add(spline); internalKnotList.Add(spline[0]);
                        }
                        //singleKnot.Remove(0);
                    }
                    else if (Vector3.Distance(pointsList[^1], intersectingSplinePoint) < 0.5f)
                    {
                        spline.SetKnot(spline.Count - 1, knot2);
                        if (!isIntersect2)
                        {
                            internalWallList.Add(spline); internalKnotList.Add(spline[^1]);
                        }
                        //singleKnot.Remove(1);
                    }

                    //insert overlapping spline
                    if (EvaluateT(walls[j].wall, intersectingSplinePoint) > 0 && EvaluateT(walls[j].wall, intersectingSplinePoint) < 1 && Vector3.Distance(walls[j].points[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(walls[j].points[^1], intersectingSplinePoint) >= 0.5f)
                    {
                        FilterPoints(walls[j].wall, spline3, walls[j].points, t2, out spline3Points);
                        walls[j].wall.Insert(0, knot3); walls[j].points.Insert(0, intersectingSplinePoint);

                        spline3.Add(knot4); spline3Points.Add(intersectingSplinePoint);
                        walls[j].resolution = ((int)(walls[j].wall.GetLength() * 2));

                        MakeSpline(spline3, spline3Points);
                        FilterIntersections(walls[j].wall, spline3, t2);
                        internalWallList.Add(walls[j].wall); internalWallList.Add(spline3);
                        internalKnotList.Add(walls[j].wall[0]); internalKnotList.Add(spline3[^1]);
                    }
                    else if (Vector3.Distance(walls[j].points[0], intersectingSplinePoint) < 0.5f)
                    {
                        walls[j].wall.SetKnot(0, knot3);
                        if (!isIntersect1)
                        {
                            internalWallList.Add(walls[j].wall); internalKnotList.Add(walls[j].wall[0]);
                        }
                    }
                    else if (Vector3.Distance(walls[j].points[^1], intersectingSplinePoint) < 0.5f)
                    {
                        walls[j].wall.SetKnot(walls[j].wall.Count - 1, knot4);
                        if (!isIntersect2)
                        {
                            internalWallList.Add(walls[j].wall); internalKnotList.Add(walls[j].wall[^1]);
                        }
                    }

                    if (internalWallList.Count > 0)
                    {
                        wallsList.Add(internalWallList);
                        knotsList.Add(internalKnotList);
                    }
                }
            }
        }

        foreach (Intersection intersection in intersections)
        {
            if (intersection.junctions.FindIndex(item => item.spline == wall.wall) != -1)
            {
                Intersection.JunctionInfo junction = intersection.junctions.Find(item => item.spline == wall.wall);
                if (junction.knotIndex == 0)
                {
                    Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(spline, spline[0]);
                    intersection.junctions[intersection.junctions.IndexOf(junction)] = newInfo;
                }
                else
                {
                    Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(spline, spline[^1]);
                    intersection.junctions[intersection.junctions.IndexOf(junction)] = newInfo;
                }
            }
        }

        wall.wall = spline;
        wall.points = pointsList;
        for (int k = 0; k < wallsList.Count; k++)
        {
            MakeIntersection(wallsList[k], knotsList[k]);
        }
        for (int k = 0; k < removeIntersectionList.Count; k++)
        {
            Destroy(removeIntersectionList[k].collider.gameObject);
            intersections.Remove(removeIntersectionList[k]);
        }

        CreateRoom(spline, spline[0], 1, new List<BezierKnot>());
        CleanRooms();
        CleanIntersections();
        MakeWalls();
    }


    private void MakeSpline(Spline spline, List<Vector3> points)
    {
        if (Vector3.Distance(points[0], points[^1]) > 0.01f)
        {
            m_SplineContainer.AddSpline(spline);

            GameObject c = new();
            c.name = $"Wall{walls.Count}";
            c.transform.SetParent(this.transform.Find("Walls"));
            c.AddComponent<MeshCollider>();
            c.AddComponent<MeshRenderer>();
            c.AddComponent<MeshFilter>();
            c.layer = LayerMask.NameToLayer("Selector");

            Wall wall = new Wall(spline, points, ((int)(spline.GetLength() * 2)), c.GetComponent<MeshCollider>(), c.GetComponent<MeshRenderer>(), c.GetComponent<MeshFilter>(), defaultWallMaterial);
            walls.Add(wall);

            BuildWall(walls.IndexOf(wall));
        }
        else
        {
            foreach (Intersection intersection in intersections)
            {
                List<Intersection.JunctionInfo> removeJunctions = new();
                foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                {
                    if (junction.spline == spline)
                    {
                        removeJunctions.Add(junction);
                    }
                }
                foreach (Intersection.JunctionInfo junction in removeJunctions)
                {
                    intersection.junctions.Remove(junction);
                }
            }
        }
    }

    private void MakeIntersection(List<Spline> splines, List<BezierKnot> knots)
    {
        Intersection intersection = new Intersection();
        List<Spline> containerSplines = new();
        foreach (Spline spline in m_SplineContainer.Splines) { containerSplines.Add(spline); }
        for (int c = 0; c < splines.Count; c++)
        {
            BezierKnot knot = knots[c];
            Spline spline = splines[c];
            intersection.AddJunction(spline, knot, 0.5f);
        }

        GameObject collider = new();
        collider.name = $"Intersection{intersections.Count}";
        collider.transform.SetParent(wallParent.transform);
        collider.AddComponent<MeshCollider>();
        collider.AddComponent<MeshFilter>();
        collider.AddComponent<MeshRenderer>();
        collider.layer = 6;

        intersections.Add(intersection);
        intersection.collider = collider.GetComponent<MeshCollider>();

        BuildIntersection(intersections.IndexOf(intersection));
    }

    private void MakeRoom(List<BezierKnot> knotList)
    {
        List<Vector3> pointsList = new();
        for (int i = 0; i < knotList.Count; i++)
        {
            pointsList.Add(knotList[i].Position);
        }

        GameObject collider = new();
        collider.name = $"Floor{rooms.Count}";
        collider.transform.SetParent(floorParent.transform);
        collider.AddComponent<MeshCollider>();
        collider.AddComponent<MeshFilter>();
        collider.AddComponent<MeshRenderer>();
        collider.layer = 6;

        GameObject ceilingCollider = new();
        ceilingCollider.name = $"Ceiling{rooms.Count}";
        ceilingCollider.transform.SetParent(ceilParent.transform);
        ceilingCollider.AddComponent<MeshFilter>();
        ceilingCollider.AddComponent<MeshRenderer>();
        ceilingCollider.layer = 6;

        Room newRoom = new Room(pointsList, knotList, collider.GetComponent<MeshCollider>(), collider.GetComponent<MeshRenderer>(), collider.GetComponent<MeshFilter>(), ceilingCollider.GetComponent<MeshFilter>(), ceilingCollider.GetComponent<MeshRenderer>(), defaultFloorMaterial);
        rooms.Add(newRoom);

        BuildRoom(rooms.IndexOf(newRoom));
    }

    public void SelectRoad(Vector3 position, Vector2Int size, int rotation, out Wall selectedWall, out int index, out List<Vector3> points)
    {
        Collider[] overlaps = Physics.OverlapBox(position, new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, 5, 0), LayerMask.GetMask("Selector"));
        selectedWall = null; index = -1; points = new();
        foreach (Wall wall in walls)
        {
            Collider selector = wall.collider;
            foreach (Collider hit in overlaps)
            {
                if (hit == selector)
                {
                    selectedWall = wall;
                    index = walls.IndexOf(wall);
                    foreach (Vector3 point in wall.points)
                    {
                        points.Add(transform.TransformPoint(point));
                    }
                }
            }
        }
    }

    public bool CheckWallSelect(Vector3 position, Vector2Int size, int rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x + 0.05f, position.y, position.z + 0.05f), new Vector3(size.x / 2f - 0.1f, 0.5f, size.y / 2f - 0.1f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
        bool ans = false;
        foreach (Wall wall in walls)
        {
            Collider selector = wall.collider;
            foreach (Collider hit in overlaps)
            {
                if (hit == selector)
                {
                    ans = true;
                }
            }
        }
        return ans;
    }

    private void DeleteWall(Wall wall)
    {
        foreach (Intersection intersection in intersections)
        {
            List<Intersection.JunctionInfo> removeJunctions = new();
            foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
            {
                if (junction.spline == wall.wall)
                {
                    removeJunctions.Add(junction);
                }
            }
            foreach (Intersection.JunctionInfo junction in removeJunctions)
            {
                intersection.junctions.Remove(junction);
            }
        }
        m_SplineContainer.RemoveSpline(wall.wall);
        Destroy(wall.collider.gameObject);
        walls.Remove(wall);
    }

    public void RemoveWall(Wall wall)
    {
        DeleteWall(wall);
        CleanIntersections();
        MakeWalls();
    }

    public void SetWallsActive(bool toggle)
    {
        wallParent.SetActive(toggle);
    }

    public void SetFloorsActive(bool toggle)
    {
        floorParent.SetActive(toggle);
    }

    public void SetCeilingsActive(bool toggle)
    {
        ceilParent.SetActive(toggle);
    }

    private void DrawPoints(List<Vector3> points)
    {
        Color c = Random.ColorHSV();

        for (int i = 0; i < points.Count; i++)
        {
            GameObject obj = Instantiate(gizmoPointer);
            obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            obj.transform.position = points[i];
            obj.GetComponent<Renderer>().material.color = c;
        }
    }
}

[System.Serializable]
public class Wall
{
    public Spline wall;
    public List<Vector3> points;
    public int resolution;
    public MeshCollider collider;
    public MeshFilter mesh;
    public MeshRenderer renderer;

    public Wall(Spline wall, List<Vector3> points, int resolution, MeshCollider collider, MeshRenderer renderer, MeshFilter mesh, Material defaultWallMaterial)
    {
        this.wall = wall;
        this.points = points;
        this.resolution = resolution;
        this.collider = collider;
        this.renderer = renderer;
        this.mesh = mesh;

        Material[] newMaterials = new Material[3];
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            newMaterials[i] = renderer.sharedMaterials[i];
        }
        newMaterials[0] = defaultWallMaterial;
        newMaterials[1] = defaultWallMaterial;
        newMaterials[2] = defaultWallMaterial;

        renderer.sharedMaterials = newMaterials;
    }
}

[System.Serializable]
public class Room
{
    public List<Vector3> points;
    public List<BezierKnot> knotList;
    public MeshCollider collider;
    public MeshFilter mesh;
    public MeshFilter ceilingMesh;
    public MeshRenderer renderer;
    public MeshRenderer ceilingRenderer;

    public Room(List<Vector3> points, List<BezierKnot> knotList, MeshCollider collider, MeshRenderer renderer, MeshFilter mesh, MeshFilter ceilingMesh, MeshRenderer ceilingRenderer, Material defaultFloorMaterial)
    {
        this.points = points;
        this.knotList = knotList;
        this.collider = collider;
        this.renderer = renderer;
        this.ceilingMesh = ceilingMesh;
        this.ceilingRenderer = ceilingRenderer;
        this.mesh = mesh;

        Material[] newMaterials = new Material[2];
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            newMaterials[i] = renderer.sharedMaterials[i];
        }
        newMaterials[0] = defaultFloorMaterial;
        newMaterials[1] = defaultFloorMaterial;

        renderer.sharedMaterials = newMaterials;
        ceilingRenderer.sharedMaterials = newMaterials;
    }
}
