using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallModifyState : IBuildingState
{
    Grid grid;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    WallMapping wallMapping;
    SoundFeedback soundFeedback;
    InputManager inputManager;
    private List<Vector3> posList;
    private List<Vector3> originalPosList;
    private Wall selectedWall;
    private float length;
    private int index;

    public WallModifyState(Vector3 gridPosition,
                            Grid grid,
                            WallMapping wallMapping,
                            PreviewSystem previewSystem,
                            PlacementSystem placementSystem,
                            InputManager inputManager,
                            SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.wallMapping = wallMapping;
        this.placementSystem = placementSystem;
        this.soundFeedback = soundFeedback;
        this.inputManager = inputManager;
        posList = new();

        wallMapping.SelectRoad(grid.LocalToWorld(gridPosition), new Vector2Int(1, 1), 0, out selectedWall, out index, out List<Vector3> points);
        if (index == -1)
            return;
        for (int i = 0; i < points.Count; i++)
        {
            posList.Add(grid.WorldToLocal(points[i]));
        }
        originalPosList = posList;
        CalculateLength();

        previewSystem.ModifyWalls(selectedWall);

        for (int i = 0; i < posList.Count; i++)
        {
            previewSystem.UpdatePointer(grid.LocalToWorld(posList[i]), true, posList.IndexOf(posList[i]), 5 * Mathf.RoundToInt(length), Mathf.RoundToInt(length), 1);
        }
    }

    public void EndState()
    {
        previewSystem.ClearPointer();
    }

    public void OnModify(Vector3 gridPosition, int rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        if (previewSystem.expand == true)
        {
            posList[previewSystem.expanders.IndexOf(previewSystem.SelectedCursor)] = gridPosition;
            CalculateLength();
            previewSystem.MovePointer(grid.LocalToWorld(gridPosition), CheckPlacementValidity(gridPosition), 5 * Mathf.RoundToInt(length), Mathf.RoundToInt(length), 1);
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

        List<Vector3> displayPos = new();
        for (int i = 0; i < posList.Count; i++)
        {
            displayPos.Add(grid.LocalToWorld(posList[i]));
        }

        wallMapping.ModifyWalls(selectedWall, displayPos);
        posList.Clear();
        length = 0;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity(Vector3 gridPosition)
    {
        for (int i = 1; i < posList.Count; i++)
        {
            Vector3 p1 = grid.LocalToWorld(posList[i - 1]);
            Vector3 p2 = grid.LocalToWorld(posList[i]);

            if (!placementSystem.CanPlaceOnArea(p1, p2, 0.04f, 2f))
            {
                return false;
            }
        }

        if (posList.Count < 2)
            return false;

        return true;
    }


    public void UpdateState(Vector3 gridPosition, int rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePointer(grid.LocalToWorld(gridPosition), placementValidity, posList.IndexOf(gridPosition), 5 * Mathf.RoundToInt(length), Mathf.RoundToInt(length), 1);
    }
}
