using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour, IDataPersistence
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TimeManager timeManager;
    private Grid grid;
    [SerializeField] private Material gridMaterial;
    

    [SerializeField] private ObjectsDatabaseSO database;
    [SerializeField] private ZonesDatabaseSO databaseZones;
    [SerializeField] private RoadsDatabaseSO databaseRoads;
    [SerializeField] private DoorDatabaseSO databaseDoors;
    [SerializeField] private WindowsDatabaseSO databaseWindows;

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

    private Vector3 gridPosition = Vector3.zero;
    private Vector3 selectedPosition = Vector3.zero;
    [HideInInspector] public Vector3 screenSelectPosition = Vector3Int.zero;
    private Vector3 pointerPosition;
    private float rotation = 0;

    private ObjectPlacer objectPlacer;
    private ZonePlacer zonePlacer;
    private RoadMapping roads;
    private WallMapping walls;

    public int itemMode;
    public static readonly int Object = 0;
    public static readonly int Wall = 1;
    public static readonly int Zone = 2;
    public static readonly int Road = 3;
    public static readonly int Door = 4;

    public bool isCreate;

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
        preview.gridSnap = true;
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
        itemMode = -1;
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
        itemMode = -1;
    }

    public void StartPlacement(ObjectData objectData)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Object;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        buildingState = new PlacementState(gridPosition,
                                           objectData,
                                           grid,
                                           preview,
                                           this,
                                           database,
                                           objectPlacer);
        buildToolsUI.Call();
        inputManager.ClearActions();
        inputManager.OnHold += TriggerUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateZone(ZonesData zonesData)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Zone;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        buildingState = new ZoneCreateState(gridPosition,
                                            zonesData,
                                            grid,
                                            preview,
                                            this,
                                            databaseZones,
                                            zonePlacer);
        buildToolsUI.Call();
        inputManager.ClearActions();
        inputManager.OnHold += TriggerUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateDoor(DoorsData doorsData)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Door;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        buildingState = new DoorCreateState(gridPosition,
                                            doorsData,
                                            grid,
                                            preview,
                                            this,
                                            databaseDoors,
                                            walls);
        buildToolsUI.Call();
        inputManager.ClearActions();
        inputManager.OnHold += TriggerUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateWindow(WindowsData windowsData)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Door;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        buildingState = new WindowCreateState(gridPosition,
                                            windowsData,
                                            grid,
                                            preview,
                                            this,
                                            databaseWindows,
                                            walls);
        buildToolsUI.Call();
        inputManager.ClearActions();
        inputManager.OnHold += TriggerUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateRoad(RoadsData roadsData)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Road;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        buildingState = new RoadCreateState(gridPosition,
                                            roadsData,
                                            grid,
                                            preview,
                                            this,
                                            databaseRoads,
                                            roads);
        buildToolsUI.Call();
        inputManager.ClearActions();
        inputManager.OnHold += TriggerLiveUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreateWall()
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Wall;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        buildingState = new WallCreateState(grid,
                                            walls,
                                            preview,
                                            this);
        buildToolsUI.Call();
        inputManager.ClearActions();
        inputManager.OnHold += TriggerLiveUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void RemoveObject(GameObject prefab)
    {
        if (objectPlacer.HasKey(prefab))
        {
            soundFeedback.PlaySound(SoundType.Remove);
            objectPlacer.RemoveObjectAt(prefab);
            StopPlacement();
        }
        else
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
    }

    public void RemoveZone(GameObject prefab)
    {
        if (zonePlacer.HasKey(prefab))
        {
            soundFeedback.PlaySound(SoundType.Remove);
            zonePlacer.RemoveZoneAt(prefab);
            StopPlacement();
        }
        else
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
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

    public void RemoveDoor(GameObject prefab)
    {
        if (walls == null)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            soundFeedback.PlaySound(SoundType.Remove);
            walls.RemoveDoor(prefab);
            StopPlacement();
        }
    }

    public void SelectObject()
    {
        if (inputManager.IsPointerOverUI())
            return;
        if (IsSelectable())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Object;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new SelectionState(gridPosition,
                                       grid,
                                       preview,
                                       this,
                                       database,
                                       objectPlacer,
                                       inputManager);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsZone())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Zone;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new ZoneSelectionState(gridPosition,
                           grid,
                           preview,
                           this,
                           databaseZones,
                           zonePlacer,
                           inputManager);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsRoadIntersection())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Road;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new IntersectionModifyState(gridPosition,
                           grid,
                           preview,
                           this,
                           databaseRoads,
                           inputManager,
                           roads);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerLiveUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsRoad())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Road;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new RoadModifyState(gridPosition,
                           grid,
                           preview,
                           this,
                           databaseRoads,
                           inputManager,
                           roads);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerLiveUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsWallIntersection())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Road;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new WallIntersectionModifyState(gridPosition,
                           grid,
                           walls,
                           preview,
                           this,
                           inputManager);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerLiveUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsWall())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Wall;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new WallModifyState(gridPosition,
                grid,
                walls,
                preview,
                this,
                inputManager);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerLiveUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsDoor())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Door;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new DoorModifyState(gridPosition,
                grid,
                preview,
                this,
                databaseDoors,
                walls,
                inputManager);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsWindow())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Door;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            buildingState = new WindowModifyState(gridPosition,
                grid,
                preview,
                this,
                databaseWindows,
                walls,
                inputManager);
            buildToolsUI.Call();
            inputManager.ClearActions();
            inputManager.OnHold += TriggerUpdate;
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
        else if (preview.CheckPreviewSpline() == true)
        {
            preview.expand = false;
            preview.dynamic = true;
            cameraController.posAdjustable = false;
            PingUpdate();
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

    private bool IsSelectable()
    {
        if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(preview.previewSelectorObject, grid.LocalToWorld(gridPosition), Vector2Int.one, 0) == false)
            return true;
        else
            return false;
    }

    private bool IsZone()
    {
        if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(preview.previewSelectorObject, grid.LocalToWorld(gridPosition), Vector2Int.one, 0) == false)
            return true;
        else
            return false;
    }

    public bool CanPlaceOnArea(Vector3 p1, Vector3 p2, float width, float height)
    {
        if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(p1, p2, width, height) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(p1, p2, width, height) == false)
            return false;
        else
            return true;
    }

    public bool CanPlaceOnArea(Vector3 pos, Vector2Int size, float rotation)
    {
        if (roads != null && roads.CheckRoadSelect(pos + new Vector3(size.x / 2f, 0, size.y / 2f), size, rotation))
            return false;
        else if (walls != null && walls.CheckWallSelect(pos + new Vector3(size.x / 2f, 0, size.y / 2f), size, rotation))
            return false;
        else if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(preview.previewSelectorObject, pos, size, rotation) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(preview.previewSelectorObject, pos, size, rotation) == false)
            return false;
        else
            return true;
    }

    public bool CanMoveOnArea(GameObject previewObject)
    {
        if (roads != null && roads.CheckRoadSelect(previewObject))
            return false;
        else if (walls != null && walls.CheckWallSelect(previewObject))
            return false;
        else if (zonePlacer != null && zonePlacer.CanMoveObjectAt(previewObject, preview.previewSelector) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.CanMoveObjectAt(previewObject, preview.previewSelector) == false)
            return false;
        else
            return true;
    }

    private bool IsRoad()
    {
        if (roads != null && roads.CheckRoadSelect(inputManager))
            return true;
        else
            return false;
    }

    private bool IsRoadIntersection()
    {
        if (roads != null && roads.CheckIntersectionSelect(inputManager))
            return true;
        else
            return false;
    }

    private bool IsWall()
    {
        if (walls != null && walls.CheckWallSelect(inputManager))
            return true;
        else
            return false;
    }

    private bool IsWallIntersection()
    {
        if (walls != null && walls.CheckIntersection(inputManager))
            return true;
        else
            return false;
    }

    private bool IsDoor()
    {
        if (walls != null && walls.CheckDoorSelect(grid.LocalToWorld(gridPosition), Vector2Int.one, 0))
            return true;
        else
            return false;
    }

    private bool IsWindow()
    {
        if (walls != null && walls.CheckWindowSelect(grid.LocalToWorld(gridPosition), Vector2Int.one, 0))
            return true;
        else
            return false;
    }

    private void PlaceStructure()
    {
        buildingState.OnAction(selectedPosition);
        DataPersistenceManager.instance.SaveGame();
    }

    private void ConfirmPlacement()
    {
        if (buildingState == null)
            return;
        if (inputManager.IsPointerOverUI())
            return;
        cameraController.posAdjustable = true;
        buildToolsUI.PlaceCheck();
        cameraController.MoveCameraToPos(preview.previewPos, preview.previewSize);
        preview.deSelect();
        //DataPersistenceManager.instance.SaveGame();
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
        itemMode = -1;
    }

    private void GetGridPosition()
    {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        pointerPosition = inputManager.GetMousePosition();
        if (preview.gridSnap || itemMode == Wall)
        {
            gridPosition = grid.CellToLocal(grid.WorldToCell(mousePosition));
        }
        else gridPosition = grid.WorldToLocal(mousePosition);
    }

    public Vector3 SmoothenPosition(Vector3 pos)
    {
        if (preview.gridSnap || itemMode == Wall)
        {
            return grid.CellToLocal(grid.LocalToCell(pos + new Vector3(0.25f, 0f, 0.25f)));
        }
        else return pos;
    }

    public void SetRotation(float rotation)
    {
        this.rotation = rotation;
    }

    public void SetSelectedPosition(Vector3 position)
    {
        this.selectedPosition = position;
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

    public GameObject GetObjectPrefab(long ID)
    {
        ObjectData data = database.objectsData.Find(item => item.ID == ID);
        return data.Prefab;
    }

    public GameObject GetDoorPrefab(long ID)
    {
        DoorsData data = databaseDoors.doorsData.Find(item => item.ID == ID);
        return data.Prefab;
    }

    private void Update()
    {
        GetGridPosition();
        screenSelectPosition = cameraController.GetScreenPos(preview.previewPos);
        if (rotation >= 360) rotation -= 360;
    }

    public void SaveData(WorldSaveData data)
    {
        data.mapObjects = mainObjectDB.furnitureData;
        data.zones = mainZoneDB.zoneData;
        data.roads = roadsDBObject.GetRoadMapSaveData();
        data.mapWalls = wallsDBObject.GetWallMapSaveData();
     }

    public void LoadData(WorldSaveData data)
    {
        mainObjectDB.LoadData(data.mapObjects);
        mainZoneDB.LoadData(data.zones);
        roadsDBObject.LoadSaveData(data.roads);
        wallsDBObject.LoadSaveData(data.mapWalls);
    }
}