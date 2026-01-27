using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoadCreateState : IBuildingState
{
    //private int selectedObjectIndex = -1;
    RoadsData roadsData;
    Grid grid;
    RoadCreatePreview previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    private InputManager inputManager;
    int width;
    float length;
    private List<Vector3> posList;
    private Vector3 displayPosition; 
    float rotation;

    public RoadCreateState(Vector3 gridPosition,
                           RoadsData roadsData,
                           Grid grid,
                           RoadCreatePreview previewSystem,
                           PlacementSystem placementSystem,
                            RoadsDatabaseSO database,
                           RoadMapping roadMapping,
                            InputManager inputManager)
    {
        this.roadsData = roadsData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.roadMapping = roadMapping;
        this.inputManager = inputManager;

        posList = new();
        //selectedObjectIndex = database.roadsData.IndexOf(roadsData);
        if (database.roadsData.Contains(roadsData))
        {
            width = roadsData.width;

            previewSystem.StartPreview(placementSystem, inputManager, width, 0.1f);
        }
        else
            throw new System.Exception($"No object with ID {roadsData.ID}");
    }

    public void EndState()
    {
        previewSystem.StopPreview();
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
                previewSystem.ModifyPointer(index, gridPosition);
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
        placementSystem.GetBuildToolsUI().AdjustLabels(roadsData.Cost * Mathf.RoundToInt(length), new Vector2Int(Mathf.RoundToInt(length), Mathf.RoundToInt(width) > 0 ? Mathf.RoundToInt(width) : 1));
        placementSystem.GetBuildToolsUI().canPlace = CheckPlacementValidity();
    }

    public void OnAction(Vector3 gridPosition)
    {
        bool placementValidity = CheckPlacementValidity();

        if (placementValidity == false)
        {
            return;
        }

        Vector3 pos = grid.WorldToLocal(previewSystem.GetPreviewPosition());

        List<Vector3> displayPos = new List<Vector3>();
        for (int i = 0; i < posList.Count; i++)
        {
            displayPos.Add(grid.LocalToWorld(posList[i]));
        }

        roadMapping.AddRoad(displayPos, roadsData.width, roadsData.ID, roadsData.tex);
        placementSystem.GetBuildToolsUI().canPlace = false;

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

            if (!placementSystem.CanPlaceOnArea(p1, p2, width, 0.1f))
            {
                return false;
            }
        }

        if (posList.Count < 2)
            return false;

        return true;
    }
}
