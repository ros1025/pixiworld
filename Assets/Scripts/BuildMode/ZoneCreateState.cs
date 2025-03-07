using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneCreateState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ZonesDatabaseSO database;
    ZonePlacer zonePlacer;
    SoundFeedback soundFeedback;
    private Vector2Int size;
    private Vector3 pos;
    private Vector3 displayPosition; 
    float rotation;

    public ZoneCreateState(Vector3 gridPosition,
                           int iD,
                           Grid grid,
                           PreviewSystem previewSystem,
                           PlacementSystem placementSystem,
                           ZonesDatabaseSO database,
                           ZonePlacer zonePlacer,
                           SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.zonePlacer = zonePlacer;
        this.soundFeedback = soundFeedback;
        size = new Vector2Int(10, 10);

        selectedObjectIndex = database.zonesData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            pos = gridPosition;

            previewSystem.StartCreatingZones(grid.LocalToWorld(pos), size, new Vector2Int(5,5));

            UpdateState(pos, 0);
        }
        else
            throw new System.Exception($"No object with ID {iD}");
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
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }

        pos = grid.WorldToLocal(previewSystem.previewPos);
        size = previewSystem.previewSize;
        displayPosition = grid.LocalToWorld(pos);
        soundFeedback.PlaySound(SoundType.Place);

        zonePlacer.PlaceZones(displayPosition, pos, ID, size, 0.05f, rotation);

        previewSystem.UpdatePosition(grid.LocalToWorld(pos), false, size, (database.zonesData[selectedObjectIndex].Cost * (size.x * size.y)), 0);
    }
    
    private bool CheckPlacementValidity(Vector3 gridPosition, int selectedObjectIndex)
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
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), placementValidity, size, (database.zonesData[selectedObjectIndex].Cost * (size.x * size.y)), rotation);
    }
}
