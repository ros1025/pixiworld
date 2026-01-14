using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WallIntersectionModifyState : IBuildingState
{
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    WallMapping wallMapping;
    InputManager inputManager;
    private Vector3 position;
    private Vector3 originalPosition;
    private List<Wall> walls;
    private Intersection selectedIntersection;
    private float length;
    private int index;

    public WallIntersectionModifyState(Vector3 gridPosition,
                            Grid grid,
                            WallMapping wallMapping,
                            PreviewSystem previewSystem,
                            PlacementSystem placementSystem,
                            InputManager inputManager)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.wallMapping = wallMapping;
        this.placementSystem = placementSystem;
        this.inputManager = inputManager;

        wallMapping.SelectIntersection(inputManager, out selectedIntersection, out index, out walls);

        if (index == -1)
            return;

        position = wallMapping.CalculateIntersectionCenter(selectedIntersection);
        originalPosition = position;

        previewSystem.ModifyIntersection(originalPosition, selectedIntersection, GetCost(), walls);
    }

    public void EndState()
    {
        previewSystem.ClearPointer();
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        if (previewSystem.expand == true)
        {
            position = gridPosition;
            previewSystem.MovePointer(grid.LocalToWorld(gridPosition), CheckPlacementValidity(), GetCost(), 0, 0, 0);
        }
    }

    public void OnAction(Vector3 gridPosition)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        bool placementValidity = CheckPlacementValidity();

        if (placementValidity == false)
        {
            return;
        }


        Vector3 pos = grid.WorldToLocal(previewSystem.previewPos);

        wallMapping.ModifyIntersection(selectedIntersection, position);

        previewSystem.ClearPointer();
        inputManager.InvokeExit();
    }

    private int GetCost()
    {
        int cost = 0;
        foreach (Wall wall in walls)
        {
            Spline spline = wall.wall;
            Intersection.JunctionInfo junctionInfo = selectedIntersection.junctions.Find(item => item.spline == spline);
            int index = junctionInfo.knotIndex == 0 ? 0 : wall.points.Count;

            for (int i = 0; i < wall.points.Count - 1; i++)
            {
                Vector3 p1 = wall.points[i];
                Vector3 p2 = wall.points[i + 1];

                if (i == index)
                {
                    p1 = position;
                }
                if (i + 1 == index)
                {
                    p2 = position;
                }

                int costFactor = 5;
                cost += costFactor * Mathf.RoundToInt(Vector3.Distance(p1, p2));
            }
        }
        return cost;
    }

    private bool CheckPlacementValidity()
    {
        foreach (Wall wall in walls)
        {
            Spline spline = wall.wall;
            Intersection.JunctionInfo junctionInfo = selectedIntersection.junctions.Find(item => item.spline == spline);
            int index = junctionInfo.knotIndex == 0 ? 0 : wall.points.Count;
            int len = 0;

            for (int i = 0; i < wall.points.Count - 1; i++)
            {
                Vector3 p1 = wall.points[i];
                Vector3 p2 = wall.points[i + 1];

                if (i == index)
                {
                    p1 = position;
                }
                if (i + 1 == index)
                {
                    p2 = position;
                }

                len += Mathf.RoundToInt(Vector3.Distance(p1, p2));

                if (!placementSystem.CanPlaceOnArea(p1, p2, 0.04f, 2f))
                {
                    return false;
                }
            }

            if (len < 0.5f)
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity();

        previewSystem.MovePointer(grid.LocalToWorld(gridPosition), CheckPlacementValidity(), GetCost(), 0, 0, 0);    
    }
}
