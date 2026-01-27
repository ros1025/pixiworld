using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class DoorCreateState : IBuildingState
{
    //private int selectedObjectIndex = -1;
    DoorsData doorsDataObject;
    Grid grid;
    ObjectPlacementPreview previewSystem;
    PlacementSystem placementSystem;
    DoorDatabaseSO database;
    WallMapping wallMapping;
    private InputManager inputManager;
    private Vector3 displayPosition;
    private Vector3 position;
    private float rotation;
    private bool isReverse;

    public DoorCreateState(Vector3 gridPosition,
                          DoorsData doorsDataObject,
                          Grid grid,
                          ObjectPlacementPreview previewSystem,
                          PlacementSystem placementSystem,
                          DoorDatabaseSO database,
                          WallMapping wallMapping,
                          InputManager inputManager)
    {
        this.doorsDataObject = doorsDataObject;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.wallMapping = wallMapping;
        List<MatData> materials = new();

        //selectedObjectIndex = database.doorsData.IndexOf(doorsDataObject);
        if (database.doorsData.Contains(doorsDataObject))
        {
            previewSystem.StartPreview(
                doorsDataObject.Prefab,
                new Vector2Int(Mathf.RoundToInt(doorsDataObject.Length), 1),
                new Vector2(0, -0.5f),
                placementSystem,
                inputManager,
                materials);
            placementSystem.GetBuildToolsUI().EnableCustomTexture(materials, () => previewSystem.RefreshColors());
            UpdateState(gridPosition);
        }
        else
            throw new System.Exception($"No object with ID {doorsDataObject.ID}");

    }

    public void EndState()
    {
        previewSystem.StopPreview();
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

        Wall targetWall = wallMapping.GetWindowsFit(previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out position, isReverse);
        displayPosition = grid.LocalToWorld(position);

        List<MatData> newMaterials = new();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            newMaterials.Add(new MatData(previewSystem.materials[i]));
        }

        rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
        if (isReverse) rotation += 180;

        wallMapping.BuildDoor(doorsDataObject.Prefab, position, rotation, doorsDataObject.Length, doorsDataObject.Height, doorsDataObject.ID, targetWall, newMaterials, isReverse);

        previewSystem.UpdatePreview(grid.LocalToWorld(position), new Vector2Int(Mathf.RoundToInt(doorsDataObject.Length), 1), doorsDataObject.Cost, rotation);
        previewSystem.ApplyFeedback(false);
        placementSystem.GetBuildToolsUI().canPlace = false;
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        bool validity = false;

        if (wallMapping.CheckWindowsFit(previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out _, isReverse))
        {
            Wall targetWall = wallMapping.GetWindowsFit(previewSystem.previewSelector, gridPosition, doorsDataObject.Length, out position, isReverse);
            rotation = Vector3.SignedAngle(Vector3.right, targetWall.points[^1] - targetWall.points[0], Vector3.up);
            if (isReverse) 
            {
                rotation += 180;
            }
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