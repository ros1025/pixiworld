using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildModeMenu : MonoBehaviour
{
    [SerializeField]
    private UIDocument BuildMenu;
    [SerializeField]
    private MainMenuController mainController;
    [SerializeField]
    private PlacementSystem placementSystem;
    [SerializeField]
    private ObjectsDatabaseSO objectsData;
    [SerializeField]
    private ZonesDatabaseSO zoneData;
    [SerializeField]
    private RoadsDatabaseSO roadsData;
    [SerializeField]
    private DoorDatabaseSO doorsData;
    [SerializeField]
    private ObjectCategoriesSO objectCategories;
    [SerializeField]
    private ObjectCategoriesSO objectMasterCategories;

    VisualElement root; VisualElement top; VisualElement bottom;
    Button cancelButton; Button furniture; Button building; Button inventory;
    VisualElement sections; VisualElement categories; VisualElement items;

    public void isActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void InvokeBuildMenu()
    {
        gameObject.SetActive(true);
        root = BuildMenu.rootVisualElement;
        top = root.Q<VisualElement>("TopBar");
        bottom = root.Q<VisualElement>("BottomBar");
        cancelButton = top.Q<Button>("CancelButton");
        sections = bottom.Q<VisualElement>("sections");
        categories = top.Q<VisualElement>("categories").Q<VisualElement>("unity-content-container");
        items = bottom.Q<VisualElement>("items").Q<VisualElement>("unity-content-container");
        AccountForSafeArea();

        furniture = sections.Q<Button>("Furniture");
        building = sections.Q<Button>("Building");
        inventory = sections.Q<Button>("Inventory");

        furniture.clicked += SetFurniture;
        building.clicked += SetBuilding;

        SetFurniture();
        cancelButton.clicked += placementSystem.ExitBuildMode;
    }

    private void SetFurniture()
    {
        categories.Clear();
        items.Clear();
        foreach (ObjectCategory type in objectMasterCategories.categories)
        {
            AddCategory(type);
        }

        ShowCategories(objectMasterCategories.categories[0]);
    }

    private void SetBuilding()
    {
        categories.Clear();
        items.Clear();
        if (placementSystem.inMapMode)
        {
            AddCategory("Zones", "Zones", ZoneCategories);
            AddCategory("Roads", "Roads", RoadCategories);
            ZoneCategories();
        }
        AddCategory("Walls", "Walls", WallCreate);
        AddCategory("Doors", "Doors", DoorCategories);


        if (!placementSystem.inMapMode)
        {
            WallCreate();
        }
    }

    public void ExitBuildMenu()
    {
        categories.Clear();
        items.Clear();
        gameObject.SetActive(false);
        mainController.SetMainMenu();
    }

    private void AddCategory(ObjectCategory type)
    {
        string label = type.Name;
        Button button = new Button();
        button.name = type.ToString();
        button.text = label;
        button.AddToClassList("tools-button");
        categories.Add(button);
        button.RegisterCallback<ClickEvent, ObjectCategory>(ShowCategoriesOnClick, type);
    }

    private void AddCategory(string name, string label, EventCallback<ClickEvent> func)
    {
        Button button = new Button();
        button.name = name;
        button.text = label;
        button.AddToClassList("tools-button");
        categories.Add(button);
        button.RegisterCallback(func);
    }

    private void ZoneCategories(ClickEvent evt)
    {
        ZoneCategories();
    }

    private void ZoneCategories()
    {
        items.Clear();
        foreach (ZonesData zone in zoneData.zonesData)
        {
            Button button = new Button();
            button.name = zone.Name;
            button.text = zone.Name;
            button.AddToClassList("viewport-button");
            items.Add(button);
            button.RegisterCallback<ClickEvent, int>(PlaceZone, zone.ID);
        }
    }

    private void RoadCategories(ClickEvent evt)
    {
        RoadCategories();
    }

    private void RoadCategories()
    {
        items.Clear();
        foreach (RoadsData road in roadsData.roadsData)
        {
            Button button = new Button();
            button.name = road.Name;
            button.text = road.Name;
            button.AddToClassList("viewport-button");
            items.Add(button);
            button.RegisterCallback<ClickEvent, int>(PlaceRoad, road.ID);
        }
    }

    private void WallCreate(ClickEvent evt)
    {
        WallCreate();
    }

    private void WallCreate()
    {
        items.Clear();
        Button button = new Button();
        button.name = "Create Wall";
        button.text = "Create Wall";
        button.AddToClassList("viewport-button");
        items.Add(button);
        button.RegisterCallback<ClickEvent>(PlaceWall);
    }

    private void DoorCategories(ClickEvent evt)
    {
        DoorCategories();
    }

    private void DoorCategories()
    {
        items.Clear();
        foreach (DoorsData door in doorsData.doorsData)
        {
            Button button = new Button();
            button.name = door.Name;
            button.text = door.Name;
            button.AddToClassList("viewport-button");
            items.Add(button);
            button.RegisterCallback<ClickEvent, int>(PlaceDoor, door.ID);
        }
    }

    private void ShowCategoriesOnClick(ClickEvent evt, ObjectCategory type)
    {
        ShowCategories(type);
    }

    private void ShowCategories(ObjectCategory category)
    {
        items.Clear();
        List<ObjectData> itemsList = objectsData.objectsData.FindAll(data => data.objectTypeId.Contains(category));
        foreach (ObjectData item in itemsList)
        {
            Button button = new Button();
            button.name = item.Name;
            button.text = item.Name;
            button.AddToClassList("viewport-button");
            items.Add(button);
            button.RegisterCallback<ClickEvent, int>(PlaceItem, item.ID);
        }
    }

    private void PlaceItem(ClickEvent evt, int iD)
    {
        placementSystem.StartPlacement(iD);
    }

    private void PlaceZone(ClickEvent evt, int iD)
    {
        placementSystem.CreateZone(iD);
    }

    private void PlaceRoad(ClickEvent evt, int iD)
    {
        placementSystem.CreateRoad(iD);
    }

    private void PlaceDoor(ClickEvent evt, int iD)
    {
        placementSystem.CreateDoor(iD);
    }

    private void PlaceWall(ClickEvent evt)
    {
        placementSystem.CreateWall();
    }

    private void AccountForSafeArea()
    {
        float hotX()
        {
            if (Screen.safeArea.x == 0) return 0;
            else return (Screen.width - Screen.safeArea.width - Screen.safeArea.x);
        }

        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            top.style.marginLeft = Screen.safeArea.x;
            top.style.marginRight = hotX();
            bottom.style.paddingLeft = Screen.safeArea.x;
            bottom.style.paddingRight = hotX();
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            top.style.marginLeft = hotX();
            top.style.marginRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
            bottom.style.paddingLeft = hotX();
            bottom.style.paddingRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
        }
        bottom.style.paddingBottom = Screen.safeArea.y;
        bottom.style.minHeight = 340 + Screen.safeArea.y;
        top.style.bottom = 340 + (Screen.safeArea.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
