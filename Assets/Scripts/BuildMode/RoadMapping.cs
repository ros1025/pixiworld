using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class RoadMapping : MonoBehaviour
{
    private List<Vector3> m_vertsP1;
    private List<Vector3> m_vertsP2;
    public List<Intersection> intersections = new();
    public List<Roads> roads = new();
    [SerializeField] private SplineSampler m_SplineSampler;
    [SerializeField] public SplineContainer m_SplineContainer;
    [SerializeField] private MeshFilter m_meshFilter;
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private Texture2D intersectionTex;
    [SerializeField] private Material roadMat;
    [SerializeField] private Material intersectMat;


    void MakeRoad()
    {
        for (int i = 0; i < 3; i++)
        {
            GetVerts();
            BuildMesh();
        }
    }

    private void GetVerts()
    {
        m_vertsP1 = new List<Vector3>();
        m_vertsP2 = new List<Vector3>();

        Vector3 p1;
        Vector3 p2;
        for (int j = 0; j < roads.Count; j++)
        {
            int resolution = (int)(roads[j].resolution);
            float step = 1f / (float)resolution;
            for (int i = 0; i < resolution; i++)
            {
                float t = step * i;
                m_SplineSampler.SampleSplineWidth(j, t, roads[j].width, out p1, out p2);
                m_vertsP1.Add(p1);
                m_vertsP2.Add(p2);
            }

            m_SplineSampler.SampleSplineWidth(j, 1f, roads[j].width, out p1, out p2);
            m_vertsP1.Add(p1);
            m_vertsP2.Add(p2);
        }
    }

    public void AddJunction(Intersection intersection, MeshCollider collider, MeshFilter mesh, MeshRenderer renderer)
    {
        intersections.Add(intersection);
        intersection.collider = collider;
        intersection.mesh = mesh;
        intersection.renderer = renderer;
    }

    public void RemoveJunction(Intersection intersection)
    {
        intersections.Remove(intersection);
    }

    private int calculateRes(int index)
    {
        int res = 0;
        for (int i = 0; i < index; i++)
        {
            res += roads[i].resolution;
        }
        return res;
    }

    private void BuildRoadMesh(int currentSplineIndex)
    {
        Mesh mesh = new Mesh();        
        Mesh c = new Mesh();
        List<Vector3> verts = new List<Vector3>(); List<Vector3> vertsC = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>(); List<int> trisC = new List<int>();

        roads[currentSplineIndex].resolution = (int)roads[currentSplineIndex].road.GetLength() * 2;
        int resolution = (int)(roads[currentSplineIndex].resolution);
        float uvOffset = 0;

        for (int currentSplinePoint = 1; currentSplinePoint <= resolution /*< resolution-1*/; currentSplinePoint++)
        {
            m_SplineSampler.SampleSplineWidth(currentSplineIndex, (float)(currentSplinePoint - 1) / resolution, roads[currentSplineIndex].width, out Vector3 p1, out Vector3 p2);
            m_SplineSampler.SampleSplineWidth(currentSplineIndex, (float)(currentSplinePoint) / resolution, roads[currentSplineIndex].width, out Vector3 p3, out Vector3 p4);

            Vector3 p5 = p1 + new Vector3(0, 0.025f, 0);
            Vector3 p6 = p2 + new Vector3(0, 0.025f, 0);
            Vector3 p7 = p3 + new Vector3(0, 0.025f, 0);
            Vector3 p8 = p4 + new Vector3(0, 0.025f, 0);

            float distance = Vector3.Distance(p1, p3);
            float uvDistance = uvOffset + distance;

            verts.AddRange(new List<Vector3> {p1, p2, p3, p4, p5, p6, p7, p8});
            vertsC.AddRange(new List<Vector3> {p1, p2, p3, p4, p5, p6, p7, p8});
            uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1),
                                    new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1)});

            uvOffset += distance;

            tris.AddRange(CreateTris(verts, new List<Vector3> {p1, p2, p4, p4, p3, p1})); trisC.AddRange(CreateTris(vertsC, new List<Vector3> {p1, p2, p4, p4, p3, p1})); 
            tris.AddRange(CreateTris(verts, new List<Vector3> {p7, p8, p6, p6, p5, p7})); trisC.AddRange(CreateTris(vertsC, new List<Vector3> {p7, p8, p6, p6, p5, p7})); 
            tris.AddRange(CreateTris(verts, new List<Vector3> {p7, p5, p1, p1, p3, p7})); trisC.AddRange(CreateTris(vertsC, new List<Vector3> {p7, p5, p1, p1, p3, p7})); 
            tris.AddRange(CreateTris(verts, new List<Vector3> {p6, p8, p4, p4, p2, p6})); trisC.AddRange(CreateTris(vertsC, new List<Vector3> {p6, p8, p4, p4, p2, p6})); 

            if (currentSplinePoint == 1)
            {
                tris.AddRange(CreateTris(verts, new List<Vector3> {p5, p6, p2, p2, p1, p5})); 
                trisC.AddRange(CreateTris(vertsC, new List<Vector3> {p5, p6, p2, p2, p1, p5}));
            }
            if (currentSplinePoint == resolution)
            {
                tris.AddRange(CreateTris(verts, new List<Vector3> {p8, p7, p3, p3, p4, p8})); 
                trisC.AddRange(CreateTris(vertsC, new List<Vector3> {p8, p7, p3, p3, p4, p8}));
            }
        }
        mesh.subMeshCount = 1;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);

        c.SetVertices(vertsC);
        c.SetTriangles(trisC, 0);

        roads[currentSplineIndex].collider.sharedMesh = c;
        roads[currentSplineIndex].mesh.mesh = mesh;
    }

    private void BuildIntersectionMesh(int i)
    {
        Mesh mesh = new Mesh();        
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        Intersection intersection = intersections[i];
        int count = 0;
        //List<Vector3> points = new List<Vector3>();
        List<Intersection.JunctionEdge> junctionEdges = new List<Intersection.JunctionEdge>();

        Vector3 center = new Vector3();
        foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
        {
            int splineIndex = junction.GetSplineIndex(m_SplineContainer);
            float t = junction.knotIndex == 0 ? 0f : 1f;
            m_SplineSampler.SampleSplineWidth(splineIndex, t, roads[splineIndex].width, out Vector3 p1, out Vector3 p2);
            //if knot index is 0 we are facing away from the junction, otherwise we are facing the junction
            if (junction.knotIndex == 0)
            {
                junctionEdges.Add(new Intersection.JunctionEdge(p1, p2));
            }
            else
            {
                junctionEdges.Add(new Intersection.JunctionEdge(p2, p1));
            }

            center += p1;
            center += p2;
            count++;
        }

        center /= (junctionEdges.Count * 2);

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

        List<Vector3> curvePoints = new List<Vector3>();
        //Additional points
        Vector3 mid;
        Vector3 c;
        Vector3 b;
        Vector3 a;
        BezierCurve curve;
        for (int j = 1; j <= junctionEdges.Count; j++)
        {
            a = junctionEdges[j - 1].left;
            curvePoints.Add(a);
            b = (j < junctionEdges.Count) ? junctionEdges[j].right : junctionEdges[0].right;
            mid = Vector3.Lerp(a, b, 0.5f);
            Vector3 dir = center - mid;
            mid = mid - dir;
            c = Vector3.Lerp(mid, center, intersection.curves[j - 1]);

            curve = new BezierCurve(a, c, b);
            for (float t = 0f; t < 1f; t += 0.2f)
            {
                Vector3 pos = CurveUtility.EvaluatePosition(curve, t);
                curvePoints.Add(pos);
            }

            curvePoints.Add(b);
        }
        curvePoints.Reverse();

        int pointsOffset = verts.Count;
        Mesh intersectionMesh = new Mesh();
        List<Vector3> currentVerts = new List<Vector3>();
        List<Vector3> contactVerts = new List<Vector3>();
        List<int> contactTris = new List<int>();

        for (int j = 1; j <= curvePoints.Count; j++)
        {
            currentVerts.Add(center);
            currentVerts.Add(center + new Vector3(0, 0.025f, 0));
            currentVerts.Add(curvePoints[j - 1]);
            currentVerts.Add(curvePoints[j - 1] + new Vector3(0, 0.025f, 0));
            uvs.Add(new Vector2(center.z, center.x));
            uvs.Add(new Vector2(center.z, center.x));
            uvs.Add(new Vector2(curvePoints[j - 1].z, curvePoints[j - 1].x));
            uvs.Add(new Vector2(curvePoints[j - 1].z, curvePoints[j - 1].x));
            if (j == curvePoints.Count)
            {
                currentVerts.Add(curvePoints[0]);
                currentVerts.Add(curvePoints[0] + new Vector3(0, 0.025f, 0));
                uvs.Add(new Vector2(curvePoints[0].z, curvePoints[0].x));
                uvs.Add(new Vector2(curvePoints[0].z, curvePoints[0].x));
            }
            else
            {
                currentVerts.Add(curvePoints[j]);
                currentVerts.Add(curvePoints[j] + new Vector3(0, 0.025f, 0));
                uvs.Add(new Vector2(curvePoints[j].z, curvePoints[j].x));
                uvs.Add(new Vector2(curvePoints[j].z, curvePoints[j].x));
            }
            contactTris.Add(((j - 1) * 6) + 0); contactTris.Add(((j - 1) * 6) + 2); contactTris.Add(((j - 1) * 6) + 4);
            contactTris.Add(((j - 1) * 6) + 1); contactTris.Add(((j - 1) * 6) + 3); contactTris.Add(((j - 1) * 6) + 5);
            contactTris.Add(((j - 1) * 6) + 2); contactTris.Add(((j - 1) * 6) + 3); contactTris.Add(((j - 1) * 6) + 5);
            contactTris.Add(((j - 1) * 6) + 5); contactTris.Add(((j - 1) * 6) + 4); contactTris.Add(((j - 1) * 6) + 2);

            tris.Add(((j - 1) * 6) + 0); tris.Add(((j - 1) * 6) + 2); tris.Add(((j - 1) * 6) + 4);
            tris.Add(((j - 1) * 6) + 1); tris.Add(((j - 1) * 6) + 3); tris.Add(((j - 1) * 6) + 5);
            tris.Add(((j - 1) * 6) + 2); tris.Add(((j - 1) * 6) + 3); tris.Add(((j - 1) * 6) + 5);
            tris.Add(((j - 1) * 6) + 5); tris.Add(((j - 1) * 6) + 4); tris.Add(((j - 1) * 6) + 2);
        }

        verts.AddRange(currentVerts);
        contactVerts.AddRange(currentVerts);
        intersectionMesh.SetVertices(contactVerts); intersectionMesh.SetTriangles(contactTris, 0);
        mesh.SetVertices(verts); mesh.SetTriangles(tris, 0); mesh.SetUVs(0, uvs);
        intersection.collider.sharedMesh = intersectionMesh;
        intersection.mesh.mesh = mesh;
    }

    private List<int> CreateTris(List<Vector3> verts, List<Vector3> trisVerts)
    {
        List<int> tris = new();

        for (int i = 0; i < trisVerts.Count / 3; i++)
        {
            Vector3 v1 = trisVerts[(i * 3)];
            Vector3 v2 = trisVerts[(i * 3) + 1];
            Vector3 v3 = trisVerts[(i * 3) + 2];

            tris.AddRange(new List<int> {verts.IndexOf(v1), verts.IndexOf(v2), verts.IndexOf(v3)});
        }

        return tris;
    }

    private void BuildMesh()
    {
        for (int currentSplineIndex = 0; currentSplineIndex < roads.Count; currentSplineIndex++)
        {
            BuildRoadMesh(currentSplineIndex);
        }

        for (int i = 0; i < intersections.Count; i++)
        {
            BuildIntersectionMesh(i);
        }
    }

    //remove points from road after the intersection and place them into a new road
    private void FilterPoints(Spline road, Spline road2, List<Vector3> points, Dictionary<Vector3, List<BezierKnot>> pointMap, float ratio, out List<Vector3> points2, out Dictionary<Vector3, List<BezierKnot>> pointMap2, out bool splitSpline)
    {
        List<BezierKnot> knotList = new(); List<BezierKnot> knotList2 = new();
        List<Vector3> removePoints = new();
        points2 = new(); pointMap2 = new();
        splitSpline = true;
        foreach (Vector3 point in points)
        {
            List<bool> isAhead = new();
            foreach (BezierKnot roadKnot in pointMap[point])
            {
                int j = road.IndexOf(roadKnot);
                float knotT = road.ConvertIndexUnit(j, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                if (knotT > ratio)
                {
                    knotList.Add(roadKnot);
                    isAhead.Add(true);
                    if (pointMap[point].IndexOf(roadKnot) > 0 && isAhead[0] == false)
                    {
                        splitSpline = false;
                    }
                }
                else
                {
                    knotList2.Add(roadKnot);
                    removePoints.Add(point);
                    if (!points2.Contains(point))
                    {
                        points2.Add(point);
                    }
                    if (!pointMap2.ContainsKey(point))
                    {
                        pointMap2.Add(point, pointMap[point]);
                    }
                    isAhead.Add(false);
                }
            }
        }
        foreach (Vector3 point in removePoints) { points.Remove(point); pointMap.Remove(point); }
        road.Clear(); road2.Clear();
        foreach (BezierKnot roadKnot in knotList) { road.Add(roadKnot); }
        foreach (BezierKnot roadKnot in knotList2) { road2.Add(roadKnot); }
    }

    private void FilterIntersections(Spline s, Spline r, float sT)
    {
        Spline combinedSpline = new Spline();
        foreach (BezierKnot k in r)
        {
            combinedSpline.Add(k);
        }
        foreach (BezierKnot k in s)
        {
            combinedSpline.Add(k);
        }

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
                }
            }
        }
    }

    private void CleanRoads()
    {
        List<Roads> removeRoads = new();
        foreach (Roads road in roads)
        {
            if (road.road.GetLength() <= road.width + 0.5f)
            {
                removeRoads.Add(road);
            }
        }
        foreach (Roads road in removeRoads)
        {
            DeleteRoad(road);
        }
    }

    private void CleanIntersections()
    {
        List<Intersection> removeIntersections = new();
        foreach (Intersection intersection in intersections)
        {
            if (intersection.junctions.Count == 2)
            {
                int indexA = intersection.junctions[0].knotIndex;
                int indexB = intersection.junctions[1].knotIndex;
                Spline splineA = intersection.junctions[0].spline;
                Spline splineB = intersection.junctions[1].spline;
                Roads roadA = null; Roads roadB = null;

                foreach (Roads road in roads)
                {
                    if (road.road == splineA)
                    {
                        roadA = road;
                    }
                    if (road.road == splineB)
                    {
                        roadB = road;
                    }
                }

                if (roadA != roadB)
                {
                    if (indexA == 0 && indexB == 0)
                    {
                        SplineUtility.ReverseFlow(splineB);
                        roadB.points.Reverse();
                        for (int pointMap = 0; pointMap < roadB.points.Count; pointMap++)
                        {
                            List<BezierKnot> newBRoadMap = roadB.roadMap[roadB.points[pointMap]].knotCluster;
                            newBRoadMap.Reverse();
                            for (int knotIndex = 0; knotIndex < newBRoadMap.Count; knotIndex++)
                            {
                                BezierKnot knot = newBRoadMap[knotIndex];
                                Quaternion newRotationQuaternion = knot.Rotation;
                                Vector3 newRotation = newRotationQuaternion.eulerAngles;
                                knot.Rotation = Quaternion.Euler(newRotation.x, newRotation.y - 180, newRotation.z);
                                newBRoadMap[knotIndex] = knot;
                            }
                            roadB.roadMap[roadB.points[pointMap]].knotCluster = newBRoadMap;
                        }
                        indexB = splineB.Count;
                    }
                    else if (indexA > 0 && indexB > 0)
                    {
                        SplineUtility.ReverseFlow(splineB);
                        roadB.points.Reverse();
                        for (int pointMap = 0; pointMap < roadB.points.Count; pointMap++)
                        {
                            List<BezierKnot> newBRoadMap = roadB.roadMap[roadB.points[pointMap]].knotCluster;
                            newBRoadMap.Reverse();
                            for (int knotIndex = 0; knotIndex < newBRoadMap.Count; knotIndex++)
                            {
                                BezierKnot knot = newBRoadMap[knotIndex];
                                Quaternion newRotationQuaternion = knot.Rotation;
                                Vector3 newRotation = newRotationQuaternion.eulerAngles;
                                knot.Rotation = Quaternion.Euler(newRotation.x, newRotation.y - 180, newRotation.z);
                                newBRoadMap[knotIndex] = knot;
                            }
                            roadB.roadMap[roadB.points[pointMap]].knotCluster = newBRoadMap;
                        }
                        indexB = 0;
                    }

                    if (indexA > indexB)
                    {
                        ClearDualIntersection(roadA, roadB, splineA, splineB);
                    }
                    else if (indexB > indexA)
                    {
                        ClearDualIntersection(roadB, roadA, splineA, splineB);
                    }

                    removeIntersections.Add(intersection);
                    DeleteRoad(roadA);
                    DeleteRoad(roadB);
                }
            }
            else if (intersection.junctions.Count <= 1)
            {
                removeIntersections.Add(intersection);
            }
        }

        foreach (Intersection intersection in removeIntersections)
        {
            Destroy(intersection.collider.gameObject);
            intersections.Remove(intersection);
        }
    }

    private void ClearDualIntersection(Roads roadA, Roads roadB, Spline splineA, Spline splineB)
    {
        Spline newSpline = new Spline();
        List<Vector3> newPoints = new List<Vector3>();
        Dictionary<Vector3, List<BezierKnot>> newMap = new Dictionary<Vector3, List<BezierKnot>>();

        for (int i = 0; i < roadA.points.Count - 1; i++)
        {
            newPoints.Add(roadA.points[i]);
            newMap.Add(roadA.points[i], roadA.roadMap[roadA.points[i]].knotCluster);
        }

        BezierKnot joinKnotA = roadA.roadMap[roadA.points[^1]].knotCluster[0]; //first intersecting knot
        BezierKnot joinKnotB = roadB.roadMap[roadB.points[0]].knotCluster[0]; //second intersecting knot

        Vector3 center = GetPointCenter((Vector3)roadA.points[^1], (roadA.points[^2] - roadA.points[^1]).normalized, (Vector3)roadB.points[0], (roadB.points[1] - roadB.points[0]).normalized);
        //Debug.Log(center);

        float prevAngle = Vector3.SignedAngle(Vector3.forward, center - roadA.points[^2], Vector3.up);
        float angle = Vector3.SignedAngle(Vector3.forward, roadB.points[1] - center, Vector3.up);
        float sign = Mathf.Abs(angle - prevAngle);

        angle = angle >= 0 ? angle : 360 + angle;
        prevAngle = prevAngle >= 0 ? prevAngle : 360 + prevAngle;
        float pointMagnitude = Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f)) < Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) ? Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) : Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f));

        if (sign > 0)
        {
            Vector3 m1 = Vector3.Cross(center - roadA.points[^2], Vector3.up).normalized * roadA.width;
            Vector3 m2 = (((roadA.points[^2] - center).normalized + (roadB.points[1] - center).normalized) / 2).normalized * (1f/pointMagnitude);
            Vector3 m3 = Vector3.Cross(roadB.points[1] - center, Vector3.up).normalized * roadB.width;

            Debug.Log($"{center} {m2} {m1} {m3} {(1f/pointMagnitude)}");

            joinKnotA.Position = (Vector3.SignedAngle(roadB.points[1] - center, center - roadA.points[^2], Vector3.up) < 0) ? center + m2 + m1 : center + m2 - m1;
            joinKnotA.Rotation = Quaternion.Euler(0, prevAngle, 0);
            joinKnotA.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
            joinKnotA.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));

            joinKnotB.Position = (Vector3.SignedAngle(roadB.points[1] - center, center - roadA.points[^2], Vector3.up) < 0) ? center + m2 + m3 : center + m2 - m3;
            joinKnotB.Rotation = Quaternion.Euler(0, angle, 0);
            joinKnotB.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
            joinKnotB.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));

            newPoints.Add(center);
            newMap.Add(center, new List<BezierKnot> { joinKnotA, joinKnotB });
        }

        for (int i = 1; i < roadB.points.Count; i++)
        {
            if (newPoints.FindIndex(item => Vector3.Distance(roadB.points[i], item) < roadB.width + 0.5f) == -1)
            {
                newPoints.Add(roadB.points[i]);
                newMap.Add(roadB.points[i], roadB.roadMap[roadB.points[i]].knotCluster);
            }
        }

        for (int i = 0; i < newPoints.Count; i++)
        {
            foreach (BezierKnot kt in newMap[newPoints[i]])
            {
                //Debug.Log($"{newPoints[i]} {kt.Position}");
                newSpline.Add(kt);
            }
        }

        MakeSpline(newSpline, newMap, newPoints, roadA.width, roadA.tex, roadA.ID);

        foreach (Intersection intersect in intersections)
        {
            for (int a = 0; a < intersect.junctions.Count; a++)
            {
                if (intersect.junctions[a].spline == splineA || intersect.junctions[a].spline == splineB)
                {
                    BezierKnot junctKnot = new BezierKnot();
                    foreach (BezierKnot kt in newSpline)
                    {
                        Vector3 junctPos = intersect.junctions[a].knot.Position;
                        Vector3 ktPos = kt.Position;
                        if (Vector3.Distance(junctPos, ktPos) < 0.1f)
                        {
                            junctKnot = kt;
                            break;
                        }
                    }
                    Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(newSpline, junctKnot);
                    intersect.junctions[a] = newInfo;
                }
            }
        }
    }

    private float EvaluateT(Spline e, Vector3 pos)
    {
        m_SplineSampler.SampleSplinePoint(e, pos, (int)(e.GetLength() * 2), out Vector3 nearestPoint, out float knotT);
        return knotT;
    }

    private bool IntersectionsContainRoad(Roads road, Intersection intersection)
    {
        foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
        {
            if (junction.spline == road.road)
            {
                return true;
            }
        }
        return false;
    }

    private bool HitsContainCollider(List<RaycastHit> hitList, RaycastHit hit, Vector3 hitbox)
    {
        foreach (RaycastHit testHit in hitList)
        {
            if (testHit.collider == hit.collider && (Mathf.Abs(testHit.point.x - hit.point.x) <= hitbox.x && Mathf.Abs(testHit.point.z - hit.point.z) <= hitbox.z))
            {
                return true;
            }
        }
        return false;
    }

    public void AddRoad(List<Vector3> points, int width, long ID, Texture2D tex)
    {
        //add a road based on the coordinates of the build mode
        Spline spline = new Spline();
        Dictionary<Vector3, List<BezierKnot>> splinePoints = new();
        foreach (Vector3 point in points)
        {
            float angle; float prevAngle;
            BezierKnot knot = new BezierKnot();
            if (points.IndexOf(point) < points.Count - 1 && points.IndexOf(point) >= 1)
            {
                angle = Vector3.SignedAngle(Vector3.forward, points[points.IndexOf(point) + 1] - point, Vector3.up);
                prevAngle = Vector3.SignedAngle(Vector3.forward, point - points[points.IndexOf(point) - 1], Vector3.up);

                angle = angle >= 0 ? angle : 360 + angle;
                prevAngle = prevAngle >= 0 ? prevAngle : 360 + prevAngle;
                float pointMagnitude = Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f)) < Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) ? Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) : Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f));

                Vector3 m1 = Vector3.Cross(points[points.IndexOf(point) + 1] - point, Vector3.up).normalized * width;
                Vector3 m2 = (((points[points.IndexOf(point) + 1] - point).normalized + (points[points.IndexOf(point) - 1] - point).normalized) / 2).normalized * (1f/pointMagnitude);
                Vector3 m3 = Vector3.Cross(point - points[points.IndexOf(point) - 1], Vector3.up).normalized * width;
                
                //make nicer curves
                BezierKnot knot2 = new BezierKnot();
                knot.Position = (Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, point - points[points.IndexOf(point) - 1], Vector3.up) < 0) ? point + m2 + m3 : point + m2 - m3;
                knot.Rotation = Quaternion.Euler(0, prevAngle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));

                knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
                knot2.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
                knot2.Position = (Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, point - points[points.IndexOf(point) - 1], Vector3.up) < 0) ? point + m2 + m1 : point + m2 - m1;
                knot2.Rotation = Quaternion.Euler(0, angle, 0);

                spline.Add(knot);
                spline.Add(knot2);
                splinePoints.Add(point, new List<BezierKnot> { knot, knot2 });
            }
            else if (points.IndexOf(point) == 0)
            {
                angle = Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, Vector3.forward, Vector3.down);
                knot.Position = point;
                knot.Rotation = Quaternion.Euler(0, angle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -1f);
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, 1f);
                spline.Add(knot);
                splinePoints.Add(point, new List<BezierKnot> { knot });
            }
            else
            {
                angle = Vector3.SignedAngle(point - points[points.IndexOf(point) - 1], Vector3.forward, Vector3.down);
                knot.Position = point;
                knot.Rotation = Quaternion.Euler(0, angle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -1f);
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, 1f);
                spline.Add(knot);
                splinePoints.Add(point, new List<BezierKnot> { knot });
            }
        }

        List<Vector3> pointsRef = points;
        Dictionary<Vector3, List<BezierKnot>> pointMapRef = splinePoints;
        List<RaycastHit> hitList = new();
        List<List<Spline>> roadsList = new();
        List<List<BezierKnot>> knotsList = new();
        List<List<Roads>> roadPairs = new();
        List<Spline> hitRemoveList = new();
        //detects overlap and creates an intersection automatically
        for (int i = 1; i < pointsRef.Count; i++)
        {
            Vector3 p1 = pointsRef[i - 1]; Vector3 p2 = pointsRef[i];
            RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(width, 0.5f, 0.1f), (p2 - p1).normalized, Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, p2 - p1, Vector3.up), 0), Vector3.Distance(p1, p2) + 0.1f, LayerMask.GetMask("Selector"));
            RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(width, 0.5f, 0.1f), (p1 - p2).normalized, Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, p1 - p2, Vector3.up), 0), Vector3.Distance(p1, p2) + 0.1f, LayerMask.GetMask("Selector"));

            List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2);
            foreach (RaycastHit hit in hits)
            {
                //Debug.Log($"{hit.collider.gameObject}, {hit.point}");
                if (!HitsContainCollider(hitList, hit, new Vector3(width * 2, 0.5f, width * 2)) && !(hit.point == Vector3.zero && p1 != Vector3.zero))
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
        }

        foreach (RaycastHit hit in hitList)
        {
            for (int j = 0; j < intersections.Count; j++)
            {
                if (hit.collider == intersections[j].collider)
                {
                    Spline spline2 = new();
                    Dictionary<Vector3, List<BezierKnot>> spline2Points = new();
                    List<Vector3> points2 = new();

                    Vector3 center = new();
                    
                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {                            
                        Vector3 tangent1 = junction.knotIndex == 0 ? Vector3.Normalize(junction.spline.EvaluateTangent(0)) : Vector3.Normalize(junction.spline.EvaluateTangent(1));
                        Unity.Mathematics.float3 hitSplinePoint = new();
                        SplineUtility.GetNearestPoint(spline, (Vector3)junction.knot.Position, out hitSplinePoint, out float tx, (int)((spline.GetLength() * 2)));
                        Vector3 tangent2 = Vector3.Normalize(spline.EvaluateTangent(tx));
                        center += GetPointCenter((Vector3)junction.knot.Position, tangent1, (Vector3)hitSplinePoint, tangent2);
                    }

                    center /= intersections[j].GetJunctions().Count();

                    Vector3 intersectingSplinePoint = transform.TransformPoint(placementSystem.SmoothenPosition(center));

                    //determine the direction to move the splines in
                    float t1 = EvaluateT(spline, intersectingSplinePoint);
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);

                    //add new splines and split the roads (plan)
                    BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                    knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                    knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                    knot1.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                    BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                    knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                    knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                    knot2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                    //insert incoming spline
                    if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) < 1 && EvaluateT(spline, intersectingSplinePoint - (dir1.normalized * (width + 0.5f))) > 0)
                    {
                        FilterPoints(spline, spline2, points, splinePoints, t1, out points2, out spline2Points, out bool splitSpline);
                        if (splitSpline)
                        {
                            spline.Insert(0, knot1); points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                            splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });

                            spline2.Add(knot2); points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                            spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });

                            MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                            for (int filterRoadList = 0; filterRoadList < roadsList.Count; filterRoadList++)
                            {
                                for (int filterRoad = 0; filterRoad < roadsList[filterRoadList].Count; filterRoad++)
                                {
                                    if (roadsList[filterRoadList][filterRoad] == spline)
                                    {
                                        roadsList[filterRoadList][filterRoad] = spline2;
                                    }
                                }
                            }

                            List<Spline> containerSplines = new();
                            foreach (Spline splinesIndex in m_SplineContainer.Splines) { containerSplines.Add(splinesIndex); }
                        }
                        else
                        {
                            Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                            points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                            splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline[0] });
                            m_SplineSampler.SampleSplinePoint(spline, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                            knot1.Position = knot1Pos;
                            knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); spline[0] = knot1;

                            Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline2, t1);
                            points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                            spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[^1] });
                            m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot2.Position = knot2Pos;
                            knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[^1] = knot2;

                            MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                            for (int filterRoadList = 0; filterRoadList < roadsList.Count; filterRoadList++)
                            {
                                for (int filterRoad = 0; filterRoad < roadsList[filterRoadList].Count; filterRoad++)
                                {
                                    if (roadsList[filterRoadList][filterRoad] == spline)
                                    {
                                        roadsList[filterRoadList][filterRoad] = spline2;
                                    }
                                }
                            }
                        }

                        intersections[j].AddJunction(spline, knot1, 0.5f);
                        intersections[j].AddJunction(spline2, knot1, 0.5f);
                    }
                    else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                    {
                        spline.SetKnot(spline.Count - 1, knot2); points[^1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                        splinePoints.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });
                        splinePoints.Remove(intersectingSplinePoint);
                        intersections[j].AddJunction(spline, knot2, 0.5f);
                    }
                    else
                    {
                        spline.SetKnot(0, knot1); points[0] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                        splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });
                        splinePoints.Remove(intersectingSplinePoint);
                        intersections[j].AddJunction(spline, knot1, 0.5f);
                    }

                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {
                        hitRemoveList.Add(junction.spline);
                    }
                }
            }
            for (int j = 0; j < roads.Count; j++)
            {
                if (hit.collider == roads[j].collider)
                {
                    Spline spline2 = new(); Spline spline3 = new(); Roads roadJ = roads[j];
                    Dictionary<Vector3, List<BezierKnot>> spline2Points = new(); Dictionary<Vector3, List<BezierKnot>> spline3Points = new();
                    List<Vector3> points2 = new(); List<Vector3> points3 = new();
                    List<Spline> internalRoadsList = new(); List<BezierKnot> internalKnotsList = new();

                    //determine the spline in which it is intersecting
                    Vector3 thisSplinePoint = new(); Vector3 otherSplinePoint = new();
                    m_SplineSampler.SampleSplinePoint(spline, hit.point, (int)(spline.GetLength() * 2), out thisSplinePoint, out float t1);
                    m_SplineSampler.SampleSplinePoint(roadJ.road, hit.point, roadJ.resolution, out otherSplinePoint, out float t2);

                    //if the roadsJ has been replaced in an earlier intersection, then find the spline it was replaced by.
                    foreach (List<Roads> roadPair in roadPairs)
                    {
                        if (roadPair.Contains(roadJ))
                        {
                            Roads otherRoad = roadPair.IndexOf(roadJ) == 0 ? roadPair[1] : roadPair[0];
                            m_SplineSampler.SampleSplinePoint(otherRoad.road, hit.point, (int)(otherRoad.road.GetLength() * 2), out Vector3 alternativeSplinePoint, out float tAlt);
                            if (Vector3.Distance(thisSplinePoint, alternativeSplinePoint) < Vector3.Distance(thisSplinePoint, otherSplinePoint))
                            {
                                otherSplinePoint = alternativeSplinePoint;
                                t2 = tAlt;
                                roadJ = otherRoad;
                            }
                        }
                    }

                    //determine the direction to move the splines in
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                    Vector3 dir2 = (Vector3)SplineUtility.EvaluateTangent(roadJ.road, t2);
                    Vector3 intersectingSplinePoint = GetPointCenter(thisSplinePoint, dir1.normalized, otherSplinePoint, dir2.normalized); //used to prioritise the existing road coordinates in the creation of an intersection

                    t1 = EvaluateT(spline, intersectingSplinePoint); t2 = EvaluateT(roadJ.road, intersectingSplinePoint);
                    dir1 = SplineUtility.EvaluateTangent(spline, t1);
                    dir2 = SplineUtility.EvaluateTangent(roadJ.road, t2);

                    if (hitRemoveList.FindIndex(item => item == roadJ.road) == -1)
                    {
                        //add new splines and split the roads (plan)
                        BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                        knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                        knot1.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                        knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                        knot2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        BezierKnot knot3 = new BezierKnot(); knot3.Position = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                        knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                        knot3.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);
                        knot3.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        BezierKnot knot4 = new BezierKnot(); knot4.Position = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                        knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                        knot4.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
                        knot4.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) < 1 && EvaluateT(spline, intersectingSplinePoint - (dir1.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(spline, spline2, points, splinePoints, t1, out points2, out spline2Points, out bool splitSpline);
                            if (splitSpline)
                            {
                                spline.Insert(0, knot1); points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                                splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });

                                spline2.Add(knot2); points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                                spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });

                                MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                                for (int filterRoadList = 0; filterRoadList < roadsList.Count; filterRoadList++)
                                {
                                    for (int filterRoad = 0; filterRoad < roadsList[filterRoadList].Count; filterRoad++)
                                    {
                                        if (roadsList[filterRoadList][filterRoad] == spline)
                                        {
                                            roadsList[filterRoadList][filterRoad] = spline2;
                                        }
                                    }
                                }
                                internalRoadsList.Add(spline); internalRoadsList.Add(spline2);
                                internalKnotsList.Add(knot1); internalKnotsList.Add(knot2);
                            }
                            else
                            {
                                Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                                points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                                splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline[0] });
                                m_SplineSampler.SampleSplinePoint(spline, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                                knot1.Position = knot1Pos;
                                knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0);
                                spline[0] = knot1;

                                Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline2, t1);
                                points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                                spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[^1] });
                                m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                                knot2.Position = knot2Pos;
                                knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0);
                                spline2[^1] = knot2;

                                MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                                for (int filterRoadList = 0; filterRoadList < roadsList.Count; filterRoadList++)
                                {
                                    for (int filterRoad = 0; filterRoad < roadsList[filterRoadList].Count; filterRoad++)
                                    {
                                        if (roadsList[filterRoadList][filterRoad] == spline)
                                        {
                                            roadsList[filterRoadList][filterRoad] = spline2;
                                        }
                                    }
                                }
                                internalRoadsList.Add(spline); internalRoadsList.Add(spline2);
                                internalKnotsList.Add(spline[0]); internalKnotsList.Add(spline2[^1]);
                            }
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                        {
                            spline.SetKnot(spline.Count - 1, knot2); points[^1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                            splinePoints.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });
                            splinePoints.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(spline); internalKnotsList.Add(knot2);
                        }
                        else
                        {
                            spline.SetKnot(0, knot1); points[0] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                            splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });
                            splinePoints.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(spline); internalKnotsList.Add(knot1);
                        }

                        //insert overlapping spline
                        if (EvaluateT(roadJ.road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) < 1 && EvaluateT(roadJ.road, intersectingSplinePoint - (dir2.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(roadJ.road, spline3, roadJ.points, roadJ.GetRoadMap(), t2, out points3, out spline3Points, out bool splitSpline);
                            //Debug.Log($"{splitSpline}, {intersectingSplinePoint},");

                            if (splitSpline)
                            {
                                roadJ.road.Insert(0, knot3); roadJ.points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                                roadJ.roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { knot3 }));
                                roadJ.resolution = (int)(roadJ.road.GetLength() * 2);

                                spline3.Add(knot4); points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                                spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot4 });

                                MakeSpline(spline3, spline3Points, points3, width, tex, ID);
                                FilterIntersections(roadJ.road, spline3, t2);
                                for (int filterRoadList = 0; filterRoadList < roadsList.Count; filterRoadList++)
                                {
                                    for (int filterRoad = 0; filterRoad < roadsList[filterRoadList].Count; filterRoad++)
                                    {
                                        if (roadsList[filterRoadList][filterRoad] == roadJ.road)
                                        {
                                            if (!roadJ.road.Contains(knotsList[filterRoadList][filterRoad]) && spline3.Contains(knotsList[filterRoadList][filterRoad]))
                                            {
                                                roadsList[filterRoadList][filterRoad] = spline3;
                                            }
                                        }
                                    }
                                }
                                internalRoadsList.Add(roadJ.road); internalRoadsList.Add(spline3);
                                internalKnotsList.Add(knot3); internalKnotsList.Add(knot4);
                            }
                            else
                            {
                                Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(roadJ.road, t2);
                                roadJ.points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                                roadJ.roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { roadJ.road[0] }));
                                m_SplineSampler.SampleSplinePoint(roadJ.road, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                                knot3.Position = knot1Pos;
                                knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); roadJ.road[0] = knot3;

                                Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline3, t2);
                                points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                                spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { spline3[^1] });
                                m_SplineSampler.SampleSplinePoint(spline3, intersectingSplinePoint - (dirB.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                                knot4.Position = knot2Pos;
                                knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline3[^1] = knot4;

                                MakeSpline(spline3, spline3Points, points3, width, tex, ID);
                                FilterIntersections(roadJ.road, spline3, t2);
                                for (int filterRoadList = 0; filterRoadList < roadsList.Count; filterRoadList++)
                                {
                                    for (int filterRoad = 0; filterRoad < roadsList[filterRoadList].Count; filterRoad++)
                                    {
                                        if (roadsList[filterRoadList][filterRoad] == roadJ.road)
                                        {
                                            if (!roadJ.road.Contains(knotsList[filterRoadList][filterRoad]) && spline3.Contains(knotsList[filterRoadList][filterRoad]))
                                            {
                                                roadsList[filterRoadList][filterRoad] = spline3;
                                            }
                                        }
                                    }
                                }
                                internalRoadsList.Add(roadJ.road); internalRoadsList.Add(spline3);
                                internalKnotsList.Add(roadJ.road[0]); internalKnotsList.Add(spline3[^1]);
                            }

                            roadPairs.Add(new List<Roads> {roadJ, roads.Find(item => item.road == spline3)});
                        }
                        else if (EvaluateT(roadJ.road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) >= 1)
                        {
                            roadJ.road.SetKnot(roadJ.road.Count - 1, knot4); roadJ.points[^1] = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);
                            roadJ.roadMap.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { knot4 }));

                            roadJ.roadMap.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(roadJ.road); internalKnotsList.Add(knot4);
                        }
                        else
                        {
                            roadJ.road.SetKnot(0, knot3); roadJ.points[0] = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);
                            roadJ.roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { knot3 }));

                            roadJ.roadMap.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(roadJ.road); internalKnotsList.Add(knot3);
                        }

                        roadsList.Add(internalRoadsList);
                        knotsList.Add(internalKnotsList);
                        //hitRemoveList.Add(roadJ.road);
                    }
                }
            }
        }

        MakeSpline(spline, splinePoints, points, width, tex, ID);
        for (int k = 0; k < roadsList.Count; k++)
        {
            MakeIntersection(roadsList[k], knotsList[k]);
        }
        CleanRoads();
        CleanIntersections();
        MakeRoad();
    }

    public void ModifyRoad(Roads road, List<Vector3> points, int width, long ID, Texture2D tex)
    {
        Spline spline = road.road;
        //modify a road based on the coordinates of the build mode
        Dictionary<Vector3, List<BezierKnot>> splinePoints = new();
        foreach (Vector3 point in points)
        {
            float angle; float prevAngle;
            BezierKnot knot = new BezierKnot();
            if (points.IndexOf(point) < points.Count - 1 && points.IndexOf(point) >= 1)
            {
                angle = Vector3.SignedAngle(Vector3.forward, points[points.IndexOf(point) + 1] - point, Vector3.up);
                prevAngle = Vector3.SignedAngle(Vector3.forward, point - points[points.IndexOf(point) - 1], Vector3.up);

                angle = angle >= 0 ? angle : 360 + angle;
                prevAngle = prevAngle >= 0 ? prevAngle : 360 + prevAngle;
                float pointMagnitude = Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f)) < Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) ? Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) : Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f));

                Vector3 m1 = Vector3.Cross(points[points.IndexOf(point) + 1] - point, Vector3.up).normalized * width;
                Vector3 m2 = (((points[points.IndexOf(point) + 1] - point).normalized + (points[points.IndexOf(point) - 1] - point).normalized) / 2).normalized * (1f/pointMagnitude);
                Vector3 m3 = Vector3.Cross(point - points[points.IndexOf(point) - 1], Vector3.up).normalized * width;

                //make nicer curves
                BezierKnot knot2 = new BezierKnot();
                knot.Position = (Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, point - points[points.IndexOf(point) - 1], Vector3.up) < 0) ? point + m2 + m3 : point + m2 - m3;
                knot.Rotation = Quaternion.Euler(0, prevAngle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));

                knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
                knot2.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)));
                knot2.Position = (Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, point - points[points.IndexOf(point) - 1], Vector3.up) < 0) ? point + m2 + m1 : point + m2 - m1;
                knot2.Rotation = Quaternion.Euler(0, angle, 0);

                if (road.points.Count > points.IndexOf(point))
                {
                    spline.SetKnot(road.road.IndexOf(road.roadMap[road.points[points.IndexOf(point)]].knotCluster[0]), knot);
                }
                else
                {
                    spline.Add(knot);
                }

                if (road.points.Count > points.IndexOf(point) && road.roadMap[road.points[points.IndexOf(point)]].knotCluster.Count > 1)
                {
                    spline.SetKnot(road.road.IndexOf(road.roadMap[road.points[points.IndexOf(point)]].knotCluster[1]), knot2);
                }
                else
                {
                    spline.Add(knot2);
                }
                splinePoints.Add(point, new List<BezierKnot> { knot, knot2 });
            }
            else if (points.IndexOf(point) == 0)
            {
                angle = Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, Vector3.forward, Vector3.down);
                knot.Position = point;
                knot.Rotation = Quaternion.Euler(0, angle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -1f);
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, 1f);
                if ((points.IndexOf(point) * 2) < road.road.Count)
                {
                    spline.SetKnot(0, knot);
                }
                else
                {
                    spline.Add(knot);
                }
                splinePoints.Add(point, new List<BezierKnot> { knot });
            }
            else
            {
                angle = Vector3.SignedAngle(point - points[points.IndexOf(point) - 1], Vector3.forward, Vector3.down);
                knot.Position = point;
                knot.Rotation = Quaternion.Euler(0, angle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -1f);
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, 1f);
                if ((points.IndexOf(point) * 2) - 1 < road.road.Count)
                {
                    spline.SetKnot(spline.Count - 1, knot);
                }
                else
                {
                    spline.Add(knot);
                }
                splinePoints.Add(point, new List<BezierKnot> { knot });
            }
        }

        foreach (Intersection intersection in intersections)
        {
            List<Intersection.JunctionEdge> junctionEdges = new List<Intersection.JunctionEdge>();
            Vector3 center = new Vector3();
            bool containsThisSpline = false;
            int knotIndex = -1;
            Intersection.JunctionInfo selectedJunction = new Intersection.JunctionInfo();
            foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
            {
                if (junction.spline == spline)
                {
                    containsThisSpline = true;
                    knotIndex = junction.knotIndex;
                    selectedJunction = junction;
                    break;
                }

            }
            if (containsThisSpline)
            {
                foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                {
                    int splineIndex = junction.GetSplineIndex(m_SplineContainer);
                    float t = junction.knotIndex == 0 ? 0f : 1f;
                    m_SplineSampler.SampleSplineWidth(splineIndex, t, roads[splineIndex].width, out Vector3 p1, out Vector3 p2);
                    //if knot index is 0 we are facing away from the junction, otherwise we are facing the junction
                    if (junction.knotIndex == 0)
                    {
                        junctionEdges.Add(new Intersection.JunctionEdge(p1, p2));
                    }
                    else
                    {
                        junctionEdges.Add(new Intersection.JunctionEdge(p2, p1));
                    }

                    center += p1;
                    center += p2;
                }
                center /= (junctionEdges.Count * 2);
                if (Vector3.Distance(center, (Vector3)spline[knotIndex].Position) > width + 3f)
                {
                    intersection.junctions.Remove(selectedJunction);
                }
            }
        }

        List<Vector3> pointsRef = points;
        Dictionary<Vector3, List<BezierKnot>> pointMapRef = splinePoints;
        List<RaycastHit> hitList = new();
        List<List<Spline>> roadsList = new();
        List<List<BezierKnot>> knotsList = new();
        List<List<Roads>> roadPairs = new();
        List<Spline> hitRemoveList = new();
        //detects overlap and creates an intersection automatically
        for (int i = 1; i < pointsRef.Count; i++)
        {
            Vector3 p1 = pointsRef[i - 1]; Vector3 p2 = pointsRef[i];
            RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(width, 0.5f, 0.1f), (p2 - p1).normalized, Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, p2 - p1, Vector3.up), 0), Vector3.Distance(p1, p2) + 0.1f, LayerMask.GetMask("Selector"));
            RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(width, 0.5f, 0.1f), (p1 - p2).normalized, Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, p1 - p2, Vector3.up), 0), Vector3.Distance(p1, p2) + 0.1f, LayerMask.GetMask("Selector"));
            List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2);
            foreach (RaycastHit hit in hits)
            {
                if (!HitsContainCollider(hitList, hit, new Vector3(width * 2, 0.5f, width * 2)) && !(hit.point == Vector3.zero && p1 != Vector3.zero) && hit.collider != road.collider)
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
        }

        foreach (RaycastHit hit in hitList)
        {
            for (int j = 0; j < intersections.Count; j++)
            {
                if (hit.collider == intersections[j].collider)
                {
                    if (!IntersectionsContainRoad(road, intersections[j]))
                    {
                        Spline spline2 = new();
                        Dictionary<Vector3, List<BezierKnot>> spline2Points = new();
                        List<Vector3> points2 = new();

                        /*                     
                        List<Intersection.JunctionEdge> junctionEdges = new List<Intersection.JunctionEdge>();

                        Vector3 center = new Vector3();
                        foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                        {
                            int splineIndex = junction.GetSplineIndex(m_SplineContainer);
                            float t = junction.knotIndex == 0 ? 0f : 1f;
                            m_SplineSampler.SampleSplineWidth(splineIndex, t, roads[splineIndex].width, out Vector3 p1, out Vector3 p2);
                            //if knot index is 0 we are facing away from the junction, otherwise we are facing the junction
                            if (junction.knotIndex == 0)
                            {
                                junctionEdges.Add(new Intersection.JunctionEdge(p1, p2));
                            }
                            else
                            {
                                junctionEdges.Add(new Intersection.JunctionEdge(p2, p1));
                            }

                            center += p1;
                            center += p2;
                        } 
                        */

                        Vector3 center = new();
                        
                        foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                        {                            
                            Vector3 tangent1 = junction.knotIndex == 0 ? Vector3.Normalize(junction.spline.EvaluateTangent(0)) : Vector3.Normalize(junction.spline.EvaluateTangent(1));
                            Unity.Mathematics.float3 hitSplinePoint = new();
                            SplineUtility.GetNearestPoint(spline, (Vector3)junction.knot.Position, out hitSplinePoint, out float tx, (int)((spline.GetLength() * 2)));
                            Vector3 tangent2 = Vector3.Normalize(spline.EvaluateTangent(tx));
                            center += GetPointCenter((Vector3)junction.knot.Position, tangent1, (Vector3)hitSplinePoint, tangent2);
                        }

                        center /= intersections[j].GetJunctions().Count();

                        Vector3 intersectingSplinePoint = transform.TransformPoint(placementSystem.SmoothenPosition(center));

                        //determine the direction to move the splines in
                        float t1 = EvaluateT(spline, intersectingSplinePoint);
                        Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);

                        //add new splines and split the roads (plan)
                        BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                        knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                        knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) < 1 && EvaluateT(spline, intersectingSplinePoint - (dir1.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(spline, spline2, points, splinePoints, t1, out points2, out spline2Points, out bool splitSpline);
                            if (splitSpline)
                            {
                                spline.Insert(0, knot1); points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                                splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });
                                road.resolution = (int)(spline.GetLength() * 2);

                                spline2.Add(knot2); points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                                spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });

                                MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                                FilterIntersections(spline, spline2, t1);
                                List<Spline> containerSplines = new();
                                foreach (Spline splinesIndex in m_SplineContainer.Splines) { containerSplines.Add(splinesIndex); }
                            }
                            else
                            {
                                Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                                points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                                splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline[0] });
                                m_SplineSampler.SampleSplinePoint(spline, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                                knot1.Position = knot1Pos;
                                knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); spline[0] = knot1;

                                Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline2, t1);
                                points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                                spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[^1] });
                                m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                                knot2.Position = knot2Pos;
                                knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[^1] = knot2;

                                MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                                FilterIntersections(spline, spline2, t1);
                                road.resolution = (int)(spline.GetLength() * 2);
                            }

                            intersections[j].AddJunction(spline, knot1, 0.5f);
                            intersections[j].AddJunction(spline2, knot1, 0.5f);
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                        {
                            spline.SetKnot(spline.Count - 1, knot2); points[^1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                            splinePoints.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });
                            splinePoints.Remove(intersectingSplinePoint);
                            intersections[j].AddJunction(spline, knot2, 0.5f);
                        }
                        else
                        {
                            spline.SetKnot(0, knot1); points[0] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                            splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });
                            splinePoints.Remove(intersectingSplinePoint);
                            intersections[j].AddJunction(spline, knot1, 0.5f);
                        }
                    }
                    
                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {
                        hitRemoveList.Add(junction.spline);
                    }
                }
            }

            for (int j = 0; j < roads.Count; j++)
            {
                if (hit.collider == roads[j].collider)
                {
                    Spline spline2 = new(); Spline spline3 = new(); Roads roadJ = roads[j];
                    Dictionary<Vector3, List<BezierKnot>> spline2Points = new(); Dictionary<Vector3, List<BezierKnot>> spline3Points = new();
                    List<Vector3> points2 = new(); List<Vector3> points3 = new();
                    List<Spline> internalRoadsList = new(); List<BezierKnot> internalKnotsList = new();

                    //determine the spline in which it is intersecting
                    Vector3 thisSplinePoint = new(); Vector3 otherSplinePoint = new();
                    m_SplineSampler.SampleSplinePoint(spline, hit.point, (int)(spline.GetLength() * 2), out thisSplinePoint, out float t1);
                    m_SplineSampler.SampleSplinePoint(roadJ.road, hit.point, roadJ.resolution, out otherSplinePoint, out float t2);

                    //if the roadsJ has been replaced in an earlier intersection, then find the spline it was replaced by.
                    foreach (List<Roads> roadPair in roadPairs)
                    {
                        if (roadPair.Contains(roadJ))
                        {
                            Roads otherRoad = roadPair.IndexOf(roadJ) == 0 ? roadPair[1] : roadPair[0];
                            m_SplineSampler.SampleSplinePoint(otherRoad.road, hit.point, (int)(otherRoad.road.GetLength() * 2), out Vector3 alternativeSplinePoint, out float tAlt);
                            if (Vector3.Distance(thisSplinePoint, alternativeSplinePoint) < Vector3.Distance(thisSplinePoint, otherSplinePoint))
                            {
                                otherSplinePoint = alternativeSplinePoint;
                                t2 = tAlt;
                                roadJ = otherRoad;
                            }
                        }
                    }

                    //determine the direction to move the splines in
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                    Vector3 dir2 = (Vector3)SplineUtility.EvaluateTangent(roadJ.road, t2);
                    Vector3 intersectingSplinePoint = GetPointCenter(thisSplinePoint, dir1.normalized, otherSplinePoint, dir2.normalized); //used to prioritise the existing road coordinates in the creation of an intersection

                    t1 = EvaluateT(spline, intersectingSplinePoint); t2 = EvaluateT(roadJ.road, intersectingSplinePoint);
                    dir1 = SplineUtility.EvaluateTangent(spline, t1);
                    dir2 = SplineUtility.EvaluateTangent(roadJ.road, t2);

                    if (hitRemoveList.FindIndex(item => item == roadJ.road) == -1)
                    {
                        //add new splines and split the roads (plan)
                        BezierKnot knot1 = new BezierKnot(); knot1.Position = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                        knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        BezierKnot knot2 = new BezierKnot(); knot2.Position = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                        knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir1.normalized, Vector3.forward, Vector3.down), 0);
                        knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        BezierKnot knot3 = new BezierKnot(); knot3.Position = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                        knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                        knot3.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

                        BezierKnot knot4 = new BezierKnot(); knot4.Position = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                        knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dir2.normalized, Vector3.forward, Vector3.down), 0);
                        knot4.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);

                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) < 1 && EvaluateT(spline, intersectingSplinePoint - (dir1.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(spline, spline2, points, splinePoints, t1, out points2, out spline2Points, out bool splitSpline);
                            if (splitSpline)
                            {
                                spline.Insert(0, knot1); points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                                splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });
                                road.resolution = (int)(spline.GetLength() * 2);

                                spline2.Add(knot2); points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                                spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });

                                MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                                FilterIntersections(spline, spline2, t1);
                                internalRoadsList.Add(spline); internalRoadsList.Add(spline2);
                                internalKnotsList.Add(knot1); internalKnotsList.Add(knot2);
                            }
                            else
                            {
                                Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                                points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                                splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline[0] });
                                m_SplineSampler.SampleSplinePoint(spline, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                                knot1.Position = knot1Pos;
                                knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); spline[0] = knot1;

                                Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline2, t1);
                                points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                                spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[^1] });
                                m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                                knot2.Position = knot2Pos;
                                knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[^1] = knot2;

                                MakeSpline(spline2, spline2Points, points2, width, tex, ID);
                                FilterIntersections(spline, spline2, t1);
                                road.resolution = (int)(spline.GetLength() * 2);
                                internalRoadsList.Add(spline); internalRoadsList.Add(spline2);
                                internalKnotsList.Add(spline[0]); internalKnotsList.Add(spline2[^1]);
                            }
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                        {
                            spline.SetKnot(spline.Count - 1, knot2); points[^1] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                            splinePoints.Add(intersectingSplinePoint + (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot2 });
                            splinePoints.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(spline); internalKnotsList.Add(knot2);
                        }
                        else
                        {
                            spline.SetKnot(0, knot1); points[0] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                            splinePoints.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { knot1 });
                            splinePoints.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(spline); internalKnotsList.Add(knot1);
                        }

                        //insert overlapping spline
                        if (EvaluateT(roadJ.road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) < 1 && EvaluateT(roadJ.road, intersectingSplinePoint - (dir2.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(roadJ.road, spline3, roadJ.points, roadJ.GetRoadMap(), t2, out points3, out spline3Points, out bool splitSpline);

                            if (splitSpline)
                            {

                                roadJ.road.Insert(0, knot3); roadJ.points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                                roadJ.roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { knot3 }));
                                roadJ.resolution = (int)(roadJ.road.GetLength() * 2);

                                spline3.Add(knot4); points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                                spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot4 });

                                MakeSpline(spline3, spline3Points, points3, width, tex, ID);
                                FilterIntersections(roadJ.road, spline3, t2);
                                internalRoadsList.Add(roadJ.road); internalRoadsList.Add(spline3);
                                internalKnotsList.Add(knot3); internalKnotsList.Add(knot4);
                            }
                            else
                            {
                                Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(roadJ.road, t2);
                                roadJ.points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                                roadJ.roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { roadJ.road[0] }));
                                m_SplineSampler.SampleSplinePoint(roadJ.road, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                                knot3.Position = knot1Pos;
                                knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); roadJ.road[0] = knot3;

                                Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline3, t2);
                                points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                                spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { spline3[^1] });
                                m_SplineSampler.SampleSplinePoint(spline3, intersectingSplinePoint - (dirB.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                                knot4.Position = knot2Pos;
                                knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline3[^1] = knot4;

                                MakeSpline(spline3, spline3Points, points3, width, tex, ID);
                                FilterIntersections(roadJ.road, spline3, t2);
                                internalRoadsList.Add(roadJ.road); internalRoadsList.Add(spline3);
                                internalKnotsList.Add(roadJ.road[0]); internalKnotsList.Add(spline3[^1]);
                            }

                            roadPairs.Add(new List<Roads> {roadJ, roads.Find(item => item.road == spline3)});
                        }
                        else if (EvaluateT(roadJ.road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) >= 1)
                        {
                            roadJ.road.SetKnot(roadJ.road.Count - 1, knot4); roadJ.points[^1] = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);
                            roadJ.roadMap.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { knot4 }));
                            roadJ.roadMap.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(roadJ.road); internalKnotsList.Add(knot4);
                        }
                        else
                        {
                            roadJ.road.SetKnot(0, knot3); roadJ.points[0] = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);
                            roadJ.roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new KnotClusterWrapper(new List<BezierKnot> { knot3 }));
                            roadJ.roadMap.Remove(intersectingSplinePoint);
                            internalRoadsList.Add(roadJ.road); internalKnotsList.Add(knot3);
                        }
                    }

                    roadsList.Add(internalRoadsList);
                    knotsList.Add(internalKnotsList);
                    //hitRemoveList.Add(roadJ.road);
                }
            }
        }

        road.points = points;
        road.roadMap = road.ConvertToSerializedRoadMap(splinePoints);
        road.width = width;
        road.ID = ID;
        road.resolution = (int)(spline.GetLength() * 2);

        for (int k = 0; k < roadsList.Count; k++)
        {
            MakeIntersection(roadsList[k], knotsList[k]);
        }
        CleanRoads();
        CleanIntersections();
        MakeRoad();
    }

    private void MakeSpline(Spline spline, Dictionary<Vector3, List<BezierKnot>> splinePoints, List<Vector3> points, int width, Texture2D tex, long ID)
    {
        //if the spline is valid, then spawn it
        if (Vector3.Distance(points[0], points[^1]) > 0.01f)
        {
            m_SplineContainer.AddSpline(spline);
            GameObject c = new();
            c.name = $"Spline{roads.Count}";
            c.transform.SetParent(this.transform);
            c.AddComponent<MeshCollider>();
            c.AddComponent<MeshFilter>();
            c.AddComponent<MeshRenderer>();
            c.layer = LayerMask.NameToLayer("Selector");
            Material material = Instantiate(roadMat);
            roads.Add(new Roads(spline, splinePoints, points, (int)(spline.GetLength() * 2), width, c.GetComponent<MeshCollider>(), c.GetComponent<MeshRenderer>(), c.GetComponent<MeshFilter>(), material, tex, ID));
        }
        //or else, undo the creation of intersections with the spline
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

    private void MakeSpline(Roads road)
    {
        if (Vector3.Distance(road.points[0], road.points[^1]) > 0.01f)
        {
            m_SplineContainer.AddSpline(road.road);
            GameObject c = new();
            c.name = $"Spline{roads.Count}";
            c.transform.SetParent(this.transform);
            c.AddComponent<MeshCollider>();
            c.AddComponent<MeshFilter>();
            c.AddComponent<MeshRenderer>();
            c.layer = LayerMask.NameToLayer("Selector");
            road.collider = c.GetComponent<MeshCollider>();
            road.mesh = c.GetComponent<MeshFilter>();
            road.renderer = c.GetComponent<MeshRenderer>();
            roads.Add(road);
        }
        else
        {
            foreach (Intersection intersection in intersections)
            {
                List<Intersection.JunctionInfo> removeJunctions = new();
                foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                {
                    if (junction.spline == road.road)
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
        for (int i = 0; i < splines.Count; i++)
        {
            if (CheckPointsEqual(splines[i], out Spline equalSpline))
            {
                splines[i] = equalSpline;
            }
            else
            {
                //Debug.Log("No possible splines found!");
                break;
            }
        }

        for (int c = 0; c < splines.Count; c++)
        {
            BezierKnot knot = knots[c];
            Spline spline = splines[c];
            intersection.AddJunction(spline, knot, 0.5f);
        }

        GameObject collider = new();
        collider.name = $"Intersection{intersections.Count}";
        collider.transform.SetParent(this.transform);
        collider.AddComponent<MeshCollider>();
        collider.AddComponent<MeshFilter>();
        collider.AddComponent<MeshRenderer>();
        collider.layer = LayerMask.NameToLayer("Selector");

        Material mat = Instantiate(intersectMat);
        collider.GetComponent<MeshRenderer>().material = mat;
        AddJunction(intersection, collider.GetComponent<MeshCollider>(), collider.GetComponent<MeshFilter>(), collider.GetComponent<MeshRenderer>());
    }

    public void SelectRoad(Vector3 position, Vector2Int size, float rotation, out Roads selectedRoad, out int index, out int width, out long ID, out List<Vector3> points)
    {
        Collider[] overlaps = Physics.OverlapBox(position, new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, 5, 0), LayerMask.GetMask("Selector"));
        selectedRoad = null; index = -1; width = 0; ID = -1; points = new();
        foreach (Roads road in roads)
        {
            Collider selector = road.collider;
            foreach (Collider hit in overlaps)
            {
                if (hit == selector)
                {
                    selectedRoad = road;
                    index = roads.IndexOf(road);
                    width = road.width;
                    ID = road.ID;
                    points = road.points;
                }
            }
        }
    }

    private void DeleteRoad(Roads road)
    {
        foreach (Intersection intersection in intersections)
        {
            List<Intersection.JunctionInfo> removeJunctions = new();
            foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
            {
                if (junction.spline == road.road)
                {
                    removeJunctions.Add(junction);
                }
            }
            foreach (Intersection.JunctionInfo junction in removeJunctions)
            {
                intersection.junctions.Remove(junction);
            }
        }
        m_SplineContainer.RemoveSpline(road.road);
        Destroy(road.collider.gameObject);
        roads.Remove(road);
    }

    public void RemoveRoad(Roads road)
    {
        DeleteRoad(road);
        CleanIntersections();
        MakeRoad();
    }

    public bool CheckRoadSelect(Vector3 position, Vector2Int size, float rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x + 0.05f, position.y, position.z + 0.05f), new Vector3(size.x/2f - 0.1f, 0.5f, size.y/2f - 0.1f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector") );
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (roads.FindIndex(road => overlapsList.Contains(road.collider)) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckRoadSelect(GameObject previewSelector)
    {
        Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (roads.FindIndex(road => overlapsList.Contains(road.collider)) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckRoadSelect(InputManager input)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (roads.FindIndex(road => overlapsList.FindIndex(col => col.collider == road.collider) != -1) != -1)
        {
            return true;
        }
        return false;
    }

    private bool CheckPointsEqual(Spline spline, out Spline equalSpline)
    {
        equalSpline = null;
        if (roads.FindIndex(item => item.road == spline) == -1)
        {
            List<Roads> possibleRoads = roads.FindAll(item => (Vector3)item.road[0].Position == (Vector3)spline[0].Position && item.road.Count == spline.Count);

            if (possibleRoads.Count > 0)
            {
                for (int i = 0; i < possibleRoads.Count; i++)
                {
                    for (int j = 1; j < spline.Count; j++)
                    {
                        if ((Vector3)possibleRoads[i].road[j].Position != (Vector3)spline[j].Position)
                        {
                            break;
                        }

                        if (j == spline.Count - 1)
                        {
                            equalSpline = possibleRoads[i].road;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        equalSpline = spline;
        return true;
    }

    private Vector3 GetPointCenter(Vector3 origin1, Vector3 tangent1, Vector3 origin2, Vector3 tangent2)
    {
        float angleA = Vector3.SignedAngle(tangent2, tangent1, Vector3.down);
        float angleB = Vector3.SignedAngle(origin1 - origin2, tangent1, Vector3.down);
        float angleC = Vector3.SignedAngle(origin2 - origin1, tangent2, Vector3.up);
        float sign1 = 1; float sign2 = 1;

        if (Mathf.Sin(angleA * (Mathf.PI / 180)) == 0)
        {
            angleA = 180; sign1 = -1; sign2 = -1;
        }

        float t1 = Vector3.Distance(origin1, origin2) * Mathf.Sin(angleC * (Mathf.PI / 180)) / Mathf.Sin(angleA * (Mathf.PI / 180)) * sign1;
        float t2 = Vector3.Distance(origin1, origin2) * Mathf.Sin(angleB * (Mathf.PI / 180)) / Mathf.Sin(angleA * (Mathf.PI / 180)) * sign2;

        Vector3 p1 = placementSystem.SmoothenPosition(origin1 + (t1 * tangent1));
        Vector3 p2 = placementSystem.SmoothenPosition(origin2 + (t2 * tangent2));

        if (Vector3.Distance(p1, p2) < 0.1f)
        {
            return p1;
        }
        else
        {
            return (p1 + p2) / 2;
        }
    }

    public RoadMapSaveData GetRoadMapSaveData()
    {
        return new RoadMapSaveData(roads, intersections);
    }

    public void LoadSaveData(RoadMapSaveData data)
    {
        for (int i = 0; i < data.roads.Count; i++)
        {
            if (!roads.Contains(data.roads[i]))
            {
                MakeSpline(data.roads[i]);
            }
        }

        for (int i = 0; i < data.intersections.Count; i++)
        {
            if (!intersections.Contains(data.intersections[i]))
            {
                List<Spline> splines = new(); List<BezierKnot> knots = new();

                for (int j = 0; j < data.intersections[i].junctions.Count; j++)
                {
                    splines.Add(data.intersections[i].junctions[j].spline);
                    knots.Add(data.intersections[i].junctions[j].knot);
                }

                MakeIntersection(splines, knots);
            }
        }

        MakeRoad();
    }
}

[System.Serializable]
public class Roads
{
    public Spline road;
    public SerializedDictionary<Vector3, KnotClusterWrapper> roadMap;
    public List<Vector3> points;
    public int resolution;
    public int width;
    public long ID;
    public MeshCollider collider;
    public MeshFilter mesh;
    public MeshRenderer renderer;
    public Texture2D tex;

    public Roads(Spline road, Dictionary<Vector3, List<BezierKnot>> roadMap, List<Vector3> points, int resolution, int width, MeshCollider collider, MeshRenderer renderer, MeshFilter mesh, Material mat, Texture2D tex, long ID)
    {
        this.road = road;

        this.roadMap = ConvertToSerializedRoadMap(roadMap);

        this.points = points;
        this.resolution = resolution;
        this.width = width;
        this.collider = collider;
        this.ID = ID;
        this.renderer = renderer;
        this.mesh = mesh;

        mat.SetTexture("_roadTex", tex);
        renderer.material = mat;
        this.tex = tex;
    }

    public Dictionary<Vector3, List<BezierKnot>> GetRoadMap()
    {
        Dictionary<Vector3, List<BezierKnot>> returnedRoadMap = new Dictionary<Vector3, List<BezierKnot>>();

        foreach (Vector3 pos in this.roadMap.Keys)
        {
            returnedRoadMap[pos] = roadMap[pos].knotCluster;
        }

        return returnedRoadMap;
    }

    public SerializedDictionary<Vector3, KnotClusterWrapper> ConvertToSerializedRoadMap(Dictionary<Vector3, List<BezierKnot>> roadMap)
    {
        SerializedDictionary<Vector3, KnotClusterWrapper> convertedMap = new SerializedDictionary<Vector3, KnotClusterWrapper>();
        foreach (Vector3 pos in roadMap.Keys)
        {
            convertedMap[pos] = new KnotClusterWrapper(roadMap[pos]);
        }
        return convertedMap;
    }
}

[System.Serializable]
public class RoadMapSaveData
{
    public List<Roads> roads;
    public List<Intersection> intersections;

    public RoadMapSaveData(List<Roads> roads, List<Intersection> intersections)
    {
        this.roads = roads;
        this.intersections = intersections;
    }
}

[System.Serializable]
public class KnotClusterWrapper
{
    public List<BezierKnot> knotCluster;

    public KnotClusterWrapper(List<BezierKnot> knotCluster)
    {
        this.knotCluster = knotCluster;
    }
}