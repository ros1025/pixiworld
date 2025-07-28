using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DoorCreateState : IBuildingState
{
    //private int selectedObjectIndex = -1;
    DoorsData doorsDataObject;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    DoorDatabaseSO database;
    WallMapping wallMapping;
    SoundFeedback soundFeedback;
    private Vector3 displayPosition;
    private Vector3 position;
    private float rotation;

    public DoorCreateState(Vector3 gridPosition,
                          DoorsData doorsDataObject,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          DoorDatabaseSO database,
                          WallMapping wallMapping,
                          SoundFeedback soundFeedback)
    {
        this.doorsDataObject = doorsDataObject;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.wallMapping = wallMapping;
        this.soundFeedback = soundFeedback;

        //selectedObjectIndex = database.doorsData.IndexOf(doorsDataObject);
        if (database.doorsData.Contains(doorsDataObject))
        {
            previewSystem.StartShowingPlacementPreview(
                doorsDataObject.Prefab,
                new Vector2Int(doorsDataObject.Length, 1));
            UpdateState(gridPosition);
        }
        else
            throw new System.Exception($"No object with ID {doorsDataObject.ID}");

    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();
        UpdateState(gridPosition, rotation);
    }

    public void OnAction(Vector3 gridPosition)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);

        Wall targetWall = wallMapping.GetWindowsFit(previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out position);
        displayPosition = grid.LocalToWorld(position);

        List<MatData> newMaterials = new();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            newMaterials.Add(new MatData(previewSystem.materials[i]));
        }

        rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
        wallMapping.BuildWindows(doorsDataObject.Prefab, position, rotation, doorsDataObject.Length, doorsDataObject.ID, targetWall, newMaterials);

        previewSystem.UpdatePosition(grid.LocalToWorld(position), false, new Vector2Int(doorsDataObject.Length, 1), doorsDataObject.Cost, rotation);
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        bool validity = false;

        if (wallMapping.CheckWindowsFit(previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out _))
        {
            Wall targetWall = wallMapping.GetWindowsFit(previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out position);
            rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
            validity = true;
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        position = gridPosition;
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePosition(grid.LocalToWorld(position), placementValidity, new Vector2Int(doorsDataObject.Length, 1), doorsDataObject.Cost, this.rotation);
    }
}