using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
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
    [SerializeField] private PoolPlacer mainPoolsDB;

    [HideInInspector] public bool inBuildMode;
    [HideInInspector] public bool inMapMode;

    //[SerializeField] private PreviewSystem preview2;
    private IPreviewSystem preview;
    [SerializeField] public Material previewMaterialPrefab;
    [SerializeField] public GameObject previewSelectorObject;
    [SerializeField] public GameObject expandingCursor;
    [SerializeField] public GameObject pointerCursor;
    [SerializeField] public SplineContainer dynamicCursor;
    [SerializeField] public MeshFilter dynamicMesh;
    [SerializeField] public  MeshRenderer dynamicRenderer;
    [SerializeField] public MeshCollider dynamicCollider;
    [SerializeField] public GameObject expanderParent;
    public GameObject cellIndicator;

    private Vector3 gridPosition = Vector3.zero;
    private Vector3 selectedPosition = Vector3.zero;
    [HideInInspector] public Vector3 screenSelectPosition = Vector3Int.zero;
    private Vector3 pointerPosition;
    private float rotation = 0;

    private ObjectPlacer objectPlacer;
    private ZonePlacer zonePlacer;
    private RoadMapping roads;
    private WallMapping walls;
    private PoolPlacer pools;

    public int itemMode;
    public static readonly int Object = 0;
    public static readonly int Wall = 1;
    public static readonly int Zone = 2;
    public static readonly int Road = 3;
    public static readonly int Door = 4;
    public static readonly int Window = 5;

    public bool isCreate;
    public bool gridSnap;

    [SerializeField]
    private CameraController cameraController;

    IBuildingState buildingState;

    [SerializeField]
    private SoundFeedback soundFeedback;

    public void Start()
    {
        grid = mainGrid;
        gridVisualization = mainGridPlane;
        GoToMainMap();
        inMapMode = true;
        gridSnap = true;
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
        ObjectPlacementPreview placementPreview = new ObjectPlacementPreview();
        preview = placementPreview;
        buildingState = new ObjectPlacementState(gridPosition,
                                                objectData,
                                                grid,
                                                placementPreview,
                                                this,
                                                database,
                                                objectPlacer,
                                                inputManager);
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
        ZonePlacementPreview placementPreview = new();
        preview = placementPreview;
        buildingState = new ZoneCreateState(gridPosition,
                                            zonesData,
                                            grid,
                                            placementPreview,
                                            this,
                                            databaseZones,
                                            zonePlacer,
                                            inputManager);
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
        ObjectPlacementPreview placementPreview = new ObjectPlacementPreview();
        preview = placementPreview;
        buildingState = new DoorCreateState(gridPosition,
                                            doorsData,
                                            grid,
                                            placementPreview,
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

    public void CreateWindow(WindowsData windowsData)
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Window;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        ObjectPlacementPreview placementPreview = new ObjectPlacementPreview();
        preview = placementPreview;
        buildingState = new WindowCreateState(gridPosition,
                                            windowsData,
                                            grid,
                                            placementPreview,
                                            this,
                                            databaseWindows,
                                            walls,
                                            inputManager);
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
        RoadCreatePreview roadCreatePreview = new();
        preview = roadCreatePreview;
        buildingState = new RoadCreateState(gridPosition,
                                            roadsData,
                                            grid,
                                            roadCreatePreview,
                                            this,
                                            databaseRoads,
                                            roads,
                                            inputManager);
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
        WallCreatePreview wallCreatePreview = new();
        preview = wallCreatePreview;
        buildingState = new WallCreateState(grid,
                                            walls,
                                            wallCreatePreview,
                                            this,
                                            inputManager);
        inputManager.ClearActions();
        inputManager.OnHold += TriggerLiveUpdate;
        inputManager.OnAction += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void CreatePool()
    {
        buildModeUI.isActive(false);
        gridVisualization.SetActive(true);
        GetGridPosition();
        itemMode = Wall;
        isCreate = true;
        buildToolsUI.Call();
        selectedPosition = gridPosition;
        PoolCreatePreview poolCreatePreview = new();
        preview = poolCreatePreview;
        buildingState = new PoolCreateState(grid,
                                            pools,
                                            poolCreatePreview,
                                            this,
                                            inputManager);
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

    public void RemoveWindow(GameObject prefab)
    {
        if (walls == null)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            soundFeedback.PlaySound(SoundType.Remove);
            walls.RemoveWindow(prefab);
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
            ObjectSelectionPreview selectionPreview = new();
            preview = selectionPreview;
            buildingState = new ObjectSelectionState(gridPosition,
                                       grid,
                                       selectionPreview,
                                       this,
                                       database,
                                       objectPlacer,
                                       inputManager);
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
            ZoneSelectionPreview selectionPreview = new();
            preview = selectionPreview;
            buildingState = new ZoneSelectionState(gridPosition,
                           grid,
                           selectionPreview,
                           this,
                           databaseZones,
                           zonePlacer,
                           inputManager);
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
            RoadIntersectionModifyPreview modifyPreview = new();
            preview = modifyPreview;
            buildingState = new RoadIntersectionModifyState(gridPosition,
                           grid,
                           modifyPreview,
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
            RoadModifyPreview roadModifyPreview = new();
            preview = roadModifyPreview;
            buildingState = new RoadModifyState(gridPosition,
                           grid,
                           roadModifyPreview,
                           this,
                           databaseRoads,
                           inputManager,
                           roads);
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
            WallIntersectionModifyPreview modifyPreview = new();
            preview = modifyPreview;
            buildingState = new WallIntersectionModifyState(gridPosition,
                           grid,
                           walls,
                           modifyPreview,
                           this,
                           inputManager);
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
            WallModifyPreview wallModifyPreview = new();
            preview = wallModifyPreview;
            buildingState = new WallModifyState(gridPosition,
                grid,
                walls,
                wallModifyPreview,
                this,
                inputManager);
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
            ObjectSelectionPreview selectionPreview = new();
            preview = selectionPreview;
            buildingState = new DoorModifyState(gridPosition,
                grid,
                selectionPreview,
                this,
                databaseDoors,
                walls,
                inputManager);
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
            itemMode = Window;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            ObjectSelectionPreview selectionPreview = new();
            preview = selectionPreview;
            buildingState = new WindowModifyState(gridPosition,
                grid,
                selectionPreview,
                this,
                databaseWindows,
                walls,
                inputManager);
            inputManager.ClearActions();
            inputManager.OnHold += TriggerUpdate;
            inputManager.OnAction += PlaceStructure;
            inputManager.OnExit += StopPlacement;
        }
        else if (IsPool())
        {
            StopPlacement();
            gridVisualization.SetActive(true);
            buildModeUI.isActive(false);
            GetGridPosition();
            itemMode = Window;
            isCreate = false;
            buildToolsUI.Call();
            selectedPosition = gridPosition;
            PoolModifyPreview poolModifyPreview = new();
            preview = poolModifyPreview;
            buildingState = new PoolModifyState(grid,
                pools,
                poolModifyPreview,
                this,
                inputManager);
            inputManager.ClearActions();
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
        if (preview is IStaticPreviewSystem)
        {
            IStaticPreviewSystem staticPreview = (IStaticPreviewSystem)preview;
            if (staticPreview.CheckExpansionHandles() == true)
            {
                staticPreview.SetExpansionState(true);
                cameraController.posAdjustable = false;
                inputManager.OnMoved += PingUpdate;
            }
            else if (staticPreview.CheckPreviewObject() == true)
            {
                staticPreview.SetExpansionState(false);
                cameraController.posAdjustable = false;
                inputManager.OnMoved += PingUpdate;
                inputManager.OnRightClick += () => 
                { 
                if (itemMode == Door || itemMode == Window)
                    ChangeRotation(180); 
                else ChangeRotation(15); } ;
            }
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
        if (preview is IDynamicPreviewSystem)
        {
            IDynamicPreviewSystem dynamicPreview = (IDynamicPreviewSystem)preview;
            if (dynamicPreview.CheckExpansionHandles() == true)
            {
                dynamicPreview.SetModifyState(true);
                cameraController.posAdjustable = false;
                inputManager.OnMoved += PingUpdate;
            }
            else if (dynamicPreview.CheckPreviewSplines() == true)
            {
                dynamicPreview.SetModifyState(false);
                cameraController.posAdjustable = false;
                PingUpdate();
            }
            else
            {
                dynamicPreview.SetModifyState(false);
                cameraController.posAdjustable = false;
                PingUpdate();
            }
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
        if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(previewSelectorObject, grid.LocalToWorld(gridPosition), Vector2Int.one, 0) == false)
            return true;
        else
            return false;
    }

    private bool IsZone()
    {
        if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(previewSelectorObject, grid.LocalToWorld(gridPosition), Vector2Int.one, 0) == false)
            return true;
        else
            return false;
    }

    public bool CanPlaceOnArea(List<Vector3> points)
    {
        if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(points) == false)
            return false;
        else if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(points) == false)
            return false;
        else if (walls != null && walls.CheckWallSelect(points) == false)
            return false;
        else if (roads != null && roads.CheckRoadSelect(points) == false)
            return false;
        else if (pools != null && pools.CheckPoolCollisions(points) == false)
            return false;
        else
            return true;
    }

    public bool CanPlaceOnArea(Vector3 p1, Vector3 p2, float width, float height)
    {
        if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(p1, p2, width, height) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(p1, p2, width, height) == false)
            return false;
        else if (pools != null && pools.CanPlaceObjectAt(p1, p2, width, height) == false)
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
        else if (zonePlacer != null && zonePlacer.CanPlaceObjectAt(previewSelectorObject, pos, size, rotation) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.CanPlaceObjectAt(previewSelectorObject, pos, size, rotation) == false)
            return false;
        else if (pools != null && pools.CheckPoolCollisions(previewSelectorObject, pos, size, rotation) == false)
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
        else if (zonePlacer != null && zonePlacer.CanMoveObjectAt(previewObject, previewSelectorObject) == false)
            return false;
        else if (objectPlacer != null && objectPlacer.CanMoveObjectAt(previewObject, previewSelectorObject) == false)
            return false;
        else if (pools != null && pools.CheckPoolCollisions(previewSelectorObject) == false)
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

    private bool IsPool()
    {
        if (pools != null && pools.SelectPool(inputManager) != null)
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
        cameraController.MoveCameraToPos(preview.GetPreviewPosition(), preview.GetPreviewSize());
        preview.Deselect();
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
        preview = null;
        rotation = 0;
        itemMode = -1;
    }

    private void GetGridPosition()
    {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        pointerPosition = inputManager.GetMousePosition();
        if (gridSnap || itemMode == Wall)
        {
            gridPosition = grid.CellToLocal(grid.WorldToCell(mousePosition));
        }
        else gridPosition = grid.WorldToLocal(mousePosition);
    }

    public Vector3 SmoothenPosition(Vector3 pos)
    {
        if (gridSnap || itemMode == Wall)
        {
            return grid.CellToLocal(grid.LocalToCell(pos + new Vector3(0.25f, 0f, 0.25f)));
        }
        else return pos;
    }

    public void SetRotation(float rotation)
    {
        this.rotation = rotation;
    }

    public void ChangeRotation(float delta)
    {
        this.rotation += delta;
        buildingState.OnModify(selectedPosition, rotation);
    }

    public void SetSelectedPosition(Vector3 position)
    {
        this.selectedPosition = position;
    }

    public void SwitchZone(GameObject zone, Renderer zoneRenderer, Grid grid, ObjectPlacer placer, WallMapping walls, PoolPlacer pools)
    {
        zone.gameObject.layer = LayerMask.NameToLayer("Grid");
        zoneRenderer.material = gridMaterial;
        gridVisualization = zone;
        this.grid = grid;
        objectPlacer = placer;
        zonePlacer = null;
        roads = null;
        this.pools = pools;
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
        pools = mainPoolsDB;
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

    public PoolPlacer GetCurrentPools()
    {
        return pools;
    }

    public ObjectPlacer GetCurrentObjectPlacer()
    {
        return objectPlacer;
    }

    public PreviewTools GetBuildToolsUI()
    {
        return this.buildToolsUI;
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
        if (preview != null)
            screenSelectPosition = cameraController.GetScreenPos(preview.GetPreviewPosition());
        if (rotation >= 360) rotation -= 360;
    }

    public void SaveData(WorldSaveData data)
    {
        data.mapObjects = mainObjectDB.furnitureData;
        data.zones = mainZoneDB.zoneData;
        data.roads = roadsDBObject.GetRoadMapSaveData();
        data.mapWalls = wallsDBObject.GetWallMapSaveData();
        data.pools = pools.pools;
     }

    public void LoadData(WorldSaveData data)
    {
        mainObjectDB.LoadData(data.mapObjects);
        mainZoneDB.LoadData(data.zones);
        roadsDBObject.LoadSaveData(data.roads);
        wallsDBObject.LoadSaveData(data.mapWalls);
        pools.LoadData(data.pools);
    }
}