using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ObjectsDatabaseSO database;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
    private Vector3 displayPosition;
    private int rotation;

    public PlacementState(Vector3Int gridPosition,
                          int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          ObjectsDatabaseSO database,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.furnitureData = objectPlacer.furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
            UpdateState(gridPosition);
        }
        else
            throw new System.Exception($"No object with ID {iD}");

    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnModify(Vector3Int gridPosition, int rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        objectPlacer = placementSystem.GetCurrentObjectPlacer();
        UpdateState(gridPosition, rotation);
        this.rotation = rotation;
    }

    public void OnAction(Vector3Int gridPosition)
    {
        grid = placementSystem.GetCurrentGrid();
        objectPlacer = placementSystem.GetCurrentObjectPlacer();

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        displayPosition = grid.CellToWorld(gridPosition);

        objectPlacer.PlaceObject(database.objectsData[selectedObjectIndex].Prefab, gridPosition,
            displayPosition, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].ID, rotation);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].Cost, rotation);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        //GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
        //    floorData :
        //    furnitureData;
        //GridData selectedData = furnitureData;
        bool validity = false;

        if (furnitureData.CanPlaceObjectAt(previewSystem.previewSelector))
        {
            if (placementSystem.CanPlaceOnArea(grid.CellToWorld(gridPosition), database.objectsData[selectedObjectIndex].Size, rotation))
            {
                validity = true;
            }
        }

        return validity;
    }

    public void UpdateState(Vector3Int gridPosition, int rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].Cost, rotation);
    }
}