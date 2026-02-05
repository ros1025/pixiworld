using System.Collections.Generic;
using UnityEngine;

public class PoolModifyState : IBuildingState
{
    private Grid grid;
    private PoolModifyPreview previewSystem;
    private PlacementSystem placementSystem;
    private PoolPlacer poolPlacer;
    private InputManager inputManager;
    private List<Vector3> posList;
    private Pool selectedPool;
    private float length;

    public PoolModifyState (Grid grid,
                            PoolPlacer poolPlacer,
                            PoolModifyPreview previewSystem,
                            PlacementSystem placementSystem,
                            InputManager inputManager)
    {
        this.grid = grid;
        this.poolPlacer = poolPlacer;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.inputManager = inputManager;
        selectedPool = poolPlacer.SelectPool(inputManager);

        posList = new();
        foreach (Vector3 pos in selectedPool.points)
        {
            posList.Add(pos);
        }    
        previewSystem.StartPreview(selectedPool, placementSystem, inputManager);
        placementSystem.GetBuildToolsUI().EnableSellButton(() => placementSystem.RemovePool(selectedPool));
        CalculateLength();
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

    public void EndState()
    {
        previewSystem.StopPreview();
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

        poolPlacer.ModifyPool(selectedPool, posList);
        posList.Clear();
        length = 0;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();

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
                if (!posList.Contains(gridPosition))
                {
                    int index = previewSystem.GetSplineIndex(gridPosition);
                    if (index >= 0 && index < posList.Count)
                    {
                        posList.Insert(index, gridPosition);
                        CalculateLength();
                        previewSystem.AddPoint(index, grid.LocalToWorld(gridPosition));
                    }
                    else if (index >= posList.Count)
                    {
                        posList.Add(gridPosition);
                        CalculateLength();
                        previewSystem.AddPoint(posList.IndexOf(gridPosition), grid.LocalToWorld(gridPosition));
                    }
                }
            }
        }    

        previewSystem.ApplyFeedback(CheckPlacementValidity());
        placementSystem.GetBuildToolsUI().AdjustLabels(5 * Mathf.RoundToInt(length), new Vector2Int(Mathf.RoundToInt(length), 1));

    }

    private bool CheckPlacementValidity()
    {
        if (posList.Count < 3)
            return false;

        if (!placementSystem.CanPlaceOnArea(posList))
        {
            return false;
        }

        if (!poolPlacer.CheckPoolCollisionsMove(posList, selectedPool))
        {
            return false;
        }

        return true;
    }
}
