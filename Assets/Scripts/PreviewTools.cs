using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PreviewTools : MonoBehaviour
{
    [SerializeField]
    private UIDocument SettingsMenu;
    [SerializeField]
    private UIDocument TexturesMenu;
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

        TexturesMenu.rootVisualElement.ElementAt(0).visible = false;
        TexturesMenu.rootVisualElement.SetEnabled(false);

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

        if (!placementSystem.isCreate)
        {
            sellButton.SetEnabled(true);
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
        }
        else sellButton.SetEnabled(false);

        if (placementSystem.itemMode == PlacementSystem.Door || placementSystem.itemMode == PlacementSystem.Object)
        {
            customTexture.SetEnabled(true);
            customTexture.RegisterCallback<ClickEvent>(SwitchBackToTextureCustomiser);
        }
        else customTexture.SetEnabled(false);

        if (placementSystem.itemMode != PlacementSystem.Wall)
        {
            gridSnap.RegisterCallback<ClickEvent>(GridSnap);
            RefreshGridSnapButtonIcon();
        }
        else gridSnap.SetEnabled(false);

        AccountForSafeArea();
    }

    public void PlaceCheck()
    {
        placeButton.SetEnabled(canPlace);
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

    public void CustomiseTexture(GameObject obj)
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        rootT.style.visibility = Visibility.Visible;
        rootT.style.left = customTexture.layout.xMin;
        rootT.style.top = Screen.height - (rootT.ElementAt(0).layout.height + controls.layout.height + 20);

        Button tCancel = rootT.Q<VisualElement>("heading").Q<Button>("CancelButton");
        tCancel.UnregisterCallback<ClickEvent>(SwitchBackToTextureCustomiser);
        tCancel.RegisterCallback<ClickEvent>(CloseTexturePopup);

        VisualElement textureList = rootT.Q("content").Q<VisualElement>("unity-content-container");
        textureList.Clear();

        for (int i = 0; i < previewSystem.materials.Count; i++)
        {
            VisualElement e = new();
            e.name = $"texture{i}";
            e.AddToClassList("texture-parent");
            textureList.Add(e);

            Label label = new();
            label.text = $"Texture {i}";
            label.AddToClassList("texture-label");
            e.Add(label);

            Button color = new();
            color.name = "Colour";
            color.AddToClassList("texture-button");
            color.style.backgroundColor = previewSystem.materials[i].color;
            color.RegisterCallback<ClickEvent, MatData>(ColorPicker, previewSystem.materials[i]);
            e.Add(color);

            Button text = new();
            text.name = "Texture";
            text.AddToClassList("texture-button");
            e.Add(text);
        }
    }

    public void SwitchBackToTextureCustomiser(ClickEvent evt)
    {
        CustomiseTexture(previewSystem.previewObject);
    }

    public void ColorPicker(ClickEvent evt, MatData mat)
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        rootT.style.visibility = Visibility.Visible;

        Button tCancel = rootT.Q<VisualElement>("heading").Q<Button>("CancelButton");
        tCancel.UnregisterCallback<ClickEvent>(CloseTexturePopup);
        tCancel.RegisterCallback<ClickEvent>(SwitchBackToTextureCustomiser);

        VisualElement textureList = rootT.Q("content").Q<VisualElement>("unity-content-container");
        textureList.Clear();

        Color.RGBToHSV(mat.color, out float H, out float S, out float V);

        VisualElement parent = new();
        parent.name = $"Color Picker";
        parent.AddToClassList("color-picker");
        textureList.Add(parent);

        VisualElement preview = new();
        preview.name = $"Color Preview";
        preview.AddToClassList("color-picker-bg");
        preview.style.backgroundColor = mat.color;
        parent.Add(preview);

        Slider hue = new();
        hue.name = "Hue";
        hue.label = "Hue";
        hue.value = H;
        hue.lowValue = 0;
        hue.highValue = 1;
        hue.showInputField = true;
        parent.Add(hue);

        Slider saturation = new();
        saturation.name = "Saturation";
        saturation.label = "Saturation";
        saturation.value = S;
        saturation.lowValue = 0;
        saturation.highValue = 1;
        saturation.showInputField = true;
        parent.Add(saturation);

        Slider value = new();
        value.name = "Value";
        value.label = "Value";
        value.value = V;
        value.lowValue = 0;
        value.highValue = 1;
        value.showInputField = true;
        parent.Add(value);

        hue.RegisterValueChangedCallback(evt => SetHSV(evt.newValue, saturation.value, value.value, mat, preview));
        saturation.RegisterValueChangedCallback(evt => SetHSV(hue.value, evt.newValue, value.value, mat, preview));
        value.RegisterValueChangedCallback(evt => SetHSV(hue.value, saturation.value, evt.newValue, mat, preview));


    }

    public void CloseTexturePopup(ClickEvent evt)
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        VisualElement textureList = rootT.Q("content").Q<VisualElement>("unity-content-container");
        textureList.Clear();
        rootT.style.visibility = Visibility.Hidden;
    }

    public void SetHSV(float H, float S, float V, MatData mat, VisualElement preview)
    {
        Color c = Color.HSVToRGB(H, S, V);
        mat.color = c;
        preview.style.backgroundColor = c;

        previewSystem.RefreshColors();
    }

    public void GridSnap(ClickEvent evt)
    {
        if (!previewSystem.gridSnap)
        {
            previewSystem.gridSnap = true;
        }
        else
        {
            previewSystem.gridSnap = false;
        }
        RefreshGridSnapButtonIcon();
    }
    

    private void RefreshGridSnapButtonIcon()
    {
        Color.RGBToHSV(gridSnap.ElementAt(0).style.unityBackgroundImageTintColor.value, out float H, out float S, out float V);
        if (!previewSystem.gridSnap)
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
