using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneCreateState : IBuildingState
{
    ZonesData zonesDataObject = null;
    Grid grid;
    ZonePlacementPreview previewSystem;
    PlacementSystem placementSystem;
    ZonesDatabaseSO database;
    ZonePlacer zonePlacer;
    private InputManager inputManager;
    private Vector2Int size;
    private Vector3 pos;
    private Vector3 displayPosition; 
    float rotation;

    public ZoneCreateState(Vector3 gridPosition,
                           ZonesData zonesDataObject,
                           Grid grid,
                           ZonePlacementPreview previewSystem,
                           PlacementSystem placementSystem,
                           ZonesDatabaseSO database,
                           ZonePlacer zonePlacer,
                           InputManager inputManager)
    {
        this.zonesDataObject = zonesDataObject;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.zonePlacer = zonePlacer;
        this.inputManager = inputManager;
        size = new Vector2Int(10, 10);

        //selectedObjectIndex = database.zonesData.IndexOf(zonesDataObject);
        if (database.zonesData.Contains(zonesDataObject))
        {
            pos = gridPosition;

            previewSystem.StartPreview(grid.LocalToWorld(pos), size, new Vector2Int(5,5), placementSystem, inputManager);

            UpdateState(pos, 0);
        }
        else
            throw new System.Exception($"No object with ID {zonesDataObject.ID}");
    }

    public void EndState()
    {
        previewSystem.StopPreview();
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        if (previewSystem.GetExpansionState() == true)
        {
            previewSystem.UpdateSize(grid.LocalToWorld(gridPosition));
            size = previewSystem.GetPreviewSize();
            pos = grid.WorldToLocal(previewSystem.GetPreviewPosition());
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
            return;
        }

        pos = grid.WorldToLocal(previewSystem.GetPreviewPosition());
        size = previewSystem.GetPreviewSize();
        displayPosition = grid.LocalToWorld(pos);

        zonePlacer.PlaceZones(displayPosition, pos, zonesDataObject.ID, size, 0.05f, rotation);

        previewSystem.UpdatePreview(grid.LocalToWorld(pos), size, (zonesDataObject.Cost * (size.x * size.y)), 0);
        previewSystem.ApplyFeedback(false);
        placementSystem.GetBuildToolsUI().canPlace = false;
    }
    
    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        bool validity = false;
        
        if (zonePlacer.CanPlaceObjectAt(previewSystem.previewSelector))
        {
            if (placementSystem.CanPlaceOnArea(grid.LocalToWorld(gridPosition), size, rotation))
            {
                validity = true;
            }
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePreview(grid.LocalToWorld(gridPosition), size, (zonesDataObject.Cost * (size.x * size.y)), rotation);
        previewSystem.ApplyFeedback(placementValidity);
        placementSystem.GetBuildToolsUI().canPlace = placementValidity;
    }
}
