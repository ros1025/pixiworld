using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class ObjectPlacementState : IBuildingState
{
    //private int selectedObjectIndex = -1;
    ObjectData objectData;
    Grid grid;
    ObjectPlacementPreview previewSystem;
    PlacementSystem placementSystem;
    ObjectsDatabaseSO database;
    ObjectPlacer objectPlacer;
    private Vector3 displayPosition;
    private float rotation;

    public ObjectPlacementState(Vector3 gridPosition,
                          ObjectData objectData,
                          Grid grid,
                          ObjectPlacementPreview previewSystem,
                          PlacementSystem placementSystem,
                          ObjectsDatabaseSO database,
                          ObjectPlacer objectPlacer,
                          InputManager inputManager)
    {
        this.objectData = objectData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.objectPlacer = objectPlacer;
        List<MatData> materials = new();

        //selectedObjectIndex = database.objectsData.IndexOf(objectData);
        if (database.objectsData.Contains(objectData))
        {
            previewSystem.StartPreview(
                objectData.Prefab,
                objectData.Size,
                Vector3.zero,
                placementSystem,
                inputManager,
                materials);
            UpdateState(gridPosition);
            placementSystem.GetBuildToolsUI().EnableCustomTexture(materials, () => previewSystem.RefreshColors());
        }
        else
            throw new System.Exception($"No object with {objectData.name}");

    }

    public void EndState()
    {
        previewSystem.StopPreview();
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        objectPlacer = placementSystem.GetCurrentObjectPlacer();
        UpdateState(gridPosition, rotation);
        this.rotation = rotation;
    }

    public void OnAction(Vector3 gridPosition)
    {
        grid = placementSystem.GetCurrentGrid();
        objectPlacer = placementSystem.GetCurrentObjectPlacer();

        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (placementValidity == false)
        {
            return;
        }
        displayPosition = grid.LocalToWorld(gridPosition);

        List<MatData> newMaterials = new();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            newMaterials.Add(new MatData(previewSystem.materials[i]));
        }

        objectPlacer.PlaceObject(objectData.Prefab, gridPosition,
            displayPosition, objectData.Size, objectData.ID, rotation, newMaterials);

        previewSystem.UpdatePreview(grid.LocalToWorld(gridPosition), objectData.Size, objectData.Cost, rotation);
        previewSystem.ApplyFeedback(false);
        placementSystem.GetBuildToolsUI().canPlace = false;
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        //GridData selectedData = objectData.ID == 0 ?
        //    floorData :
        //    furnitureData;
        //GridData selectedData = furnitureData;
        bool validity = false;

        if (objectPlacer.CanPlaceObjectAt(previewSystem.previewSelector))
        {
            if (placementSystem.CanPlaceOnArea(grid.LocalToWorld(gridPosition), objectData.Size, rotation))
            {
                validity = true;
            }
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePreview(grid.LocalToWorld(gridPosition), objectData.Size, objectData.Cost, rotation);
        previewSystem.ApplyFeedback(placementValidity);
        placementSystem.GetBuildToolsUI().canPlace = placementValidity;
    }
}