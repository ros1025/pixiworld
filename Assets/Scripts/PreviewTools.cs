using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PreviewTools : MonoBehaviour
{
    [SerializeField]
    private UIDocument SettingsMenu;
    [SerializeField]
    private ColorPicker colorPicker;
    [SerializeField]
    private GameObject settingsMenuObject;
    [SerializeField]
    private InputManager inputManager;
    //[SerializeField]
    //private PreviewSystem previewSystem;
    [SerializeField]
    private PlacementSystem placementSystem;

    VisualElement root; VisualElement controls;
    Button placeButton; Button cancelButton; Button sellButton; Button inventoryButton;
    Button gridSnap; Button customTexture; Button rotateLeft; Button rotateRight; Button height;
    Label costPrice; Label size;

    public bool canPlace;

    public void Call()
    {
        settingsMenuObject.SetActive(true);
        root = SettingsMenu.rootVisualElement;
        controls = root.Q<VisualElement>("Controls");

        colorPicker.HideTexturePopup();

        placeButton = controls.Q<Button>("PlaceButton");
        cancelButton = controls.Q<Button>("CancelButton");
        sellButton = controls.Q<Button>("SellButton");
        inventoryButton = controls.Q<Button>("InventoryButton");
        gridSnap = controls.Q<Button>("GridSnap");
        customTexture = controls.Q<Button>("CustomTexture");
        rotateLeft = controls.Q<Button>("RotateLeft");
        rotateRight = controls.Q<Button>("RotateRight");
        height = controls.Q<Button>("AdjustHeight");

        costPrice = controls.Q<Label>("CostPrice");
        size = controls.Q<Label>("Size");

        placeButton.SetEnabled(canPlace);
        sellButton.SetEnabled(!placementSystem.isCreate);
        if (placementSystem.itemMode == PlacementSystem.Object)
            inventoryButton.SetEnabled(!placementSystem.isCreate);
        else inventoryButton.SetEnabled(false);

        placeButton.RegisterCallback<ClickEvent>(InvokeAction);
        cancelButton.RegisterCallback<ClickEvent>(InvokeExit);

        sellButton.SetEnabled(false);

        if (placementSystem.itemMode == PlacementSystem.Door || placementSystem.itemMode == PlacementSystem.Object)
        {
            rotateLeft.SetEnabled(true);
            rotateRight.SetEnabled(true);
            height.SetEnabled(true);

            if (placementSystem.itemMode == PlacementSystem.Door)
            {
                rotateLeft.RegisterCallback<ClickEvent>(evt => {placementSystem.ChangeRotation(-180);});
                rotateRight.RegisterCallback<ClickEvent>(evt => {placementSystem.ChangeRotation(180);});
                
            }
            else
            {
                rotateLeft.RegisterCallback<ClickEvent>(evt => {placementSystem.ChangeRotation(-15);});
                rotateRight.RegisterCallback<ClickEvent>(evt => {placementSystem.ChangeRotation(15);});
            }
        }
        else 
        {
            customTexture.SetEnabled(false);
            rotateLeft.SetEnabled(false);
            rotateRight.SetEnabled(false);
            height.SetEnabled(false);
        }

        if (placementSystem.itemMode != PlacementSystem.Wall)
        {
            gridSnap.RegisterCallback<ClickEvent>(GridSnap);
            RefreshGridSnapButtonIcon();
        }
        else gridSnap.SetEnabled(false);

        AccountForSafeArea();
    }

    public void EnableCustomTexture(List<MatData> matData, Action changeColorFunction)
    {
        customTexture.SetEnabled(true);
        colorPicker.TransferMaterialData(matData, changeColorFunction, () => {return customTexture.layout.xMin;}, () => {return SettingsMenu.runtimePanel.panelSettings.referenceResolution.y - (colorPicker.GetRootT().ElementAt(0).layout.height + controls.layout.height + 50);});
        customTexture.RegisterCallback<ClickEvent>(SwitchBackToTextureCustomiser);
    }

    public void SwitchBackToTextureCustomiser(ClickEvent evt)
    {
        colorPicker.CustomiseTexture();
    }

    public void EnableSellButton(Action mapAction)
    {
        sellButton.SetEnabled(true);
        sellButton.RegisterCallback<ClickEvent>((evt) => {mapAction();});
    }

    public void PlaceCheck()
    {
        placeButton.SetEnabled(canPlace);
    }

    public void Hide()
    {
        settingsMenuObject.SetActive(false);
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

    public void GridSnap(ClickEvent evt)
    {
        if (!placementSystem.gridSnap)
        {
            placementSystem.gridSnap = true;
        }
        else
        {
            placementSystem.gridSnap = false;
        }
        RefreshGridSnapButtonIcon();
    }
    

    private void RefreshGridSnapButtonIcon()
    {
        Color.RGBToHSV(gridSnap.ElementAt(0).style.unityBackgroundImageTintColor.value, out float H, out float S, out float V);
        if (!placementSystem.gridSnap)
        {
            
            Color bgColor = Color.HSVToRGB(H, S, V);
            bgColor.a = 0.5f;
            gridSnap.ElementAt(0).style.unityBackgroundImageTintColor = bgColor;
        }
        else
        {
            Color bgColor = Color.HSVToRGB(H, S, V);
            bgColor.a = 1f;
            gridSnap.ElementAt(0).style.unityBackgroundImageTintColor = bgColor;
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
