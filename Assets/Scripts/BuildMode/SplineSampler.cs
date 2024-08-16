using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SplineSampler : MonoBehaviour
{
    [SerializeField] private SplineContainer m_SplineContainer;

    public int NumSplines;
    //[SerializeField, Range(0f, 1f)] private float m_time;

    float3 position;
    float3 forward;
    float3 upVector;


    // Update is called once per frame
    void Update()
    {
        NumSplines = m_SplineContainer.Splines.Count;
    }

    //Sample mesh coordinates for the width of the road. This does not work if one or both beziers are 0, so straight roads must be set with 0.001.
    public void SampleSplineWidth(int i, float t, float m_width, out Vector3 p1, out Vector3 p2)
    {

        m_SplineContainer.Evaluate(i, t, out position, out forward, out upVector);

        float3 right = Vector3.Cross(forward, upVector).normalized;
        p1 = position + (right * m_width);
        p2 = position + (-right * m_width);
    }

    public void SampleSplinePoint(Spline spline, Vector3 point, int resolution, out Vector3 nearestPoint, out float t)
    {
        SplineUtility.GetNearestPoint<Spline>(spline, point, out float3 nearestPoint3, out t, resolution);
        nearestPoint = nearestPoint3;
    }
}
