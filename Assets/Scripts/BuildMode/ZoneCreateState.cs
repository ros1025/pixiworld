using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneCreateState : IBuildingState
{
    ZonesData zonesDataObject = null;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ZonesDatabaseSO database;
    ZonePlacer zonePlacer;
    private Vector2Int size;
    private Vector3 pos;
    private Vector3 displayPosition; 
    float rotation;

    public ZoneCreateState(Vector3 gridPosition,
                           ZonesData zonesDataObject,
                           Grid grid,
                           PreviewSystem previewSystem,
                           PlacementSystem placementSystem,
                           ZonesDatabaseSO database,
                           ZonePlacer zonePlacer)
    {
        this.zonesDataObject = zonesDataObject;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.zonePlacer = zonePlacer;
        size = new Vector2Int(10, 10);

        //selectedObjectIndex = database.zonesData.IndexOf(zonesDataObject);
        if (database.zonesData.Contains(zonesDataObject))
        {
            pos = gridPosition;

            previewSystem.StartCreatingZones(grid.LocalToWorld(pos), size, new Vector2Int(5,5));

            UpdateState(pos, 0);
        }
        else
            throw new System.Exception($"No object with ID {zonesDataObject.ID}");
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
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
            return;
        }

        pos = grid.WorldToLocal(previewSystem.previewPos);
        size = previewSystem.previewSize;
        displayPosition = grid.LocalToWorld(pos);

        zonePlacer.PlaceZones(displayPosition, pos, zonesDataObject.ID, size, 0.05f, rotation);

        previewSystem.UpdatePosition(grid.LocalToWorld(pos), false, size, (zonesDataObject.Cost * (size.x * size.y)), 0);
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

        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), placementValidity, size, (zonesDataObject.Cost * (size.x * size.y)), rotation);
    }
}
