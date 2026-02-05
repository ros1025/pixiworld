using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

public class PoolPlacer : MonoBehaviour
{
    public List<Pool> pools;
    public Material water;
    public float depth = 2f;

    private Mesh CreateWaterMesh(List<Vector3> points)
    {
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();

        List<Vector3> p1 = new(), p2 = new();
        List<Vector2> newUVs = new();

        
        foreach (Vector3 point in points)
        {
            p1.Add(point + new Vector3(0, 0, 0));
            newUVs.Add(new Vector2(point.x - points[0].x, point.z - points[0].z));
        }
        tris.AddRange(CreateTris(verts, p1, Vector3.up, uvs, newUVs));

        Mesh mesh = new();
        mesh.subMeshCount = 1;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        return mesh;
    }

    private Mesh CreateCollisionMesh(List<Vector3> points)
    {
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();

        List<Vector3> p1 = new(), p2 = new();
        List<Vector2> newUVs = new();
        List<Vector2> newUVs2 = new();

        foreach (Vector3 point in points)
        {
            p1.Add(point + new Vector3(0, 0.05f, 0));
            p2.Add(point + new Vector3(0, -depth, 0));
            newUVs.Add(new Vector2(point.x - points[0].x, point.z - points[0].z));
            newUVs2.Add(new Vector2(point.x - points[0].x, point.z - points[0].z));
        }
        tris.AddRange(CreateTris(verts, p1, Vector3.up, uvs, newUVs));
        tris.AddRange(CreateTris(verts, p2, Vector3.up, uvs, newUVs2));

        for (int i = 0; i < points.Count; i++)
        {
            List<Vector3> sidePoints = new()  
            { 
                points[i] + new Vector3(0, 0.05f, 0), points[(i + 1) % points.Count] + new Vector3(0, 0.05f, 0), 
                points[(i + 1) % points.Count] + new Vector3(0, -depth, 0), points[i] + new Vector3(0, -depth, 0)
            };
            List<Vector2> newSideUVs = new()
            {
                new Vector2(points[i].x - points[0].x, points[i].y - points[0].y),
                new Vector2(points[(i + 1) % points.Count].x - points[0].x, points[(i + 1) % points.Count].y - points[0].y),
                new Vector2(points[(i + 1) % points.Count].x - points[0].x, points[(i + 1) % points.Count].y - points[0].y),
                new Vector2(points[i].x - points[0].x, points[i].y - points[0].y)
            };
            tris.AddRange(CreateTris(verts, sidePoints, Vector3.Cross(points[(i + 1) % points.Count] - points[i], Vector3.up), uvs, newSideUVs));
        }

        Mesh mesh = new();
        mesh.subMeshCount = 1;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        return mesh;
    }

    private void RenderPool(Pool pool)
    {
        Mesh mesh = CreateWaterMesh(pool.points);
        pool.mesh.mesh = mesh;
        pool.renderer.material = water;

        Mesh col = CreateCollisionMesh(pool.points);
        pool.collider.sharedMesh = col;
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

    public void AddPool(List<Vector3> points)
    {
        GameObject gameObject = new();
        gameObject.transform.SetParent(this.transform);
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshCollider>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.layer = LayerMask.NameToLayer("Selector");

        Pool newPool = new Pool(points, gameObject.GetComponent<MeshFilter>(), gameObject.GetComponent<MeshRenderer>(), gameObject.GetComponent<MeshCollider>());
        pools.Add(newPool);
        RenderPool(newPool);
    }

    public void ModifyPool(Pool pool, List<Vector3> points)
    {
        pool.points = points;
        RenderPool(pool);
    }

    public void RemovePool(Pool pool)
    {
        Destroy(pool.mesh.gameObject);
        pools.Remove(pool);
    }

    public void RemovePoolAt(GameObject prefab)
    {
        int index = pools.FindIndex(item => item.collider.gameObject == prefab);
        if (index == -1)
            return;
        else
        {
            RemovePool(pools[index]);
        }
    }

    public void DetectOverlappingPools(List<Vector3> points, float angle, List<(Pool, Vector3, Vector3)> overlappingPools)
    {
        if (points.Count > 2 && Mathf.Abs(Mathf.Abs(angle) - ((points.Count - 2) * 180)) < 0.1f) 
        {
            Bounds boundBox = new();

            foreach (Vector3 point in points)
            {
                boundBox.Encapsulate(transform.TransformPoint(point));
            }
            List<Pool> collidePools = pools.FindAll(p => ObjectValidation(boundBox, p.collider.bounds));
            foreach (Pool collidePool in collidePools)
            {
                Vector3 nearestPoint = new();
                Vector3 roomAnchorPoint = points[0];
                bool isEncapsulated = false;

                for (int j = 0; j < points.Count; j++)
                {
                    Vector3 p1 = points[j];
                    Vector3 p2 = points[(j + 1) % points.Count];

                    Vector3 c1 = GetNearestPoint(p1, collidePool.points);
                    if (PointInPool(c1, points, angle))
                    {
                        isEncapsulated = true;
                        if (p1 == points[0] || Vector3.Distance(p1, c1) < Vector3.Distance(nearestPoint, roomAnchorPoint))
                        {
                            nearestPoint = c1;
                            roomAnchorPoint = p1;
                        }
                    }
                }

                if (isEncapsulated)
                {
                    overlappingPools.Add((collidePool, nearestPoint, roomAnchorPoint));
                }
            }
        }
    }

    public bool CheckPoolCollisions(List<Vector3> points)
    {
        float angle = GetAngle(points, Vector3.up);

        if (points.Count > 2 && Mathf.Abs(Mathf.Abs(angle) - ((points.Count - 2) * 180)) < 0.1f) 
        {
            Mesh m1 = CreateCollisionMesh(points);
            List<Pool> collidePools = pools.FindAll(p => ObjectValidation(m1.bounds, p.collider.bounds));
            if (collidePools.Count != 0)
            {
                foreach (Pool collidePool in collidePools)
                {
                    for (int j = 0; j < points.Count; j++)
                    {
                        Vector3 p1 = points[j];
                        Vector3 p2 = points[(j + 1) % points.Count];

                        Vector3 c1 = GetNearestPoint(p1, collidePool.points);
                        if (!PointInPool(c1, points, angle))
                        {
                            RaycastHit[] hits1 = Physics.RaycastAll(p1, (p2 - p1).normalized, Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
                            foreach (RaycastHit hit in hits1)
                            {
                                if (hit.collider == collidePool.collider)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        return false;
    }

    public bool CheckPoolCollisionsMove(List<Vector3> points, Pool pool)
    {
        float angle = GetAngle(points, Vector3.up);

        if (points.Count > 2 && Mathf.Abs(Mathf.Abs(angle) - ((points.Count - 2) * 180)) < 0.1f) 
        {
            
            Mesh m1 = CreateCollisionMesh(points);

            List<Pool> collidePools = pools.FindAll(p => p != pool && ObjectValidation(m1.bounds, p.collider.bounds));
            if (collidePools.Count != 0)
            {
                foreach (Pool collidePool in collidePools)
                {
                    for (int j = 0; j < points.Count; j++)
                    {
                        Vector3 p1 = points[j];
                        Vector3 p2 = points[(j + 1) % points.Count];

                        Vector3 c1 = GetNearestPoint(p1, collidePool.points);
                        if (!PointInPool(c1, points, angle))
                        {
                            RaycastHit[] hits1 = Physics.RaycastAll(p1, (p2 - p1).normalized, Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
                            foreach (RaycastHit hit in hits1)
                            {
                                if (hit.collider == collidePool.collider)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        return false;
    }

    public bool CheckPoolCollisions(GameObject cursor, Vector3 position, Vector2Int size, float rotation)
    {
        bool ans = true;
        GameObject previewSelector = GameObject.Instantiate(cursor, position, Quaternion.Euler(0, rotation, 0));
        previewSelector.name = "Intersector";
        previewSelector.transform.localScale = new Vector3Int(size.x, size.y, size.y);
        Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));

        if (pools.FindIndex(item => overlaps.Contains(item.collider)) != -1)
            ans = false;

        GameObject.Destroy(previewSelector);
        return ans;
    }  

    public bool CheckPoolCollisions(GameObject previewSelector)
    {
        bool ans = true;
        Collider[] overlaps = Physics.OverlapBox(previewSelector.transform.GetChild(0).position, previewSelector.transform.localScale / 2, previewSelector.transform.rotation, LayerMask.GetMask("Selector"));

        if (pools.FindIndex(item => overlaps.Contains(item.collider)) != -1)
            ans = false;

        return ans;
    } 

    public bool CanPlaceObjectAt(Vector3 p1, Vector3 p2, float width, float height)
    {
        RaycastHit[] hits = Physics.BoxCastAll(p1 + new Vector3(0, height / 2f, 0), new Vector3(width - 0.05f, height / 2f, width - 0.05f), p2 - p1, Quaternion.Euler(0, Vector3.SignedAngle(p2 - p1, Vector3.forward, Vector3.down), 0), Vector3.Distance(p1, p2), LayerMask.GetMask("Selector"));
        List<RaycastHit> hitList = new(); hitList.AddRange(hits);

        if (pools.FindIndex(item => hitList.FindIndex(col => col.collider == item.collider) != -1) != -1)
        {
            return false;
        }

        return true;
    }

    public Pool SelectPool(InputManager input)
    {
        RaycastHit[] overlaps = input.RayHitAllObjects();
        List<RaycastHit> overlapsList = new(); overlapsList.AddRange(overlaps);
        Pool selectedPool = pools.Find(pool => overlapsList.FindIndex(col => col.collider == pool.collider) != -1); 
        return selectedPool;
    }

    private bool ObjectValidation(Bounds b1, Bounds b2)
    {
        bool isCollide = b1.Intersects(b2);
        return isCollide;
    }

    private Vector3 GetNearestPoint(Vector3 compPoint, List<Vector3> boundPoints)
    {
        Vector3 nearest = new();
        float nearestDist = -1;
        for (int i = 0; i < boundPoints.Count; i++)
        {
            Vector3 np = GetNearestPoint(boundPoints[i], boundPoints[(i + 1) % boundPoints.Count], compPoint);

            if (nearestDist < 0 || Vector3.Distance(np, compPoint) < nearestDist)
            {
                nearestDist = Vector3.Distance(np, compPoint);
                nearest = np;
            }
        }

        return nearest;
    }

    private Vector3 GetNearestPoint(Vector3 start, Vector3 end, Vector3 point)
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

    private bool PointInPool(Vector3 point, List<Vector3> points, float angle)
    {
        for (int j = 0; j < points.Count; j++)
        {
            Vector3 p1 = points[j];
            Vector3 p2 = points[(j + 1) % points.Count];

            Vector3 crossVector = Vector3.Cross((p2 - p1).normalized, Vector3.up).normalized;
            if (angle > 0) crossVector *= -1;

            if (Vector3.Angle(point - p1, crossVector) > 90 && Vector3.Angle(point - p2, crossVector) > 90)
            {                
                return false;
            }
        }

        return true;
    }

    public void LoadData(List<Pool> loadData)
    {
        foreach (Pool pool in pools)
        {
            if (!loadData.Contains(pool))
            {
                RemovePool(pool);
            }
        }

        foreach (Pool pool in loadData)
        {
            if (!pools.Contains(pool))
            {
                AddPool(pool.points);
            }
        }
    }
}

[Serializable]
public class Pool
{
    public List<Vector3> points;
    public MeshFilter mesh;
    public MeshRenderer renderer;
    public MeshCollider collider;

    public Pool(List<Vector3> points, MeshFilter mesh, MeshRenderer renderer, MeshCollider collider)
    {
        this.points = points;
        this.mesh = mesh;
        this.renderer = renderer;
        this.collider = collider;
    }
}
