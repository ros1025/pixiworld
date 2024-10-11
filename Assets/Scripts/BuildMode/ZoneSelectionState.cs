using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSelectionState : IBuildingState
{
    private int gameObjectIndex = -1;
    private GameObject selectedObject;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ZonesDatabaseSO database;
    ZonePlacer zonePlacer;
    InputManager inputManager;
    SoundFeedback soundFeedback;
    private Vector3 displayPosition;
    private Vector2Int size;
    private Vector3Int pos;
    private int rotation; private int originalRotation;
    bool edited;
    private Vector3Int originalPosition;
    private Vector2Int originalSize;

    public ZoneSelectionState(Vector3Int gridPosition,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          ZonesDatabaseSO database,
                          ZonePlacer zonePlacer,
                          InputManager inputManager,
                          SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.zonePlacer = zonePlacer;
        this.inputManager = inputManager;
        this.soundFeedback = soundFeedback;

        soundFeedback.PlaySound(SoundType.Click);
        selectedObject = zonePlacer.GetObject(previewSystem.previewSelectorObject, grid.CellToWorld(gridPosition), Vector2Int.one, 0);
        if (selectedObject == null || !zonePlacer.HasKey(selectedObject))
            return;
        edited = false;
        gameObjectIndex = zonePlacer.GetObjectID(selectedObject);
        originalPosition = zonePlacer.GetObjectCoordinate(selectedObject);
        originalRotation = zonePlacer.GetObjectRotation(selectedObject);
        originalSize = zonePlacer.GetObjectSize(selectedObject);
        pos = originalPosition;
        size = originalSize;
        rotation = originalRotation;
        placementSystem.SetRotation(rotation);
        previewSystem.StartMovingZones(
            grid.CellToWorld(pos),
            rotation,
            selectedObject,
            size
        );
        UpdateState(pos, rotation);
    }

    public void EndState()
    {
        previewSystem.StopMovingObject();
        if (edited == false)
        {
            displayPosition = grid.CellToWorld(originalPosition);
            zonePlacer.MoveZoneAt(selectedObject, pos, database.zonesData[gameObjectIndex].ID, displayPosition, size, rotation);
        }
        else
        {
            zonePlacer.MoveZoneAt(selectedObject, pos, database.zonesData[gameObjectIndex].ID, displayPosition, size, rotation);
        }
    }

    public void OnModify(Vector3Int gridPosition, int rotation = 0)
    {
        if (previewSystem.expand == true)
        {
            previewSystem.UpdateSize(grid.CellToWorld(gridPosition));
            size = previewSystem.previewSize;
            pos = grid.WorldToCell(previewSystem.previewPos);
            UpdateState(pos, rotation);
        }
        else
        {
            pos = gridPosition;
            UpdateState(pos, rotation);
            this.rotation = rotation;
        }
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, gameObjectIndex);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        pos = grid.WorldToCell(previewSystem.previewPos);
        displayPosition = grid.CellToWorld(pos);
        size = previewSystem.previewSize;

        zonePlacer.MoveZoneAt(selectedObject, pos, database.zonesData[gameObjectIndex].ID, displayPosition, size, rotation);

        previewSystem.UpdatePosition(grid.CellToWorld(pos), true, size, (database.zonesData[gameObjectIndex].Cost * (size.x * size.y)) - (database.zonesData[gameObjectIndex].Cost * (originalSize.x * originalSize.y)), rotation);
        originalPosition = gridPosition;
        originalRotation = rotation;
        edited = true;
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        bool validity = false;

        if (zonePlacer.CanMoveObjectAt(originalPosition, previewSystem.previewSelector))
        {
            if (placementSystem.CanPlaceOnArea(grid.CellToWorld(gridPosition), size, rotation))
            {
                validity = true;
            }
        }

        return validity;
    }
    public void UpdateState(Vector3Int gridPosition, int rotation = 0)
    {
        bool validity = CheckPlacementValidity(gridPosition, gameObjectIndex);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity, size, (database.zonesData[gameObjectIndex].Cost * (size.x * size.y)) - (database.zonesData[gameObjectIndex].Cost * (originalSize.x * originalSize.y)), rotation);
    }
}