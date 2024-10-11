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
    Label costPrice; Label size;

    public bool canRemove; 
    public bool canPlace;
    public bool isFurniture;
    public bool isRoad;
    public bool isWall;

    public void Call()
    {
        settingsMenuObject.SetActive(true);
        root = SettingsMenu.rootVisualElement;
        controls = root.Q<VisualElement>("Controls");

        placeButton = controls.Q<Button>("PlaceButton");
        cancelButton = controls.Q<Button>("CancelButton");
        sellButton = controls.Q<Button>("SellButton");
        inventoryButton = controls.Q<Button>("InventoryButton");

        costPrice = controls.Q<Label>("CostPrice");
        size = controls.Q<Label>("Size");

        placeButton.SetEnabled(canPlace);
        sellButton.SetEnabled(canRemove);
        if (isFurniture)
            inventoryButton.SetEnabled(canRemove);
        else inventoryButton.SetEnabled(false);

        placeButton.RegisterCallback<ClickEvent>(InvokeAction);
        cancelButton.RegisterCallback<ClickEvent>(InvokeExit);
        if (isRoad)
            sellButton.RegisterCallback<ClickEvent, Roads>(RemoveRoad, previewSystem.selectedRoad);
        else if (isWall)
            sellButton.RegisterCallback<ClickEvent, Wall>(RemoveWall, previewSystem.selectedWall);
        else if (isFurniture)
            sellButton.RegisterCallback<ClickEvent, GameObject>(SellObject, previewSystem.previewObject);
        else sellButton.RegisterCallback<ClickEvent, GameObject>(RemoveZone, previewSystem.previewObject);

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
