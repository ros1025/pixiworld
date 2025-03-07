using UnityEngine;
using System.Collections.Generic;

public class DoorModifyState : IBuildingState
{
    private int selectedObjectIndex = -1;
    private Door selectedDoor = null;
    bool edited;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    DoorDatabaseSO database;
    WallMapping wallMapping;
    SoundFeedback soundFeedback;
    InputManager inputManager;
    private Vector3 displayPosition;
    private Vector3 position; private Vector3 originalPosition;
    private float rotation; private float originalRotation;
    List<MatData> materials;

    public DoorModifyState(Vector3 gridPosition,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          DoorDatabaseSO database,
                          WallMapping wallMapping,
                          InputManager inputManager,
                          SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.wallMapping = wallMapping;
        this.soundFeedback = soundFeedback;
        this.inputManager = inputManager;

        Door door = wallMapping.GetDoorSelect(grid.LocalToWorld(gridPosition), Vector2Int.one, 0);
        selectedObjectIndex = door.ID;
        if (door != null)
        {
            edited = false;
            originalPosition = door.point;
            originalRotation = door.rotation;
            materials = door.materials;
            rotation = originalRotation;
            selectedDoor = door;
            previewSystem.StartMovingObjectPreview(
            grid.LocalToWorld(originalPosition),
            originalRotation,
            door.prefab,
            new Vector2Int(database.doorsData[selectedObjectIndex].Length, 1),
            materials
            );
        }
        else return;
    }

    public void EndState()
    {
        previewSystem.StopMovingObject();
        if (edited == false)
        {
            displayPosition = grid.LocalToWorld(originalPosition);
            wallMapping.MoveWindows(selectedDoor, originalPosition, originalRotation, database.doorsData[selectedObjectIndex].Length, database.doorsData[selectedObjectIndex].ID, selectedDoor.targetWall, selectedDoor.materials);
        }
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

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);

        Renderer[] renderers = database.doorsData[selectedObjectIndex].Prefab.GetComponentsInChildren<Renderer>();

        Wall targetWall = wallMapping.GetWindowsMove(selectedDoor, previewSystem.previewSelector, gridPosition, database.doorsData[selectedObjectIndex].Length, out position);
        displayPosition = grid.LocalToWorld(position);
        rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);

        materials.Clear();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            materials.Add(previewSystem.materials[i]);
        }

        wallMapping.MoveWindows(selectedDoor, position, rotation, database.doorsData[selectedObjectIndex].Length, database.doorsData[selectedObjectIndex].ID, targetWall, materials);
        originalPosition = gridPosition;
        originalRotation = rotation;
        edited = true;
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition, int selectedObjectIndex)
    {
        bool validity = false;

        if (wallMapping.CheckWindowsMove(selectedDoor, previewSystem.previewSelector, gridPosition, database.doorsData[selectedObjectIndex].Length, out _))
        {
            Wall targetWall = wallMapping.GetWindowsMove(selectedDoor, previewSystem.previewSelector, gridPosition, database.doorsData[selectedObjectIndex].Length, out position);
            rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
            validity = true;
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        position = gridPosition;
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.LocalToWorld(position), placementValidity, new Vector2Int(database.doorsData[selectedObjectIndex].Length, 1), database.doorsData[selectedObjectIndex].Cost, this.rotation);
    }
}
