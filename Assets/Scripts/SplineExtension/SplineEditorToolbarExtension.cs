using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.Splines
{
    public static class SplineToolUtility
    {
        public static event System.Action Changed;

        // Is the spline point selected?
        public static bool HasSelection()
        {
            return SplineSelection.HasActiveSplineSelection();
        }

        public static void RegisterSelectionChangedEvent()
        {
            SplineSelection.changed += InvokeChange;
        }

        internal static void InvokeChange()
        {
            Changed?.Invoke();
        }

        public static List<SelectedSplineElementInfo> GetSelection()
        {
            //Get internal struct data
            List<SelectableSplineElement> elements = SplineSelection.selection;

            //Make new public struct data
            List<SelectedSplineElementInfo> infos = new List<SelectedSplineElementInfo>();

            //Convert internal struct to public struct data
            foreach (SelectableSplineElement element in elements)
            {
                infos.Add(new SelectedSplineElementInfo(element.target, element.targetIndex, element.knotIndex));
            }

            return infos;
        }

        public struct SelectedSplineElementInfo
        {
            public Object target;
            public int targetIndex;
            public int knotIndex;

            public SelectedSplineElementInfo(Object Object, int Index, int knot)
            {
                target = Object;
                targetIndex = Index;
                knotIndex = knot;
            }
        }
    }
}
