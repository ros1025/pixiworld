using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TimeManager timeManager;
    private Grid grid;
    [SerializeField] private Material gridMaterial;
    

    [SerializeField] private ObjectsDatabaseSO database;
    [SerializeField] private ZonesDatabaseSO databaseZones;
    [SerializeField] private RoadsDatabaseSO databaseRoads;

    private GameObject gridVisualization;

    [SerializeField] private PreviewTools buildToolsUI;
    [SerializeField] private BuildModeMenu buildModeUI;

    [SerializeField] private Grid mainGrid;
    [SerializeField] private GameObject mainGridPlane;
    [SerializeField] private ObjectPlacer mainObjectDB;
    [SerializeField] private ZonePlacer mainZoneDB;
    [SerializeField] private RoadMapping roadsDBObject;
    [SerializeField] private WallMapping wallsDBObject;

    [HideInInspector] public bool inBuildMode;
    [HideInInspector] public bool inMapMode;

    [SerializeField] private PreviewSystem preview;

    private Vector3Int gridPosition = Vector3Int.zero;
    private Vector3Int selectedPosition = Vector3Int.zero;
    [HideInInspector] public Vector3 screenSelectPosition = Vector3Int.zero;
    private Vector3 pointerPosition;
    private int rotation = 0;

    private ObjectPlacer objectPlacer;
    private ZonePlacer zonePlacer;
    private RoadMapping roads;
    private WallMapping walls;

    [SerializeField]
    private CameraController cameraController;

    IBuildingState buildingState;

    [SerializeField]
    private SoundFeedback soundFeedback;

    public void Start()
    {
        grid = mainGrid;
        gridVisualization = mainGridPlane;
        objectPlacer = mainObjectDB;
        zonePlacer = mainZoneDB;
        roads = roadsDBObject;
        walls = wallsDBObject;
        inMapMode = true;
    }

    public void EnterBuildMode()
    {
        soundFeedback.PlaySound(SoundType.Click);
        gridVisualization.SetActive(true);
        inBuildMode = true;
        buildModeUI.isActive(true);
        buildModeUI.InvokeBuildMenu();
        buildToolsUI.Hide();
        inputManager.OnHold += SelectObject;
        cameraController.TopDownView();
        timeManager.StopTime();
        cameraController.yawAdjustable = false;
    }

    public void ExitBuildMode()
    {
        soundFeedback.PlaySound(SoundType.Click);
        buildModeUI.isActive(false);
        inBuildMode = false;
        gridVisualization.SetActive(false);
        buildModeUI.ExitBuildMenu();
        buildToolsUI.Hide();
        inputManager.ClearActions();
        cameraController.PerspectiveView();
        timeManager.ResumeTime();
        cameraController.yawAdjustable = true;
    }

    public void StartPlacement(int ID)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        buildToolsUI.Call();
        buildingState = new PlacementState(gridPosition,
                                           ID,
                                           grid,
                                           preview,
                                           this,
                                           database,
                                           objectPlacer,
                                           soundFeedback);
        buildToolsUI.Call();
<<<<<<< HEAD
        inputManager.ClearActions();
=======
        inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
        inputManager.OnHold += TriggerUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateZone(int ID)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        buildToolsUI.Call();
        buildingState = new ZoneCreateState(gridPosition,
                                            ID,
                                            grid,
                                            preview,
                                            this,
                                            databaseZones,
                                            zonePlacer,
                                            soundFeedback);
        buildToolsUI.Call();
<<<<<<< HEAD
        inputManager.ClearActions();
=======
        inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
        inputManager.OnHold += TriggerUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateRoad(int ID)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        buildToolsUI.Call();
        buildingState = new RoadCreateState(gridPosition,
                                            ID,
                                            grid,
                                            preview,
                                            this,
                                            databaseRoads,
                                            roads,
                                            soundFeedback);
        buildToolsUI.Call();
<<<<<<< HEAD
        inputManager.ClearActions();
=======
        inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
        inputManager.OnHold += TriggerLiveUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateWall()
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        buildToolsUI.Call();
        buildingState = new WallCreateState(grid,
                                            walls,
                                            preview,
                                            this,
                                            soundFeedback);
        buildToolsUI.Call();
<<<<<<< HEAD
        inputManager.ClearActions();
=======
        inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
        inputManager.OnHold += TriggerLiveUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving(GameObject prefab)
    {
        GridData selectedData = null;
        if (objectPlacer.furnitureData.HasKey(prefab))
        {
            selectedData = objectPlacer.furnitureData;
        }
        else if (zonePlacer.zoneData.HasKey(prefab))
        {
            selectedData = zonePlacer.zoneData;
        }

        if (selectedData == null)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else if (selectedData == zonePlacer.zoneData)
        {
            soundFeedback.PlaySound(SoundType.Remove);
            selectedData.RemoveObjectAt(prefab);
            objectPlacer.RemoveObjectAt(prefab);
            StopPlacement();
        }
        else if (selectedData == objectPlacer.furnitureData)
        {
            soundFeedback.PlaySound(SoundType.Remove);
            selectedData.RemoveObjectAt(prefab);
            zonePlacer.RemoveZoneAt(prefab);
            StopPlacement();
        }
    }

    public void RemoveRoad(Roads road)
    {
        if (roads == null)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            soundFeedback.PlaySound(SoundType.Remove);
            roads.RemoveRoad(road);
            StopPlacement();
        }
    }

    public void RemoveWall(Wall wall)
    {
        if (walls == null)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            soundFeedback.PlaySound(SoundType.Remove);
            walls.RemoveWall(wall);
            StopPlacement();
        }
    }

    public void SelectObject()
    {
        if (inputManager.IsPointerOverUI())
            return;
        if (isSelectable())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            buildToolsUI.Call();
            buildingState = new SelectionState(gridPosition,
                                       grid,
                                       preview,
                                       this,
                                       database,
                                       objectPlacer,
                                       inputManager,
                                       soundFeedback);
            buildToolsUI.Call();
<<<<<<< HEAD
            inputManager.ClearActions();
=======
            inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
            inputManager.OnHold += TriggerUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (isZone())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            buildToolsUI.Call();
            buildingState = new ZoneSelectionState(gridPosition,
                           grid,
                           preview,
                           this,
                           databaseZones,
                           zonePlacer,
                           inputManager,
                           soundFeedback);
            buildToolsUI.Call();
<<<<<<< HEAD
            inputManager.ClearActions();
=======
            inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
            inputManager.OnHold += TriggerUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (isRoad())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            buildToolsUI.Call();
            buildingState = new RoadModifyState(gridPosition,
                           grid,
                           preview,
                           this,
                           databaseRoads,
                           inputManager,
                           roads,
                           soundFeedback);
            buildToolsUI.Call();
<<<<<<< HEAD
            inputManager.ClearActions();
=======
            inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
            inputManager.OnHold += TriggerLiveUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (isWall())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            buildToolsUI.Call();
            buildingState = new WallModifyState(gridPosition,
                grid,
                walls,
                preview,
                this,
                inputManager,
                soundFeedback);
            buildToolsUI.Call();
<<<<<<< HEAD
            inputManager.ClearActions();
=======
            inputManager.OnHold -= SelectObject;
>>>>>>> ef16a6effb940f44dabded59d2944f4ed867b362
            inputManager.OnHold += TriggerLiveUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
    }

    private void TriggerUpdate()
    {
        if (buildingState == null)
            return;
        if (inputManager.IsPointerOverUI())
            return;
        GetGridPosition();
        if (preview.CheckExpansionHandle() == true)
        {
            preview.expand = true;
            preview.dynamic = false;
            cameraController.posAdjustable = false;
            inputManager.OnMoved += PingUpdate;
        }
        else if (preview.CheckPreviewPositions() == true)
        {
            preview.expand = false;
            preview.dynamic = false;
            cameraController.posAdjustable = false;
            inputManager.OnMoved += PingUpdate;
            inputManager.OnRightClick += () => { rotation += 15; buildingState.OnModify(selectedPosition, rotation); } ;
        }
        else
            return;
    }

    private void TriggerLiveUpdate()
    {
        if (buildingState == null)
            return;
        if (inputManager.IsPointerOverUI())
            return;
        GetGridPosition();
        if (preview.CheckExpansionHandle() == true)
        {
            preview.expand = true;
            preview.dynamic = true;
            cameraController.posAdjustable = false;
            inputManager.OnMoved += PingUpdate;
        }
        else
        {
            preview.expand = false;
            preview.dynamic = true;
            cameraController.posAdjustable = false;
            PingUpdate();
        }
    }

    private void PingUpdate()
    {
        if (buildingState == null)
            return;
        if (inputManager.IsPointerOverUI())
            return;
        GetGridPosition();
        MouseController();
        selectedPosition = gridPosition;
        buildingState.OnModify(gridPosition, rotation);
        inputManager.OnRelease += ConfirmPlacement;
    }

    private bool isSelectable()
    {
        if (objectPlacer != null && objectPlacer.furnitureData.CanPlaceObjectAt(preview.previewSelectorObject, grid.CellToWorld(gridPosition), Vector2Int.one, 0) == false)
            return true;
        else
            return false;
    }

    private bool isZone()
    {
        if (zonePlacer != null && zonePlacer.zoneData.CanPlaceObjectAt(preview.previewSelectorObject, grid.CellToWorld(gridPosition), Vector2Int.one, 0) == false)
            return true;
        else
            return false;
    }

    public bool CanPlaceOnArea(Vector3 p1, Vector3 p2, float width, float height)
    {
        if (zonePlacer != null && zonePlacer.zoneData.CanPlaceObjectAt(p1, p2, width, height) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.furnitureData.CanPlaceObjectAt(p1, p2, width, height) == false)
            return false;
        else
            return true;
    }

    public bool CanPlaceOnArea(Vector3 pos, Vector2Int size, int rotation)
    {
        if (roads != null && roads.CheckRoadSelect(pos + new Vector3(size.x / 2f, 0, size.y/2f), size, rotation) == true)
            return false;
        else if (walls != null && walls.CheckWallSelect(pos + new Vector3(size.x / 2f, 0, size.y / 2f), size, rotation) == true)
            return false;
        else if (zonePlacer != null && zonePlacer.zoneData.CanPlaceObjectAt(preview.previewSelectorObject, pos, size, rotation) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.furnitureData.CanPlaceObjectAt(preview.previewSelectorObject, pos, size, rotation) == false)
            return false;
        else
            return true;
    }

    private bool isRoad()
    {
        if (roads != null && roads.CheckRoadSelect(grid.CellToWorld(gridPosition), Vector2Int.one, 0))
            return true;
        else
            return false;
    }

    private bool isWall()
    {
        if (walls != null && walls.CheckWallSelect(grid.CellToWorld(gridPosition), Vector2Int.one, 0))
            return true;
        else
            return false;
    }

    private void PlaceStructure()
    {
        buildingState.OnAction(selectedPosition);
    }

    private void ConfirmPlacement()
    {
        if (buildingState == null)
            return;
        if (inputManager.IsPointerOverUI())
            return;
        cameraController.posAdjustable = true;
        buildToolsUI.Call();
        cameraController.MoveCameraToPos(preview.previewPos, preview.previewSize);
        preview.deSelect();
        inputManager.OnMoved -= PingUpdate;
        inputManager.OnRelease -= ConfirmPlacement;
        inputManager.ClearRightClickAction();
    }

    private void MouseController()
    {
        if (pointerPosition.x > Screen.width - 200)
            cameraController.MoveMouseX(1);
        else if (pointerPosition.x < 200)
            cameraController.MoveMouseX(-1);

        if (pointerPosition.y > Screen.height - 200)
            cameraController.MoveMouseY(1);
        else if (pointerPosition.y < 200)
            cameraController.MoveMouseY(-1);
    }

    //private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    //{
    //    GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? 
    //        floorData : 
    //        furnitureData;

    //    return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    //}

    private void StopPlacement()
    {
        soundFeedback.PlaySound(SoundType.Click);
        if (buildingState == null)
            return;
        gridVisualization.SetActive(true);
        buildModeUI.isActive(true);
        buildModeUI.InvokeBuildMenu();
        buildingState.EndState();
        inputManager.ClearActions();
        inputManager.OnHold += SelectObject;
        buildToolsUI.Hide();
        selectedPosition = Vector3Int.zero;
        buildingState = null;
        rotation = 0;
    }

    private void GetGridPosition()
    {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        pointerPosition = inputManager.GetMousePosition();
        gridPosition = grid.WorldToCell(mousePosition);
    }

    public void SetRotation(int rotation)
    {
        this.rotation = rotation;
    }

    public void SwitchZone(GameObject zone, Renderer zoneRenderer, Grid grid, ObjectPlacer placer, WallMapping walls)
    {
        zone.gameObject.layer = LayerMask.NameToLayer("Grid");
        zoneRenderer.material = gridMaterial;
        gridVisualization = zone;
        this.grid = grid;
        objectPlacer = placer;
        zonePlacer = null;
        roads = null;
        this.walls = walls;
        inMapMode = false;
    }

    public void GoToMainMap()
    {
        mainGridPlane.gameObject.layer = LayerMask.NameToLayer("Grid");
        gridVisualization = mainGridPlane;
        grid = mainGrid;
        objectPlacer = mainObjectDB;
        zonePlacer = mainZoneDB;
        roads = roadsDBObject;
        walls = wallsDBObject;
        inMapMode = true;
    }

    public bool IsGridZone(GameObject zone)
    {
        if (zone == gridVisualization)
            return true;
        return false;
    }

    public Grid GetCurrentGrid()
    {
        return grid;
    }

    public WallMapping GetCurrentWalls()
    {
        return walls;
    }

    public ObjectPlacer GetCurrentObjectPlacer()
    {
        return objectPlacer;
    }

    private void Update()
    {
        GetGridPosition();
        screenSelectPosition = cameraController.GetScreenPos(preview.previewPos);
        if (rotation >= 360) rotation -= 360;
    }
}