using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreateState : IBuildingState
{
    Grid grid;
    WallMapping wallMapping;
    PreviewSystem previewSystem;
    PlacementSystem placementSystem;
    SoundFeedback soundFeedback;
    private List<Vector3Int> posList;
    float length;

    public WallCreateState(Grid grid,
                            WallMapping wallMapping,
                            PreviewSystem previewSystem,
                            PlacementSystem placementSystem,
                            SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.wallMapping = wallMapping;
        this.previewSystem = previewSystem;
        this.placementSystem = placementSystem;
        this.soundFeedback = soundFeedback;

        posList = new();
        previewSystem.AddWalls();
    }

    public void EndState()
    {
        previewSystem.ClearPointer();
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


    public void OnModify(Vector3Int gridPosition, int rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        if (previewSystem.expand == true)
        {
            posList[previewSystem.expanders.IndexOf(previewSystem.SelectedCursor)] = gridPosition;
            CalculateLength();
            previewSystem.MovePointer(grid.CellToWorld(gridPosition), CheckPlacementValidity(gridPosition), 5 * Mathf.RoundToInt(length), Mathf.RoundToInt(length), 1);
        }
        else
        {
            if (!(posList.Contains(gridPosition)))
            {
                posList.Add(gridPosition);
                CalculateLength();
                UpdateState(gridPosition, 0);
            }
        }

    }

    public void OnAction(Vector3Int gridPosition)
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
            displayPos.Add(grid.CellToWorld(posList[i]));
        }

        wallMapping.AddWalls(displayPos);
        posList.Clear();
        length = 0;
        previewSystem.ClearPointer();
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        for (int i = 1; i < posList.Count; i++)
        {
            Vector3 p1 = grid.CellToWorld(posList[i - 1]);
            Vector3 p2 = grid.CellToWorld(posList[i]);

            if (!placementSystem.CanPlaceOnArea(p1, p2, 0.04f, 2f))
            {
                return false;
            }
        }

        if (posList.Count < 2)
            return false;

        return true;
    }


    public void UpdateState(Vector3Int gridPosition, int rotation = 0)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);

        previewSystem.UpdatePointer(grid.CellToWorld(gridPosition), placementValidity, posList.IndexOf(gridPosition), 5 * Mathf.RoundToInt(length), Mathf.RoundToInt(length), 1);
    }
}
