using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    ObjectData objectData;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    ObjectsDatabaseSO database;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
    private Vector3 displayPosition;
    private float rotation;

    public PlacementState(Vector3 gridPosition,
                          ObjectData objectData,
                          Grid grid,
                          PreviewSystem previewSystem,
                          PlacementSystem placementSystem,
                          ObjectsDatabaseSO database,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback)
    {
        this.objectData = objectData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

        selectedObjectIndex = database.objectsData.IndexOf(objectData);
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
            UpdateState(gridPosition);
        }
        else
            throw new System.Exception($"No object with {objectData.name}");

    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
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

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        displayPosition = grid.LocalToWorld(gridPosition);

        List<MatData> newMaterials = new();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            newMaterials.Add(new MatData(previewSystem.materials[i]));
        }

        objectPlacer.PlaceObject(database.objectsData[selectedObjectIndex].Prefab, gridPosition,
            displayPosition, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].ID, rotation, newMaterials, objectData);

        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), false, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].Cost, rotation);
    }

    private bool CheckPlacementValidity(Vector3 gridPosition, int selectedObjectIndex)
    {
        //GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
        //    floorData :
        //    furnitureData;
        //GridData selectedData = furnitureData;
        bool validity = false;

        if (objectPlacer.CanPlaceObjectAt(previewSystem.previewSelector))
        {
            if (placementSystem.CanPlaceOnArea(grid.LocalToWorld(gridPosition), database.objectsData[selectedObjectIndex].Size, rotation))
            {
                validity = true;
            }
        }

        return validity;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.LocalToWorld(gridPosition), placementValidity, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].Cost, rotation);
    }
}