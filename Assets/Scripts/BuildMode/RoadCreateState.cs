using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCreateState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    SoundFeedback soundFeedback;
    int width;
    float length;
    private List<Vector3Int> posList;
    private Vector3 displayPosition; 
    int rotation;

    public RoadCreateState(Vector3Int gridPosition,
                           int iD,
                           Grid grid,
                           PreviewSystem previewSystem,
                           PlacementSystem placementSystem,
                            RoadsDatabaseSO database,
                           RoadMapping roadMapping,
                           SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.roadMapping = roadMapping;
        this.soundFeedback = soundFeedback;


        posList = new();
        selectedObjectIndex = database.roadsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            width = database.roadsData[selectedObjectIndex].width;

            previewSystem.StartCreatingRoads(grid.CellToWorld(gridPosition));
        }
        else
            throw new System.Exception($"No object with ID {iD}");
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
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

        roadMapping.AddRoad(displayPos, database.roadsData[selectedObjectIndex].width, ID);

        posList.Clear();
        length = 0;
        previewSystem.ClearPointer();
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
