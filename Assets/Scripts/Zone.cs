using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Zone : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private UIDocument levels;
    [SerializeField] private GameObject zonePrefab;
    [SerializeField] public List<LevelData> floors { get; private set; }
    [SerializeField] private int level;
    private Vector2Int size;
    private int minLevel; private int maxLevel;
    [HideInInspector] public int ID;
    [HideInInspector] public PlacementSystem placement;
    [SerializeField] private Material zoneMaterial;
    VisualElement root; VisualElement lRoot;
    [SerializeField] UnityEngine.UI.Button button; 
    Button upButton; Button downButton;
    Label floorLabel;

    // Start is called before the first frame update
    void Start()
    {
        root = document.rootVisualElement;
        //button = root.Q<Button>();
        lRoot = levels.rootVisualElement;
        upButton = lRoot.Q<Button>("UpButton");
        upButton.RegisterCallback<ClickEvent, int>(SwitchLevel, level + 1);
        downButton = lRoot.Q<Button>("DownButton");
        downButton.RegisterCallback<ClickEvent, int>(SwitchLevel, level - 1);
        floorLabel = lRoot.Q<Label>("Floor");
        floorLabel.text = $"{level}";
        upButton.visible = false;
        downButton.visible = false;
        floorLabel.visible = false;
        level = 0;
    }

    public void InstantiateNew(PlacementSystem placement, int ID, Vector2Int size)
    {
        this.placement = placement;
        this.ID = ID;
        this.size = size;
        minLevel = 0;
        maxLevel = 0;
        floors = new();
        GameObject newZone = Instantiate(zonePrefab);
        newZone.transform.position = transform.parent.position;
        newZone.transform.SetParent(transform.parent);
        newZone.transform.Find("FloorInsert").localScale = new Vector3(size.x, size.y, size.y);
        LevelData level = new LevelData(0, newZone, newZone.transform.Find("FloorInsert").GetChild(0).gameObject, newZone.transform.Find("FloorInsert").GetChild(0).GetChild(0).GetComponent<Renderer>(),
            newZone.transform.GetComponentInChildren<Grid>(), newZone.transform.GetComponentInChildren<ObjectPlacer>(), newZone.transform.GetComponentInChildren<WallMapping>(), 0);
        floors.Insert(0, level);

        level.walls.SetCeilingsActive(false);
    }

    public void EditPosition(int ID, Vector2Int size)
    {
        this.ID = ID;
        this.size = size;
        foreach (LevelData level in floors)
        {
            level.floor.transform.Find("FloorInsert").localScale = new Vector3(size.x, size.y, size.y);
        }
    }

    public void EnterZone()
    {
        placement.SwitchZone(floors[level - minLevel].cursor, floors[level - minLevel].renderer, floors[level - minLevel].grid, floors[level - minLevel].objectPlacer, floors[level - minLevel].walls);
        for (int i = 0; i <= level - minLevel; i++)
        {
            floors[i].floor.SetActive(true);
        }
        for (int i = level - minLevel + 1; i < floors.Count; i++)
        {
            floors[i].floor.SetActive(false);
        }
    }

    void UpdatePosition()
    {
        if (placement.inBuildMode)
        {
            button.gameObject.SetActive(false);
            if (floors.Count > 0 && placement.IsGridZone(floors[level - minLevel].cursor))
            {
                upButton.visible = true;
                downButton.visible = true;
                floorLabel.visible = true;
            }
        }
        else
        {
            Camera camera = Camera.main;
            Vector3 coordinates = camera.WorldToScreenPoint(floors[maxLevel - minLevel].cursor.transform.position);
            upButton.visible = true;
            downButton.visible = true;
            floorLabel.visible = true;
            if (coordinates.y > (Screen.height * 3 / 4))
            {
                button.gameObject.SetActive(false);
            }
            if (coordinates.y < (Screen.height * 3 / 4))
            {
                button.gameObject.SetActive(true);
                /*
                float ratio = (float)(1 - (0.1 * (((coordinates.y / 150) * Mathf.Cos(camera.transform.eulerAngles.x * (Mathf.PI / 180))) + (camera.transform.position.y / 50))));
                float scaleX = (float)1920 / Screen.width; float scaleY = (float)1080 / Screen.height;
                root.style.left = (coordinates.x - (35 * ratio)) * scaleX;
                root.style.top = (Screen.height - coordinates.y - (120 * ratio)) * scaleY;
                button.style.height = (120 * ratio);
                button.style.width = button.style.height;
                float radius = (50 * ratio);
                button.style.borderBottomLeftRadius = radius;
                button.style.borderBottomRightRadius = radius;
                button.style.borderTopLeftRadius = radius;
                button.style.borderTopRightRadius = radius;
                float padding = (15 * ratio);
                button.style.paddingBottom = padding;
                button.style.paddingTop = padding;
                button.style.paddingLeft = padding;
                button.style.paddingRight = padding;
                */
                button.transform.position = floors[maxLevel - minLevel].cursor.transform.position + new Vector3(0, 3, 0);
            }
        }
    }

    public void AddLevelAbove()
    {
        GameObject newZone = Instantiate(zonePrefab);
        newZone.transform.position = new Vector3(transform.parent.position.x, floors[maxLevel - minLevel].height + 2f, transform.parent.position.z);
        newZone.transform.SetParent(transform.parent);
        newZone.transform.Find("FloorInsert").localScale = new Vector3(size.x, size.y, size.y);

        LevelData level = new LevelData(maxLevel + 1, newZone, newZone.transform.Find("FloorInsert").GetChild(0).gameObject, newZone.transform.Find("FloorInsert").GetChild(0).GetChild(0).GetComponent<Renderer>(),
            newZone.transform.GetComponentInChildren<Grid>(), newZone.transform.GetComponentInChildren<ObjectPlacer>(), newZone.transform.GetComponentInChildren<WallMapping>(), floors[maxLevel - minLevel].height + 2f);
        level.cursor.SetActive(false);
        floors.Add(level);

        level.floor.SetActive(false);

        maxLevel += 1;
    }

    public void AddLevelBelow()
    {
        GameObject newZone = Instantiate(zonePrefab);
        newZone.transform.position = new Vector3(transform.parent.position.x, floors[minLevel - minLevel].height - 2f, transform.parent.position.z);
        newZone.transform.SetParent(transform.parent);
        newZone.transform.Find("FloorInsert").localScale = new Vector3(size.x, size.y, size.y);
        LevelData level = new LevelData(minLevel - 1, newZone, newZone.transform.Find("FloorInsert").GetChild(0).gameObject, newZone.transform.Find("FloorInsert").GetChild(0).GetChild(0).GetComponent<Renderer>(),
            newZone.transform.GetComponentInChildren<Grid>(), newZone.transform.GetComponentInChildren<ObjectPlacer>(), newZone.transform.GetComponentInChildren<WallMapping>(), floors[minLevel - minLevel].height - 2f);
        level.cursor.SetActive(false);
        floors.Insert(0, level);
        minLevel -= 1;
    }

    public void SwitchLevel(ClickEvent evt, int level)
    {
        if (level >= minLevel && level <= maxLevel)
        {
            floors[this.level - minLevel].cursor.SetActive(false);
            this.level = level;
            floorLabel.text = $"{this.level}";
            placement.SwitchZone(floors[level - minLevel].cursor, floors[level - minLevel].renderer, floors[level - minLevel].grid, floors[level - minLevel].objectPlacer, floors[level - minLevel].walls);
            upButton.RegisterCallback<ClickEvent, int>(SwitchLevel, level + 1);
            downButton.RegisterCallback<ClickEvent, int>(SwitchLevel, level - 1);
            for (int i = 0; i <= level - minLevel; i++)
            {
                floors[i].floor.SetActive(true);
            }
            for (int i = level - minLevel + 1; i < floors.Count; i++)
            {
                floors[i].floor.SetActive(false);
            }

            for (int i = 0; i < level - minLevel; i++)
            {
                floors[i].walls.SetCeilingsActive(true);
            }
            for (int i = level - minLevel; i < floors.Count; i++)
            {
                floors[i].walls.SetCeilingsActive(false);
            }
            floors[level - minLevel].cursor.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        if (placement.inMapMode == true)
        {
            floors[0 - minLevel].cursor.SetActive(true);
            floors[0 - minLevel].renderer.material = zoneMaterial;
            upButton.visible = false;
            downButton.visible = false;
            floorLabel.visible = false;
            for (int i = 0 - minLevel; i <= maxLevel - minLevel; i++)
            {
                floors[i].floor.SetActive(true);
                floors[i].walls.SetCeilingsActive(true);
            }
        }
        else
        {
            button.gameObject.SetActive(false);
            if (placement.inBuildMode == false)
            {
                foreach (LevelData level in floors)
                {
                    level.cursor.SetActive(false);
                }
            }
        }

        if (floors[maxLevel - minLevel].walls.rooms.Count > 0)
        {
            AddLevelAbove();
        }
    }
}

[System.Serializable]
public class LevelData
{
    public int level;
    public GameObject floor;
    public GameObject cursor;
    public Renderer renderer;
    public Grid grid;
    public ObjectPlacer objectPlacer;
    public WallMapping walls;
    public float height;

    public LevelData(int level, GameObject floor, GameObject cursor, Renderer renderer, Grid grid, ObjectPlacer objectPlacer, WallMapping walls, float height)
    {
        this.level = level;
        this.floor = floor;
        this.cursor = cursor;
        this.renderer = renderer;
        this.grid = grid;
        this.objectPlacer = objectPlacer;
        this.walls = walls;
        this.height = height;
    }
}