using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadModifyState : IBuildingState
{
    private int selectedObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    SoundFeedback soundFeedback;
    InputManager inputManager;
    Roads selectedRoad;
    int width;
    int index;
    float length;
    private List<Vector3Int> posList;
    private List<Vector3Int> originalPosList;
    bool edited;

    public RoadModifyState(Vector3Int gridPosition,
                           Grid grid,
                           PreviewSystem previewSystem,
                           PlacementSystem placementSystem,
                           RoadsDatabaseSO database,
                           InputManager inputManager,
                           RoadMapping roadMapping,
                           SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.roadMapping = roadMapping;
        this.soundFeedback = soundFeedback;
        this.inputManager = inputManager;


        soundFeedback.PlaySound(SoundType.Click);
        roadMapping.SelectRoad(grid.CellToWorld(gridPosition), Vector2Int.one, 0, out selectedRoad, out index, out width, out selectedObjectIndex, out List<Vector3> displayPosList);
        if (index == -1)
            return;
        edited = false;
        posList = new();
        for (int i = 0; i < displayPosList.Count; i++)
        {
            posList.Add(grid.WorldToCell(displayPosList[i]));
        }
        originalPosList = posList;
        CalculateLength();
        previewSystem.ModifyRoad(displayPosList, selectedRoad, database.roadsData[selectedObjectIndex].Cost, length, width);
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
        if (edited == false)
        {
            List<Vector3> displayPosList = new();
            for (int i = 0; i < originalPosList.Count; i++)
            {
                displayPosList.Add(grid.CellToWorld(originalPosList[i]));
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

    public void OnModify(Vector3Int gridPosition, int rotation = 0)
    {
        if (previewSystem.expand == true)
        {
            posList[previewSystem.expanders.IndexOf(previewSystem.SelectedCursor)] = gridPosition;
            CalculateLength();
            previewSystem.MovePointer(grid.CellToWorld(gridPosition), CheckPlacementValidity(gridPosition, selectedObjectIndex), database.roadsData[selectedObjectIndex].Cost * Mathf.RoundToInt(length), Mathf.RoundToInt(length), width);
        }
        else
        {
            if (!(posList.Contains(gridPosition)))
            {
                posList.Add(gridPosition);
                CalculateLength();
                UpdateState(gridPosition, 0);
            }
        }

    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }

        Vector3Int pos = grid.WorldToCell(previewSystem.previewPos);
        soundFeedback.PlaySound(SoundType.Place);

        List<Vector3> displayPos = new List<Vector3>();
        for (int i = 0; i < posList.Count; i++)
        {
            displayPos.Add(grid.CellToWorld(posList[i]));
        }

        roadMapping.ModifyRoad(selectedRoad, displayPos, database.roadsData[selectedObjectIndex].width, selectedObjectIndex);

        posList.Clear();
        length = 0;
        edited = true;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        for (int i = 1; i < posList.Count; i++)
        {
            Vector3 p1 = grid.CellToWorld(posList[i - 1]);
            Vector3 p2 = grid.CellToWorld(posList[i]);

            if (!placementSystem.CanPlaceOnArea(p1, p2, width, 0.1f))
            {
                return false;
            }
        }

        if (posList.Count < 2)
            return false;

        return true;
    }

    public void UpdateState(Vector3Int gridPosition, int rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePointer(grid.CellToWorld(gridPosition), placementValidity, posList.IndexOf(gridPosition), database.roadsData[selectedObjectIndex].Cost * Mathf.RoundToInt(length), Mathf.RoundToInt(length), width);
    }
}

