using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class RoadMapping : MonoBehaviour
{
    private List<Vector3> m_vertsP1;
    private List<Vector3> m_vertsP2;
    public List<Intersection> intersections = new();
    public List<Roads> roads = new();
    [SerializeField] private SplineSampler m_SplineSampler;
    [SerializeField] public SplineContainer m_SplineContainer;
    [SerializeField] private MeshFilter m_meshFilter;

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

    public void AddJunction(Intersection intersection, MeshCollider collider)
    {
        intersections.Add(intersection);
        intersection.collider = collider;
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

    private void BuildMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<int> trisB = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int offset = 0;
        float uvOffset = 0;

        int length = m_vertsP2.Count;

        for (int currentSplineIndex = 0; currentSplineIndex < roads.Count; currentSplineIndex++)
        {
            List<Vector3> currentVerts = new List<Vector3>();
            List<int> currentTris = new List<int>();
            List<Vector3> contactVerts = new List<Vector3>();
            List<int> contactTris = new List<int>();
            Mesh c = new Mesh();

            roads[currentSplineIndex].resolution = (int)roads[currentSplineIndex].road.GetLength() * 2;
            int resolution = (int)(roads[currentSplineIndex].resolution);
            //int splineOffset = resolution * currentSplineIndex;
            int splineOffset = calculateRes(currentSplineIndex);
            splineOffset += currentSplineIndex;

            for (int currentSplinePoint = 1; currentSplinePoint <= resolution /*< resolution-1*/; currentSplinePoint++)
            {
                int vertoffset = splineOffset + currentSplinePoint;
                Vector3 p1 = m_vertsP1[vertoffset - 1];
                Vector3 p2 = m_vertsP2[vertoffset - 1];
                Vector3 p3 = m_vertsP1[vertoffset];
                Vector3 p4 = m_vertsP2[vertoffset];

                offset = 8 * calculateRes(currentSplineIndex);
                offset += 8 * (currentSplinePoint - 1);

                int t1 = offset + 0;
                int t2 = offset + 2;
                int t3 = offset + 3;
                int t4 = offset + 3;
                int t5 = offset + 1;
                int t6 = offset + 0;

                int t7 = offset + 4;
                int t8 = offset + 6;
                int t9 = offset + 7;
                int t10 = offset + 7;
                int t11 = offset + 5;
                int t12 = offset + 4;

                currentVerts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
                currentVerts.AddRange(new List<Vector3> { p1 + new Vector3(0, 0.025f, 0), p2 + new Vector3(0, 0.025f, 0), p3 + new Vector3(0, 0.025f, 0), p4 + new Vector3(0, 0.025f, 0) });
<<<<<<< HEAD
                currentTris.AddRange(new List<int> { t1 - (8 * calculateRes(currentSplineIndex)), t2 - (8 * calculateRes(currentSplineIndex)), t3 - (8 * calculateRes(currentSplineIndex)), t4 - (8 * calculateRes(currentSplineIndex)), t5 - (8 * calculateRes(currentSplineIndex)), t6 - (8 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t7 - (8 * calculateRes(currentSplineIndex)), t8 - (8 * calculateRes(currentSplineIndex)), t9 - (8 * calculateRes(currentSplineIndex)), t10 - (8 * calculateRes(currentSplineIndex)), t11 - (8 * calculateRes(currentSplineIndex)), t12 - (8 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t1 - (8 * calculateRes(currentSplineIndex)), t7 - (8 * calculateRes(currentSplineIndex)), t8 - (8 * calculateRes(currentSplineIndex)), t8 - (8 * calculateRes(currentSplineIndex)), t2 - (8 * calculateRes(currentSplineIndex)), t1 - (8 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t5 - (8 * calculateRes(currentSplineIndex)), t11 - (8 * calculateRes(currentSplineIndex)), t9 - (8 * calculateRes(currentSplineIndex)), t9 - (8 * calculateRes(currentSplineIndex)), t3 - (8 * calculateRes(currentSplineIndex)), t5 - (8 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t1 - (8 * calculateRes(currentSplineIndex)), t7 - (8 * calculateRes(currentSplineIndex)), t11 - (8 * calculateRes(currentSplineIndex)), t11 - (8 * calculateRes(currentSplineIndex)), t5 - (8 * calculateRes(currentSplineIndex)), t1 - (8 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t2 - (8 * calculateRes(currentSplineIndex)), t8 - (8 * calculateRes(currentSplineIndex)), t9 - (8 * calculateRes(currentSplineIndex)), t9 - (8 * calculateRes(currentSplineIndex)), t3 - (8 * calculateRes(currentSplineIndex)), t2 - (8 * calculateRes(currentSplineIndex)) });
=======
                currentTris.AddRange(new List<int> { t1 - (4 * calculateRes(currentSplineIndex)), t2 - (4 * calculateRes(currentSplineIndex)), t3 - (4 * calculateRes(currentSplineIndex)), t4 - (4 * calculateRes(currentSplineIndex)), t5 - (4 * calculateRes(currentSplineIndex)), t6 - (4 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t7 - (4 * calculateRes(currentSplineIndex)), t8 - (4 * calculateRes(currentSplineIndex)), t9 - (4 * calculateRes(currentSplineIndex)), t10 - (4 * calculateRes(currentSplineIndex)), t11 - (4 * calculateRes(currentSplineIndex)), t12 - (4 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t1 - (4 * calculateRes(currentSplineIndex)), t7 - (4 * calculateRes(currentSplineIndex)), t8 - (4 * calculateRes(currentSplineIndex)), t8 - (4 * calculateRes(currentSplineIndex)), t2 - (4 * calculateRes(currentSplineIndex)), t1 - (4 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t5 - (4 * calculateRes(currentSplineIndex)), t11 - (4 * calculateRes(currentSplineIndex)), t9 - (4 * calculateRes(currentSplineIndex)), t9 - (4 * calculateRes(currentSplineIndex)), t3 - (4 * calculateRes(currentSplineIndex)), t5 - (4 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t1 - (4 * calculateRes(currentSplineIndex)), t7 - (4 * calculateRes(currentSplineIndex)), t11 - (4 * calculateRes(currentSplineIndex)), t11 - (4 * calculateRes(currentSplineIndex)), t5 - (4 * calculateRes(currentSplineIndex)), t1 - (4 * calculateRes(currentSplineIndex)) });
                contactTris.AddRange(new List<int> { t2 - (4 * calculateRes(currentSplineIndex)), t8 - (4 * calculateRes(currentSplineIndex)), t9 - (4 * calculateRes(currentSplineIndex)), t9 - (4 * calculateRes(currentSplineIndex)), t3 - (4 * calculateRes(currentSplineIndex)), t2 - (4 * calculateRes(currentSplineIndex)) });
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
                tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
                tris.AddRange(new List<int> { t7, t8, t9, t10, t11, t12 });
                tris.AddRange(new List<int> { t1, t7, t8, t8, t2, t1 });
                tris.AddRange(new List<int> { t5, t11, t9, t9, t3, t5 });
                tris.AddRange(new List<int> { t1, t7, t11, t11, t5, t1 });
                tris.AddRange(new List<int> {t2, t8, t9, t9, t3, t2 });

                float distance = Vector3.Distance(p1, p3) / 4f;
                float uvDistance = uvOffset + distance;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1),
                                        new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1)});

                uvOffset += distance;
            }

            verts.AddRange(currentVerts);
            contactVerts.AddRange(currentVerts); contactTris.AddRange(currentTris);
            c.SetVertices(contactVerts); c.SetTriangles(contactTris, 0);
            roads[currentSplineIndex].collider.sharedMesh = c;
        }

        offset = verts.Count;

        for (int i = 0; i < intersections.Count; i++)
        {
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
<<<<<<< HEAD
                currentVerts.Add(center + new Vector3(0, 0.025f, 0));
                currentVerts.Add(curvePoints[j - 1]);
                currentVerts.Add(curvePoints[j - 1] + new Vector3(0, 0.025f, 0));
                uvs.Add(new Vector2(center.z, center.x));
                uvs.Add(new Vector2(center.z, center.x));
                uvs.Add(new Vector2(curvePoints[j - 1].z, curvePoints[j - 1].x));
=======
                contactVerts.Add(center + (Vector3.up / 4));
                currentVerts.Add(curvePoints[j - 1]);
                contactVerts.Add(curvePoints[j - 1] + (Vector3.up / 4));
                uvs.Add(new Vector2(center.z, center.x));
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
                uvs.Add(new Vector2(curvePoints[j - 1].z, curvePoints[j - 1].x));
                if (j == curvePoints.Count)
                {
                    currentVerts.Add(curvePoints[0]);
<<<<<<< HEAD
                    currentVerts.Add(curvePoints[0] + new Vector3(0, 0.025f, 0));
                    uvs.Add(new Vector2(curvePoints[0].z, curvePoints[0].x));
=======
                    contactVerts.Add(curvePoints[0] + (Vector3.up / 4));
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
                    uvs.Add(new Vector2(curvePoints[0].z, curvePoints[0].x));
                }
                else
                {
                    currentVerts.Add(curvePoints[j]);
<<<<<<< HEAD
                    currentVerts.Add(curvePoints[j] + new Vector3(0, 0.025f, 0));
                    uvs.Add(new Vector2(curvePoints[j].z, curvePoints[j].x));
                    uvs.Add(new Vector2(curvePoints[j].z, curvePoints[j].x));
                }

                trisB.Add(pointsOffset + ((j - 1) * 6) + 0); trisB.Add(pointsOffset + ((j - 1) * 6) + 2); trisB.Add(pointsOffset + ((j - 1) * 6) + 4);
                trisB.Add(pointsOffset + ((j - 1) * 6) + 1); trisB.Add(pointsOffset + ((j - 1) * 6) + 3); trisB.Add(pointsOffset + ((j - 1) * 6) + 5);
                trisB.Add(pointsOffset + ((j - 1) * 6) + 2); trisB.Add(pointsOffset + ((j - 1) * 6) + 3); trisB.Add(pointsOffset + ((j - 1) * 6) + 5);
                trisB.Add(pointsOffset + ((j - 1) * 6) + 5); trisB.Add(pointsOffset + ((j - 1) * 6) + 4); trisB.Add(pointsOffset + ((j - 1) * 6) + 2);
                contactTris.Add(((j - 1) * 6) + 0); contactTris.Add(((j - 1) * 6) + 2); contactTris.Add(((j - 1) * 6) + 4);
                contactTris.Add(((j - 1) * 6) + 1); contactTris.Add(((j - 1) * 6) + 3); contactTris.Add(((j - 1) * 6) + 5);
                contactTris.Add(((j - 1) * 6) + 2); contactTris.Add(((j - 1) * 6) + 3); contactTris.Add(((j - 1) * 6) + 5);
                contactTris.Add(((j - 1) * 6) + 5); contactTris.Add(((j - 1) * 6) + 4); contactTris.Add(((j - 1) * 6) + 2);
=======
                    contactVerts.Add(curvePoints[j] + (Vector3.up / 4));
                    uvs.Add(new Vector2(curvePoints[j].z, curvePoints[j].x));
                }

                trisB.Add(pointsOffset + ((j - 1) * 3) + 0); trisB.Add(pointsOffset + ((j - 1) * 3) + 1); trisB.Add(pointsOffset + ((j - 1) * 3) + 2);
                contactTris.Add(((j - 1) * 3) + 0); contactTris.Add(((j - 1) * 3) + 1); contactTris.Add(((j - 1) * 3) + 2);
                contactTris.Add(((j - 1 + curvePoints.Count) * 3) + 0); contactTris.Add(((j - 1 + curvePoints.Count) * 3) + 1); contactTris.Add(((j - 1 + curvePoints.Count) * 3) + 2);
                contactTris.Add(((j - 1) * 3) + 0); contactTris.Add(((j - 1 + curvePoints.Count) * 3) + 0); contactTris.Add(((j - 1 + curvePoints.Count) * 3) + 2);
                contactTris.Add(((j - 1 + curvePoints.Count) * 3) + 2); contactTris.Add(((j - 1) * 3) + 2); contactTris.Add(((j - 1) * 3) + 0);
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
            }

            verts.AddRange(currentVerts);
            contactVerts.AddRange(currentVerts);
            intersectionMesh.SetVertices(contactVerts); intersectionMesh.SetTriangles(contactTris, 0);
            intersection.collider.sharedMesh = intersectionMesh;
        }

        int numVerts = verts.Count;
        //GetIntersectionVerts(verts, trisB, uvs);

        m.subMeshCount = 2;

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.SetTriangles(trisB, 1);
        m.SetUVs(0, uvs);

        m_meshFilter.mesh = m;
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
                if ((indexA == 0 && indexB == 0))
                {
                    SplineUtility.ReverseFlow(splineB);
                    roadB.points.Reverse();
                    for (int pointMap = 0; pointMap < roadB.points.Count; pointMap++)
                    {
                        List<BezierKnot> newBRoadMap = roadB.roadMap[roadB.points[pointMap]];
                        newBRoadMap.Reverse();
                        for (int knotIndex = 0; knotIndex < newBRoadMap.Count; knotIndex++)
                        {
                            BezierKnot knot = newBRoadMap[knotIndex];
                            Quaternion newRotationQuaternion = knot.Rotation;
                            Vector3 newRotation = newRotationQuaternion.eulerAngles;
                            knot.Rotation = Quaternion.Euler(newRotation.x, newRotation.y - 180, newRotation.z);
                            newBRoadMap[knotIndex] = knot;
                        }
                        roadB.roadMap[roadB.points[pointMap]] = newBRoadMap;
                    }
                    indexB = splineB.Count;
                }
                else if ((indexA > 0 && indexB > 0))
                {
                    SplineUtility.ReverseFlow(splineB);
                    roadB.points.Reverse();
                    for (int pointMap = 0; pointMap < roadB.points.Count; pointMap++)
                    {
                        List<BezierKnot> newBRoadMap = roadB.roadMap[roadB.points[pointMap]];
                        newBRoadMap.Reverse();
                        for (int knotIndex = 0; knotIndex < newBRoadMap.Count; knotIndex++)
                        {
                            BezierKnot knot = newBRoadMap[knotIndex];
                            Quaternion newRotationQuaternion = knot.Rotation;
                            Vector3 newRotation = newRotationQuaternion.eulerAngles;
                            knot.Rotation = Quaternion.Euler(newRotation.x, newRotation.y - 180, newRotation.z);
                            newBRoadMap[knotIndex] = knot;
                        }
                        roadB.roadMap[roadB.points[pointMap]] = newBRoadMap;
                    }
                    indexB = 0;
                }
                if (indexA > indexB)
                {
                    Spline newSpline = new Spline();
                    List<Vector3> newPoints = new List<Vector3>();
                    Dictionary<Vector3, List<BezierKnot>> newMap = new Dictionary<Vector3, List<BezierKnot>>();
                    for (int i = 0; i < roadA.points.Count - 1; i++)
                    {
                        newPoints.Add(roadA.points[i]);
                        newMap.Add(roadA.points[i], roadA.roadMap[roadA.points[i]]);
                    }

                    BezierKnot joinKnotA = roadA.roadMap[roadA.points[roadA.points.Count - 1]][0]; //first intersecting knot
                    BezierKnot joinKnotB = roadB.roadMap[roadB.points[0]][0]; //second intersecting knot

                    BezierKnot extendKnotA = new();
                    extendKnotA.Position = ((Vector3)joinKnotA.Position) + (((Quaternion)joinKnotA.Rotation).eulerAngles * 5);
                    BezierKnot extendKnotB = new();
                    extendKnotB.Position = ((Vector3)joinKnotB.Position) + (((Quaternion)joinKnotA.Rotation).eulerAngles * 5);

                    splineA.Add(extendKnotA);
                    splineB.Add(extendKnotB);

                    Vector3 center = (roadA.points[roadA.points.Count - 1] + roadB.points[0]) / 2;
                    m_SplineSampler.SampleSplinePoint(splineA, center, roadA.resolution, out center, out float t1);
                    m_SplineSampler.SampleSplinePoint(splineB, center, roadB.resolution, out center, out float t2);
                    m_SplineSampler.SampleSplinePoint(splineA, center, roadA.resolution, out center, out t1);
                    m_SplineSampler.SampleSplinePoint(splineB, center, roadB.resolution, out center, out t2);

                    float prevAngle = Vector3.SignedAngle(center - roadA.points[roadA.points.Count - 2], Vector3.forward, Vector3.down);
                    float angle = Vector3.SignedAngle(roadB.points[1] - center, Vector3.forward, Vector3.down);
                    float sign = Mathf.Abs(angle - prevAngle);

                    if (sign > 0)
                    {
                        joinKnotA.Position = (Quaternion.Euler(0, prevAngle, 0) * new Vector3(0, 0, sign > 90 ? -(roadA.width + 0.5f) : ((roadA.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? -((roadA.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : -1f)) + center;
                        joinKnotA.Rotation = Quaternion.Euler(0, prevAngle, 0);
                        joinKnotA.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        joinKnotA.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        joinKnotB.Position = (Quaternion.Euler(0, angle - 90, 0) * new Vector3(sign > 90 ? (roadB.width + 0.5f) : ((roadB.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? ((roadB.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : 1f, 0, 0)) + center;
                        joinKnotB.Rotation = Quaternion.Euler(0, angle, 0);
                        joinKnotB.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        joinKnotB.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        newPoints.Add(center);
                        newMap.Add(center, new List<BezierKnot> { joinKnotA, joinKnotB });
                    }

                    for (int i = 1; i < roadB.points.Count; i++)
                    {
                        newPoints.Add(roadB.points[i]);
                        newMap.Add(roadB.points[i], roadB.roadMap[roadB.points[i]]);
                    }

                    for (int i = 0; i < newPoints.Count; i++)
                    {
                        foreach (BezierKnot kt in newMap[newPoints[i]])
                        {
                            newSpline.Add(kt);
                        }
                    }

                    MakeSpline(newSpline, newMap, newPoints, roadA.width, roadA.ID);

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
                                    if (Vector3.Distance(junctPos, ktPos) < 1f)
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

                    removeIntersections.Add(intersection);
                    DeleteRoad(roadA);
                    DeleteRoad(roadB);
                }
                else if (indexB > indexA)
                {
                    Spline newSpline = new Spline();
                    List<Vector3> newPoints = new List<Vector3>();
                    Dictionary<Vector3, List<BezierKnot>> newMap = new Dictionary<Vector3, List<BezierKnot>>();
                    for (int i = 0; i < roadB.points.Count - 1; i++)
                    {
                        newPoints.Add(roadB.points[i]);
                        newMap.Add(roadB.points[i], roadB.roadMap[roadB.points[i]]);
                    }

                    BezierKnot joinKnotA = roadB.roadMap[roadB.points[roadB.points.Count - 1]][0]; //first intersecting knot
                    BezierKnot joinKnotB = roadA.roadMap[roadA.points[0]][0]; //second intersecting knot

                    BezierKnot extendKnotA = new();
                    extendKnotA.Position = ((Vector3)joinKnotA.Position) + (((Quaternion)joinKnotA.Rotation).eulerAngles * 5);
                    BezierKnot extendKnotB = new();
                    extendKnotB.Position = ((Vector3)joinKnotB.Position) + (((Quaternion)joinKnotB.Rotation).eulerAngles * 5);

                    splineA.Add(extendKnotB);
                    splineB.Add(extendKnotA);

                    Vector3 center = (roadB.points[roadB.points.Count - 1] + roadA.points[0]) / 2;
                    m_SplineSampler.SampleSplinePoint(splineA, center, roadA.resolution, out center, out float t1);
                    m_SplineSampler.SampleSplinePoint(splineB, center, roadB.resolution, out center, out float t2);

                    float prevAngle = Vector3.SignedAngle(center - roadB.points[roadB.points.Count - 2], Vector3.forward, Vector3.down);
                    float angle = Vector3.SignedAngle(roadA.points[1] - center, Vector3.forward, Vector3.down);
                    float sign = Mathf.Abs(angle - prevAngle);

                    if (sign > 0)
                    {
                        joinKnotA.Position = (Quaternion.Euler(0, prevAngle, 0) * new Vector3(0, 0, sign > 90 ? -(roadB.width + 0.5f) : ((roadB.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? -((roadB.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : -1f)) + center;
                        joinKnotA.Rotation = Quaternion.Euler(0, prevAngle, 0);
                        joinKnotA.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        joinKnotA.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        joinKnotB.Position = (Quaternion.Euler(0, angle - 90, 0) * new Vector3(sign > 90 ? (roadA.width + 0.5f) : ((roadA.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? ((roadA.width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : 1f, 0, 0)) + center;
                        joinKnotB.Rotation = Quaternion.Euler(0, angle, 0);
                        joinKnotB.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        joinKnotB.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * 180 / Mathf.PI)));
                        newPoints.Add(center);
                        newMap.Add(center, new List<BezierKnot> { joinKnotA, joinKnotB });
                    }

                    for (int i = 1; i < roadA.points.Count; i++)
                    {
                        newPoints.Add(roadA.points[i]);
                        newMap.Add(roadA.points[i], roadA.roadMap[roadA.points[i]]);
                    }

                    for (int i = 0; i < newPoints.Count; i++)
                    {
                        foreach (BezierKnot kt in newMap[newPoints[i]])
                        {
                            newSpline.Add(kt);
                        }
                    }

                    MakeSpline(newSpline, newMap, newPoints, roadA.width, roadA.ID);

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
                                    if (Vector3.Distance(junctPos, ktPos) < 3f)
                                    {
                                        junctKnot = kt;
                                    }
                                }
                                Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(newSpline, junctKnot);
                                intersect.junctions[a] = newInfo;
                            }
                        }
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

    public void AddRoad(List<Vector3> points, int width, int ID)
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
                angle = Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, Vector3.forward, Vector3.down);
                prevAngle = Vector3.SignedAngle(point - points[points.IndexOf(point) - 1], Vector3.forward, Vector3.down);
                float sign = Mathf.Abs(angle - prevAngle);
                //make nicer curves
                BezierKnot knot2 = new BezierKnot();
                knot.Position = (Quaternion.Euler(0, prevAngle, 0) * new Vector3(0, 0, sign > 90 ? -(width + 0.5f) : ((width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? (-(width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : -1f)) + point;
                knot.Rotation = Quaternion.Euler(0, prevAngle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot2.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot2.Position = (Quaternion.Euler(0, angle - 90, 0) * new Vector3(sign > 90 ? (width + 0.5f) : ((width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? ((width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : 1f, 0, 0)) + point;
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

        MakeSpline(spline, splinePoints, points, width, ID);

        List<Vector3> pointsRef = points;
        Dictionary<Vector3, List<BezierKnot>> pointMapRef = splinePoints;
        List<RaycastHit> hitList = new();
        List<List<Spline>> roadsList = new();
        List<List<BezierKnot>> knotsList = new();
        List<int> hitRemoveList = new();
        //detects overlap and creates an intersection automatically
        for (int i = 1; i < pointsRef.Count; i++)
        {
            Vector3 p1 = pointsRef[i - 1]; Vector3 p2 = pointsRef[i];
            RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(width, 0.5f, 0.1f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
            RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(width, 0.5f, 0.1f), p1 - p2, Quaternion.Euler(0, Vector3.SignedAngle(p1 - p2, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
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

                    center /= (junctionEdges.Count * 2);

                    //determine the spline in which it is intersecting
                    m_SplineSampler.SampleSplinePoint(spline, center, (int)(spline.GetLength() * 2), out Vector3 intersectingSplinePoint, out float t1);

                    //determine the direction to move the splines in
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

                            MakeSpline(spline2, spline2Points, points2, width, ID);
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
                            spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[spline2.Count - 1] });
                            m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot2.Position = knot2Pos;
                            knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[spline2.Count - 1] = knot2;

                            MakeSpline(spline2, spline2Points, points2, width, ID);
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
                        spline.SetKnot(spline.Count - 1, knot2); points[points.Count - 1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
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

                    for (int k = 0; k < hitList.Count; k++)
                    {
                        if ((hitList[k].point.x >= intersectingSplinePoint.x - (dir1.normalized.x * 3f) || hitList[k].point.x <= intersectingSplinePoint.x + (dir1.normalized.x * 3f) ||
                            hitList[k].point.z >= intersectingSplinePoint.z - (dir1.normalized.z * 3f) || hitList[k].point.z <= intersectingSplinePoint.z + (dir1.normalized.z * 3f)) && !hitList[k].Equals(hit))
                        {
                            hitRemoveList.Add(k);
                        }
                    }
                }
            }
            for (int j = 0; j < roads.Count; j++)
            {
                if (hit.collider == roads[j].collider && !hitRemoveList.Contains(hitList.IndexOf(hit)))
                {
                    Spline spline2 = new(); Spline spline3 = new();
                    Dictionary<Vector3, List<BezierKnot>> spline2Points = new(); Dictionary<Vector3, List<BezierKnot>> spline3Points = new();
                    List<Vector3> points2 = new(); List<Vector3> points3 = new();
                    List<Spline> internalRoadsList = new(); List<BezierKnot> internalKnotsList = new();

                    //determine the spline in which it is intersecting
                    Vector3 thisSplinePoint = new(); Vector3 otherSplinePoint = new();
                    m_SplineSampler.SampleSplinePoint(spline, hit.point, (int)(spline.GetLength() * 2), out thisSplinePoint, out float t1);
                    m_SplineSampler.SampleSplinePoint(roads[j].road, hit.point, roads[j].resolution, out otherSplinePoint, out float t2);
                    m_SplineSampler.SampleSplinePoint(spline, otherSplinePoint, (int)(spline.GetLength() * 2), out thisSplinePoint, out t1);
                    m_SplineSampler.SampleSplinePoint(roads[j].road, thisSplinePoint, roads[j].resolution, out otherSplinePoint, out t2);
                    Vector3 intersectingSplinePoint = otherSplinePoint; //used to prioritise the existing road coordinates in the creation of an intersection

                    //determine the direction to move the splines in
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                    Vector3 dir2 = (Vector3)SplineUtility.EvaluateTangent(roads[j].road, t2);

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

                            MakeSpline(spline2, spline2Points, points2, width, ID);
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
                            knot1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); spline[0] = knot1;

                            Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline2, t1);
                            points2.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)));
                            spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[spline2.Count - 1] });
                            m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot2.Position = knot2Pos;
                            knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[spline2.Count - 1] = knot2;

                            MakeSpline(spline2, spline2Points, points2, width, ID);
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
                            internalKnotsList.Add(spline[0]); internalKnotsList.Add(spline2[spline2.Count - 1]);
                        }
                    }
                    else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                    {
                        spline.SetKnot(spline.Count - 1, knot2); points[points.Count - 1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
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
                    if (EvaluateT(roads[j].road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) < 1 && EvaluateT(roads[j].road, intersectingSplinePoint - (dir2.normalized * (width + 0.5f))) > 0)
                    {
                        FilterPoints(roads[j].road, spline3, roads[j].points, roads[j].roadMap, t2, out points3, out spline3Points, out bool splitSpline);

                        if (splitSpline)
                        {

                            roads[j].road.Insert(0, knot3); roads[j].points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                            roads[j].roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot3 });
                            roads[j].resolution = (int)(roads[j].road.GetLength() * 2);

                            spline3.Add(knot4); points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                            spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot4 });

                            MakeSpline(spline3, spline3Points, points3, width, ID);
                            FilterIntersections(roads[j].road, spline3, t2);
                            internalRoadsList.Add(roads[j].road); internalRoadsList.Add(spline3);
                            internalKnotsList.Add(knot3); internalKnotsList.Add(knot4);
                        }
                        else
                        {
                            Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(roads[j].road, t2);
                            roads[j].points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                            roads[j].roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { roads[j].road[0] });
                            m_SplineSampler.SampleSplinePoint(roads[j].road, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                            knot3.Position = knot1Pos;
                            knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); roads[j].road[0] = knot3;

                            Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline3, t2);
                            points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                            spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { spline3[spline3.Count - 1] });
                            m_SplineSampler.SampleSplinePoint(spline3, intersectingSplinePoint - (dirB.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot4.Position = knot2Pos;
                            knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline3[spline3.Count - 1] = knot4;

                            MakeSpline(spline3, spline3Points, points3, width, ID);
                            FilterIntersections(roads[j].road, spline3, t2);
                            internalRoadsList.Add(roads[j].road); internalRoadsList.Add(spline3);
                            internalKnotsList.Add(roads[j].road[0]); internalKnotsList.Add(spline3[spline3.Count - 1]);
                        }
                    }
                    else if (EvaluateT(roads[j].road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) >= 1)
                    {
                        roads[j].road.SetKnot(roads[j].road.Count - 1, knot4); roads[j].points[roads[j].points.Count - 1] = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                        roads[j].resolution = (int)(roads[j].road.GetLength() * 2);
                        roads[j].roadMap.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot4 });
                        roads[j].roadMap.Remove(intersectingSplinePoint);
                        internalRoadsList.Add(roads[j].road); internalKnotsList.Add(knot4);
                    }
                    else
                    {
                        roads[j].road.SetKnot(0, knot3); roads[j].points[0] = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                        roads[j].resolution = (int)(roads[j].road.GetLength() * 2);
                        roads[j].roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot3 });
                        roads[j].roadMap.Remove(intersectingSplinePoint);
                        internalRoadsList.Add(roads[j].road); internalKnotsList.Add(knot3);
                    }

                    roadsList.Add(internalRoadsList);
                    knotsList.Add(internalKnotsList);
                }
            }
        }

        for (int k = 0; k < roadsList.Count; k++)
        {
            MakeIntersection(roadsList[k], knotsList[k]);
        }
        CleanRoads();
        CleanIntersections();
        MakeRoad();
    }

    public void ModifyRoad(Roads road, List<Vector3> points, int width, int ID)
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
                angle = Vector3.SignedAngle(points[points.IndexOf(point) + 1] - point, Vector3.forward, Vector3.down);
                prevAngle = Vector3.SignedAngle(point - points[points.IndexOf(point) - 1], Vector3.forward, Vector3.down);
                float sign = Mathf.Abs(angle - prevAngle);
                //make nicer curves
                BezierKnot knot2 = new BezierKnot();
                knot.Position = (Quaternion.Euler(0, prevAngle, 0) * new Vector3(0, 0, sign > 90 ? -(width + 0.5f) : ((width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? (-(width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : -1f)) + point;
                knot.Rotation = Quaternion.Euler(0, prevAngle, 0);
                knot.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot2.TangentIn = new Unity.Mathematics.float3(0, 0, -Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot2.TangentOut = new Unity.Mathematics.float3(0, 0, Mathf.Abs(Mathf.Sin(sign * Mathf.PI / 180)));
                knot2.Position = (Quaternion.Euler(0, angle - 90, 0) * new Vector3(sign > 90 ? (width + 0.5f) : ((width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180) > 1) ? ((width + 0.5f) * Mathf.Sin(sign * Mathf.PI / 180)) : 1f, 0, 0)) + point;
                knot2.Rotation = Quaternion.Euler(0, angle, 0);
                if (road.points.Count > points.IndexOf(point))
                {
                    spline.SetKnot(road.road.IndexOf(road.roadMap[road.points[points.IndexOf(point)]][0]), knot);
                }
                else
                {
                    spline.Add(knot);
                }

                if (road.points.Count > points.IndexOf(point) && road.roadMap[road.points[points.IndexOf(point)]].Count > 1)
                {
                    spline.SetKnot(road.road.IndexOf(road.roadMap[road.points[points.IndexOf(point)]][1]), knot2);
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
        
        road.points = points;
        road.roadMap = splinePoints;
        road.width = width;
        road.ID = ID;
        road.resolution = (int)(spline.GetLength() * 2);

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
        List<int> hitRemoveList = new();
        //detects overlap and creates an intersection automatically
        for (int i = 1; i < pointsRef.Count; i++)
        {
            Vector3 p1 = pointsRef[i - 1]; Vector3 p2 = pointsRef[i];
            RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(width, 0.5f, 0.1f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
            RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(width, 0.5f, 0.1f), p1 - p2, Quaternion.Euler(0, Vector3.SignedAngle(p1 - p2, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
            List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2);
            foreach (RaycastHit hit in hits)
            {
                if (!HitsContainCollider(hitList, hit) && !(hit.point == Vector3.zero && p1 != Vector3.zero) && hit.collider != road.collider)
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
                if (hit.collider == intersections[j].collider && !IntersectionsContainRoad(road, intersections[j]))
                {
                    Spline spline2 = new();
                    Dictionary<Vector3, List<BezierKnot>> spline2Points = new();
                    List<Vector3> points2 = new();

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

                    center /= (junctionEdges.Count * 2);

                    //determine the spline in which it is intersecting
                    m_SplineSampler.SampleSplinePoint(spline, center, (int)(spline.GetLength() * 2), out Vector3 intersectingSplinePoint, out float t1);

                    //determine the direction to move the splines in
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

                            MakeSpline(spline2, spline2Points, points2, width, ID);
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
                            spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[spline2.Count - 1] });
                            m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot2.Position = knot2Pos;
                            knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[spline2.Count - 1] = knot2;

                            MakeSpline(spline2, spline2Points, points2, width, ID);
                            FilterIntersections(spline, spline2, t1);
                            road.resolution = (int)(spline.GetLength() * 2);
                        }

                        intersections[j].AddJunction(spline, knot1, 0.5f);
                        intersections[j].AddJunction(spline2, knot1, 0.5f);
                    }
                    else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                    {
                        spline.SetKnot(spline.Count - 1, knot2); points[points.Count - 1] = intersectingSplinePoint - (dir1.normalized * (width + 0.5f));
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
                    
                    for (int k = 0; k < hitList.Count; k++)
                    {
                        if ((hitList[k].point.x >= intersectingSplinePoint.x - (dir1.normalized.x * 3f) || hitList[k].point.x <= intersectingSplinePoint.x + (dir1.normalized.x * 3f) ||
                            hitList[k].point.z >= intersectingSplinePoint.z - (dir1.normalized.z * 3f) || hitList[k].point.z <= intersectingSplinePoint.z + (dir1.normalized.z * 3f)) && !hitList[k].Equals(hit))
                        {
                            hitRemoveList.Add(k);
                        }
                    }
                }
            }

            for (int j = 0; j < roads.Count; j++)
            {
                if (hit.collider == roads[j].collider && !hitRemoveList.Contains(j))
                {
                    Spline spline2 = new(); Spline spline3 = new();
                    Dictionary<Vector3, List<BezierKnot>> spline2Points = new(); Dictionary<Vector3, List<BezierKnot>> spline3Points = new();
                    List<Vector3> points2 = new(); List<Vector3> points3 = new();
                    List<Spline> internalRoadsList = new(); List<BezierKnot> internalKnotsList = new();

                    //determine the spline in which it is intersecting
                    Vector3 thisSplinePoint = new(); Vector3 otherSplinePoint = new();
                    m_SplineSampler.SampleSplinePoint(spline, hit.point, (int)(spline.GetLength() * 2), out thisSplinePoint, out float t1);
                    m_SplineSampler.SampleSplinePoint(roads[j].road, hit.point, roads[j].resolution, out otherSplinePoint, out float t2);
                    m_SplineSampler.SampleSplinePoint(spline, otherSplinePoint, (int)(spline.GetLength() * 2), out thisSplinePoint, out t1);
                    m_SplineSampler.SampleSplinePoint(roads[j].road, thisSplinePoint, roads[j].resolution, out otherSplinePoint, out t2);
                    Vector3 intersectingSplinePoint = otherSplinePoint; //used to prioritise the existing road coordinates in the creation of an intersection

                    //determine the direction to move the splines in
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                    Vector3 dir2 = (Vector3)SplineUtility.EvaluateTangent(roads[j].road, t2);

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

                            MakeSpline(spline2, spline2Points, points2, width, ID);
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
                            spline2Points.Add(intersectingSplinePoint - (dir1.normalized * (width + 0.5f)), new List<BezierKnot> { spline2[spline2.Count - 1] });
                            m_SplineSampler.SampleSplinePoint(spline2, intersectingSplinePoint - (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot2.Position = knot2Pos;
                            knot2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline2[spline2.Count - 1] = knot2;

                            MakeSpline(spline2, spline2Points, points2, width, ID);
                            FilterIntersections(spline, spline2, t1);
                            road.resolution = (int)(spline.GetLength() * 2);
                            internalRoadsList.Add(spline); internalRoadsList.Add(spline2);
                            internalKnotsList.Add(spline[0]); internalKnotsList.Add(spline2[spline2.Count - 1]);
                        }
                    }
                    else if (EvaluateT(spline, intersectingSplinePoint + (dir1.normalized * (width + 0.5f))) >= 1)
                    {
                        spline.SetKnot(spline.Count - 1, knot2); points[points.Count - 1] = intersectingSplinePoint + (dir1.normalized * (width + 0.5f));
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
                    if (EvaluateT(roads[j].road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) < 1 && EvaluateT(roads[j].road, intersectingSplinePoint - (dir2.normalized * (width + 0.5f))) > 0)
                    {
                        FilterPoints(roads[j].road, spline3, roads[j].points, roads[j].roadMap, t2, out points3, out spline3Points, out bool splitSpline);

                        if (splitSpline)
                        {

                            roads[j].road.Insert(0, knot3); roads[j].points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                            roads[j].roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot3 });
                            roads[j].resolution = (int)(roads[j].road.GetLength() * 2);

                            spline3.Add(knot4); points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                            spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot4 });

                            MakeSpline(spline3, spline3Points, points3, width, ID);
                            FilterIntersections(roads[j].road, spline3, t2);
                            internalRoadsList.Add(roads[j].road); internalRoadsList.Add(spline3);
                            internalKnotsList.Add(knot3); internalKnotsList.Add(knot4);
                        }
                        else
                        {
                            Vector3 dirA = (Vector3)SplineUtility.EvaluateTangent(roads[j].road, t2);
                            roads[j].points.Insert(0, intersectingSplinePoint + (dir2.normalized * (width + 0.5f)));
                            roads[j].roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { roads[j].road[0] });
                            m_SplineSampler.SampleSplinePoint(roads[j].road, intersectingSplinePoint + (dirA.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot1Pos, out float tx);
                            knot3.Position = knot1Pos;
                            knot3.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirA.normalized, Vector3.forward, Vector3.down), 0); roads[j].road[0] = knot3;

                            Vector3 dirB = (Vector3)SplineUtility.EvaluateTangent(spline3, t2);
                            points3.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)));
                            spline3Points.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { spline3[spline3.Count - 1] });
                            m_SplineSampler.SampleSplinePoint(spline3, intersectingSplinePoint - (dirB.normalized * (width + 0.5f)), (int)(spline.GetLength() * 2), out Vector3 knot2Pos, out float ty);
                            knot4.Position = knot2Pos;
                            knot4.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(dirB.normalized, Vector3.forward, Vector3.down), 0); spline3[spline3.Count - 1] = knot4;

                            MakeSpline(spline3, spline3Points, points3, width, ID);
                            FilterIntersections(roads[j].road, spline3, t2);
                            internalRoadsList.Add(roads[j].road); internalRoadsList.Add(spline3);
                            internalKnotsList.Add(roads[j].road[0]); internalKnotsList.Add(spline3[spline3.Count - 1]);
                        }
                    }
                    else if (EvaluateT(roads[j].road, intersectingSplinePoint + (dir2.normalized * (width + 0.5f))) >= 1)
                    {
                        roads[j].road.SetKnot(roads[j].road.Count - 1, knot4); roads[j].points[roads[j].points.Count - 1] = intersectingSplinePoint - (dir2.normalized * (width + 0.5f));
                        roads[j].resolution = (int)(roads[j].road.GetLength() * 2);
                        roads[j].roadMap.Add(intersectingSplinePoint - (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot4 });
                        roads[j].roadMap.Remove(intersectingSplinePoint);
                        internalRoadsList.Add(roads[j].road); internalKnotsList.Add(knot4);
                    }
                    else
                    {
                        roads[j].road.SetKnot(0, knot3); roads[j].points[0] = intersectingSplinePoint + (dir2.normalized * (width + 0.5f));
                        roads[j].resolution = (int)(roads[j].road.GetLength() * 2);
                        roads[j].roadMap.Add(intersectingSplinePoint + (dir2.normalized * (width + 0.5f)), new List<BezierKnot> { knot3 });
                        roads[j].roadMap.Remove(intersectingSplinePoint);
                        internalRoadsList.Add(roads[j].road); internalKnotsList.Add(knot3);
                    }

                    roadsList.Add(internalRoadsList);
                    knotsList.Add(internalKnotsList);
                }
            }
        }

        for (int k = 0; k < roadsList.Count; k++)
        {
            MakeIntersection(roadsList[k], knotsList[k]);
        }
        CleanRoads();
        CleanIntersections();
        MakeRoad();
    }

    private void MakeSpline(Spline spline, Dictionary<Vector3, List<BezierKnot>> splinePoints, List<Vector3> points, int width, int ID)
    {
        if (Vector3.Distance(points[0], points[^1]) > 0.01f)
        {
            m_SplineContainer.AddSpline(spline);
            GameObject c = new();
            c.name = $"Spline{roads.Count}";
            c.transform.SetParent(this.transform);
            c.AddComponent<MeshCollider>();
            c.layer = LayerMask.NameToLayer("Selector");
            roads.Add(new Roads(spline, splinePoints, points, (int)(spline.GetLength() * 2), width, c.GetComponent<MeshCollider>(), ID));
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
        collider.transform.SetParent(this.transform);
        collider.AddComponent<MeshCollider>();
        collider.layer = 6;
        AddJunction(intersection, collider.GetComponent<MeshCollider>());
    }

    public void SelectRoad(Vector3 position, Vector2Int size, int rotation, out Roads selectedRoad, out int index, out int width, out int iD, out List<Vector3> points)
    {
        Collider[] overlaps = Physics.OverlapBox(position, new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, 5, 0), LayerMask.GetMask("Selector"));
        selectedRoad = null; index = -1; width = 0; iD = -1; points = new();
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
                    iD = road.ID;
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

    public bool CheckRoadSelect(Vector3 position, Vector2Int size, int rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x + 0.05f, position.y, position.z + 0.05f), new Vector3(size.x/2f - 0.1f, 0.5f, size.y/2f - 0.1f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector") );
        bool ans = false;
        foreach (Roads road in roads)
        {
            Collider selector = road.collider;
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
}

[System.Serializable]
public class Roads
{
    public Spline road;
    public Dictionary<Vector3, List<BezierKnot>> roadMap;
    public List<Vector3> points;
    public int resolution;
    public int width;
    public int ID;
    public MeshCollider collider;

    public Roads(Spline road, Dictionary<Vector3, List<BezierKnot>> roadMap, List<Vector3> points, int resolution, int width, MeshCollider collider, int iD)
    {
        this.road = road;
        this.roadMap = roadMap;
        this.points = points;
        this.resolution = resolution;
        this.width = width;
        this.collider = collider;
        iD = ID;
    }
}