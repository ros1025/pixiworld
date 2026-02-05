using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class WallModifyState : IBuildingState
{
    Grid grid;
    WallModifyPreview previewSystem;
    PlacementSystem placementSystem;
    WallMapping wallMapping;
    InputManager inputManager;
    private List<Vector3> posList;
    private List<Vector3> originalPosList;
    private List<MatData> materials;
    private Wall selectedWall;
    private float length;
    private int index;
    private bool isEdit = false;

    public WallModifyState(Vector3 gridPosition,
                            Grid grid,
                            WallMapping wallMapping,
                            WallModifyPreview previewSystem,
                            PlacementSystem placementSystem,
                            InputManager inputManager)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.wallMapping = wallMapping;
        this.placementSystem = placementSystem;
        this.inputManager = inputManager;
        posList = new();

        wallMapping.SelectWall(inputManager, out selectedWall, out index, out List<Vector3> points);
        if (index == -1)
            return;
        for (int i = 0; i < points.Count; i++)
        {
            posList.Add(grid.WorldToLocal(points[i]));
        }
        originalPosList = posList;
        CalculateLength();

        previewSystem.StartPreview(selectedWall, placementSystem, inputManager, 0.1f, 2f);
        materials = selectedWall.materials;

        for (int i = 0; i < posList.Count; i++)
        {
            previewSystem.AddPoint(i, posList[i]);
        }

        placementSystem.GetBuildToolsUI().EnableSellButton(() => placementSystem.RemoveWall(selectedWall));
        placementSystem.GetBuildToolsUI().EnableCustomTexture(previewSystem.materials, () => previewSystem.RefreshColors());
    }

    public void EndState()
    {
        previewSystem.ClearPointer();

        if (!isEdit)
        {
            for (int i = 0; i < selectedWall.renderer.sharedMaterials.Length; i++)
            {
                selectedWall.renderer.sharedMaterials[i].color = materials[i].color;
            }
        }
    }

    public void OnModify(Vector3 gridPosition, float rotation = 0)
    {
        grid = placementSystem.GetCurrentGrid();
        wallMapping = placementSystem.GetCurrentWalls();

        if (previewSystem.GetModifyState() == true)
        {
            int index = previewSystem.expanders.IndexOf(previewSystem.selectedCursor);
            posList[index] = gridPosition;
            CalculateLength();
            previewSystem.ModifyPointer(index, grid.LocalToWorld(gridPosition));

            previewSystem.ApplyFeedback(CheckPlacementValidity());
            placementSystem.GetBuildToolsUI().AdjustLabels(5 * Mathf.RoundToInt(length), new Vector2Int(Mathf.RoundToInt(length), 1));
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

        bool placementValidity = CheckPlacementValidity();

        if (placementValidity == false)
        {
            return;
        }


        List<Vector3> displayPos = new();
        for (int i = 0; i < posList.Count; i++)
        {
            displayPos.Add(grid.LocalToWorld(posList[i]));
        }

        materials.Clear();
        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            materials.Add(previewSystem.materials[i]);
        }
        wallMapping.ModifyWalls(selectedWall, displayPos, materials);
        posList.Clear();
        length = 0;
        isEdit = true;
        previewSystem.ClearPointer();
        inputManager.InvokeExit();
    }

    private bool CheckPlacementValidity()
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
}
