using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionState : IBuildingState
{
    private int gameObjectIndex = -1;
    private GameObject selectedObject = null;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ObjectsDatabaseSO database;
    ObjectPlacer objectPlacer;
    InputManager inputManager;
    SoundFeedback soundFeedback;
    List<MatData> materials;
    private Vector3 displayPosition;
    private float rotation; private float originalRotation;
    bool edited;
    private Vector3 originalPosition;

    public SelectionState(Vector3 gridPosition,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          ObjectsDatabaseSO database,
                          ObjectPlacer objectPlacer,
                          InputManager inputManager,
                          SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.objectPlacer = objectPlacer;
        this.inputManager = inputManager;
        this.soundFeedback = soundFeedback;

        soundFeedback.PlaySound(SoundType.Click);
        selectedObject = objectPlacer.GetObject(previewSystem.previewSelectorObject, grid.LocalToWorld(gridPosition), Vector2Int.one, 0);
        if (selectedObject == null || !objectPlacer.HasKey(selectedObject))
            return;
        edited = false;
        gameObjectIndex = objectPlacer.GetObjectID(selectedObject);
        originalPosition = objectPlacer.GetObjectCoordinate(selectedObject);
        originalRotation = objectPlacer.GetObjectRotation(selectedObject);
        materials = objectPlacer.GetObjectRenderers(selectedObject);
        rotation = originalRotation;
        placementSystem.SetRotation(originalRotation);
        placementSystem.SetSelectedPosition(originalPosition);
        previewSystem.StartMovingObjectPreview(
            grid.LocalToWorld(objectPlacer.GetObjectCoordinate(selectedObject)),
            objectPlacer.GetObjectRotation(selectedObject),
            selectedObject,
            database.objectsData[gameObjectIndex].Size,
            materials
        );
        UpdateState(originalPosition, rotation);
    }

    public void EndState()
    {
        previewSystem.StopMovingObject();
        if (edited == false)
        {
            
            displayPosition = grid.LocalToWorld(originalPosition);
            objectPlacer.MoveObjectAt(selectedObject, originalPosition, displayPosition, database.objectsData[gameObjectIndex].Size, database.objectsData[gameObjectIndex].ID, originalRotation, materials);
        }
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        objectPlacer = placementSystem.GetCurrentObjectPlacer();
        UpdateState(gridPosition, this.rotation);
        this.rotation = rotation;
    }

    public void OnAction(Vector3 gridPosition)
    {
        grid = placementSystem.GetCurrentGrid();
        objectPlacer = placementSystem.GetCurrentObjectPlacer();

        bool placementValidity = CheckPlacementValidity(gridPosition, gameObjectIndex);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        displayPosition = grid.LocalToWorld(gridPosition);

        materials.Clear();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            materials.Add(previewSystem.materials[i]);
        }
        objectPlacer.MoveObjectAt(selectedObject, gridPosition, displayPosition, database.objectsData[gameObjectIndex].Size, database.objectsData[gameObjectIndex].ID, rotation, materials);

        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), true, database.objectsData[gameObjectIndex].Size, database.objectsData[gameObjectIndex].Cost, rotation);
        originalPosition = gridPosition;
        originalRotation = rotation;
        edited = true;
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition, int selectedObjectIndex)
    {
        //GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
        //    floorData :
        //    furnitureData;
        bool validity = false;

        if (objectPlacer.CanMoveObjectAt(selectedObject, previewSystem.previewSelector))
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
        bool validity = CheckPlacementValidity(gridPosition, gameObjectIndex);
        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), validity, database.objectsData[gameObjectIndex].Size, database.objectsData[gameObjectIndex].Cost, rotation);
    }
}
