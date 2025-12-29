using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCreateState : IBuildingState
{
    //private int selectedObjectIndex = -1;
    RoadsData roadsData;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    SoundFeedback soundFeedback;
    int width;
    float length;
    private List<Vector3> posList;
    private Vector3 displayPosition; 
    float rotation;

    public RoadCreateState(Vector3 gridPosition,
                           RoadsData roadsData,
                           Grid grid,
                           PreviewSystem previewSystem,
                           PlacementSystem placementSystem,
                            RoadsDatabaseSO database,
                           RoadMapping roadMapping,
                           SoundFeedback soundFeedback)
    {
        this.roadsData = roadsData;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.roadMapping = roadMapping;
        this.soundFeedback = soundFeedback;


        posList = new();
        //selectedObjectIndex = database.roadsData.IndexOf(roadsData);
        if (database.roadsData.Contains(roadsData))
        {
            width = roadsData.width;

            previewSystem.StartCreatingRoads(grid.LocalToWorld(gridPosition));
        }
        else
            throw new System.Exception($"No object with ID {roadsData.ID}");
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    private void CalculateLength()
    {
        length = 0;
        if (posList.Count > 1)
        {
            for (int i = 1; i < posList.Count; i++)
            {
                length += Vector3.Distance(posList[i - 1], posList[i]);
            }
        }
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        if (previewSystem.expand == true)
        {
            int index = previewSystem.expanders.IndexOf(previewSystem.SelectedCursor);
            if ((index > 0 && Vector3.Distance(posList[index - 1], gridPosition) < 0.1f) || (index < posList.Count - 1 && Vector3.Distance(posList[index + 1], gridPosition) < 0.1f))
            {
                posList.RemoveAt(index);
                CalculateLength();
                previewSystem.RemovePointer(index, CheckPlacementValidity(gridPosition), roadsData.Cost * Mathf.RoundToInt(length), length, width, 0.1f);
            }
            else if (index >= 0)
            {
                posList[index] = gridPosition;
                CalculateLength();
                previewSystem.MovePointer(grid.LocalToWorld(gridPosition), CheckPlacementValidity(gridPosition), roadsData.Cost * Mathf.RoundToInt(length), length, width, 0.1f);
            }
        }
        else
        {
            if (previewSystem.SelectedCursor == previewSystem.gameObject)
            {
                if (!(posList.Contains(gridPosition)))
                {
                    int index = previewSystem.GetSplineIndex(gridPosition);
                    if (index >= 0)
                    {
                        posList.Insert(index, gridPosition);
                        CalculateLength();
                        UpdateState(gridPosition, 0);
                    }
                }
            }
            else if (!(posList.Contains(gridPosition)))
            {
                posList.Add(gridPosition);
                CalculateLength();
                UpdateState(gridPosition, 0);
            }
        }

    }

    public void OnAction(Vector3 gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);

        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }

        Vector3 pos = grid.WorldToLocal(previewSystem.previewPos);
        soundFeedback.PlaySound(SoundType.Place);

        List<Vector3> displayPos = new List<Vector3>();
        for (int i = 0; i < posList.Count; i++)
        {
            displayPos.Add(grid.LocalToWorld(posList[i]));
        }

        roadMapping.AddRoad(displayPos, roadsData.width, roadsData.ID, roadsData.tex);

        posList.Clear();
        length = 0;
        previewSystem.ClearPointer();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        for (int i = 1; i < posList.Count; i++)
        {
            Vector3 p1 = grid.LocalToWorld(posList[i - 1]);
            Vector3 p2 = grid.LocalToWorld(posList[i]);

            if (!placementSystem.CanPlaceOnArea(p1, p2, width, 0.1f))
            {
                return false;
            }
        }

        if (posList.Count < 2)
            return false;

        return true;
    }

    public void UpdateState(Vector3 gridPosition, float rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePointer(grid.LocalToWorld(gridPosition), placementValidity, posList.IndexOf(gridPosition), roadsData.Cost * Mathf.RoundToInt(length), length, width, 0.1f);
    }
}
