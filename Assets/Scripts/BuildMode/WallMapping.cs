using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using UnityEngine.Splines.ExtrusionShapes;

public class WallMapping : MonoBehaviour
{
    public List<Wall> walls;
    public List<Room> rooms;
    public List<Door> doors;
    public List<Window> windows;
    public List<Intersection> intersections;
    [SerializeField] private GameObject wallParent;
    [SerializeField] private GameObject floorParent;
    [SerializeField] private GameObject ceilParent;
    [SerializeField] private GameObject selectorObject;
    [SerializeField] private SplineSampler m_SplineSampler;
    [SerializeField] private SplineContainer m_SplineContainer;
    [SerializeField] private Material defaultWallMaterial;
    [SerializeField] private Material defaultFloorMaterial;
    [SerializeField] private Material selectorMaterial;
    [SerializeField] private PlacementSystem placementSystem;
    public float height;
    //[SerializeField] private GameObject gizmos;

    public void MakeWalls()
    {
        BuildMesh();
    }

    private void GetVerts(int index, float d, out List<Vector3> vertsP1, out List<Vector3> vertsP2)
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

            m_SplineSampler.SampleSplineWidth(index, t, d, out p1, out p2);
            vertsP1.Add(p1);
            vertsP2.Add(p2);
        }

        m_SplineSampler.SampleSplineWidth(index, 1f, d, out p1, out p2);
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
            if (Vector3.Distance(points[i], SplineUtility.EvaluatePosition(wall, ratio)) < 0.04f)
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
        float uvOffset = 0;

        Mesh wall = new Mesh();
        wall.subMeshCount = 3;

        List<Vector3> vertsA = new List<Vector3>();
        List<Vector3> vertsB = new List<Vector3>();

        List<int> trisA = new List<int>();
        List<int> trisB = new List<int>();
        List<int> trisS = new List<int>();

        List<Vector3> normalsA = new();

        List<Vector2> uvs = new List<Vector2>();

        Mesh c = new Mesh();
        List<Vector3> vertsC = new List<Vector3>();
        List<int> trisC = new List<int>();

        int resolution = ((int)(walls[currentSplineIndex].resolution));
        for (int currentPointIndex = 1; currentPointIndex <= resolution; currentPointIndex++)
        {
            List<Vector3> currentVertsA = new();
            List<Vector3> currentVertsB = new();
            List<List<Vector3>> currentVertsC = new();
            List<List<Vector3>> currentVertsS = new();

            List<Vector2> currentUVsA = new();
            List<Vector2> currentUVsB = new();

            MapWallPoints(currentSplineIndex, currentPointIndex, resolution, 
                currentVertsA, currentVertsB, currentVertsC, currentVertsS,
                currentUVsA, currentUVsB, uvOffset, 
                out float uvDistance, out Vector3 crossDir1, out Vector3 crossDir2, out List<Vector3> crossDirC,  out List<Vector3> crossDirS);

            List<int> currentTrisA = CreateTris(vertsA, currentVertsA, crossDir1, uvs, currentUVsA);
            List<int> currentTrisB = CreateTris(vertsA, currentVertsB, crossDir2, uvs, currentUVsB);

            List<int> currentTrisS = new();
            for (int i = 0; i < currentVertsS.Count; i++)
            {
                currentTrisS.AddRange(CreateTris(vertsA, currentVertsS[i], crossDirS[i]));
            }

            List<int> currentTrisC = new();
            for (int i = 0; i < currentVertsC.Count; i++)
            {
                currentTrisC.AddRange(CreateTris(vertsC, currentVertsC[i], crossDirC[i]));
            }

            trisA.AddRange(currentTrisA);
            trisB.AddRange(currentTrisB);
            trisS.AddRange(currentTrisS);
            trisC.AddRange(currentTrisC);
        }

        wall.SetVertices(vertsA);
        wall.SetTriangles(trisS, 0);
        wall.SetTriangles(trisA, 1);
        wall.SetTriangles(trisB, 2);

        c.SetVertices(vertsC);
        c.SetTriangles(trisC, 0);
        walls[currentSplineIndex].collider.sharedMesh = c;

        wall.SetUVs(0, uvs);
        wall.RecalculateNormals();
        wall.RecalculateTangents();
        walls[currentSplineIndex].mesh.mesh = wall;
    }

    private bool DetectDoorPresence(int currentSplineIndex, float minStartT, float maxStartT, float minEndT, float maxEndT, out List<Door> doorsIntersect)
    {
        List<Door> doorsWall = doors.FindAll(item => item.targetWall == walls[currentSplineIndex]);
        doorsIntersect = new();
        if (doorsWall.Count >= 1)
        {
            foreach (Door door in doorsWall)
            {
                if (door.isReverse)
                {
                    if (
                        EvaluateT(walls[currentSplineIndex].wall, door.point) >= minEndT && EvaluateT(walls[currentSplineIndex].wall, door.point) <= maxEndT &&
                        EvaluateT(walls[currentSplineIndex].wall, door.point - ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (door.length))) >= minStartT &&
                        EvaluateT(walls[currentSplineIndex].wall, door.point - ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (door.length))) <= maxStartT
                    )
                    {
                        Debug.Log($"{EvaluateT(walls[currentSplineIndex].wall, door.point - ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (door.length)))} {EvaluateT(walls[currentSplineIndex].wall, door.point)} {minStartT} {maxStartT} {minEndT} {maxEndT}");
                        doorsIntersect.Add(door);
                    }
                }
                else
                {
                    if (
                        EvaluateT(walls[currentSplineIndex].wall, door.point) >= minStartT && EvaluateT(walls[currentSplineIndex].wall, door.point) <= maxStartT &&
                        EvaluateT(walls[currentSplineIndex].wall, door.point + ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (door.length))) >= minEndT &&
                        EvaluateT(walls[currentSplineIndex].wall, door.point + ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (door.length))) <= maxEndT
                    )
                    {
                        Debug.Log($"{EvaluateT(walls[currentSplineIndex].wall, door.point + ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (door.length)))} {EvaluateT(walls[currentSplineIndex].wall, door.point)} {minStartT} {maxStartT} {minEndT} {maxEndT}");
                        doorsIntersect.Add(door);
                    }
                }
            }
        }

        if (doorsIntersect.Count >= 1)
        {
            return true;
        }
        return false;
    }

    private bool DetectWindowPresence(int currentSplineIndex, float minStartT, float maxStartT, float minEndT, float maxEndT, out List<Window> windowsIntersect)
    {
        List<Window> windowsWall = windows.FindAll(item => item.targetWall == walls[currentSplineIndex]);
        windowsIntersect = new();
        if (windowsWall.Count >= 1)
        {
            foreach (Window window in windowsWall)
            {
                if (window.isReverse)
                {
                    if (
                        EvaluateT(walls[currentSplineIndex].wall, window.point) >= minEndT && EvaluateT(walls[currentSplineIndex].wall, window.point) <= maxEndT &&
                        EvaluateT(walls[currentSplineIndex].wall, window.point - ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (window.length))) >= minStartT &&
                        EvaluateT(walls[currentSplineIndex].wall, window.point - ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (window.length))) <= maxStartT
                    )
                    {
                        windowsIntersect.Add(window);
                    }
                }
                else
                {
                    if (
                        EvaluateT(walls[currentSplineIndex].wall, window.point) >= minStartT && EvaluateT(walls[currentSplineIndex].wall, window.point) <= maxStartT &&
                        EvaluateT(walls[currentSplineIndex].wall, window.point + ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (window.length))) >= minEndT &&
                        EvaluateT(walls[currentSplineIndex].wall, window.point + ((walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * (window.length))) <= maxEndT
                    )
                    {
                        windowsIntersect.Add(window);
                    }
                }
            }
        }

        if (windowsIntersect.Count >= 1)
        {
            return true;
        }
        return false;
    }

    private void MapWallPoints(int currentSplineIndex, int currentPointIndex, float resolution, 
        List<Vector3> currentVertsA, List<Vector3> currentVertsB, List<List<Vector3>> currentVertsC, List<List<Vector3>> currentVertsS, 
        List<Vector2> currentUVsA, List<Vector2> currentUVsB, float uvOffset, 
        out float uvDistance, out Vector3 crossDir1, out Vector3 crossDir2, out List<Vector3> crossDirC, out List<Vector3> crossDirS)
    {
        //Get the coordinates of the wall at start
        m_SplineSampler.SampleSplineWidth(currentSplineIndex, (float)(currentPointIndex - 1) / resolution, 0.04f, out Vector3 point1, out Vector3 point2);
        m_SplineSampler.SampleSplineWidth(currentSplineIndex, (float)(currentPointIndex) / resolution, 0.04f, out Vector3 point3, out Vector3 point4);
        float distance = Vector3.Distance(point3, point1);
        uvDistance = uvOffset + distance;

        crossDir1 = Vector3.Cross(point3 - point1, Vector3.up);
        crossDir2 = -Vector3.Cross(point4 - point2, Vector3.up);
        crossDirS = new();
        crossDirC = new();

        List<(Vector3, int, float)> values = new();

        Vector3 h1 = Vector3.zero;

        //Start of the wall segment
        DetectDoorPresence(currentSplineIndex, 0f, (float)(currentPointIndex - 1) / resolution, (float)(currentPointIndex - 1) / resolution, 1f, out List<Door> d1);
        DetectWindowPresence(currentSplineIndex, 0f, (float)(currentPointIndex - 1) / resolution, (float)(currentPointIndex - 1) / resolution, 1f, out List<Window> w1);

        foreach (Door d in d1)
        {
            if (d.height > h1.y)
            {
                h1 = new Vector3(0, d.height, 0);
            }
        }
        foreach (Window w in w1)
        {
            if (w.height > h1.y)
            {
                h1 = new Vector3(0, w.height + w.point.y, 0);
            }
        }

        currentVertsA.AddRange(new List<Vector3>
        {
            point1 + new Vector3(0, height, 0), point1 + h1
        });
        currentVertsB.AddRange(new List<Vector3>
        {
            point2 + new Vector3(0, height, 0), point2 + h1
        });
        currentUVsA.AddRange(new List<Vector2>
        {
            new Vector2(uvOffset, 1), new Vector2(uvOffset, h1.y/height)
        });
        currentUVsB.AddRange(new List<Vector2>
        {
            new Vector2(uvOffset, 1), new Vector2(uvOffset, h1.y/height)
        });

        if (currentPointIndex == 1)
        {
            currentVertsS.Add(new List<Vector3>
            {
                point1 + new Vector3(0, height, 0), point1 + h1, point2 + h1, point2 + new Vector3(0, height, 0)
            });
            crossDirS.Add(Vector3.Cross(point2 - point1, Vector3.up));

            currentVertsC.Add(new List<Vector3>
            {
                point1 + new Vector3(0, height, 0), point1 + h1, point2 + h1, point2 + new Vector3(0, height, 0)
            });
            crossDirC.Add(Vector3.Cross(point2 - point1, Vector3.up));
        }

        //Find doors or windows in the middle of the wall segment [ENDING]
        bool hasDoorAtEnd = DetectDoorPresence(currentSplineIndex, 0f, 1f, (float)(currentPointIndex - 1) / resolution, (float)(currentPointIndex) / resolution, out List<Door> d2);
        bool hasWindowAtEnd = DetectWindowPresence(currentSplineIndex, 0f, 1f, (float)(currentPointIndex - 1) / resolution, (float)(currentPointIndex) / resolution, out List<Window> w2);
        if (hasDoorAtEnd)
        {
            foreach (Door d in d2)
            {
                if (d.isReverse)
                {
                    values.Add((d.point, 1, d.height));
                }
                else
                {
                    values.Add((d.point + (walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * d.length, 1, d.height));
                }
            }
        }
        if (hasWindowAtEnd)
        {
            foreach (Window w in w2)
            {
                if (w.isReverse)
                {
                    values.Add((w.point, 1, w.height + w.point.y));
                }
                else
                {
                    values.Add((w.point + (walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * w.length, 1, w.height + w.point.y));
                }
            }
        }

        bool hasDoorAtStart = DetectDoorPresence(currentSplineIndex, (float)(currentPointIndex - 1) / resolution, (float)(currentPointIndex) / resolution, 0f, 1f, out List<Door> d3);
        bool hasWindowAtStart = DetectWindowPresence(currentSplineIndex, (float)(currentPointIndex - 1) / resolution, (float)(currentPointIndex) / resolution, 0f, 1f, out List<Window> w3);
        if (hasDoorAtStart)
        {
            foreach (Door d in d3)
            {
                if (d.isReverse)
                {
                    values.Add((d.point - (walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * d.length, 0, d.height));
                    
                }
                else
                {
                    values.Add((d.point, 0, d.height));
                }
            }
        }
        if (hasWindowAtStart)
        {
            foreach (Window w in w3)
            {
                if (w.isReverse)
                {
                    values.Add((w.point - (walls[currentSplineIndex].points[^1] - walls[currentSplineIndex].points[0]).normalized * w.length, 0, w.height + w.point.y));
                    
                }
                else
                {
                    values.Add((w.point, 0, w.height + w.point.y));
                }
            }
        }

        values.Sort((a, b) =>
        {
            float aT = EvaluateT(walls[currentSplineIndex].wall, a.Item1);
            float bT = EvaluateT(walls[currentSplineIndex].wall, b.Item1);

            if (aT < bT)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        });

        int sortIndex = 0;

        while (sortIndex < values.Count)
        {
            if (values[sortIndex].Item2 == 0)
            {
                if (sortIndex + 1 < values.Count)
                {
                    if (values[sortIndex + 1].Item2 == 0)
                    {
                        values.RemoveAt(sortIndex + 1);
                    }
                }
                else sortIndex++;
            }
            else
            {
                if (sortIndex > 0)
                {
                    if (values[sortIndex - 1].Item2 != 0)
                    {
                        values.RemoveAt(sortIndex - 1);
                    }
                }
                else sortIndex++;
            }
        }

        foreach ((Vector3, int, float) value in values)
        {
            m_SplineSampler.SampleSplineWidth(currentSplineIndex, EvaluateT(walls[currentSplineIndex].wall, value.Item1), 0.04f, out Vector3 pointA, out Vector3 pointB);
            float uvLocalDistance = uvOffset + Vector3.Distance(pointA, point1);

            if (value.Item2 == 0)
            {
                currentVertsA.AddRange(new List<Vector3>
                {
                    pointA, pointA + new Vector3(0, value.Item3, 0)
                });
                currentVertsB.AddRange(new List<Vector3>
                {
                    pointB, pointB + new Vector3(0, value.Item3, 0)
                });
                currentUVsA.AddRange(new List<Vector2>
                {
                    new Vector2(uvLocalDistance, 0), new Vector2(uvLocalDistance, (value.Item3)/height)
                });
                currentUVsB.AddRange(new List<Vector2>
                {
                    new Vector2(uvLocalDistance, 0), new Vector2(uvLocalDistance, (value.Item3)/height)
                });
            }
            else
            {
                currentVertsA.AddRange(new List<Vector3>
                {
                    pointA + new Vector3(0, value.Item3, 0), pointA
                });
                currentVertsB.AddRange(new List<Vector3>
                {
                    pointB + new Vector3(0, value.Item3, 0), pointB
                });
                currentUVsA.AddRange(new List<Vector2>
                {
                    new Vector2(uvLocalDistance, (value.Item3)/height), new Vector2(uvLocalDistance, 0)
                });
                currentUVsB.AddRange(new List<Vector2>
                {
                    new Vector2(uvLocalDistance, (value.Item3)/height), new Vector2(uvLocalDistance, 0)
                });
            }
        }

        Vector3 h2 = Vector3.zero;

        DetectDoorPresence(currentSplineIndex, 0f, (float)(currentPointIndex) / resolution, (float)(currentPointIndex) / resolution, 1f, out List<Door> d4);
        DetectWindowPresence(currentSplineIndex, 0f, (float)(currentPointIndex) / resolution, (float)(currentPointIndex) / resolution, 1f, out List<Window> w4);

        foreach (Door d in d4)
        {
            if (d.height > h2.y)
            {
                h2 = new Vector3(0, d.height, 0);
            }
        }
        foreach (Window w in w4)
        {
            if (w.height > h2.y)
            {
                h2 = new Vector3(0, w.height + w.point.y, 0);
            }
        }

        currentVertsA.AddRange(new List<Vector3>
        {
            point3 + h2, point3 + new Vector3(0, height, 0)
        });
        currentVertsB.AddRange(new List<Vector3>
        {
            point4 + h2, point4 + new Vector3(0, height, 0)
        });
        currentUVsA.AddRange(new List<Vector2>
        {
            new Vector2(uvDistance, h2.y/height), new Vector2(uvDistance, 1)
        });
        currentUVsB.AddRange(new List<Vector2>
        {
            new Vector2(uvDistance, h2.y/height), new Vector2(uvDistance, 1)
        });

        if (currentPointIndex == resolution)
        {
            currentVertsS.Add(new List<Vector3>
            {
                point4 + new Vector3(0, height, 0), point4 + h2, point3 + h2, point3 + new Vector3(0, height, 0)
            });
            crossDirS.Add(Vector3.Cross(point4 - point3, Vector3.up));

            currentVertsC.Add(new List<Vector3>
            {
                point4 + new Vector3(0, height, 0), point4 + h2, point3 + h2, point3 + new Vector3(0, height, 0)
            });
            crossDirC.Add(Vector3.Cross(point4 - point3, Vector3.up));
        }

        currentVertsC.Add(new List<Vector3>
        {
            point1 + new Vector3(0, height, 0), point1, point3, point3 + new Vector3(0, height, 0)
        });
        crossDirC.Add(crossDir1);

        currentVertsC.Add(new List<Vector3>
        {
            point2 + new Vector3(0, height, 0), point2, point4, point4 + new Vector3(0, height, 0)
        });
        crossDirC.Add(crossDir2);

        currentVertsC.Add(new List<Vector3>
        {
            point3 + new Vector3(0, height, 0), point1 + new Vector3(0, height, 0), point2 + new Vector3(0, height, 0), point4 + new Vector3(0, height, 0)
        });
        crossDirC.Add(Vector3.up);

        currentVertsC.Add(new List<Vector3>
        {
            point4, point2, point1, point3
        });
        crossDirC.Add(Vector3.down);

        currentVertsS.Add(new List<Vector3>
        {
            point3 + new Vector3(0, height, 0), point1 + new Vector3(0, height, 0), point2 + new Vector3(0, height, 0), point4 + new Vector3(0, height, 0)
        });
        crossDirS.Add(Vector3.up);
    }

    private List<int> CreateTris(List<Vector3> totalVerts, List<Vector3> newVerts, Vector3 cross, List<Vector2> totalUVs = null, List<Vector2> newUVs = null)
    {
        List<int> tris = new();
        float angle = GetAngle(newVerts, cross);

        if (angle < 0)
        {
            angle *= -1;
            newVerts.Reverse();
        }

        List<Vector3> remainingVerts = new();
        remainingVerts.AddRange(newVerts);

        for (int j = 0; j < newVerts.Count; j++)
        {
            if (!totalVerts.Contains(newVerts[j]))
            {
                totalVerts.Add(newVerts[j]);
                totalUVs?.Add((Vector2)(newUVs?[j]));
            }
        }

        int i = 0;
        while (remainingVerts.Count > 2)
        {
            //Debug.Log(i);
            Vector3 point1 = remainingVerts[i % remainingVerts.Count];
            Vector3 point2 = remainingVerts[(i + 1) % remainingVerts.Count];
            Vector3 point3 = remainingVerts[(i + 2) % remainingVerts.Count];

            float localAngle = Vector3.SignedAngle(point3 - point2, point1 - point2, cross);


            if (localAngle == 0 || Math.Abs(localAngle) == 180)
            {
                tris.AddRange(new List<int> {totalVerts.IndexOf(point1), totalVerts.IndexOf(point2), totalVerts.IndexOf(point3)});
                remainingVerts.Remove(point2);
            }
            else if (point1 == point2 || point2 == point3)
            {
                remainingVerts.Remove(point2);
            }
            else if (Math.Sign(localAngle) == Math.Sign(angle))
            {
                int vertSteps = ((i + 2) % remainingVerts.Count > i % remainingVerts.Count) ? remainingVerts.Count - (((i + 2) % remainingVerts.Count) - (i % remainingVerts.Count)) : ((i + 2) % remainingVerts.Count) - (i % remainingVerts.Count);

                for (int j = 0; j < vertSteps; j++)
                {
                    Vector3 pointJ = remainingVerts[(i + j + 2) % remainingVerts.Count];

                    float originalAngle = Vector3.SignedAngle(point3 - point1, point2 - point1, cross);
                    float compAngle = Vector3.SignedAngle(point3 - point1, pointJ - point1, cross);

                    if (Math.Sign(originalAngle) == Math.Sign(compAngle) && Math.Abs(compAngle) < Math.Abs(originalAngle) && Vector3.Distance(point1, pointJ) < Vector3.Distance(point1, point3))
                    {
                        break;
                    }

                    if (j == vertSteps - 1)
                    {
                        tris.AddRange(new List<int> {totalVerts.IndexOf(point1), totalVerts.IndexOf(point2), totalVerts.IndexOf(point3)});
                        remainingVerts.Remove(point2);
                    }
                }
            }
            
            i++;

            if (i >= Math.Pow(newVerts.Count, 2))
            {
                Debug.LogError($"Maximum allowed iterations elapsed!");
                Debug.Log($"newVerts: {string.Join(",", newVerts)} remaining: {string.Join(",", remainingVerts)}");
                Debug.Log($"tris: {string.Join(",", tris)}");
                Debug.Log($"{localAngle}, {angle}");
                break;
            }
        }

        return tris;
    }

    private void BuildIntersection(int i)
    {
        int offset = 0; float uvOffset = 0;

        Intersection intersection = intersections[i];
        Vector3 center = new Vector3();
        List<(Intersection.JunctionEdge, int, int)> junctionEdges = new();

        foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
        {
            int splineIndex = junction.GetSplineIndex(m_SplineContainer);
            float t = junction.knotIndex == 0 ? 0f : 1f;
            m_SplineSampler.SampleSplineWidth(splineIndex, t, 0.04f, out Vector3 p1, out Vector3 p2);
            //if knot index is 0 we are facing away from the junction, otherwise we are facing the junction
            if (junction.knotIndex == 0)
            {
                junctionEdges.Add((new Intersection.JunctionEdge(p1, p2), splineIndex, 1));
            }
            else
            {
                junctionEdges.Add((new Intersection.JunctionEdge(p2, p1), splineIndex, 2));
            }
            center += p1;
            center += p2;
        }

        center /= (junctionEdges.Count * 2);

        junctionEdges.Sort((x, y) =>
        {
            Vector3 xDir = x.Item1.center - center;
            Vector3 yDir = y.Item1.center - center;

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
        List<Vector3> normals = new();
        offset = 0; uvOffset = 0;

        mesh.subMeshCount = 1 + junctionEdges.Count;
        MeshRenderer renderer = collider.gameObject.GetComponent<MeshRenderer>();
        Material[] newMaterials = new Material[1 + junctionEdges.Count];
        newMaterials[0] = defaultWallMaterial;

        for (int j = 0; j < junctionEdges.Count; j++)
        {
            if (junctionEdges.Count > 1)
            {
                Vector3 a = junctionEdges[j].Item1.right;
                Vector3 b = junctionEdges[j].Item1.left;
                Vector3 c = (j < junctionEdges.Count - 1) ? junctionEdges[j + 1].Item1.right : junctionEdges[0].Item1.right;

                vertices.AddRange(new List<Vector3> { a, b, c });
                vertices.AddRange(new List<Vector3> { a + new Vector3(0, height, 0), b + new Vector3(0, height, 0), c + new Vector3(0, height, 0) });
                normals.AddRange(new List<Vector3> { Vector3.Cross(a - b, Vector3.up).normalized, Vector3.Cross(b - c, Vector3.up).normalized, Vector3.Cross(b - c, Vector3.up).normalized });
                normals.AddRange(new List<Vector3> { Vector3.Cross(a - b, Vector3.up).normalized, Vector3.Cross(b - c, Vector3.up).normalized, Vector3.Cross(b - c, Vector3.up).normalized });

                offset = j * 6;
                trisS.AddRange(new List<int> { offset + 0, offset + 1, offset + 2 });
                trisS.AddRange(new List<int> { offset + 3, offset + 4, offset + 5 });
                trisS.AddRange(new List<int> { offset + 0, offset + 1, offset + 4, offset + 4, offset + 3, offset + 0 });
                trisS.AddRange(new List<int> { offset + 0, offset + 3, offset + 4, offset + 4, offset + 1, offset + 0 });
                trisA.Add(new List<int> { offset + 1, offset + 2, offset + 5, offset + 5, offset + 4, offset + 1, offset + 1, offset + 4, offset + 5, offset + 5, offset + 2, offset + 1 });
                //trisC.AddRange(new List<int> { offset + 1, offset + 2, offset + 5, offset + 5, offset + 4, offset + 1, offset + 1, offset + 4, offset + 5, offset + 5, offset + 2, offset + 1 });

                float distanceA = Vector3.Distance(a, b) / 4f;
                float distanceB = Vector3.Distance(b, c) / 4f;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset + distanceA, 0), new Vector2(uvOffset + distanceA + distanceB, 0),
                    new Vector2(uvOffset, 1), new Vector2(uvOffset + distanceA, 1), new Vector2(uvOffset + distanceA + distanceB, 1)});

                uvOffset += distanceA + distanceB;
                newMaterials[j + 1] = walls[junctionEdges[j].Item2].renderer.materials[junctionEdges[j].Item3];
            }
            else
            {
                Vector3 a = junctionEdges[j].Item1.right;
                Vector3 b = junctionEdges[j].Item1.left;

                vertices.AddRange(new List<Vector3> { a, b, a + new Vector3(0, height, 0), b + new Vector3(0, height, 0) });
                normals.AddRange(new List<Vector3> { Vector3.Cross(a - b, Vector3.up).normalized, Vector3.Cross(a - b, Vector3.up).normalized, Vector3.Cross(a - b, Vector3.up).normalized, Vector3.Cross(a - b, Vector3.up).normalized });

                offset = j * 4;
                trisA.Add(new List<int> { offset + 0, offset + 2, offset + 3, offset + 3, offset + 1, offset + 0 });
                //trisC.AddRange(new List<int> { offset + 0, offset + 2, offset + 3, offset + 3, offset + 1, offset + 0 });

                float distanceA = Vector3.Distance(a, b);
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset + distanceA, 0),
                    new Vector2(uvOffset, 1), new Vector2(uvOffset + distanceA, 1)});

                uvOffset += distanceA;

                newMaterials[j + 1] = walls[junctionEdges[j].Item2].renderer.materials[junctionEdges[j].Item3];
            }
        }

        List<Vector3> colVert = new();
        Vector3 p1x = center + new Vector3(-0.4f, 0, -0.4f);
        Vector3 p2x = center + new Vector3(-0.4f, 0, 0.4f);
        Vector3 p3x = center + new Vector3(0.4f, 0, -0.4f);
        Vector3 p4x = center + new Vector3(0.4f, 0, 0.4f);
        Vector3 p5x = p1x + new Vector3(0, height, 0);
        Vector3 p6x = p2x + new Vector3(0, height, 0);
        Vector3 p7x = p3x + new Vector3(0, height, 0);
        Vector3 p8x = p4x + new Vector3(0, height, 0);

        //Debug.Log($"{p1} {p2} {p3} {p4}");

        int t1 = 0;
        int t2 = 2;
        int t3 = 3;
        int t4 = 3;
        int t5 = 1;
        int t6 = 0;

        int t7 = 4;
        int t8 = 6;
        int t9 = 7;
        int t10 = 7;
        int t11 = 5;
        int t12 = 4;

        int t13 = 0;
        int t14 = 2;
        int t15 = 6;
        int t16 = 6;
        int t17 = 4;
        int t18 = 0;

        int t19 = 2;
        int t20 = 3;
        int t21 = 7;
        int t22 = 7;
        int t23 = 6;
        int t24 = 2;

        int t25 = 3;
        int t26 = 1;
        int t27 = 5;
        int t28 = 5;
        int t29 = 7;
        int t30 = 3;

        int t31 = 1;
        int t32 = 0;
        int t33 = 4;
        int t34 = 4;
        int t35 = 5;
        int t36 = 1;

        colVert.AddRange(new List<Vector3> { p1x, p2x, p3x, p4x, p5x, p6x, p7x, p8x });
        trisC.AddRange(new List<int> { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12,
        t13, t14, t15, t16, t17, t18, t19, t20, t21, t22, t23, t24,
        t25, t26, t27, t28, t29, t30, t31, t32, t33, t34, t35, t36});

        renderer.sharedMaterials = newMaterials;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(trisS, 0);
        mesh.SetNormals(normals);
        for (int m = 1; m <= trisA.Count; m++)
        {
            mesh.SetTriangles(trisA[m - 1], m);
        }
        mesh.SetUVs(0, uvs);
        intersection.mesh.mesh = mesh;

        col.SetVertices(colVert);
        //trisC.AddRange(trisS);
        col.SetTriangles(trisC, 0);
        //col.SetUVs(0, uvs);
        collider.sharedMesh = col;
    }

    private void BuildRoom(int i)
    {
        List<Vector3> verts = new(); List<Vector3> vertsB = new();
        List<Vector3> verts2 = new(); List<Vector3> verts2B = new();
        List<int> tris = new(); List<int> trisB = new();
        List<int> tris2 = new(); List<int> tris2B = new();
        List<Vector2> uvs = new(); List<Vector2> uvs2 = new();
        List<Vector3> normals = new(); List<Vector3> normals2 = new();
        Mesh mesh = new Mesh(); Mesh ceilingMesh = new Mesh();
        Mesh colliderMesh = new Mesh();

        float angle = GetAngle(rooms[i].points, Vector3.up);
        
        List<Wall> roomWalls = new();
        for (int pointIndex = 0; pointIndex < rooms[i].points.Count; pointIndex++)
        {
            Wall thisWall = walls.Find(item => item.points.FindIndex(obj => Vector3.Distance(obj, rooms[i].points[(pointIndex + 1) % rooms[i].points.Count]) < 0.1f) != -1 && item.points.FindIndex(obj => Vector3.Distance(obj, rooms[i].points[pointIndex]) < 0.1f) != -1);
            roomWalls.Add(thisWall);
        }

        List<Vector3> points = new();
        List<Vector3> points2 = new();

        foreach (Vector3 point in rooms[i].points)
        {
            points.Add(transform.TransformPoint(point));
            points2.Add(transform.TransformPoint(point) + new Vector3(0, height, 0));
        }

        BuildRoomPoints(roomWalls, 0.02f, points, angle, verts, tris, uvs);
        BuildRoomPoints(roomWalls, -0.02f, points2, angle, verts2, tris2, uvs2);

        mesh.subMeshCount = 1;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        ceilingMesh.subMeshCount = 1;
        ceilingMesh.SetVertices(verts2);
        ceilingMesh.SetTriangles(tris2, 0);
        ceilingMesh.SetUVs(0, uvs2);
        ceilingMesh.SetNormals(normals2);

        colliderMesh.SetVertices(verts);
        colliderMesh.SetTriangles(tris, 0);

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

    private Vector3 GetMinPoint(float x, float minY, float maxY, float angle, List<Vector3> points, out Vector3 nearest, out Wall wall)
    {
        float midY = (minY + maxY) / 2f;
        Vector3 point = new Vector3(x, points[0].y, midY);
        wall = GetNearestWall(points, point, angle, out nearest, out Vector3 tp2);
        Vector3 dirA = Vector3.Cross(wall.points[^1] - wall.points[0], Vector3.up).normalized;
        if (angle > 0) //reverse angle
        {
            dirA *= -1;
        }
        if (Vector3.Distance(wall.points[^1], tp2) > 0.1f)
        {
            dirA *= -1;
        }

        if (minY >= maxY)
        {
            return point;
        }
        else
        {
            if (Vector3.Angle(dirA, point - nearest) < 90 || Vector3.Distance(point, nearest) < 0.01f)
            {
                return GetMinPoint(x, minY, midY, angle, points, out nearest, out wall);
            }
            else
            {
                return GetMinPoint(x, midY, maxY, angle, points, out nearest, out wall);
            }
        }
    }
    private Vector3 GetMaxPoint(float x, float minY, float maxY, float angle, List<Vector3> points, out Vector3 nearest, out Wall wall)
    {
        float midY = (minY + maxY) / 2f;
        Vector3 point = new Vector3(x, points[0].y, midY);
        wall = GetNearestWall(points, point, angle, out nearest, out Vector3 tp2);
        Vector3 dirA = Vector3.Cross(wall.points[^1] - wall.points[0], Vector3.up).normalized;
        if (angle > 0) //reverse angle
        {
            dirA *= -1;
        }
        if (Vector3.Distance(wall.points[^1], tp2) > 0.1f)
        {
            dirA *= -1;
        }

        if (minY >= maxY)
        {
            return point;
        }
        else
        {
            if (Vector3.Angle(dirA, point - nearest) < 90 || Vector3.Distance(point, nearest) < 0.01f)
            {
                return GetMaxPoint(x, midY, maxY, angle, points, out nearest, out wall);
            }
            else
            {
                return GetMaxPoint(x, minY, midY, angle, points, out nearest, out wall);
            }
        }
    }

    private void BuildRoomPoints(List<Wall> wallObjects, float height, List<Vector3> points, float angle, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        List<Vector2> uv1 = new();
        for (int i = 0; i < points.Count; i++)
        {
            uv1.Add(new Vector2(points[i].x - points[0].x, points[i].y - points[0].y));
        }

        tris.AddRange(CreateTris(verts, points, height >= 0 ? Vector3.down : Vector3.up, uvs, uv1));

        List<Vector3> points2 = new();
        for (int i = 0; i < points.Count; i++)
        {
            points2.Add(points[i] + new Vector3(0, height, 0));
        }

        tris.AddRange(CreateTris(verts, points2, height >= 0 ? Vector3.up : Vector3.down, uvs, uv1));

        for (int i = 1; i < points.Count; i++)
        {
            List<Vector3> sidePoints = new List<Vector3> {points2[^(i)], points[i - 1], points[i], points2[^(i + 1)]};
            Vector3 cross = angle >= 0? Vector3.Cross(points[i] - points[i - 1], Vector3.up).normalized : Vector3.Cross(points[i - 1] - points[i], Vector3.up).normalized;
            tris.AddRange(CreateTris(verts, sidePoints, cross));
        }
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
                    //int indexR = combinedSpline.IndexOf(intersect.junctions[a].knot);
                    ///float rT = combinedSpline.ConvertIndexUnit(indexR, PathIndexUnit.Knot, PathIndexUnit.Normalized);
                    float rT = EvaluateT(combinedSpline, intersect.junctions[a].knot.Position);
                    Debug.Log($"{rT} {sT}");
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
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].points.Count == knots.Count)
            {
                for (int j = 0; j < knots.Count; j++)
                {
                    if (rooms[i].points.FindIndex(item => item == (Vector3)knots[j].Position) == -1)
                    {
                        break;
                    }

                    if (j == knots.Count - 1)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private float GetAngle(List<BezierKnot> points)
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

        return angle;
    }

    private float GetAngle(List<Vector3> points, Vector3 cross)
    {
        float angle = 0;
        float angler = 0;
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
        {
            angler += Vector3.SignedAngle(points[(pointIndex + 2) % points.Count] - points[(pointIndex + 1) % points.Count], points[(pointIndex + 0) % points.Count] - points[(pointIndex + 1) % points.Count], cross);
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
                float localAngle = Vector3.SignedAngle(points[(pointIndex + 2) % points.Count] - points[(pointIndex + 1) % points.Count], points[(pointIndex + 0) % points.Count] - points[(pointIndex + 1) % points.Count], cross);
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

        return angle;
    }

    private bool IsRoomMeshContinuous(List<BezierKnot> points)
    {
        float angle = GetAngle(points);

        if (!SmallestAngleInIntersection(points, 1, angle))
        {
            return false;
        }

        float length = 0;
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
        {
            if (walls.FindIndex(item => item.points.FindIndex(obj => Vector3.Distance(obj, points[(pointIndex + 1) % points.Count].Position) < 0.1f) != -1 && item.points.FindIndex(obj => Vector3.Distance(obj, points[(pointIndex) % points.Count].Position) < 0.1f) != -1) == -1)
            {
                return false;
            }
            length += Vector3.Distance(points[(pointIndex + 1) % points.Count].Position, points[pointIndex].Position);
        }

        Spline s1 = walls.Find(item => item.points.FindIndex(obj => Vector3.Distance(obj, points[0].Position) < 0.1f) != -1 && item.points.FindIndex(obj => Vector3.Distance(obj, points[1].Position) < 0.1f) != -1).wall;
        float minLength = DetermineMinLength(s1, points[1], transform.TransformPoint(points[0].Position), angle, (s1.IndexOf(points[0]) == 0 ? 1 : 0), points, new(), Vector3.Distance(points[1].Position, points[0].Position));
        if (minLength < length)
        {
            return false;
        }
        return true;
    }

    private Wall GetNearestWall(List<Vector3> points, Vector3 point, float angle, out Vector3 nearest, out Vector3 p2)
    {
        float minDistance = Mathf.Infinity; nearest = Vector3.zero; p2 = Vector3.zero;
        Wall selectedWall = null;

        for (int i = 1; i <= points.Count; i++)
        {
            Vector3 tp1 = points[i - 1];
            Vector3 tp2 = points[i % points.Count];

            Wall wall = walls.Find(item => item.points.FindIndex(obj => Vector3.Distance(obj, tp1) < 0.1f) != -1 && item.points.FindIndex(obj => Vector3.Distance(obj, tp2) < 0.1f) != -1);
            Vector3 np = GetNearestPoint(wall.points[0], wall.points[^1], point);
            float thisDistance = Vector3.SqrMagnitude(point - np);


            if (thisDistance < minDistance)
            {
                selectedWall = wall;
                minDistance = thisDistance;
                nearest = np;
                p2 = tp2;
            }
            else if (thisDistance == minDistance)
            {
                Vector3 dirA = Vector3.Cross(wall.points[^1] - wall.points[0], Vector3.up).normalized;
                //m_SplineSampler.SampleSplinePoint(selectedWall.wall, point, selectedWall.resolution, out Vector3 np2, out float t2);
                Vector3 np2 = GetNearestPoint(selectedWall.points[0], selectedWall.points[^1], point);
                Vector3 dirB = Vector3.Cross(selectedWall.points[^1] - selectedWall.points[0], Vector3.up).normalized;
                if (angle > 0) //reverse angle
                {
                    dirA *= -1;
                    dirB *= -1;
                }
                if (Vector3.Distance(wall.points[^1], tp2) > 0.1f)
                {
                    dirA *= -1;
                }
                if (Vector3.Distance(selectedWall.points[^1], p2) > 0.1f)
                {
                    dirB *= -1;
                }

                float thisAngle = Vector3.Angle(dirA, point - np);
                float nearAngle = Vector3.Angle(dirB, point - np2);

                if (thisAngle < nearAngle)
                {
                    selectedWall = wall;
                    minDistance = thisDistance;
                    nearest = np;
                    p2 = tp2;
                }
            }
        }

        return selectedWall;
    }

    private Wall GetNearestWall(Vector3 point, float maxDistance, out Vector3 nearest, out float t)
    {
        float minDistance = Mathf.Infinity; nearest = Vector3.zero; t = -1;
        Wall selectedWall = null;

        for (int i = 0; i < walls.Count; i++)
        {
            Wall wall = walls[i];
            Vector3 np = GetNearestPoint(wall.points[0], wall.points[^1], point, out float thisT);
            float thisDistance = Vector3.SqrMagnitude(point - np);

            if (thisDistance < minDistance && thisDistance < maxDistance)
            {
                selectedWall = wall;
                minDistance = thisDistance;
                nearest = np;
                t = thisT;
            }
            else if (thisDistance == minDistance && thisDistance < maxDistance)
            {
                Vector3 dirA = Vector3.Cross(wall.points[^1] - wall.points[0], Vector3.up).normalized;
                //m_SplineSampler.SampleSplinePoint(selectedWall.wall, point, selectedWall.resolution, out Vector3 np2, out float t2);
                Vector3 np2 = GetNearestPoint(selectedWall.points[0], selectedWall.points[^1], point);
                Vector3 dirB = Vector3.Cross(selectedWall.points[^1] - selectedWall.points[0], Vector3.up).normalized;

                float thisAngle = Vector3.Angle(dirA, point - np);
                float nearAngle = Vector3.Angle(dirB, point - np2);

                if (thisAngle < nearAngle)
                {
                    selectedWall = wall;
                    minDistance = thisDistance;
                    nearest = np;
                    t = thisT;
                }
            }
        }

        return selectedWall;
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

    private static Vector3 GetNearestPoint(Vector3 start, Vector3 end, Vector3 point, out float t)
    {
        var wander = point - start;
        var span = end - start;

        // Compute how far along the line is the closest approach to our point.
        t = Vector3.Dot(wander, span) / span.sqrMagnitude;

        // Restrict this point to within the line segment from start to end.
        t = Mathf.Clamp01(t);

        Vector3 nearest = start + t * span;
        return nearest;
    }

    private void CleanWalls()
    {
        List<Wall> deleteWalls = new();

        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i].wall.GetLength() < 0.5f)
            {
                deleteWalls.Add(walls[i]);
            }
        }

        for (int i = 0; i < deleteWalls.Count; i++)
        {
            Destroy(deleteWalls[i].collider.gameObject);
            Destroy(deleteWalls[i].mesh.gameObject);
            DeleteWall(deleteWalls[i]);
            walls.Remove(deleteWalls[i]);
        }
    }

    private void CleanRooms()
    {
        List<Room> deleteRooms = new();

        for (int i = 0; i < rooms.Count; i++)
        {
            bool isContinuous = IsRoomMeshContinuous(rooms[i].knotList);
            if (!isContinuous)
            {
                deleteRooms.Add(rooms[i]);
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


        List<Intersection> deleteIntersections = new();
        for (int i = 0; i < intersections.Count; i++)
        {
            if (!deleteIntersections.Contains(intersections[i]))
            {
                Vector3 center = CalculateIntersectionCenter(intersections[i]);
                List<Intersection> commonIntersections = intersections.FindAll(item => Vector3.Distance(CalculateIntersectionCenter(item), center) < 0.5f && item != intersections[i]);

                foreach (Intersection intersection in commonIntersections)
                {
                    foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                    {
                        if (intersections[i].junctions.FindIndex(item => item.spline == junction.spline) != -1)
                        {
                            BezierKnot knot = junction.knotIndex == 0 ? junction.spline[0] : junction.spline[^1];
                            intersections[i].AddJunction(junction.spline, knot, 0.5f);
                        }
                    }

                    deleteIntersections.Add(intersection);
                }
            }
        }

        foreach (Intersection intersection in deleteIntersections)
        {
            Destroy(intersection.collider.gameObject);
            intersections.Remove(intersection);
        }
    }

    private bool EditKnotsInIntersection(Spline mainSpline, int knotIndex, Vector3 position, out List<Spline> intersectList)
    {
        intersectList = new();
        bool isEdit = false;
        foreach (Intersection intersection in intersections)
        {
            if (intersection.junctions.FindIndex(item => item.spline == mainSpline && item.knotIndex == knotIndex) != -1)
            {
                for (int i = 0; i < intersection.junctions.Count; i++)
                {
                    if (i != intersection.junctions.FindIndex(item => item.spline == mainSpline && item.knotIndex == knotIndex))
                    {
                        if (walls.FindIndex(item => item.wall == intersection.junctions[i].spline) != -1)
                        {
                            Wall wall = walls.Find(item => item.wall == intersection.junctions[i].spline);
                            wall.points[intersection.junctions[i].knotIndex] = position;
                            BuildSplineKnots(wall.wall, wall.points);
                            Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(wall.wall, wall.wall[intersection.junctions[i].knotIndex]);
                            intersection.junctions[i] = newInfo;
                            BuildWall(walls.IndexOf(wall));
                            intersectList.Add(wall.wall);
                        }
                    }
                }

                BuildIntersection(intersections.IndexOf(intersection));
                isEdit = true;
            }
        }
        return isEdit;
    }

    private void EditKnotsInIntersection(Intersection intersection, Vector3 position)
    {
        for (int i = 0; i < intersection.junctions.Count; i++)
        {
            if (walls.FindIndex(item => item.wall == intersection.junctions[i].spline) != -1)
            {
                Wall wall = walls.Find(item => item.wall == intersection.junctions[i].spline);
                wall.points[intersection.junctions[i].knotIndex] = position;
                BuildSplineKnots(wall.wall, wall.points);
                Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(wall.wall, wall.wall[intersection.junctions[i].knotIndex]);
                intersection.junctions[i] = newInfo;

                BuildWall(walls.IndexOf(wall));
            }
        }

        BuildIntersection(intersections.IndexOf(intersection));
    }

    private bool SmallestAngleInIntersection(List<BezierKnot> knots, int knotIndex, float angle)
    {
        if (knotIndex > 0 && knotIndex < knots.Count) //returns true if exceeding index limit
        {
            Intersection intersection = intersections.Find(item => item.junctions.FindIndex(obj => obj.knot.Equals(knots[knotIndex])) != -1);

            float thisAngle = Vector3.SignedAngle(knots[(knotIndex + 1) % knots.Count].Position - knots[knotIndex].Position, knots[knotIndex - 1].Position - knots[knotIndex].Position, Vector3.up);
            if (thisAngle < 0 && angle >= 0)
            {
                thisAngle += 360;
            }
            else if (thisAngle > 0 && angle <= 0)
            {
                thisAngle -= 360;
            }

            if (intersection == null)
            {
                return false;
            }

            foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
            {
                BezierKnot nextKnot = junction.knotIndex == 0 ? junction.spline.Next(junction.knotIndex) : junction.spline.Previous(junction.knotIndex);

                if (Vector3.Distance(knots[knotIndex - 1].Position, nextKnot.Position) >= 0.1f)
                {
                    float compareAngle = Vector3.SignedAngle(nextKnot.Position - knots[knotIndex].Position, knots[knotIndex - 1].Position - knots[knotIndex].Position, Vector3.up);
                    if (compareAngle < 0 && angle >= 0)
                    {
                        compareAngle += 360;
                    }
                    else if (compareAngle > 0 && angle <= 0)
                    {
                        compareAngle -= 360;
                    }

                    if (Mathf.Abs(compareAngle) < Mathf.Abs(thisAngle))
                    {
                        return false;
                    }
                }
            }

            return SmallestAngleInIntersection(knots, knotIndex + 1, angle);
        }
        else return true;
    }

    private float DetermineMinLength(Spline spline, BezierKnot knot, Vector3 target, float angle, int direction, List<BezierKnot> points, List<BezierKnot> knotList, float distance)
    {
        knotList.Add(knot);
        if (Vector3.Distance(knot.Position, target) < 0.1f)
        {
            return distance;
        }

        List<float> items = new();
        if (spline.IndexOf(knot) - 1 >= 0 && direction == 0)
        {
            List<BezierKnot> newKnotList = new();
            for (int i = 0; i < knotList.Count; i++)
            {
                newKnotList.Add(knotList[i]);
            }
            items.Add(DetermineMinLength(spline, spline[spline.IndexOf(knot) - 1], target, angle, direction, points, newKnotList, distance + Vector3.Distance(spline[spline.IndexOf(knot) - 1].Position, knot.Position)));
        }
        else if (spline.IndexOf(knot) + 1 < spline.Count && direction == 1)
        {
            List<BezierKnot> newKnotList = new();
            for (int i = 0; i < knotList.Count; i++)
            {
                newKnotList.Add(knotList[i]);
            }
            items.Add(DetermineMinLength(spline, spline[spline.IndexOf(knot) + 1], target, angle, direction, points, newKnotList, distance + Vector3.Distance(spline[spline.IndexOf(knot) + 1].Position, knot.Position)));
        }

        else
        {
            List<Intersection> nextJunctions = new();
            for (int i = 0; i < intersections.Count; i++)
            {
                if (SplineInIntersection(intersections[i], spline, knot))
                {
                    //Debug.Log($"Current spline in intersection with {i} at {knot.Position}");
                    nextJunctions.Add(intersections[i]);
                }
            }

            List<BezierKnot> possibleNextKnots = new();
            List<Intersection.JunctionInfo> possibleNextJuncts = new();

            foreach (Intersection intersection in nextJunctions)
            {
                foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                {
                    if (junction.spline != spline)
                    {
                        int splineIndex = junction.GetSplineIndex(m_SplineContainer);
                        BezierKnot nextKnot = junction.knotIndex == 0 ? m_SplineContainer[splineIndex].Next(junction.knotIndex) : m_SplineContainer[splineIndex].Previous(junction.knotIndex);
                        int dir = junction.knotIndex == 0 ? 1 : 0;

                        if (!knotList.Contains(nextKnot))
                        {
                            possibleNextKnots.Add(nextKnot);
                            possibleNextJuncts.Add(junction);
                        }
                    }
                }
            }

            if (possibleNextKnots.Count > 0)
            {
                Vector3 prevPos = knotList.Count >= 2 ? knotList[^2].Position : target;
                int nextIndex = 0;
                float currentSmallAngle = Vector3.SignedAngle(possibleNextKnots[0].Position - knot.Position, prevPos - (Vector3)knot.Position, Vector3.up);
                if (angle < 0)
                {
                    if (currentSmallAngle > 0)
                    {
                        currentSmallAngle -= 360;
                    }
                }
                else
                {
                    if (currentSmallAngle < 0)
                    {
                        currentSmallAngle += 360;
                    }
                }
                currentSmallAngle = Mathf.Abs(currentSmallAngle);

                for (int i = 1; i < possibleNextKnots.Count; i++)
                {
                    float nextAngle = Vector3.SignedAngle(possibleNextKnots[i].Position - knot.Position, prevPos - (Vector3)knot.Position, Vector3.up);
                    
                    if (angle < 0)
                    {
                        if (nextAngle > 0)
                        {
                            nextAngle -= 360;
                        }
                    }
                    else
                    {
                        if (nextAngle < 0)
                        {
                            nextAngle += 360;
                        }
                    }

                    if (Mathf.Abs(nextAngle) < currentSmallAngle)
                    {
                        nextIndex = i;
                        currentSmallAngle = Mathf.Abs(nextAngle);
                    }
                }

                BezierKnot currentKnot = possibleNextKnots[nextIndex];
                Intersection.JunctionInfo currentJunction = possibleNextJuncts[nextIndex];

                List<BezierKnot> newKnotList = new();
                for (int j = 0; j < knotList.Count; j++)
                {
                    newKnotList.Add(knotList[j]);
                }
                items.Add(DetermineMinLength(currentJunction.spline, currentKnot, target, angle, currentJunction.knotIndex == 0 ? 1 : 0, points, newKnotList, distance + Vector3.Distance(currentKnot.Position, knot.Position)));
                //Debug.Log($"{knot.Position} {currentKnot.Position} {angle}");

            }
        }

        if (items.Count == 0)
        {
            return Mathf.Infinity;
        }
        else
        {
            float min = Mathf.Infinity;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] < min)
                {
                    min = items[i];
                }
            }
            return min;
        }
    }

    private void CreateRoom(Spline spline, BezierKnot knot, int direction, List<BezierKnot> knotList)
    {
        int repIndex = knotList.IndexOf(knot);
        if (repIndex != -1)
        {
            if (knotList.Count >= 3)
            {
                List<BezierKnot> newKnotList = knotList.GetRange(repIndex, knotList.Count - repIndex);

                if (!HasRoomWithPoints(newKnotList) && newKnotList.Count >= 3 && IsRoomMeshContinuous(newKnotList))
                {
                    Debug.Log(string.Join(",", newKnotList));
                    MakeRoom(newKnotList);
                }
            }
            return;
        }
        knotList.Add(knot);

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
                Debug.Log($"Current spline in intersection with {i} at {knot.Position}");
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
                    //Debug.Log($"{(Vector3)knot.Position} to {(Vector3)nextKnot.Position} at {splineIndex}");
                    int dir = junction.knotIndex == 0 ? 1 : 0;

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

    public Vector3 CalculateIntersectionCenter(Intersection intersection)
    {
        Vector3 center = new();
        
        for (int i = 1; i < intersection.junctions.Count(); i++)
        {                            
            Vector3 tangent1 = intersection.junctions[i].knotIndex == 0 ? -Vector3.Normalize(intersection.junctions[i].spline.EvaluateTangent(0)) : Vector3.Normalize(intersection.junctions[i].spline.EvaluateTangent(1));
            Vector3 tangent2 = intersection.junctions[0].knotIndex == 0 ? -Vector3.Normalize(intersection.junctions[0].spline.EvaluateTangent(0)) : Vector3.Normalize(intersection.junctions[0].spline.EvaluateTangent(1));
            Vector3 localCenter = GetPointCenter((Vector3)intersection.junctions[i].knot.Position, tangent1, (Vector3)intersection.junctions[0].knot.Position, tangent2);
            center += localCenter;
        }

        center /= (intersection.GetJunctions().Count() - 1);

        return transform.TransformPoint(placementSystem.SmoothenPosition(center));
    }

    private void CreateIntersection(Spline spline, Vector3 p1, Vector3 p2, List<Vector3> splinePoints, List<List<Spline>> wallsList, List<List<BezierKnot>> knotsList, List<int> intersectionBuild, bool isEdit, Wall wall)
    {
        List<Vector3> hitRemoveList = new();
        List<RaycastHit> hitList = new();

        //Uses a boxcast to determine intersection points
        RaycastHit[] hits1 = Physics.BoxCastAll(p1, new Vector3(0.04f, height, 0.25f), (p2 - p1).normalized, Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, p2 - p1, Vector3.up), 0), Vector3.Distance(p1, p2) + 0.04f, LayerMask.GetMask("Selector"));
        RaycastHit[] hits2 = Physics.BoxCastAll(p2, new Vector3(0.04f, height, 0.25f), (p1 - p2).normalized, Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, p1 - p2, Vector3.up), 0), Vector3.Distance(p1, p2) + 0.04f, LayerMask.GetMask("Selector"));
        List<RaycastHit> hits = new(); hits.AddRange(hits1); hits.AddRange(hits2);
        foreach (RaycastHit hit in hits)
        {
            if (!HitsContainCollider(hitList, hit) && !(hit.point == Vector3.zero && p2 != Vector3.zero))
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

                    Vector3 center = new();
                    
                    foreach (Intersection.JunctionInfo junction in intersections[j].GetJunctions())
                    {                            
                        Vector3 tangent1 = junction.knotIndex == 0 ? Vector3.Normalize(junction.spline.EvaluateTangent(0)) : Vector3.Normalize(junction.spline.EvaluateTangent(1));
                        Unity.Mathematics.float3 hitSplinePoint = new();
                        SplineUtility.GetNearestPoint(spline, (Vector3)junction.knot.Position, out hitSplinePoint, out float tx, (int)((spline.GetLength() * 2)));
                        Vector3 tangent2 = Vector3.Normalize(spline.EvaluateTangent(tx));
                        center += GetPointCenter((Vector3)junction.knot.Position, tangent1, (Vector3)hitSplinePoint, tangent2);
                        //Debug.Log($"{junction.knot.Position} {GetPointCenter((Vector3)junction.knot.Position, tangent1, (Vector3)hitSplinePoint, tangent2)}");
                    }
                    center /= intersections[j].GetJunctions().Count();

                    Vector3 intersectingSplinePoint = placementSystem.SmoothenPosition(center);

                    //determine the direction to move the splines in
                    float t1 = EvaluateT(spline, intersectingSplinePoint);
                    Debug.Log($"{center} {intersectingSplinePoint} {placementSystem.SmoothenPosition(center)}");

                    if (intersections[j].junctions.FindIndex(item => item.spline == spline) == -1)
                    {
                        Debug.Log(intersectingSplinePoint);
                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint) == 0f || Vector3.Distance(splinePoints[0], intersectingSplinePoint) < 0.5f)
                        {
                            //spline.SetKnot(0, knot1);
                            intersections[j].AddJunction(spline, spline[0], 0.5f);
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint) == 1f || Vector3.Distance(splinePoints[^1], intersectingSplinePoint) < 0.5f)
                        {
                            //spline.SetKnot(spline.Count - 1, knot2);
                            intersections[j].AddJunction(spline, spline[^1], 0.5f);
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint) < 1f && EvaluateT(spline, intersectingSplinePoint) > 0f
                            && Vector3.Distance(splinePoints[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(splinePoints[^1], intersectingSplinePoint) >= 0.5f)
                        {
                            FilterPoints(spline, spline2, splinePoints, t1, out spline2Points);
                            splinePoints.Insert(0, intersectingSplinePoint);
                            spline2Points.Add(intersectingSplinePoint);

                            BuildSplineKnots(spline, splinePoints);
                            BuildSplineKnots(spline2, spline2Points);

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
                            MakeSpline(spline2, spline2Points);
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
                                    BuildWall(w);
                                }
                            }
                        }
                    }

                    hitRemoveList.Add(intersectingSplinePoint);
                    intersectionBuild.Add(j);
                    Debug.Log(j);
                }
            }
        }

        foreach (RaycastHit hit in hitList)
        {
            for (int j = 0; j < walls.Count; j++)
            {
                if (hit.collider == walls[j].collider && hitRemoveList.FindIndex(item => Vector3.Distance(item, hit.point) < 0.5f) == -1 && !(isEdit && walls[j] == wall))
                {
                    Spline spline2 = new Spline();
                    List<Vector3> spline2Points = new();
                    Spline spline3 = new Spline();
                    List<Vector3> spline3Points = new();

                    List<Spline> internalWallList = new();
                    List<BezierKnot> internalKnotList = new();

                    Unity.Mathematics.float3 thisSplinePoint = new(); Unity.Mathematics.float3 otherSplinePoint = new();
                    SplineUtility.GetNearestPoint(spline, transform.InverseTransformPoint(hit.point), out thisSplinePoint, out float t1, (int)((spline.GetLength() * 2)));
                    SplineUtility.GetNearestPoint(walls[j].wall, transform.InverseTransformPoint(hit.point), out otherSplinePoint, out float t2, walls[j].resolution);
                    
                    Vector3 dir1 = (Vector3)SplineUtility.EvaluateTangent(spline, t1);
                    Vector3 dir2 = (Vector3)SplineUtility.EvaluateTangent(walls[j].wall, t2);
                    Vector3 intersectingSplinePoint = placementSystem.SmoothenPosition(GetPointCenter(thisSplinePoint, dir1.normalized, otherSplinePoint, dir2.normalized)); //used to prioritise the existing road coordinates in the creation of an intersection
                    t1 = EvaluateT(spline, intersectingSplinePoint);
                    t2 = EvaluateT(walls[j].wall, intersectingSplinePoint);

                    if (hitRemoveList.FindIndex(item => Vector3.Distance(item, intersectingSplinePoint) < 0.5f) == -1)
                    {
                        Debug.Log($"{hit.point} {intersectingSplinePoint} {thisSplinePoint} {otherSplinePoint}");
                        //insert incoming spline
                        if (EvaluateT(spline, intersectingSplinePoint) == 0f || Vector3.Distance(splinePoints[0], intersectingSplinePoint) < 0.5f)
                        {
                            internalWallList.Add(spline); internalKnotList.Add(spline[0]);
                        }
                        else if (EvaluateT(spline, intersectingSplinePoint) == 1f || Vector3.Distance(splinePoints[^1], intersectingSplinePoint) < 0.5f)
                        {
                            internalWallList.Add(spline); internalKnotList.Add(spline[^1]);
                        }
                        else if (Vector3.Distance(splinePoints[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(splinePoints[^1], intersectingSplinePoint) >= 0.5f)
                        {
                            FilterPoints(spline, spline2, splinePoints, t1, out spline2Points);
                            splinePoints.Insert(0, intersectingSplinePoint);
                            spline2Points.Add(intersectingSplinePoint);

                            BuildSplineKnots(spline, splinePoints);
                            BuildSplineKnots(spline2, spline2Points);
                            Debug.Log($"{string.Join(",", splinePoints)} {string.Join(",", spline2Points)}");

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
                            MakeSpline(spline2, spline2Points);
                            //CreateRoom(spline2, spline2[spline2.Count - 1], 0, new List<BezierKnot>());
                        }

                        //insert overlapping spline
                        if (EvaluateT(walls[j].wall, intersectingSplinePoint) == 0f || Vector3.Distance(walls[j].points[0], intersectingSplinePoint) < 0.5f)
                        {
                            internalWallList.Add(walls[j].wall); internalKnotList.Add(walls[j].wall[0]);
                            walls[j].resolution = (int)(walls[j].wall.GetLength() * 2);
                            BuildWall(j);
                        }
                        else if (EvaluateT(walls[j].wall, intersectingSplinePoint) == 1f || Vector3.Distance(walls[j].points[^1], intersectingSplinePoint) < 0.5f)
                        {
                            internalWallList.Add(walls[j].wall); internalKnotList.Add(walls[j].wall[^1]);
                            walls[j].resolution = (int)(walls[j].wall.GetLength() * 2);
                            BuildWall(j);
                        }
                        else if (EvaluateT(walls[j].wall, intersectingSplinePoint) > 0f && EvaluateT(walls[j].wall, intersectingSplinePoint) < 1f && Vector3.Distance(walls[j].points[0], intersectingSplinePoint) >= 0.5f && Vector3.Distance(walls[j].points[^1], intersectingSplinePoint) >= 0.5f)
                        {
                            FilterPoints(walls[j].wall, spline3, walls[j].points, t2, out spline3Points);
                            walls[j].points.Insert(0, intersectingSplinePoint);
                            spline3Points.Add(intersectingSplinePoint);
                            
                            BuildSplineKnots(walls[j].wall, walls[j].points);
                            BuildSplineKnots(spline3, spline3Points);
                            walls[j].resolution = (int)(walls[j].wall.GetLength() * 2);
                            Debug.Log($"{string.Join(",", walls[j].points)} {string.Join(",", spline3Points)}");

                            MakeSpline(spline3, spline3Points);
                            FilterIntersections(walls[j].wall, spline3, t2);
                            internalWallList.Add(walls[j].wall); internalWallList.Add(spline3);
                            internalKnotList.Add(walls[j].wall[0]); internalKnotList.Add(spline3[^1]);
                            BuildWall(j);
                        }

                        if (internalWallList.Count > 0)
                        {
                            wallsList.Add(internalWallList);
                            knotsList.Add(internalKnotList);
                        }
                    }

                    hitRemoveList.Add(intersectingSplinePoint);
                }
            }
        }
    }

    private void BuildSplineKnots(Spline spline, List<Vector3> points)
    {
        spline.Clear();
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 p1 = points[i - 1];
            Vector3 p2 = points[i];

            BezierKnot k1 = new BezierKnot();
            k1.Position = p1;
            k1.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0);
            k1.TangentIn = new Unity.Mathematics.float3(0, 0, 0.1f);
            k1.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

            BezierKnot k2 = new BezierKnot();
            k2.Position = p2;
            k2.Rotation = Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0);
            k2.TangentIn = new Unity.Mathematics.float3(0, 0, -0.1f);
            k2.TangentOut = new Unity.Mathematics.float3(0, 0, 0.1f);

            spline.Add(k1); spline.Add(k2);
        }
    }

    public void AddWalls(List<Vector3> points)
    {
        for (int i = 1; i < points.Count; i++)
        {
            Spline spline = new Spline();
            List<Vector3> splinePoints = new();

            Vector3 p1 = transform.InverseTransformPoint(points[i - 1]);
            Vector3 p2 = transform.InverseTransformPoint(points[i]);
            splinePoints.Add(p1); splinePoints.Add(p2); //gets only the two points
            BuildSplineKnots(spline, splinePoints);

            List<List<Spline>> wallsList = new();
            List<List<BezierKnot>> knotsList = new();
            List<int> intersectionBuild = new();
            CreateIntersection(spline, transform.TransformPoint(p1), transform.TransformPoint(p2), splinePoints, wallsList, knotsList, intersectionBuild, false, null);
            
            MakeSpline(spline, splinePoints);
            for (int k = 0; k < wallsList.Count; k++)
            {
                MakeIntersection(wallsList[k], knotsList[k]);
            }
            foreach (int index in intersectionBuild)
            {
                BuildIntersection(index);
            }

            CleanWalls();
            CleanRooms();
            CleanIntersections();
            CreateRoom(spline, spline[0], 1, new List<BezierKnot>());
            //CreateRoom(spline, spline[^1], 0, new List<BezierKnot>());
            //MakeWalls();
        }
    }

    public void ModifyWalls(Wall wall, List<Vector3> points)
    {
        Spline spline = wall.wall;
        List<Vector3> pointsList = points;
        List<Spline> intersectList = new();

        pointsList[0] = transform.InverseTransformPoint(points[0]);
        pointsList[^1] = transform.InverseTransformPoint(points[^1]);
        Vector3 p1 = pointsList[0];
        Vector3 p2 = pointsList[^1];
        BuildSplineKnots(spline, pointsList);
        ModifyWindowsInWall(wall);

        bool isIntersect1 = EditKnotsInIntersection(spline, 0, p1, out List<Spline> newIntersectList);
        intersectList.AddRange(newIntersectList);

        bool isIntersect2 = EditKnotsInIntersection(spline, spline.Count - 1, p2, out List<Spline> newIntersectList2);
        intersectList.AddRange(newIntersectList2);

        wall.wall = spline;
        List<int> intersectionBuild = new();
        foreach (Intersection intersection in intersections)
        {
            if (intersection.junctions.FindIndex(item => item.spline == wall.wall) != -1)
            {
                Intersection.JunctionInfo junction = intersection.junctions.Find(item => item.spline == wall.wall);
                if (junction.knotIndex == 0)
                {
                    Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(spline, spline[0]);
                    intersection.junctions[intersection.junctions.IndexOf(junction)] = newInfo;

                    BuildIntersection(intersections.IndexOf(intersection));
                }
                else
                {
                    Intersection.JunctionInfo newInfo = new Intersection.JunctionInfo(spline, spline[^1]);
                    intersection.junctions[intersection.junctions.IndexOf(junction)] = newInfo;

                    BuildIntersection(intersections.IndexOf(intersection));
                }
            }
        }

        CleanWalls();
        CleanIntersections();

        List<List<Spline>> wallsList = new();
        List<List<BezierKnot>> knotsList = new();
        
        CreateIntersection(spline, transform.TransformPoint(p1), transform.TransformPoint(p2), pointsList, wallsList, knotsList, intersectionBuild, true, wall);

        wall.wall = spline;
        wall.points = pointsList;
        wall.resolution = (int)(spline.GetLength() * 2);
        for (int k = 0; k < wallsList.Count; k++)
        {
            MakeIntersection(wallsList[k], knotsList[k]);
        }
        foreach (int index in intersectionBuild)
        {
            BuildIntersection(index);
        }
        BuildWall(walls.IndexOf(wall));

        foreach (Spline s in intersectList)
        {
            Wall w = walls.Find(item => item.wall == s);
            if (w != null)
            {
                List<Vector3> sPointsList = w.points;
                List<List<Spline>> wallsList2 = new();
                List<List<BezierKnot>> knotsList2 = new();
                List<int> intersectionBuild2 = new();
                CreateIntersection(s, transform.TransformPoint(w.points[0]), transform.TransformPoint(w.points[^1]), sPointsList, wallsList2, knotsList2, intersectionBuild2, true, w);

                w.wall = s;
                w.points = sPointsList;
                w.resolution = (int)(spline.GetLength() * 2);
                for (int k = 0; k < wallsList2.Count; k++)
                {
                    MakeIntersection(wallsList2[k], knotsList2[k]);
                }
                foreach (int index in intersectionBuild2)
                {
                    BuildIntersection(index);
                }

                BuildWall(walls.IndexOf(w));
            }
        }

        CleanWalls();
        CleanRooms();
        CleanIntersections();
        CreateRoom(spline, spline[0], 1, new List<BezierKnot>());
        //CreateRoom(spline, spline[^1], 0, new List<BezierKnot>());
        //MakeWalls();

    }

    public void ModifyIntersection(Intersection intersection, Vector3 position)
    {
        Vector3 pos = transform.InverseTransformPoint(position);
        EditKnotsInIntersection(intersection, pos);
        CleanWalls();
        CleanIntersections();

        for (int i = 0; i < intersection.junctions.Count; i++)
        {
            Wall wall = walls.Find(item => item.wall == intersection.junctions[i].spline);
            List<List<Spline>> wallsList = new();
            List<List<BezierKnot>> knotsList = new();
            List<int> intersectionBuild = new();
            CreateIntersection(wall.wall, transform.TransformPoint(wall.points[0]), transform.TransformPoint(wall.points[^1]), wall.points, wallsList, knotsList, intersectionBuild, true, wall);
            for (int k = 0; k < wallsList.Count; k++)
            {
                MakeIntersection(wallsList[k], knotsList[k]);
            }
            foreach (int index in intersectionBuild)
            {
                BuildIntersection(index);
            }

            BuildWall(walls.IndexOf(wall));
        }

        CleanWalls();
        CleanRooms();
        CleanIntersections();
        BuildIntersection(intersections.IndexOf(intersection));
    }

    private void ModifyWindowsInWall(Wall wall)
    {
        List<Door> wallDoors = doors.FindAll(item => item.targetWall == wall);
        List<Window> wallWindows = windows.FindAll(item => item.targetWall == wall);

        foreach (Door door in wallDoors)
        {
            float t = EvaluateT(wall.wall, door.point);
            Vector3 pos = wall.wall.EvaluatePosition(t);
            float rotation = Vector3.SignedAngle(Vector3.right, wall.points[^1] - wall.points[0], Vector3.up);
            Collider[] overlaps = Physics.OverlapBox(pos, door.prefab.transform.Find("Selector").localScale / 2, Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == wall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == wall);
            if ((otherDoors.Count == 0 || (otherDoors.Count == 1 && otherDoors.Contains(door))) && otherWindows.Count == 0)
            {
                if (wall.wall.GetLength() * (1 - t) <= door.length && !door.isReverse)
                {
                    RemoveDoor(door.prefab);
                }
                else if (wall.wall.GetLength() * t <= door.length && door.isReverse)
                {
                    RemoveDoor(door.prefab);
                }
            }
            else
            {
                RemoveDoor(door.prefab);
            }
        }

        foreach (Window window in wallWindows)
        {
            float t = EvaluateT(wall.wall, window.point);
            Vector3 pos = wall.wall.EvaluatePosition(t);
            float rotation = Vector3.SignedAngle(Vector3.right, wall.points[^1] - wall.points[0], Vector3.up);
            Collider[] overlaps = Physics.OverlapBox(pos, window.prefab.transform.Find("Selector").localScale / 2, Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == wall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == wall);
            if (otherDoors.Count == 0 && (otherWindows.Count == 0 || (otherWindows.Count == 1 && otherWindows.Contains(window))))
            {
                if (wall.wall.GetLength() * (1 - t) <= window.length && !window.isReverse)
                {
                    RemoveDoor(window.prefab);
                }
                else if (wall.wall.GetLength() * t <= window.length && window.isReverse)
                {
                    RemoveDoor(window.prefab);
                }
            }
            else
            {
                RemoveDoor(window.prefab);
            }
        }
    }

    public void BuildDoor(GameObject prefab, Vector3 point, float rotation, float length, float height, long ID, Wall targetWall, List<MatData> materials, bool isReverse)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = transform.TransformPoint(point);
        newObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        int index = 0;
        for (int i = 0; i < newObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                newObject.GetComponentsInChildren<Renderer>()[i].materials[j] = Instantiate(newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j]);
                newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                index++;
            }
        }
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, -0.5f);
        previewSelector.transform.localScale = new Vector3(1f, height - 0.1f, length - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        doors.Add(new Door(newObject, point, rotation, length, height, ID, targetWall, materials, isReverse));
        newObject.transform.SetParent(this.transform);

        BuildWall(walls.IndexOf(targetWall));
    }

    public void BuildWindow(GameObject prefab, Vector3 point, float rotation, float length, float height, long ID, Wall targetWall, List<MatData> materials, bool isReverse)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = transform.TransformPoint(point);
        newObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        int index = 0;
        for (int i = 0; i < newObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                newObject.GetComponentsInChildren<Renderer>()[i].materials[j] = Instantiate(newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j]);
                newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                index++;
            }
        }
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, -0.5f);
        previewSelector.transform.localScale = new Vector3(1f, height - 0.1f, length - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        windows.Add(new Window(newObject, point, rotation, length, height, ID, targetWall, materials, isReverse));
        newObject.transform.SetParent(this.transform);

        BuildWall(walls.IndexOf(targetWall));
    }

    public void BuildDoor(Door loadedDoor)
    {
        GameObject newObject = Instantiate(placementSystem.GetDoorPrefab(loadedDoor.ID));
        newObject.transform.position = transform.TransformPoint(loadedDoor.point);
        int index = 0;
        for (int i = 0; i < newObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                newObject.GetComponentsInChildren<Renderer>()[i].materials[j] = Instantiate(newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j]);
                newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = loadedDoor.materials[index].color;
                index++;
            }
        }
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, -0.5f);
        previewSelector.transform.localScale = new Vector3(1f, loadedDoor.height - 0.1f, loadedDoor.length - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, loadedDoor.rotation, 0);
        doors.Add(loadedDoor);
        newObject.transform.SetParent(this.transform);

        loadedDoor.prefab = newObject;
        if (walls.FindIndex(item => item.wall == loadedDoor.targetWall.wall) == -1)
        {
            if (walls.FindIndex(item => (Vector3)item.wall[0].Position == (Vector3)loadedDoor.targetWall.wall[0].Position && (Vector3)item.wall[^1].Position == (Vector3)loadedDoor.targetWall.wall[^1].Position) != -1)
            {
                loadedDoor.targetWall = walls.Find(item => (Vector3)item.wall[0].Position == (Vector3)loadedDoor.targetWall.wall[0].Position && (Vector3)item.wall[^1].Position == (Vector3)loadedDoor.targetWall.wall[^1].Position);
            }
        }
        BuildWall(walls.IndexOf(loadedDoor.targetWall));
    }

    public void BuildWindow(Window loadedWindow)
    {
        GameObject newObject = Instantiate(placementSystem.GetDoorPrefab(loadedWindow.ID));
        newObject.transform.position = transform.TransformPoint(loadedWindow.point);
        newObject.transform.rotation = Quaternion.Euler(0, loadedWindow.rotation, 0);
        int index = 0;
        for (int i = 0; i < newObject.GetComponentsInChildren<Renderer>().Length; i++)
        {
            for (int j = 0; j < newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
            {
                newObject.GetComponentsInChildren<Renderer>()[i].materials[j] = Instantiate(newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j]);
                newObject.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = loadedWindow.materials[index].color;
                index++;
            }
        }
        GameObject previewSelector = Instantiate(selectorObject);
        previewSelector.transform.SetParent(newObject.transform.transform);
        previewSelector.transform.name = "Selector";
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, -0.5f);
        previewSelector.transform.localScale = new Vector3(1f, loadedWindow.height - 0.1f, loadedWindow.length - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, loadedWindow.rotation, 0);
        windows.Add(loadedWindow);
        newObject.transform.SetParent(this.transform);

        loadedWindow.prefab = newObject;
        if (walls.FindIndex(item => item.wall == loadedWindow.targetWall.wall) == -1)
        {
            if (walls.FindIndex(item => (Vector3)item.wall[0].Position == (Vector3)loadedWindow.targetWall.wall[0].Position && (Vector3)item.wall[^1].Position == (Vector3)loadedWindow.targetWall.wall[^1].Position) != -1)
            {
                loadedWindow.targetWall = walls.Find(item => (Vector3)item.wall[0].Position == (Vector3)loadedWindow.targetWall.wall[0].Position && (Vector3)item.wall[^1].Position == (Vector3)loadedWindow.targetWall.wall[^1].Position);
            }
        }
        BuildWall(walls.IndexOf(loadedWindow.targetWall));
    }

    public void MoveDoors(Door door, Vector3 point, float rotation, float length, float height, long ID, Wall targetWall, List<MatData> materials, bool isReverse)
    {
        door.prefab.transform.position = transform.TransformPoint(point);
        door.prefab.transform.rotation = Quaternion.Euler(0, rotation, 0);
        GameObject previewSelector = door.prefab.transform.Find("Selector").gameObject;
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, -0.5f);
        previewSelector.transform.localScale = new Vector3(1f, height - 0.1f, length - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        previewSelector.GetComponentInChildren<Renderer>().material = selectorMaterial;

        int index = 0;
        for (int i = 0; i < door.prefab.GetComponentsInChildren<Renderer>().Length; i++)
        {
            if (door.prefab.GetComponentsInChildren<Renderer>()[i] != previewSelector.GetComponentInChildren<Renderer>())
            {
                for (int j = 0; j < door.prefab.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    door.prefab.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                    index++;
                }
            }
        }

        door.point = point;
        door.rotation = rotation;
        door.length = length;
        door.materials = materials;
        door.isReverse = isReverse;
        if (door.targetWall != targetWall)
        {
            int oldIndex = walls.IndexOf(door.targetWall);
            door.targetWall = targetWall;
            BuildWall(oldIndex);
        }

        BuildWall(walls.IndexOf(targetWall));
    }

    public void MoveWindows(Window window, Vector3 point, float rotation, float length, float height, long ID, Wall targetWall, List<MatData> materials, bool isReverse)
    {
        window.prefab.transform.position = transform.TransformPoint(point);
        window.prefab.transform.rotation = Quaternion.Euler(0, rotation, 0);
        GameObject previewSelector = window.prefab.transform.Find("Selector").gameObject;
        previewSelector.transform.localPosition = new Vector3(0.05f, 0f, -0.5f);
        previewSelector.transform.localScale = new Vector3(1f, height - 0.1f, length - 0.1f);
        previewSelector.transform.rotation = Quaternion.Euler(0, rotation, 0);
        previewSelector.GetComponentInChildren<Renderer>().material = selectorMaterial;

        int index = 0;
        for (int i = 0; i < window.prefab.GetComponentsInChildren<Renderer>().Length; i++)
        {
            if (window.prefab.GetComponentsInChildren<Renderer>()[i] != previewSelector.GetComponentInChildren<Renderer>())
            {
                for (int j = 0; j < window.prefab.GetComponentsInChildren<Renderer>()[i].sharedMaterials.Length; j++)
                {
                    window.prefab.GetComponentsInChildren<Renderer>()[i].sharedMaterials[j].color = materials[index].color;
                    index++;
                }
            }
        }

        window.point = point;
        window.rotation = rotation;
        window.length = length;
        window.materials = materials;
        window.isReverse = isReverse;
        if (window.targetWall != targetWall)
        {
            int oldIndex = walls.IndexOf(window.targetWall);
            window.targetWall = targetWall;
            BuildWall(oldIndex);
        }

        BuildWall(walls.IndexOf(targetWall));
    }

    public void RemoveDoor(GameObject prefab)
    {
        int index = doors.FindIndex(item => item.prefab == prefab);
        
        if (index != -1)
        {
            int oldIndex = walls.IndexOf(doors[index].targetWall);
            Destroy(doors[index].prefab);
            doors.Remove(doors[index]);

            BuildWall(oldIndex);
        }
    }

    public void RemoveWindow(GameObject prefab)
    {
        int index = windows.FindIndex(item => item.prefab == prefab);
        
        if (index != -1)
        {
            int oldIndex = walls.IndexOf(windows[index].targetWall);
            Destroy(windows[index].prefab);
            windows.Remove(windows[index]);

            BuildWall(oldIndex);
        }
    }

    private void MakeSpline(Spline spline, List<Vector3> points)
    {
        if (Vector3.Distance(points[0], points[^1]) >= 0.5f)
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
        for (int i = 0; i < splines.Count; i++)
        {
            if (walls.FindIndex(item => item.wall == splines[i]) == -1)
            {
                if (walls.FindIndex(item => (Vector3)item.wall[0].Position == (Vector3)splines[i][0].Position && (Vector3)item.wall[^1].Position == (Vector3)splines[i][^1].Position) != -1)
                {
                    splines[i] = walls.Find(item => (Vector3)item.wall[0].Position == (Vector3)splines[i][0].Position && (Vector3)item.wall[^1].Position == (Vector3)splines[i][^1].Position).wall;
                }
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
        collider.transform.SetParent(wallParent.transform);
        collider.AddComponent<MeshCollider>();
        collider.AddComponent<MeshFilter>();
        collider.AddComponent<MeshRenderer>();
        collider.layer = LayerMask.NameToLayer("Selector");

        intersections.Add(intersection);
        intersection.collider = collider.GetComponent<MeshCollider>();
        intersection.mesh = collider.GetComponent<MeshFilter>();
        intersection.renderer = collider.GetComponent<MeshRenderer>();

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
        collider.layer = LayerMask.NameToLayer("Selector");

        GameObject ceilingCollider = new();
        ceilingCollider.name = $"Ceiling{rooms.Count}";
        ceilingCollider.transform.SetParent(ceilParent.transform);
        ceilingCollider.AddComponent<MeshFilter>();
        ceilingCollider.AddComponent<MeshRenderer>();
        ceilingCollider.layer = LayerMask.NameToLayer("Selector");

        Room newRoom = new Room(pointsList, knotList, collider.GetComponent<MeshCollider>(), collider.GetComponent<MeshRenderer>(), collider.GetComponent<MeshFilter>(), ceilingCollider.GetComponent<MeshFilter>(), ceilingCollider.GetComponent<MeshRenderer>(), defaultFloorMaterial);
        rooms.Add(newRoom);

        BuildRoom(rooms.IndexOf(newRoom));
    }

    public void SelectWall(InputManager input, out Wall selectedWall, out int index, out List<Vector3> points)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        selectedWall = walls.Find(wall => overlapsList.FindIndex(col => col.collider == wall.collider) != -1); 
        index = -1; points = new();
        if (selectedWall != null)
        {
            index = walls.IndexOf(selectedWall);
            foreach (Vector3 point in selectedWall.points)
            {
                points.Add(transform.TransformPoint(point));
            }
        }
    }

    public void SelectIntersection(InputManager input, out Intersection selectedIntersection, out int index, out List<Wall> walls)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        selectedIntersection = intersections.Find(intersection => overlapsList.FindIndex(col => col.collider == intersection.collider) != -1); 
        index = -1; walls = new();
        if (selectedIntersection != null)
        {
            index = intersections.IndexOf(selectedIntersection);
            foreach (Intersection.JunctionInfo junction in selectedIntersection.GetJunctions())
            {
                Wall wall = this.walls.Find(item => item.wall == junction.spline);
                walls.Add(wall);
            }
        }
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

    public Door GetDoorSelect(Vector3 position, Vector2Int size, float rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x, position.y, position.z), new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        Door door = doors.Find(door => overlapsList.Contains(door.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()));
        return door;
    }

    public bool CheckDoorSelect(Vector3 position, Vector2Int size, float rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x, position.y, position.z), new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (doors.FindIndex(door => overlapsList.Contains(door.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>())) != -1)
        {
            return true;
        }
        return false;
    }

    public Window GetWindowSelect(Vector3 position, Vector2Int size, float rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x, position.y, position.z), new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        Window window = windows.Find(window => overlapsList.Contains(window.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()));
        return window;
    }

    public bool CheckWindowSelect(Vector3 position, Vector2Int size, float rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x, position.y, position.z), new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (windows.FindIndex(window => overlapsList.Contains(window.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>())) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckWallSelect(Vector3 position, Vector2Int size, float rotation)
    {
        Collider[] overlaps = Physics.OverlapBox(new Vector3(position.x, position.y, position.z), new Vector3(size.x / 2f, 0.5f, size.y / 2f), Quaternion.Euler(0, rotation, 0), LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (walls.FindIndex(wall => overlapsList.Contains(wall.collider)) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckWallSelect(GameObject previewSelector)
    {
        Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
        List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (walls.FindIndex(wall => overlapsList.Contains(wall.collider)) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckWallSelect(InputManager input)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (walls.FindIndex(wall => overlapsList.FindIndex(col => col.collider == wall.collider) != -1) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckIntersection(InputManager input)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        if (intersections.FindIndex(intersection => overlapsList.FindIndex(col => col.collider == intersection.collider) != -1) != -1)
        {
            return true;
        }
        return false;
    }

    public bool CheckWindowsFit(GameObject previewSelector, Vector3 position, float length, out Vector3 nearest, bool isReverse)
    {
        Wall nearestWall = GetNearestWall(position, 0.5f, out nearest, out float t);
        if (nearestWall != null)
        {
            //return true;
            Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            if (otherDoors.Count == 0 && otherWindows.Count == 0)
            {
                if (nearestWall.wall.GetLength() * (1 - t) <= length && !isReverse)
                {
                    nearest = nearestWall.points[^1] - ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                else if (nearestWall.wall.GetLength() * t <= length && isReverse)
                {
                    nearest = nearestWall.points[0] + ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                return true;
            }
            return false;
        }
        else return false;
    }

    public bool CheckWindowsMove(Door door, GameObject previewSelector, Vector3 position, float length, out Vector3 nearest, bool isReverse)
    {
        Wall nearestWall = GetNearestWall(position, 0.5f, out nearest, out float t);
        if (nearestWall != null)
        {
            //return true;
            Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            if (otherWindows.Count == 0 && (otherDoors.Count == 0 || (otherDoors.Count == 1 && otherDoors.Contains(door))))
            {
                if (nearestWall.wall.GetLength() * (1 - t) <= length && !isReverse)
                {
                    nearest = nearestWall.points[^1] - ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                else if (nearestWall.wall.GetLength() * t <= length && isReverse)
                {
                    nearest = nearestWall.points[0] + ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                return true;
            }
            return false;
        }
        else return false;
    }

    public bool CheckWindowsMove(Window window, GameObject previewSelector, Vector3 position, float length, out Vector3 nearest, bool isReverse)
    {
        Wall nearestWall = GetNearestWall(position, 0.5f, out nearest, out float t);
        if (nearestWall != null)
        {
            //return true;
            Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            if (otherDoors.Count == 0 && (otherWindows.Count == 0 || (otherWindows.Count == 1 && otherWindows.Contains(window))))
            {
                if (nearestWall.wall.GetLength() * (1 - t) <= length && !isReverse)
                {
                    nearest = nearestWall.points[^1] - ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                else if (nearestWall.wall.GetLength() * t <= length && isReverse)
                {
                    nearest = nearestWall.points[0] + ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                return true;
            }
            return false;
        }
        else return false;
    }

    public Wall GetWindowsFit(GameObject previewSelector, Vector3 position, float length, out Vector3 nearest, bool isReverse)
    {
        Wall nearestWall = GetNearestWall(position, 0.5f, out nearest, out float t);
        if (nearestWall != null)
        {
            Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            if (otherDoors.Count == 0 && otherWindows.Count == 0)
            {
                if (nearestWall.wall.GetLength() * (1 - t) <= length && !isReverse)
                {
                    nearest = nearestWall.points[^1] - ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                else if (nearestWall.wall.GetLength() * t <= length && isReverse)
                {
                    nearest = nearestWall.points[0] + ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                return nearestWall;
            }
            return null;
        }
        else return null;
    }

    public Wall GetWindowsMove(Door door, GameObject previewSelector, Vector3 position, float length, out Vector3 nearest, bool isReverse)
    {
        Wall nearestWall = GetNearestWall(position, 0.5f, out nearest, out float t);
        if (nearestWall != null)
        {
            //return true;
            Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            if (otherWindows.Count == 0 && (otherDoors.Count == 0 || (otherDoors.Count == 1 && otherDoors.Contains(door))))
            {
                if (nearestWall.wall.GetLength() * (1 - t) <= length && !isReverse)
                {
                    nearest = nearestWall.points[^1] - ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                else if (nearestWall.wall.GetLength() * t <= length && isReverse)
                {
                    nearest = nearestWall.points[0] + ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                return nearestWall;
            }
            return null;
        }
        else return null;
    }

    public Wall GetWindowsMove(Window window, GameObject previewSelector, Vector3 position, float length, out Vector3 nearest, bool isReverse)
    {
        Wall nearestWall = GetNearestWall(position, 0.5f, out nearest, out float t);
        if (nearestWall != null)
        {
            //return true;
            Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));
            List<Collider> overlapsList = new(); overlapsList.AddRange(overlaps);
            List<Door> otherDoors = doors.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            List<Window> otherWindows = windows.FindAll(item => overlapsList.Contains(item.prefab.transform.Find("Selector").GetChild(0).gameObject.GetComponent<Collider>()) && item.targetWall == nearestWall);
            if (otherDoors.Count == 0 && (otherWindows.Count == 0 || (otherWindows.Count == 1 && otherWindows.Contains(window))))
            {
                if (nearestWall.wall.GetLength() * (1 - t) <= length && !isReverse)
                {
                    nearest = nearestWall.points[^1] - ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                else if (nearestWall.wall.GetLength() * t <= length && isReverse)
                {
                    nearest = nearestWall.points[0] + ((nearestWall.points[^1] - nearestWall.points[0]).normalized * length);
                }
                return nearestWall;
            }
            return null;
        }
        else return null;
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
        CleanRooms();
        CleanIntersections();
        //MakeWalls();
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

    public WallMapSaveData GetWallMapSaveData()
    {
        return new WallMapSaveData(walls, intersections, rooms, doors, windows);
    }

    public void LoadSaveData(WallMapSaveData data)
    {
        for (int i = 0; i < data.walls.Count; i++)
        {
            if (!walls.Contains(data.walls[i]))
            {
                MakeSpline(data.walls[i].wall, data.walls[i].points);
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

        for (int i = 0; i < data.rooms.Count; i++)
        {
            if (!rooms.Contains(data.rooms[i]))
            {
                MakeRoom(data.rooms[i].knotList);
            }
        }

        for (int i = 0; i < data.doors.Count; i++)
        {
            if (!doors.Contains(data.doors[i]))
            {
                BuildDoor(data.doors[i]);
            }
        }

        for (int i = 0; i < data.windows.Count; i++)
        {
            if (!windows.Contains(data.windows[i]))
            {
                BuildWindow(data.windows[i]);
            }
        }
    }

    public void SetPlacementSystem(PlacementSystem system)
    {
        placementSystem = system;
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


        renderer.material = defaultFloorMaterial;
        ceilingRenderer.material = defaultFloorMaterial;

        /*
        Material[] newMaterials = new Material[2];
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            newMaterials[i] = renderer.sharedMaterials[i];
        }
        newMaterials[0] = defaultFloorMaterial;
        newMaterials[1] = defaultFloorMaterial;

        renderer.sharedMaterials = newMaterials;
        ceilingRenderer.sharedMaterials = newMaterials;
        */

    }
}

[System.Serializable]
public class Door
{
    public GameObject prefab;
    public Vector3 point;
    public float rotation;
    public float length;
    public float height;
    public long ID;
    public Wall targetWall;
    public List<MatData> materials;
    public bool isReverse;

    public Door(GameObject prefab, Vector3 point, float rotation, float length, float height, long ID, Wall targetWall, List<MatData> materials, bool isReverse)
    {
        this.prefab = prefab;
        this.point = point;
        this.rotation = rotation;
        this.length = length;
        this.height = height;
        this.ID = ID;
        this.targetWall = targetWall;
        this.materials = materials;
        this.isReverse = isReverse;
    }
}

[System.Serializable]
public class Window
{
    public GameObject prefab;
    public Vector3 point;
    public float rotation;
    public float length;
    public float height;
    public long ID;
    public Wall targetWall;
    public List<MatData> materials;
    public bool isReverse;

    public Window(GameObject prefab, Vector3 point, float rotation, float length, float height, long ID, Wall targetWall, List<MatData> materials, bool isReverse)
    {
        this.prefab = prefab;
        this.point = point;
        this.rotation = rotation;
        this.length = length;
        this.height = height;
        this.ID = ID;
        this.targetWall = targetWall;
        this.materials = materials;
        this.isReverse = isReverse;
    }
}

[System.Serializable]
public class WallMapSaveData
{
    public List<Wall> walls;
    public List<Intersection> intersections;
    public List<Room> rooms;
    public List<Door> doors;
    public List<Window> windows;

    public WallMapSaveData(List<Wall> walls, List<Intersection> intersections, List<Room> rooms, List<Door> doors, List<Window> windows)
    {
        this.walls = walls;
        this.intersections = intersections;
        this.rooms = rooms;
        this.doors = doors;
        this.windows = windows;
    }
}