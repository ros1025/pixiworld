using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreateState : IBuildingState
{
    Grid grid;
    WallMapping wallMapping;
    WallCreatePreview previewSystem;
    PlacementSystem placementSystem;
    private List<Vector3> posList;
    float length;

    public WallCreateState(Grid grid,
                            WallMapping wallMapping,
                            WallCreatePreview previewSystem,
                            PlacementSystem placementSystem,
                            InputManager inputManager)
    {
        this.grid = grid;
        this.wallMapping = wallMapping;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;

        posList = new();
        previewSystem.StartPreview(placementSystem, inputManager, 0.1f, 2f);
    }

    public void EndState()
    {
        previewSystem.ClearPointer();
    }

    private void CalculateLength()
    {
        length = 0;
        if (posList.Count > 1)
        {
            for (int i = 1; i < posList.Count; i++)
            {
                length += Vector3.Distance(posList[i - 1], posList[i]);
            }
        }
    }


    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        if (previewSystem.GetModifyState() == true)
        {
            int index = previewSystem.expanders.IndexOf(previewSystem.selectedCursor);
            if ((index > 0 && Vector3.Distance(posList[index - 1], gridPosition) < 0.1f) || (index < posList.Count - 1 && Vector3.Distance(posList[index + 1], gridPosition) < 0.1f))
            {
                posList.RemoveAt(index);
                CalculateLength();
                previewSystem.DeletePointer(index);
            }
            else if (index >= 0)
            {
                posList[index] = gridPosition;
                CalculateLength();
                previewSystem.ModifyPointer(index, grid.LocalToWorld(gridPosition));
            }
        }
        else
        {
            if (previewSystem.selectedCursor == previewSystem.GetPreviewObject())
            {
                if (!(posList.Contains(gridPosition)))
                {
                    int index = previewSystem.GetSplineIndex(gridPosition);
                    if (index >= 0)
                    {
                        posList.Insert(index, gridPosition);
                        CalculateLength();
                        previewSystem.AddPoint(index, grid.LocalToWorld(gridPosition));
                    }
                }
            }
            else if (!(posList.Contains(gridPosition)))
            {
                posList.Add(gridPosition);
                CalculateLength();
                previewSystem.AddPoint(posList.IndexOf(gridPosition), grid.LocalToWorld(gridPosition));
            }
        }

        previewSystem.ApplyFeedback(CheckPlacementValidity());
        placementSystem.GetBuildToolsUI().AdjustLabels(5 * Mathf.RoundToInt(length), new Vector2Int(Mathf.RoundToInt(length), 1));
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


        List<Vector3> displayPos = new();
        for (int i = 0; i < posList.Count; i++)
        {
            displayPos.Add(grid.LocalToWorld(posList[i]));
        }

        wallMapping.AddWalls(displayPos);
        posList.Clear();
        length = 0;
        previewSystem.ClearPointer();
    }

    private bool CheckPlacementValidity()
    {
        for (int i = 1; i < posList.Count; i++)
        {
            Vector3 p1 = grid.LocalToWorld(posList[i - 1]);
            Vector3 p2 = grid.LocalToWorld(posList[i]);

            if (!placementSystem.CanPlaceOnArea(p1, p2, 0.04f, 2f))
            {
                return false;
            }
        }

        if (posList.Count < 2)
            return false;

        return true;
    }
}
