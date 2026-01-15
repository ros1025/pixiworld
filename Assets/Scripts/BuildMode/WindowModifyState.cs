using UnityEngine;
using System.Collections.Generic;

public class WindowModifyState : IBuildingState
{
    private long selectedObjectIndex = -1;
    private Window selectedWindow = null;
    private WindowsData windowsDataObject = null;
    bool edited;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    WindowsDatabaseSO database;
    WallMapping wallMapping;
    InputManager inputManager;
    private Vector3 displayPosition;
    private Vector3 position; private Vector3 originalPosition;
    private float rotation; private float originalRotation;
    List<MatData> materials;

    public WindowModifyState(Vector3 gridPosition,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          WindowsDatabaseSO database,
                          WallMapping wallMapping,
                          InputManager inputManager)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.wallMapping = wallMapping;
        this.inputManager = inputManager;

        Window window = wallMapping.GetWindowSelect(grid.LocalToWorld(gridPosition), Vector2Int.one, 0);
        selectedObjectIndex = window.ID;

        if (window != null)
        {
            windowsDataObject = database.windowsData.Find(data => data.ID == selectedObjectIndex);

            edited = false;
            originalPosition = window.point;
            originalRotation = window.rotation;
            materials = window.materials;
            rotation = originalRotation;
            selectedWindow = window;
            previewSystem.StartMovingObjectPreview(
            grid.LocalToWorld(originalPosition),
            originalRotation,
            window.prefab,
            new Vector2Int(Mathf.RoundToInt(windowsDataObject.Length), 1),
            new Vector2(0, -0.5f),
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
            wallMapping.MoveWindows(selectedWindow, originalPosition, originalRotation, windowsDataObject.Length, windowsDataObject.Height, windowsDataObject.ID, selectedWindow.targetWall, selectedWindow.materials);
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

        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (placementValidity == false)
        {
            return;
        }

        Renderer[] renderers = windowsDataObject.Prefab.GetComponentsInChildren<Renderer>();

        Wall targetWall = wallMapping.GetWindowsMove(selectedWindow, previewSystem.previewSelector, gridPosition, windowsDataObject.Length, out position);
        displayPosition = grid.LocalToWorld(position);
        rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);

        materials.Clear();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            materials.Add(previewSystem.materials[i]);
        }

        wallMapping.MoveWindows(selectedWindow, position, rotation, windowsDataObject.Length, windowsDataObject.Height, windowsDataObject.ID, targetWall, materials);
        originalPosition = gridPosition;
        originalRotation = rotation;
        edited = true;
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        bool validity = false;

        if (wallMapping.CheckWindowsMove(selectedWindow, previewSystem.previewSelector, gridPosition, windowsDataObject.Length, out _))
        {
            Wall targetWall = wallMapping.GetWindowsMove(selectedWindow, previewSystem.previewSelector, gridPosition, windowsDataObject.Length, out position);
            rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
            validity = true;
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        position = gridPosition;
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePosition(grid.LocalToWorld(position), placementValidity, new Vector2Int(Mathf.RoundToInt(windowsDataObject.Length), 1), windowsDataObject.Cost, this.rotation);
    }
}
