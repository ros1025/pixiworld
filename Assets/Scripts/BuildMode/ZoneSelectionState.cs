using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSelectionState : IBuildingState
{
    private long gameObjectIndex = -1;
    private GameObject selectedObject;
    private ZonesData zonesDataObject = null;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ZonesDatabaseSO database;
    ZonePlacer zonePlacer;
    InputManager inputManager;
    SoundFeedback soundFeedback;
    private Vector3 displayPosition;
    private Vector2Int size;
    private Vector3 pos;
    private float rotation; private float originalRotation;
    bool edited;
    private Vector3 originalPosition;
    private Vector2Int originalSize;

    public ZoneSelectionState(Vector3 gridPosition,
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
        selectedObject = zonePlacer.GetObject(previewSystem.previewSelectorObject, grid.LocalToWorld(gridPosition), Vector2Int.one, 0);
        if (selectedObject == null || !zonePlacer.HasKey(selectedObject))
            return;
        edited = false;
        gameObjectIndex = zonePlacer.GetObjectID(selectedObject);
        originalPosition = zonePlacer.GetObjectCoordinate(selectedObject);
        originalRotation = zonePlacer.GetObjectRotation(selectedObject);
        originalSize = zonePlacer.GetObjectSize(selectedObject);

        zonesDataObject = database.zonesData.Find(item => item.ID == gameObjectIndex);

        pos = originalPosition;
        size = originalSize;
        rotation = originalRotation;
        placementSystem.SetRotation(rotation);
        previewSystem.StartMovingZones(
            grid.LocalToWorld(pos),
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
            displayPosition = grid.LocalToWorld(originalPosition);
            zonePlacer.MoveZoneAt(selectedObject, pos, zonesDataObject.ID, displayPosition, size, rotation);
        }
        else
        {
            zonePlacer.MoveZoneAt(selectedObject, pos, zonesDataObject.ID, displayPosition, size, rotation);
        }
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        if (previewSystem.expand == true)
        {
            previewSystem.UpdateSize(grid.LocalToWorld(gridPosition));
            size = previewSystem.previewSize;
            pos = grid.WorldToLocal(previewSystem.previewPos);
            UpdateState(pos, rotation);
        }
        else
        {
            pos = gridPosition;
            UpdateState(pos, rotation);
            this.rotation = rotation;
        }
    }

    public void OnAction(Vector3 gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        pos = grid.WorldToLocal(previewSystem.previewPos);
        displayPosition = grid.LocalToWorld(pos);
        size = previewSystem.previewSize;

        zonePlacer.MoveZoneAt(selectedObject, pos, zonesDataObject.ID, displayPosition, size, rotation);

        previewSystem.UpdatePosition(grid.LocalToWorld(pos), true, size, (zonesDataObject.Cost * (size.x * size.y)) - (zonesDataObject.Cost * (originalSize.x * originalSize.y)), rotation);
        originalPosition = gridPosition;
        originalRotation = rotation;
        edited = true;
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        bool validity = false;

        if (zonePlacer.CanMoveObjectAt(selectedObject, previewSystem.previewSelector))
        {
            if (placementSystem.CanMoveOnArea(selectedObject))
            {
                validity = true;
            }
        }

        return validity;
    }
    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool validity = CheckPlacementValidity(gridPosition);
        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), validity, size, (zonesDataObject.Cost * (size.x * size.y)) - (zonesDataObject.Cost * (originalSize.x * originalSize.y)), rotation);
    }
}