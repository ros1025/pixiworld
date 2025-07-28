using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadModifyState : IBuildingState
{
    private long selectedObjectIndex = -1;
    private RoadsData roadsDataObject = null;
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    RoadsDatabaseSO database;
    RoadMapping roadMapping;
    SoundFeedback soundFeedback;
    InputManager inputManager;
    Roads selectedRoad;
    int width;
    int index;
    float length;
    private List<Vector3> posList;
    private List<Vector3> originalPosList;
    bool edited;

    public RoadModifyState(Vector3 gridPosition,
                           Grid grid,
                           PreviewSystem previewSystem,
                           PlacementSystem placementSystem,
                           RoadsDatabaseSO database,
                           InputManager inputManager,
                           RoadMapping roadMapping,
                           SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.database = database;
        this.roadMapping = roadMapping;
        this.soundFeedback = soundFeedback;
        this.inputManager = inputManager;


        soundFeedback.PlaySound(SoundType.Click);
        roadMapping.SelectRoad(grid.LocalToWorld(gridPosition), Vector2Int.one, 0, out selectedRoad, out index, out width, out selectedObjectIndex, out List<Vector3> displayPosList);
        roadsDataObject = database.roadsData.Find(data => data.ID == selectedObjectIndex);


        if (index == -1)
            return;
        edited = false;
        posList = new();
        for (int i = 0; i < displayPosList.Count; i++)
        {
            posList.Add(grid.WorldToLocal(displayPosList[i]));
        }
        originalPosList = posList;
        CalculateLength();
        previewSystem.ModifyRoad(displayPosList, selectedRoad, roadsDataObject.Cost, length, width);
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
        if (edited == false)
        {
            List<Vector3> displayPosList = new();
            for (int i = 0; i < originalPosList.Count; i++)
            {
                displayPosList.Add(grid.LocalToWorld(originalPosList[i]));
            }
        }
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
                previewSystem.RemovePointer(index, CheckPlacementValidity(gridPosition), roadsDataObject.Cost * Mathf.RoundToInt(length), length, width, 0.1f);
            }
            else if (index >= 0)
            {
                posList[index] = gridPosition;
                CalculateLength();
                previewSystem.MovePointer(grid.LocalToWorld(gridPosition), CheckPlacementValidity(gridPosition), roadsDataObject.Cost * Mathf.RoundToInt(length), length, width, 0.1f);
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

        roadMapping.ModifyRoad(selectedRoad, displayPos, roadsDataObject.width, selectedObjectIndex);

        posList.Clear();
        length = 0;
        edited = true;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();
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

        previewSystem.UpdatePointer(grid.LocalToWorld(gridPosition), placementValidity, posList.IndexOf(gridPosition), roadsDataObject.Cost * Mathf.RoundToInt(length), length, width, 0.1f);
    }
}

