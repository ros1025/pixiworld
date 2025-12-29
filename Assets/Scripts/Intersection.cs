using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Splines;

    [System.Serializable]
    public class Intersection
    {
        public List<JunctionInfo> junctions;
        public List<float> curves;
        public MeshCollider collider = new();
        public MeshRenderer renderer = new();
        public MeshFilter mesh = new();

        public void AddJunction(Spline spline, BezierKnot knot, float curve)
        {
            if (junctions == null)
            {
                junctions = new List<JunctionInfo>();
            }

            if (curves == null)
            {
                curves = new List<float>();
            }

            junctions.Add(new JunctionInfo(spline, knot));
            curves.Add(curve);
        }

        public IEnumerable<JunctionInfo> GetJunctions()
        {
            return junctions;
        }

        [System.Serializable]
        public struct JunctionInfo
        {
            public Spline spline;
            public BezierKnot knot;
            public int knotIndex;

            public JunctionInfo(Spline spline, BezierKnot knot)
            {
                this.spline = spline;
                this.knot = knot;

                knotIndex = -1;
                if (spline.IndexOf(knot) != -1)
                {
                    knotIndex = spline.IndexOf(knot);
                }
                else
                {
                    for (int i = 0; i < spline.Count; i++)
                    {
                        if ((Vector3)spline[i].Position == (Vector3)knot.Position)
                        {
                            knotIndex = i;
                        }
                    }
                }
            }

            public int GetSplineIndex(SplineContainer container)
            {
                List<Spline> containerSplines = new();
                foreach (Spline spline in container.Splines) { containerSplines.Add(spline); }
                return containerSplines.IndexOf(spline);
            }

            public void UpdateKnotIndex(BezierKnot knot)
            {
                this.knot = knot;
                knotIndex = spline.IndexOf(knot);
            }
        }

        public struct JunctionEdge
        {
            public Vector3 left;
            public Vector3 right;

            public Vector3 center => (left + right) / 2;

            public JunctionEdge (Vector3 p1, Vector3 p2)
            {
                this.left = p1;
                this.right = p2;
            }
        }
    }