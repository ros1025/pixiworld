using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
using UnityEngine.Splines;
using UnityEngine.Splines.ExtrusionShapes;

public class RoadMapping : MonoBehaviour
{
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
        BuildMesh();
    }

    public void AddJunction(Intersection intersection, MeshCollider collider, MeshFilter mesh, MeshRenderer renderer)
    {
        intersections.Add(intersection);
        intersection.collider = collider;
        intersection.mesh = mesh;
        intersection.renderer = renderer;
        BuildIntersectionMesh(intersections.IndexOf(intersection));
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
            intersection.curves[j - 1] = 0.5f + 
                                        (0.1f * Mathf.Sin(Vector3.SignedAngle(junctionEdges[j - 1].center - center, ((j < junctionEdges.Count) ? junctionEdges[j].center : junctionEdges[0].center) - center, Vector3.down)));
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
            contactTris.Add(((j - 1) * 6) + 4); contactTris.Add(((j - 1) * 6) + 2); contactTris.Add(((j - 1) * 6) + 0);
            contactTris.Add(((j - 1) * 6) + 1); contactTris.Add(((j - 1) * 6) + 3); contactTris.Add(((j - 1) * 6) + 5);
            contactTris.Add(((j - 1) * 6) + 5); contactTris.Add(((j - 1) * 6) + 3); contactTris.Add(((j - 1) * 6) + 2);
            contactTris.Add(((j - 1) * 6) + 2); contactTris.Add(((j - 1) * 6) + 4); contactTris.Add(((j - 1) * 6) + 5);

            tris.Add(((j - 1) * 6) + 4); tris.Add(((j - 1) * 6) + 2); tris.Add(((j - 1) * 6) + 0);
            tris.Add(((j - 1) * 6) + 1); tris.Add(((j - 1) * 6) + 3); tris.Add(((j - 1) * 6) + 5);
            tris.Add(((j - 1) * 6) + 5); tris.Add(((j - 1) * 6) + 3); tris.Add(((j - 1) * 6) + 2);
            tris.Add(((j - 1) * 6) + 2); tris.Add(((j - 1) * 6) + 4); tris.Add(((j - 1) * 6) + 5);
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
    private void FilterPoints(Spline road, Spline road2, List<Vector3> points, Dictionary<Vector3, List<BezierKnot>> pointMap, float ratio, float width, out List<Vector3> points2, out Dictionary<Vector3, List<BezierKnot>> pointMap2, out bool splitSpline)
    {
        List<BezierKnot> knotList = new(); List<BezierKnot> knotList2 = new();
        List<Vector3> removePoints = new();
        points2 = new(); pointMap2 = new();
        splitSpline = true;
        float widthT = width / road.GetLength();

        foreach (Vector3 point in points)
        {
            List<bool> isAhead = new();
            foreach (BezierKnot roadKnot in pointMap[point])
            {
                int j = road.IndexOf(roadKnot);
                float knotT = road.ConvertIndexUnit(j, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                if (knotT > ratio + widthT)
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

    private void FilterPoints(Spline road, Spline road2, List<Vector3> points, float ratio, int width, out List<Vector3> points2, out Vector3 dir1, out Vector3 dir2)
    {
        //List<BezierKnot> knotList = new(); List<BezierKnot> knotList2 = new();
        List<Vector3> removePoints = new();
        points2 = new();
        float widthT = width / road.GetLength();
        Vector3 selectedPoint = road.EvaluatePosition(ratio);

        foreach (Vector3 point in points)
        {
            float knotT = EvaluateT(road, point);
            if (Vector3.Distance(point, selectedPoint) < width + 0.1f)
            {
                removePoints.Add(point);
            }
            else if (knotT <= ratio + widthT)
            {
                removePoints.Add(point);
                if (!points2.Contains(point))
                {
                    points2.Add(point);
                }
            }
        }
        foreach (Vector3 point in removePoints) { points.Remove(point); }

        List<BezierKnot> knotList = BuildKnots(points, width);
        List<BezierKnot> knotList2 = BuildKnots(points2, width);
        road.Clear(); road2.Clear();
    
        foreach (BezierKnot roadKnot in knotList) { road.Add(roadKnot); }
        foreach (BezierKnot roadKnot in knotList2) { road2.Add(roadKnot); }

        if (points.Count >= 1)
            dir1 = points[0] - selectedPoint;
        else dir1 = Vector3.forward;

        if (points2.Count >= 1)
            dir2 = selectedPoint - points2[^1];
        else dir2 = Vector3.forward;
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
                    int knotIndex = intersect.junctions[a].knotIndex == 0 ? 0 : r.Count - 1;
                    float rT = EvaluateT(combinedSpline, intersect.junctions[a].knot.Position);
                    if (rT < sT)
                    {
                        Intersection.JunctionInfo jinfo = new Intersection.JunctionInfo(r, r[knotIndex]);
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
                        
                        for (int knotIndex = 0; knotIndex < splineB.Count; knotIndex++)
                        {
                            BezierKnot knot = splineB[knotIndex];
                            Quaternion newRotationQuaternion = knot.Rotation;
                            Vector3 newRotation = newRotationQuaternion.eulerAngles;
                            knot.Rotation = Quaternion.Euler(newRotation.x, newRotation.y - 180, newRotation.z);
                            splineB[knotIndex] = knot;
                        }
                        indexB = splineB.Count;
                    }

                    else if (indexA > 0 && indexB > 0)
                    {
                        SplineUtility.ReverseFlow(splineB);
                        roadB.points.Reverse();
                        for (int knotIndex = 0; knotIndex < splineB.Count; knotIndex++)
                        {
                            BezierKnot knot = splineB[knotIndex];
                            Quaternion newRotationQuaternion = knot.Rotation;
                            Vector3 newRotation = newRotationQuaternion.eulerAngles;
                            knot.Rotation = Quaternion.Euler(newRotation.x, newRotation.y - 180, newRotation.z);
                            splineB[knotIndex] = knot;
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

        for (int i = 0; i < roadA.points.Count - 1; i++)
        {
            newPoints.Add(roadA.points[i]);
        }

        Vector3 center = placementSystem.SmoothenPosition(GetPointCenter((Vector3)roadA.points[^1], (roadA.points[^2] - roadA.points[^1]).normalized, (Vector3)roadB.points[0], (roadB.points[1] - roadB.points[0]).normalized));
        //Debug.Log(center);

        float prevAngle = Vector3.SignedAngle(Vector3.forward, center - roadA.points[^2], Vector3.up);
        float angle = Vector3.SignedAngle(Vector3.forward, roadB.points[1] - center, Vector3.up);
        float sign = Mathf.Abs(angle - prevAngle);

        angle = angle >= 0 ? angle : 360 + angle;
        prevAngle = prevAngle >= 0 ? prevAngle : 360 + prevAngle;
        float pointMagnitude = Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f)) < Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) ? Mathf.Abs(Mathf.Sin((angle - prevAngle) / 2f * Mathf.PI / 180f)) : Mathf.Abs(Mathf.Cos((angle - prevAngle) / 2f * Mathf.PI / 180f));

        if (sign > 0)
        {
            newPoints.Add(center);
        }

        for (int i = 1; i < roadB.points.Count; i++)
        {
            if (newPoints.FindIndex(item => Vector3.Distance(roadB.points[i], item) < roadB.width + 0.5f) == -1)
            {
                newPoints.Add(roadB.points[i]);
            }
        }

        List<BezierKnot> knots = BuildKnots(newPoints, roadB.width);
        foreach (BezierKnot kt in knots)
        {
            //Debug.Log($"{newPoints[i]} {kt.Position}");
            newSpline.Add(kt);
        }

        MakeSpline(newSpline, newPoints, roadA.width, roadA.tex, roadA.ID);

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

    public Vector3 CalculateIntersectionCenter(Spline spline, Intersection intersection)
    {
        Vector3 center = new();
        
        foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
        {                            
            Vector3 tangent1 = junction.knotIndex == 0 ? Vector3.Normalize(junction.spline.EvaluateTangent(0)) : Vector3.Normalize(junction.spline.EvaluateTangent(1));
            Unity.Mathematics.float3 hitSplinePoint = new();
            SplineUtility.GetNearestPoint(spline, (Vector3)junction.knot.Position, out hitSplinePoint, out float tx, (int)((spline.GetLength() * 2)));
            Vector3 tangent2 = Vector3.Normalize(spline.EvaluateTangent(tx));
            center += GetPointCenter((Vector3)junction.knot.Position, tangent1, (Vector3)hitSplinePoint, tangent2);
        }

        center /= intersection.GetJunctions().Count();

        return transform.TransformPoint(placementSystem.SmoothenPosition(center));
    }

    public Vector3 CalculateIntersectionCenter(Intersection intersection)
    {
        Vector3 center = new();
        
        for (int i = 1; i < intersection.junctions.Count(); i++)
        {                            
            Vector3 tangent1 = intersection.junctions[i].knotIndex == 0 ? -Vector3.Normalize(intersection.junctions[i].spline.EvaluateTangent(0)) : Vector3.Normalize(intersection.junctions[i].spline.EvaluateTangent(1));
            Vector3 tangent2 = intersection.junctions[0].knotIndex == 0 ? -Vector3.Normalize(intersection.junctions[0].spline.EvaluateTangent(0)) : Vector3.Normalize(intersection.junctions[0].spline.EvaluateTangent(1));
            Vector3 localCenter = GetPointCenter((Vector3)intersection.junctions[i].knot.Position, tangent1, (Vector3)intersection.junctions[0].knot.Position, tangent2);
            Debug.Log($"{(Vector3)intersection.junctions[i].knot.Position}:{tangent1} {(Vector3)intersection.junctions[0].knot.Position}:{tangent2} || Center: {localCenter}");
            center += localCenter;
        }

        center /= (intersection.GetJunctions().Count() - 1);

        return transform.TransformPoint(placementSystem.SmoothenPosition(center));
    }

    public void CreateIntersection(List<int> intersectionBuild, List<Vector3> points, /*Dictionary<Vector3, List<BezierKnot>> splinePoints*/ int width, Spline spline, List<List<Spline>> roadsList, List<List<BezierKnot>> knotsList, Texture2D tex, long ID, Roads road, bool isEdit)
    {
        List<Vector3> pointsRef = points;
        List<RaycastHit> hitList = new();
        List<List<Roads>> roadPairs = new();
        List<(Spline, Vector3, Vector3)> hitRemoveList = new();

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
                if (!HitsContainCollider(hitList, hit, new Vector3(width * 2, 0.5f, width * 2)) && !(hit.point == Vector3.zero && p1 != Vector3.zero) && !(isEdit && hit.collider == road.collider))
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
                    //Dictionary<Vector3, List<BezierKnot>> spline2Points = new();
                    List<Vector3> points2 = new();

                    Vector3 intersectingSplinePoint = CalculateIntersectionCenter(intersections[j]);

                    //determine the direction to move the splines in
                    float t1 = EvaluateT(spline, intersectingSplinePoint);
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);

                    if (intersections[j].junctions.FindIndex(item => item.spline == spline) == -1)
                    {
                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) < 1 && EvaluateT(spline, intersectingSplinePoint - (dir1.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(spline, spline2, points, t1, width, out points2, out dir1, out Vector3 dir2);
                            
                            points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                            points2.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));

                            RebuildRoadKnots(points, spline, width);
                            RebuildRoadKnots(points2, spline2, width);
                            MakeSpline(spline2, points2, width, tex, ID);
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

                            intersections[j].AddJunction(spline, spline[0], 0.5f);
                            intersections[j].AddJunction(spline2, spline[^1], 0.5f);
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                        {
                            points[^1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                            RebuildRoadKnots(points, spline, width);
                            intersections[j].AddJunction(spline, spline[^1], 0.5f);
                        }
                        else
                        {
                            points[0] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                            RebuildRoadKnots(points, spline, width);
                            intersections[j].AddJunction(spline, spline[0], 0.5f);
                        }

                        intersectionBuild.Add(j);
                    }

                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {
                        hitRemoveList.Add((junction.spline, junction.knot.Position, ((Quaternion)junction.knot.Rotation) * new Vector3(width + 0.5f, 0.1f, 0.5f)));
                        Debug.Log(((Quaternion)junction.knot.Rotation) * new Vector3(width + 0.5f, 0.1f, 1.5f));
                    }
                }
                else if (intersections[j].junctions.FindIndex(item => item.spline == spline) != -1)
                {
                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {
                        hitRemoveList.Add((junction.spline, junction.knot.Position, ((Quaternion)junction.knot.Rotation) * new Vector3(width + 0.5f, 0.1f, 1.5f)));
                    }

                    intersectionBuild.Add(j);
                }
            }
            for (int j = 0; j < roads.Count; j++)
            {
                if (hit.collider == roads[j].collider)
                {
                    Spline spline2 = new(); Spline spline3 = new(); Roads roadJ = roads[j];
                    //Dictionary<Vector3, List<BezierKnot>> spline2Points = new(); Dictionary<Vector3, List<BezierKnot>> spline3Points = new();
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
                    Vector3 intersectingSplinePoint = placementSystem.SmoothenPosition(GetPointCenter(thisSplinePoint, dir1.normalized, otherSplinePoint, dir2.normalized)); //used to prioritise the existing road coordinates in the creation of an intersection

                    t1 = EvaluateT(spline, intersectingSplinePoint); t2 = EvaluateT(roadJ.road, intersectingSplinePoint);
                    dir1 = SplineUtility.EvaluateTangent(spline, t1);
                    dir2 = SplineUtility.EvaluateTangent(roadJ.road, t2);

                    if (hitRemoveList.FindIndex(item => item.Item1 == roadJ.road && (
                        (Mathf.Abs(intersectingSplinePoint.x - item.Item2.x) < Mathf.Abs(item.Item3.x) && Mathf.Abs(intersectingSplinePoint.y - item.Item2.y) < Mathf.Abs(item.Item3.y) && Mathf.Abs(intersectingSplinePoint.z - item.Item2.z) < Mathf.Abs(item.Item3.z)) 
                        || (Mathf.Abs(hit.point.x - item.Item2.x) < Mathf.Abs(item.Item3.x) && Mathf.Abs(hit.point.y - item.Item2.y) < Mathf.Abs(item.Item3.y) && Mathf.Abs(hit.point.z - item.Item2.z) < Mathf.Abs(item.Item3.z))
                    )) == -1)
                    {
                        Debug.Log($"{roads.IndexOf(roadJ)} {hit.point} {intersectingSplinePoint}");

                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) < 1 && EvaluateT(spline, intersectingSplinePoint - (dir1.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(spline, spline2, points, t1, width, out points2, out dir1, out Vector3 dir3);

                            points.Insert(0, intersectingSplinePoint + (dir1.normalized * (width + 0.5f)));
                            points2.Add(intersectingSplinePoint - (dir3.normalized * (width + 0.5f)));

                            RebuildRoadKnots(points, spline, width);
                            RebuildRoadKnots(points2, spline2, width);
                            MakeSpline(spline2, points2, width, tex, ID);
                            FilterIntersections(spline, spline2, t1);
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
                        else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                        {
                            points[^1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
                            RebuildRoadKnots(points, spline, width);
                            internalRoadsList.Add(spline); internalKnotsList.Add(spline[^1]);
                        }
                        else
                        {
                            points[0] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
                            RebuildRoadKnots(points, spline, width);
                            internalRoadsList.Add(spline); internalKnotsList.Add(spline[0]);
                        }

                        //insert overlapping spline
                        if (EvaluateT(roadJ.road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) < 1 && EvaluateT(roadJ.road, intersectingSplinePoint - (dir2.normalized * (width + 0.5f))) > 0)
                        {
                            FilterPoints(roadJ.road, spline3, roadJ.points, t2, width, out points3, out dir2, out Vector3 dir4);

                            roadJ.points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);

                            points3.Add(intersectingSplinePoint - (dir4.normalized * (width + 0.5f)));

                            RebuildRoadKnots(roadJ.points, roadJ.road, width);
                            RebuildRoadKnots(points3, spline3, width);
                            MakeSpline(spline3, points3, width, tex, ID);
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
                            roadPairs.Add(new List<Roads> {roadJ, roads.Find(item => item.road == spline3)});

                            hitRemoveList.Add((spline3, intersectingSplinePoint, (Quaternion)spline3[^1].Rotation * new Vector3(width + 1.5f, 0.1f, width + 1.5f)));
                            hitRemoveList.Add((spline3, points3[^1], (Quaternion)spline3[^1].Rotation * new Vector3(width + 0.5f, 0.1f, 1.5f)));
                        }
                        else if (EvaluateT(roadJ.road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) >= 1)
                        {
                            roadJ.points[^1] = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);
                            RebuildRoadKnots(roadJ.points, roadJ.road, width);
                            internalRoadsList.Add(roadJ.road); internalKnotsList.Add(roadJ.road[^1]);
                        }
                        else
                        {
                            roadJ.points[0] = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                            roadJ.resolution = (int)(roadJ.road.GetLength() * 2);
                            RebuildRoadKnots(roadJ.points, roadJ.road, width);
                            internalRoadsList.Add(roadJ.road); internalKnotsList.Add(roadJ.road[0]);
                        }

                        roadsList.Add(internalRoadsList);
                        knotsList.Add(internalKnotsList);
                        hitRemoveList.Add((roadJ.road, intersectingSplinePoint, (Quaternion)roadJ.road[^1].Rotation * new Vector3(width + 1.5f, 0.1f, width + 1.5f)));
                        hitRemoveList.Add((roadJ.road, roadJ.points[0], (Quaternion)roadJ.road[^1].Rotation * new Vector3(width + 0.5f, 0.1f, 1.5f)));
                        Debug.Log(roadJ.points[0] + (Quaternion)roadJ.road[^1].Rotation * new Vector3(width + 0.5f, 0.1f, 0.5f));

                        BuildRoadMesh(roads.IndexOf(roadJ));
                    }
                }
            }
        }
    }

    public List<BezierKnot> BuildKnots(List<Vector3> points, int width)
    {
        List<BezierKnot> knots = new();

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

                knots.Add(knot);
                knots.Add(knot2);
            }
            else if (points.IndexOf(point) == 0)
            {
                if (points.Count > 1)
                {
                    angle = Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, Vector3.forward, Vector3.down);
                }
                else
                {
                    angle = 0;
                }
                knot.Position = point;
                knot.Rotation = Quaternion.Euler(0, angle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -1f);
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, 1f);
                knots.Add(knot);
            }
            else
            {
                angle = Vector3.SignedAngle(point - points[points.IndexOf(point) - 1], Vector3.forward, Vector3.down);
                knot.Position = point;
                knot.Rotation = Quaternion.Euler(0, angle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -1f);
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, 1f);
                knots.Add(knot);
            }
        }

        return knots;
    }

    public void RebuildRoadKnots(List<Vector3> points, Spline spline, int width)
    {
        List<BezierKnot> knots = BuildKnots(points, width);
        spline.Clear();
        spline.AddRange(knots);
    }

    public void AddRoad(List<Vector3> points, int width, long ID, Texture2D tex)
    {
        //add a road based on the coordinates of the build mode
        Spline spline = new Spline();
        List<BezierKnot> knots = BuildKnots(points, width);

        foreach (BezierKnot knot in knots)
        {
            spline.Add(knot);
        }

        List<List<Spline>> roadsList = new();
        List<List<BezierKnot>> knotsList = new();
        List<int> intersectionBuild = new();
        CreateIntersection(intersectionBuild, points, width, spline, roadsList, knotsList, tex, ID, null, false);

        MakeSpline(spline, points, width, tex, ID);

        for (int k = 0; k < roadsList.Count; k++)
        {
            MakeIntersection(roadsList[k], knotsList[k]);
        }
        foreach (int index in intersectionBuild)
        {
            BuildIntersectionMesh(index);
        }

        CleanRoads();
        CleanIntersections();
    }

    public void ModifyRoad(Roads road, List<Vector3> points, int width, long ID, Texture2D tex)
    {
        Spline spline = road.road;
        road.points = points;
        //modify a road based on the coordinates of the build mode
        List<BezierKnot> knots = BuildKnots(points, width);

        for (int knotAddIndex = 0; knotAddIndex < knots.Count; knotAddIndex++)
        {
            if (knotAddIndex < spline.Count)
            {
                spline.SetKnot(knotAddIndex, knots[knotAddIndex]);
            }
            else
            {
                spline.Add(knots[knotAddIndex]);
            }
        }

        foreach (Intersection intersection in intersections)
        {
            if (intersection.junctions.FindIndex(item => item.spline == spline) != -1)
            {
                Intersection.JunctionInfo selectedJunction = intersection.junctions.Find(item => item.spline == spline);
                Vector3 intersectingSplinePoint = CalculateIntersectionCenter(spline, intersection);

                int knotIndex = selectedJunction.knotIndex == 0 ? 0 : spline.Count - 1;

                if (Vector3.Distance(intersectingSplinePoint, spline[knotIndex].Position) > width + 3f)
                {
                    intersection.junctions.Remove(intersection.junctions.Find(item => item.spline == spline));
                }
                else
                {
                    Intersection.JunctionInfo newJunction = new();
                    newJunction.spline = spline;

                    if (knotIndex == 0)
                    {
                        newJunction.knotIndex = 0;
                        newJunction.knot = spline[0];
                    }
                    else
                    {
                        newJunction.knotIndex = spline.Count - 1;
                        newJunction.knot = spline[spline.Count - 1];
                    }

                    intersection.junctions[intersection.junctions.FindIndex(item => item.spline == spline)] = newJunction;
                }
            }
        }

        List<List<Spline>> roadsList = new();
        List<List<BezierKnot>> knotsList = new();
        List<int> intersectionBuild = new();
        CreateIntersection(intersectionBuild, points, width, spline, roadsList, knotsList, tex, ID, road, true);

        road.points = points;
        road.width = width;
        road.ID = ID;
        road.resolution = (int)(spline.GetLength() * 2);
        BuildRoadMesh(roads.IndexOf(road));

        for (int k = 0; k < roadsList.Count; k++)
        {
            MakeIntersection(roadsList[k], knotsList[k]);
        }
        foreach (int index in intersectionBuild)
        {
            BuildIntersectionMesh(index);
        }
        CleanRoads();
        CleanIntersections();
    }

    public void ModifyIntersection(Intersection intersection, Vector3 center)
    {
        List<(int, Intersection.JunctionInfo)> newJunctions = new();

        foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
        {
            Roads road = roads.Find(item => item.road == junction.spline);
            int index = junction.knotIndex == 0 ? 0 : road.points.Count;

            if (index == 0)
            {
                Vector3 dir = road.points[1] - center;
                road.points[0] = center + (dir.normalized * (road.width + 0.5f));
                road.resolution = (int)(road.road.GetLength() * 2);
                RebuildRoadKnots(road.points, road.road, road.width);

                Intersection.JunctionInfo newJunction = new();
                newJunction.spline = road.road;
                newJunction.knotIndex = 0;
                newJunction.knot = road.road[0];
                newJunctions.Add((intersection.junctions.IndexOf(junction), newJunction));
            }
            else
            {
                Vector3 dir = road.points[^2] - center;
                road.points[^1] = center + (dir.normalized * (road.width + 0.5f));
                road.resolution = (int)(road.road.GetLength() * 2);
                RebuildRoadKnots(road.points, road.road, road.width);

                Intersection.JunctionInfo newJunction = new();
                newJunction.spline = road.road;
                newJunction.knotIndex = road.road.Count - 1;
                newJunction.knot = road.road[road.road.Count - 1];
                newJunctions.Add((intersection.junctions.IndexOf(junction), newJunction));
            }
        }

        for (int i = 0; i < newJunctions.Count; i++)
        {
            intersection.junctions[newJunctions[i].Item1] = newJunctions[i].Item2;
            Roads road = roads.Find(item => item.road == newJunctions[i].Item2.spline);
            List<List<Spline>> roadsList = new();
            List<List<BezierKnot>> knotsList = new();
            List<int> intersectionBuild = new();
            CreateIntersection(intersectionBuild, road.points, road.width, road.road, roadsList, knotsList, road.tex, road.ID, road, true);
            for (int k = 0; k < roadsList.Count; k++)
            {
                MakeIntersection(roadsList[k], knotsList[k]);
            }
            foreach (int index in intersectionBuild)
            {
                BuildIntersectionMesh(index);
            }

            BuildRoadMesh(roads.IndexOf(road));
        }

        CleanRoads();
        CleanIntersections();
        BuildIntersectionMesh(intersections.IndexOf(intersection));
    }

    private void MakeSpline(Spline spline, List<Vector3> points, int width, Texture2D tex, long ID)
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
            Roads road = new Roads(spline, points, (int)(spline.GetLength() * 2), width, c.GetComponent<MeshCollider>(), c.GetComponent<MeshRenderer>(), c.GetComponent<MeshFilter>(), material, tex, ID);
            roads.Add(road);
            BuildRoadMesh(roads.IndexOf(road));
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
            BuildRoadMesh(roads.IndexOf(road));
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
        List<(Spline, BezierKnot)> removeIndex = new();
        for (int i = 0; i < splines.Count; i++)
        {
            if (CheckPointsEqual(splines[i], out Spline equalSpline))
            {
                splines[i] = equalSpline;
            }
            else
            {
                removeIndex.Add((splines[i], knots[i]));
            }
        }

        foreach ((Spline, BezierKnot) remove in removeIndex)
        {
            splines.Remove(remove.Item1);
            knots.Remove(remove.Item2);
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

    public void SelectRoad(InputManager input, out Roads selectedRoad, out int index, out int width, out long ID, out List<Vector3> points)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        index = -1; width = 0; ID = -1; points = new();
        selectedRoad = roads.Find(road => overlapsList.FindIndex(col => col.collider == road.collider) != -1);
        if (selectedRoad != null)
        {
            index = roads.IndexOf(selectedRoad);
            width = selectedRoad.width;
            ID = selectedRoad.ID;
            points = selectedRoad.points;
        }
    }

    public void SelectIntersection(InputManager input, out Intersection selectedIntersection, out int index, out List<Roads> roads)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        index = -1; roads = new();
        selectedIntersection = intersections.Find(intersection => overlapsList.FindIndex(col => col.collider == intersection.collider) != -1);
        if (selectedIntersection != null)
        {
            index = intersections.IndexOf(selectedIntersection);

            foreach (Intersection.JunctionInfo junction in selectedIntersection.GetJunctions())
            {
                Roads road = this.roads.Find(item => item.road == junction.spline);
                roads.Add(road);
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

    public bool CheckIntersectionSelect(InputManager input)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (intersections.FindIndex(intersection => overlapsList.FindIndex(col => col.collider == intersection.collider) != -1) != -1)
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
        float t1 = Vector3.Dot(Vector3.Cross(origin2 - origin1, tangent2.normalized), Vector3.Cross(tangent1.normalized, tangent2.normalized)) * Vector3.Dot(Vector3.Cross(tangent1.normalized, tangent2.normalized), Vector3.Cross(tangent1.normalized, tangent2.normalized));
        float t2 = Vector3.Dot(Vector3.Cross(origin2 - origin1, tangent1.normalized), Vector3.Cross(tangent1.normalized, tangent2.normalized)) * Vector3.Dot(Vector3.Cross(tangent1.normalized, tangent2.normalized), Vector3.Cross(tangent1.normalized, tangent2.normalized));

        Vector3 p1 = origin1 + (t1 * tangent1);
        Vector3 p2 = origin2 + (t2 * tangent2);

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
    //public SerializedDictionary<Vector3, KnotClusterWrapper> roadMap;
    public List<Vector3> points;
    public int resolution;
    public int width;
    public long ID;
    public MeshCollider collider;
    public MeshFilter mesh;
    public MeshRenderer renderer;
    public Texture2D tex;

    public Roads(Spline road, List<Vector3> points, int resolution, int width, MeshCollider collider, MeshRenderer renderer, MeshFilter mesh, Material mat, Texture2D tex, long ID)
    {
        this.road = road;

        //this.roadMap = ConvertToSerializedRoadMap(roadMap);

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

/*
    public Dictionary<Vector3, List<BezierKnot>> GetRoadMap()
    {
        Dictionary<Vector3, List<BezierKnot>> returnedRoadMap = new Dictionary<Vector3, List<BezierKnot>>();

        foreach (Vector3 pos in this.roadMap.Keys)
        {
            returnedRoadMap[pos] = roadMap[pos].knotCluster;
        }

        return returnedRoadMap;
    }
    */

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