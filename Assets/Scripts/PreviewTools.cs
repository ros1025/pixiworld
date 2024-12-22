using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PreviewTools : MonoBehaviour
{
    [SerializeField]
    private UIDocument SettingsMenu;
    [SerializeField]
    private GameObject settingsMenuObject;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private PreviewSystem previewSystem;
    [SerializeField]
    private PlacementSystem placementSystem;

    VisualElement root; VisualElement controls;
    Button placeButton; Button cancelButton; Button sellButton; Button inventoryButton;
    Button gridSnap; Button customTexture;
    Label costPrice; Label size;

    public bool canPlace;

    public void Call()
    {
        settingsMenuObject.SetActive(true);
        root = SettingsMenu.rootVisualElement;
        controls = root.Q<VisualElement>("Controls");

        placeButton = controls.Q<Button>("PlaceButton");
        cancelButton = controls.Q<Button>("CancelButton");
        sellButton = controls.Q<Button>("SellButton");
        inventoryButton = controls.Q<Button>("InventoryButton");
        gridSnap = controls.Q<Button>("GridSnap");
        customTexture = controls.Q<Button>("CustomTexture");

        costPrice = controls.Q<Label>("CostPrice");
        size = controls.Q<Label>("Size");

        placeButton.SetEnabled(canPlace);
        sellButton.SetEnabled(!placementSystem.isCreate);
        if (placementSystem.itemMode == PlacementSystem.Object)
            inventoryButton.SetEnabled(!placementSystem.isCreate);
        else inventoryButton.SetEnabled(false);

        placeButton.RegisterCallback<ClickEvent>(InvokeAction);
        cancelButton.RegisterCallback<ClickEvent>(InvokeExit);

        if (placementSystem.itemMode == PlacementSystem.Road)
            sellButton.RegisterCallback<ClickEvent, Roads>(RemoveRoad, previewSystem.selectedRoad);
        else if (placementSystem.itemMode == PlacementSystem.Wall)
            sellButton.RegisterCallback<ClickEvent, Wall>(RemoveWall, previewSystem.selectedWall);
        else if (placementSystem.itemMode == PlacementSystem.Object)
            sellButton.RegisterCallback<ClickEvent, GameObject>(SellObject, previewSystem.previewObject);
        else if (placementSystem.itemMode == PlacementSystem.Zone)
            sellButton.RegisterCallback<ClickEvent, GameObject>(RemoveZone, previewSystem.previewObject);
        else if (placementSystem.itemMode == PlacementSystem.Door)
            sellButton.RegisterCallback<ClickEvent, GameObject>(RemoveDoor, previewSystem.previewObject);
        else sellButton.SetEnabled(false);

        if (placementSystem.itemMode != PlacementSystem.Wall)
            gridSnap.clicked += GridSnap;

        AccountForSafeArea();
    }

    public void Hide()
    {
        settingsMenuObject.SetActive(false);
    }

    public void SellObject(ClickEvent evt, GameObject prefab)
    {
        placementSystem.RemoveObject(prefab);
    }

    public void RemoveZone(ClickEvent evt, GameObject prefab)
    {
        placementSystem.RemoveZone(prefab);
    }

    public void RemoveRoad(ClickEvent evt, Roads road)
    {
        placementSystem.RemoveRoad(road);
    }

    public void RemoveWall(ClickEvent evt, Wall wall)
    {
        placementSystem.RemoveWall(wall);
    }

    public void RemoveDoor(ClickEvent evt, GameObject prefab)
    {
        placementSystem.RemoveDoor(prefab);
    }

    private void InvokeAction(ClickEvent evt)
    {
        inputManager.InvokeAction();
    }

    private void InvokeExit(ClickEvent evt)
    {
        inputManager.InvokeExit();
    }

    public void AdjustLabels(int cost, Vector2Int size)
    {
        costPrice.text = $"${cost}";
        this.size.text = $"{size.x}x{size.y}";
    }

    public void GridSnap()
    {
        if (!previewSystem.gridSnap)
        {
            previewSystem.gridSnap = true;
        }
        else
        {
            previewSystem.gridSnap = false;
        }
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
            root.style.marginLeft = 5 + Screen.safeArea.x;
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            root.style.marginLeft = 5 + hotX();
        }
        root.style.marginBottom = 5 + Screen.height - Screen.safeArea.height;
    }
}
