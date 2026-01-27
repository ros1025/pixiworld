using UnityEngine;
using System.Collections.Generic;

public class DoorModifyState : IBuildingState
{
    private long selectedObjectIndex = -1;
    private Door selectedDoor = null;
    private DoorsData doorsDataObject = null;
    bool edited;
    Grid grid;
    ObjectSelectionPreview previewSystem;
    PlacementSystem placementSystem;
    DoorDatabaseSO database;
    WallMapping wallMapping;
    InputManager inputManager;
    private Vector3 displayPosition;
    private Vector3 position; private Vector3 originalPosition;
    private float rotation; private float originalRotation;
    List<MatData> materials;
    private bool isReverse;

    public DoorModifyState(Vector3 gridPosition,
                          Grid grid,
                          ObjectSelectionPreview previewSystem,
                          PlacementSystem placementSystem,
                          DoorDatabaseSO database,
                          WallMapping wallMapping,
                          InputManager inputManager)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.wallMapping = wallMapping;
        this.inputManager = inputManager;

        Door door = wallMapping.GetDoorSelect(grid.LocalToWorld(gridPosition), Vector2Int.one, 0);
        selectedObjectIndex = door.ID;

        if (door != null)
        {
            doorsDataObject = database.doorsData.Find(data => data.ID == selectedObjectIndex);
            isReverse = door.isReverse;

            if (door.isReverse)
            {
                placementSystem.SetRotation(180);
            }
            else
            {
                placementSystem.SetRotation(0);
            }

            edited = false;
            originalPosition = door.point;
            originalRotation = door.rotation;
            materials = door.materials;
            rotation = originalRotation;
            selectedDoor = door;
            previewSystem.StartPreview(
            grid.LocalToWorld(originalPosition),
            originalRotation,
            door.prefab,
            new Vector2Int(Mathf.RoundToInt(doorsDataObject.Length), 1),
            new Vector2(0, -0.5f),
            materials, placementSystem, inputManager
            );

            placementSystem.GetBuildToolsUI().EnableSellButton(() => placementSystem.RemoveDoor(selectedDoor.prefab));
            placementSystem.GetBuildToolsUI().EnableCustomTexture(previewSystem.materials, () => previewSystem.RefreshColors());
        }
        else return;
    }

    public void EndState()
    {
        previewSystem.previewObject = null;
        previewSystem.StopPreview();
        if (edited == false)
        {
            displayPosition = grid.LocalToWorld(originalPosition);
            wallMapping.MoveDoors(selectedDoor, originalPosition, originalRotation, doorsDataObject.Length, doorsDataObject.Height, doorsDataObject.ID, selectedDoor.targetWall, selectedDoor.materials, selectedDoor.isReverse);
        }
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        if (rotation % 360 >= 0 && rotation % 360 < 180)
        {
            isReverse = false;
        }
        else
        {
            isReverse = true;
        }

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
            return;
        }

        Renderer[] renderers = doorsDataObject.Prefab.GetComponentsInChildren<Renderer>();

        Wall targetWall = wallMapping.GetWindowsMove(selectedDoor, previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out position, isReverse);
        displayPosition = grid.LocalToWorld(position);
        rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
        if (isReverse) rotation += 180;

        materials.Clear();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            materials.Add(previewSystem.materials[i]);
        }

        wallMapping.MoveDoors(selectedDoor, position, rotation, doorsDataObject.Length, doorsDataObject.Height, doorsDataObject.ID, targetWall, materials, isReverse);
        originalPosition = gridPosition;
        originalRotation = rotation;
        edited = true;
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        bool validity = false;

        if (wallMapping.CheckWindowsMove(selectedDoor, previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out _, isReverse))
        {
            Wall targetWall = wallMapping.GetWindowsMove(selectedDoor, previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out position, isReverse);
            rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
            if (isReverse) rotation += 180;
            validity = true;
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        position = gridPosition;
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePreview(grid.LocalToWorld(position), new Vector2Int(Mathf.RoundToInt(doorsDataObject.Length), 1), doorsDataObject.Cost, this.rotation);
        previewSystem.ApplyFeedback(placementValidity);
        placementSystem.GetBuildToolsUI().canPlace = placementValidity;
    }
}
