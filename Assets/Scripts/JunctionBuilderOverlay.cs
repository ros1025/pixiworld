#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Overlays;
using UnityEditor.UI;
using UnityEngine.Splines;
using UnityEditor;
using UnityEditor.Splines;

//[Overlay(typeof(SceneView), "Junction Builder", true)] UNUSED
public class JunctionBuilderOverlay : Overlay
{
    Label SelectionInfoLabel = new Label();
    Button BuildJunctionButton = new Button();
    Button ClearJunctionButton = new Button();
    VisualElement SliderArea = new VisualElement();

    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();
        root.Add(SelectionInfoLabel);
        root.Add(BuildJunctionButton);
        BuildJunctionButton.text = "Build Junction";
        root.Add(ClearJunctionButton);
        ClearJunctionButton.text = "Remove Junction";
        root.Add(SliderArea);

        SplineToolUtility.RegisterSelectionChangedEvent();
        SplineToolUtility.Changed += OnSelectionChange;
        BuildJunctionButton.clicked += OnBuildJunction;
        ClearJunctionButton.clicked += OnClearJunction;

        return root;
    }

    private void OnSelectionChange()
    {
        UpdateSelectionInfo();
    }

    private void ClearSelectionInfo()
    {
        SelectionInfoLabel.text = "";
    }

    private void UpdateSelectionInfo()
    {
        ClearSelectionInfo();

        List<SplineToolUtility.SelectedSplineElementInfo> infos = SplineToolUtility.GetSelection();

        BuildJunctionButton.visible = true;
        ClearJunctionButton.visible = false;
        SliderArea.Clear();

        foreach (SplineToolUtility.SelectedSplineElementInfo element in infos)
        {
            SelectionInfoLabel.text += $"Spline {element.targetIndex}, Knot {element.knotIndex} \n";

            foreach (Intersection intersection in Selection.activeGameObject.GetComponent<RoadMapping>().intersections)
            {
                foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                {
                    if (junction.GetSplineIndex(Selection.activeGameObject.GetComponent<RoadMapping>().m_SplineContainer).Equals(element.targetIndex) && junction.knotIndex.Equals(element.knotIndex))
                    {
                        ShowIntersection(intersection);
                    }
                }
            }
        }
    }

    private void OnBuildJunction()
    {
        List<SplineToolUtility.SelectedSplineElementInfo> selection = SplineToolUtility.GetSelection();

        Intersection intersection = new Intersection();
        foreach(SplineToolUtility.SelectedSplineElementInfo item in selection)
        {
            //Get the spline container
            SplineContainer container = (SplineContainer)item.target;
            Spline spline = container.Splines[item.targetIndex];
            BezierKnot knots = spline.ToArray()[item.knotIndex];
            intersection.AddJunction(spline, knots, 0.5f);
        }

        GameObject collider = new();
        collider.transform.SetParent(GameObject.Find("RoadSystem").transform);
        collider.AddComponent<MeshCollider>();
        collider.layer = 6;
        Selection.activeGameObject.GetComponent<RoadMapping>().AddJunction(intersection, collider.GetComponent<MeshCollider>());
    }

    private void OnClearJunction()
    {
        List<SplineToolUtility.SelectedSplineElementInfo> selection = SplineToolUtility.GetSelection();

        foreach (SplineToolUtility.SelectedSplineElementInfo item in selection)
        {
            foreach (Intersection intersection in Selection.activeGameObject.GetComponent<RoadMapping>().intersections)
            {
                foreach (Intersection.JunctionInfo junction in intersection.GetJunctions())
                {
                    if (junction.GetSplineIndex(Selection.activeGameObject.GetComponent<RoadMapping>().m_SplineContainer).Equals(item.targetIndex) && junction.knotIndex.Equals(item.knotIndex))
                    {
                        intersection.junctions.Remove(junction);
                        Selection.activeGameObject.GetComponent<RoadMapping>().RemoveJunction(intersection);
                    }
                }
            }
        }
    }

    public void ShowIntersection(Intersection intersection)
    {
        SelectionInfoLabel.text = "Selected Intersection";
        BuildJunctionButton.visible = false;
        ClearJunctionButton.visible = true;
        SliderArea.Clear();

        for (int i = 0; i < intersection.curves.Count; i++)
        {
            int value = i;
            Slider slider = new Slider($"Curve {i}", 0, 1, SliderDirection.Horizontal);
            slider.labelElement.style.minWidth = 60;
            slider.labelElement.style.maxWidth = 80;
            slider.value = intersection.curves[i];
            slider.RegisterValueChangedCallback((x) => { intersection.curves[value] = x.newValue; });
            SliderArea.Add(slider);
        }
    }
}
#endif