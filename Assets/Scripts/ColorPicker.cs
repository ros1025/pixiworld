using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorPicker : MonoBehaviour
{
    [SerializeField]
    private UIDocument TexturesMenu;
    private List<MatData> matData;
    private Action changeColorFunction;
    private Func<float> calcXMin, calcYMin;
    private Texture2D hueTexture, svTexture;
    private Image previewImage, hueImage; 
    private VisualElement outputImage;
    private VisualElement colorCursor, hueCursor;

    public void TransferMaterialData(List<MatData> matData, Action changeColorFunction, Func<float> xMin, Func<float> yMin)
    {
        this.matData = matData;
        this.changeColorFunction = changeColorFunction;
        this.calcXMin = xMin;
        this.calcYMin = yMin;
        Debug.Log($"{xMin}, {yMin}");
    }



    public VisualElement GetRootT()
    {
        return TexturesMenu.rootVisualElement;
    }

    public void CustomiseTexture()
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        rootT.style.visibility = Visibility.Visible;
        rootT.style.left = calcXMin();
        rootT.style.top = calcYMin();

        Button tCancel = rootT.Q<VisualElement>("heading").Q<Button>("CancelButton");
        tCancel.UnregisterCallback<ClickEvent>((evt) => HideTexturePopup());
        tCancel.RegisterCallback<ClickEvent>((evt) => CustomiseTexture());

        VisualElement textureList = rootT.Q("content").Q<VisualElement>("unity-content-container");
        textureList.Clear();

        for (int i = 0; i < matData.Count; i++)
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
            color.style.backgroundColor = matData[i].color;
            color.RegisterCallback<ClickEvent, MatData>(ColorPickerUI, matData[i]);
            e.Add(color);

            Button text = new();
            text.name = "Texture";
            text.AddToClassList("texture-button");
            e.Add(text);
        }
    }

    public void ColorPickerUI(ClickEvent evt, MatData mat)
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        rootT.style.visibility = Visibility.Visible;

        Button tCancel = rootT.Q<VisualElement>("heading").Q<Button>("CancelButton");
        tCancel.UnregisterCallback<ClickEvent>((evt) => {HideTexturePopup();});
        tCancel.RegisterCallback<ClickEvent>((evt) => {CustomiseTexture();});

        VisualElement textureList = rootT.Q("content").Q<VisualElement>("unity-content-container");
        textureList.Clear();

        Color.RGBToHSV(mat.color, out float H, out float S, out float V);

        VisualElement parent = new();
        parent.name = $"Color Picker";
        parent.AddToClassList("color-picker");
        textureList.Add(parent);

        VisualElement preview = new();
        preview.name = $"Color Preview";
        preview.AddToClassList("color-picker-wrapper");
        parent.Add(preview);

        previewImage = new();
        previewImage.name = $"Color Preview Image";
        previewImage.AddToClassList("color-picker-bg");
        previewImage.image = svTexture;
        preview.Add(previewImage);

        hueImage = new();
        hueImage.name = $"Color Hue Image";
        hueImage.AddToClassList("color-picker-side");
        hueImage.image = hueTexture;
        preview.Add(hueImage);

        outputImage = new();
        outputImage.name = $"Color Output Image";
        outputImage.AddToClassList("color-picker-side");
        outputImage.style.backgroundColor = mat.color;
        preview.Add(outputImage);

        colorCursor = new();
        colorCursor.name = $"Color Preview Cursor";
        colorCursor.AddToClassList("color-picker-widget");
        previewImage.Add(colorCursor);

        GenerateImages(H, S, V);

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

    private void GenerateImages(float H, float S, float V)
    {
        GenerateHueImage(H, S, V);
        GenerateSVImage(H, S, V);
    }

    private void GenerateHueImage(float H, float S, float V)
    {
        hueTexture = new Texture2D(1, 10);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float) i / hueTexture.height, 1f, 0.95f));
        }

        hueTexture.Apply();
        hueImage.image = hueTexture;
    }

    private void GenerateSVImage(float H, float S, float V)
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(H, (float) x / svTexture.width, (float) y / svTexture.height));
            }
        }

        svTexture.Apply();
        previewImage.image = svTexture;
        colorCursor.style.left = (float) S * (previewImage.resolvedStyle.width - colorCursor.resolvedStyle.width);
        colorCursor.style.top = (float) (previewImage.resolvedStyle.height - colorCursor.resolvedStyle.height) - (V * (previewImage.resolvedStyle.height - colorCursor.resolvedStyle.height));
    }

    public void HideTexturePopup()
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
        outputImage.style.backgroundColor = c;

        changeColorFunction();
        GenerateImages(H, S, V);
    }
}
