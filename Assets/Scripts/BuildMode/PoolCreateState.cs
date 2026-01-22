using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolCreateState : IBuildingState
{
    private Grid grid;
    private PreviewSystem previewSystem;
    private PlacementSystem placementSystem;
    private PoolPlacer poolPlacer;
    private List<Vector3> posList;
    private float length;

    public PoolCreateState (Grid grid,
                            PoolPlacer poolPlacer,
                            PreviewSystem previewSystem,
                            PlacementSystem placementSystem)
    {
        this.grid = grid;
        this.poolPlacer = poolPlacer;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;

        posList = new();
        previewSystem.AddWalls();
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

    public void OnAction(Vector3 gridPosition)
    {
        grid = placementSystem.GetCurrentGrid();
        poolPlacer = placementSystem.GetCurrentPools();

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

        poolPlacer.AddPool(displayPos);
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

        if (posList.Count < 3)
            return false;

        return true;
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();

        if (previewSystem.expand == true)
        {
            int index = previewSystem.expanders.IndexOf(previewSystem.SelectedCursor);
            if ((index > 0 && Vector3.Distance(posList[index - 1], gridPosition) < 0.1f) || (index < posList.Count - 1 && Vector3.Distance(posList[index + 1], gridPosition) < 0.1f))
            {
                posList.RemoveAt(index);
                CalculateLength();
                previewSystem.RemovePointer(index, CheckPlacementValidity(), 5 * Mathf.RoundToInt(length), length, 0.1f, 2f);
            }
            else if (index >= 0)
            {
                posList[index] = gridPosition;
                CalculateLength();
                previewSystem.MovePointer(grid.LocalToWorld(gridPosition), CheckPlacementValidity(), 5 * Mathf.RoundToInt(length), length, 0.1f, 2f);
            }
        }
        else
        {
            if (previewSystem.SelectedCursor == previewSystem.gameObject)
            {
                if (!posList.Contains(gridPosition))
                {
                    int index = previewSystem.GetSplineIndex(gridPosition);
                    if (index >= 0)
                    {
                        posList.Insert(index, gridPosition);
                        CalculateLength();
                        UpdateState(gridPosition, 0);
                    }
                }
            }
            else if (!posList.Contains(gridPosition))
            {
                posList.Add(gridPosition);
                CalculateLength();
                UpdateState(gridPosition, 0);
            }
        }
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity();

        previewSystem.UpdatePointer(grid.LocalToWorld(gridPosition), placementValidity, posList.IndexOf(gridPosition), 5 * Mathf.RoundToInt(length), length, 0.1f, 2f);
    }
}
