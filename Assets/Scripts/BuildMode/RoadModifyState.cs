using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadModifyState : IBuildingState
{
    private long selectedObjectIndex = -1;
    private RoadsData roadsDataObject = null;
    Grid grid;
    RoadModifyPreview previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    InputManager inputManager;
    Roads selectedRoad;
    int width;
    int index;
    float length;
    private List<Vector3> posList;
    private List<Vector3> originalPosList;

    private Dictionary<Vector3, Vector3> posMap;
    bool edited;

    public RoadModifyState(Vector3 gridPosition,
                           Grid grid,
                           RoadModifyPreview previewSystem,
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

        roadMapping.SelectRoad(inputManager, out selectedRoad, out index, out width, out selectedObjectIndex, out List<Vector3> displayPosList);
        roadsDataObject = database.roadsData.Find(data => data.ID == selectedObjectIndex);


        if (index == -1)
            return;
        edited = false;
        posList = new();
        posMap = new();
        for (int i = 0; i < displayPosList.Count; i++)
        {
            posList.Add(grid.WorldToLocal(displayPosList[i]));
            posMap.Add(grid.WorldToLocal(displayPosList[i]), grid.WorldToLocal(displayPosList[i]));
        }
        originalPosList = posList;
        CalculateLength();
        previewSystem.StartPreview(displayPosList, selectedRoad, placementSystem, inputManager, width, 0.1f);
        placementSystem.GetBuildToolsUI().EnableSellButton(() => placementSystem.RemoveRoad(selectedRoad));
    }

    public void EndState()
    {
        previewSystem.StopPreview();
        if (edited == false)
        {
            List<Vector3> displayPosList = new();
            for (int i = 0; i < originalPosList.Count; i++)
            {
                displayPosList.Add(grid.LocalToWorld(originalPosList[i]));
            }
        }
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
                if (index != 0 || index != posList.Count - 1)
                {
                    posList.RemoveAt(index);
                    if (index < originalPosList.Count)
                    {
                        posMap[originalPosList[index]] = Vector3.negativeInfinity;
                    }
                    CalculateLength();
                    previewSystem.DeletePointer(index);
                }
            }
            else if (index >= 0)
            {
                posList[index] = gridPosition;
                if (index < originalPosList.Count)
                {
                    posMap[originalPosList[index]] = gridPosition;
                }
                else if (index == posList.Count - 1)
                {
                    posMap[originalPosList[^1]] = gridPosition;
                }
                CalculateLength();
                previewSystem.ModifyPointer(index, gridPosition);
            }
        }
        else
        {
            if (previewSystem.selectedCursor == previewSystem.GetPreviewObject())
            {
                if (!posList.Contains(gridPosition))
                {
                    int index = previewSystem.GetSplineIndex(gridPosition);
                    if (index >= 0)
                    {
                        posList.Insert(index, gridPosition);
                        CalculateLength();
                        previewSystem.AddPoint(index, gridPosition);
                    }
                }
            }
            /*
            else if (!posList.Contains(gridPosition))
            {
                posList.Add(gridPosition);
                CalculateLength();
                UpdateState(gridPosition, 0);
            }
            */
        }

        previewSystem.ApplyFeedback(CheckPlacementValidity());
        placementSystem.GetBuildToolsUI().AdjustLabels(roadsDataObject.Cost * Mathf.RoundToInt(length), new Vector2Int(Mathf.RoundToInt(length), Mathf.RoundToInt(width) > 0 ? Mathf.RoundToInt(width) : 1));
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

        roadMapping.ModifyRoad(selectedRoad, displayPos, roadsDataObject.width, selectedObjectIndex, roadsDataObject.tex);

        posList.Clear();
        length = 0;
        edited = true;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();
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

