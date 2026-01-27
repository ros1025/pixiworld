using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RoadIntersectionModifyState : IBuildingState
{
    Grid grid;
    RoadIntersectionModifyPreview previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    InputManager inputManager;
    Intersection selectedIntersection;
    int index;
    private List<Roads> junctions;
    private bool edited;
    private Vector3 position;
    private Vector3 originalPosition;

    public RoadIntersectionModifyState(Vector3 gridPosition,
                                    Grid grid,
                                    RoadIntersectionModifyPreview previewSystem,
                                    PlacementSystem placementSystem,
                                    RoadsDatabaseSO database,
                                    InputManager inputManager,
                                    RoadMapping roadMapping)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.roadMapping = roadMapping;
        this.inputManager = inputManager;

        roadMapping.SelectIntersection(inputManager, out selectedIntersection, out index, out junctions);

        if (index == -1)
        {
            return;
        }
        edited = false;
        position = roadMapping.CalculateIntersectionCenter(selectedIntersection);
        originalPosition = position;
        
        previewSystem.StartPreview(originalPosition, selectedIntersection, junctions, placementSystem, inputManager);
    }

    public void EndState()
    {
        previewSystem.StopPreview();
        if (edited == false)
        {
            position = originalPosition;
        }
    }

    public void OnAction(Vector3 gridPosition)
    {
        bool placementValidity = CheckPlacementValidity();

        if (placementValidity == false)
        {
            return;
        }

        Vector3 pos = grid.WorldToLocal(previewSystem.GetPreviewPosition());

        roadMapping.ModifyIntersection(selectedIntersection, pos);

        edited = true;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();    
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        position = gridPosition;
        previewSystem.ModifyPointer(0,grid.LocalToWorld(gridPosition));
        previewSystem.ApplyFeedback(CheckPlacementValidity());
        placementSystem.GetBuildToolsUI().AdjustLabels(GetCost(), Vector2Int.one);
        placementSystem.GetBuildToolsUI().canPlace = CheckPlacementValidity();
    }

    private bool CheckPlacementValidity()
    {
        foreach (Roads road in junctions)
        {
            Spline spline = road.road;
            Intersection.JunctionInfo junctionInfo = selectedIntersection.junctions.Find(item => item.spline == spline);
            int index = junctionInfo.knotIndex == 0 ? 0 : road.points.Count;
            int len = 0;

            for (int i = 0; i < road.points.Count - 1; i++)
            {
                Vector3 p1 = road.points[i];
                Vector3 p2 = road.points[i + 1];

                if (i == index)
                {
                    p1 = position;
                }
                if (i + 1 == index)
                {
                    p2 = position;
                }

                len += Mathf.RoundToInt(Vector3.Distance(p1, p2));

                if (!placementSystem.CanPlaceOnArea(p1, p2, road.width, 0.1f))
                {
                    return false;
                }
            }

            if (len < road.width + 0.5f)
            {
                return false;
            }
        }

        return true;
    }

    private int GetCost()
    {
        int cost = 0;
        foreach (Roads road in junctions)
        {
            Spline spline = road.road;
            Intersection.JunctionInfo junctionInfo = selectedIntersection.junctions.Find(item => item.spline == spline);
            int index = junctionInfo.knotIndex == 0 ? 0 : road.points.Count;

            for (int i = 0; i < road.points.Count - 1; i++)
            {
                Vector3 p1 = road.points[i];
                Vector3 p2 = road.points[i + 1];

                if (i == index)
                {
                    p1 = position;
                }
                if (i + 1 == index)
                {
                    p2 = position;
                }

                int costFactor = database.roadsData.Find(id => id.ID == road.ID).Cost;
                cost += costFactor * Mathf.RoundToInt(Vector3.Distance(p1, p2));
            }
        }
        return cost;
    }
}
