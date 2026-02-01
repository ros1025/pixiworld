using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
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
    private Slider hue, saturation, value;
    private bool svDragModifyState, hueDragModifyState;

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

    public void CustomiseTexture(ClickEvent evt)
    {
        CustomiseTexture();
    }

    public void CustomiseTexture()
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        rootT.style.visibility = Visibility.Visible;
        rootT.style.left = calcXMin();
        rootT.style.top = calcYMin();

        Button tCancel = rootT.Q<VisualElement>("heading").Q<Button>("CancelButton");
        tCancel.UnregisterCallback<ClickEvent>(CustomiseTexture);
        tCancel.RegisterCallback<ClickEvent>(HideTexturePopup);

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
        tCancel.UnregisterCallback<ClickEvent>(HideTexturePopup);
        tCancel.RegisterCallback<ClickEvent>(CustomiseTexture);

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

        hueCursor = new();
        hueCursor.name = $"Hue Cursor";
        hueCursor.AddToClassList("hue-picker-widget");
        hueImage.Add(hueCursor);

        hue = new();
        hue.name = "Hue";
        hue.label = "Hue";
        hue.value = H;
        hue.lowValue = 0;
        hue.highValue = 1;
        hue.showInputField = true;
        parent.Add(hue);

        saturation = new();
        saturation.name = "Saturation";
        saturation.label = "Saturation";
        saturation.value = S;
        saturation.lowValue = 0;
        saturation.highValue = 1;
        saturation.showInputField = true;
        parent.Add(saturation);

        value = new();
        value.name = "Value";
        value.label = "Value";
        value.value = V;
        value.lowValue = 0;
        value.highValue = 1;
        value.showInputField = true;
        parent.Add(value);

        hue.RegisterValueChangedCallback(evt => SetHSV(evt.newValue, saturation.value, value.value, mat));
        saturation.RegisterValueChangedCallback(evt => SetHSV(hue.value, evt.newValue, value.value, mat));
        value.RegisterValueChangedCallback(evt => SetHSV(hue.value, saturation.value, evt.newValue, mat));
        previewImage.RegisterCallback<PointerDownEvent>(evt => {
            svDragModifyState = true;
        });
        previewImage.RegisterCallback<PointerMoveEvent>(evt => {
            if (svDragModifyState)
            {
                SetHSVByPreview(hue.value, evt.localPosition.x, evt.localPosition.y, mat);
            }
        });
        previewImage.RegisterCallback<PointerLeaveEvent>(evt => {
            if (svDragModifyState)
            {
                SetHSVByPreview(hue.value, evt.localPosition.x, evt.localPosition.y, mat);
            }
        });

        hueImage.RegisterCallback<PointerDownEvent>(evt => {
            hueDragModifyState = true;
        });
        hueImage.RegisterCallback<PointerMoveEvent>(evt => {
            if (hueDragModifyState)
            {
                SetHSVByHueSlider(evt.localPosition.y, saturation.value, value.value, mat);
            }
        });
        hueImage.RegisterCallback<PointerLeaveEvent>(evt => {
            if (hueDragModifyState)
            {
                SetHSVByHueSlider(evt.localPosition.y, saturation.value, value.value, mat);
            }
        });

        rootT.RegisterCallback<PointerUpEvent>(evt => {
            hueDragModifyState = false;
            svDragModifyState = false;
        });

        SetHSV(H, S, V, mat);
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
        float height = hueImage.resolvedStyle.height - hueCursor.resolvedStyle.height - hueImage.resolvedStyle.marginBottom - hueImage.resolvedStyle.marginTop;
        hueCursor.style.top = (float) (height) - (H * (height));
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

        float height = previewImage.resolvedStyle.height - colorCursor.resolvedStyle.height - previewImage.resolvedStyle.marginBottom - previewImage.resolvedStyle.marginTop;
        float width = previewImage.resolvedStyle.width - colorCursor.resolvedStyle.width;
        colorCursor.style.left = (float) S * (width);
        colorCursor.style.top = (float) (height) - (V * (height));
    }

    public void HideTexturePopup(ClickEvent evt)
    {
        HideTexturePopup();
    }

    public void HideTexturePopup()
    {
        VisualElement rootT = TexturesMenu.rootVisualElement;
        VisualElement textureList = rootT.Q("content").Q<VisualElement>("unity-content-container");
        textureList.Clear();
        rootT.style.visibility = Visibility.Hidden;
    }

    private void SetHSVByPreview(float H, float mouseX, float mouseY, MatData mat)
    {
        float height = previewImage.resolvedStyle.height - colorCursor.resolvedStyle.height - previewImage.resolvedStyle.marginBottom - previewImage.resolvedStyle.marginTop;
        float width = previewImage.resolvedStyle.width - colorCursor.resolvedStyle.width;

        float S = Mathf.Clamp(mouseX / width, 0, 1);
        float V = Mathf.Clamp((height - mouseY) / height, 0, 1);

        SetHSV(H, S, V, mat);
    }

    private void SetHSVByHueSlider(float mouseY, float S, float V, MatData mat)
    {
        float height = hueImage.resolvedStyle.height - hueCursor.resolvedStyle.height - hueImage.resolvedStyle.marginBottom - hueImage.resolvedStyle.marginTop;

        float H = Mathf.Clamp((height - mouseY) / height, 0, 1);
        SetHSV(H, S, V, mat);
    }

    public void SetHSV(float H, float S, float V, MatData mat)
    {
        Color c = Color.HSVToRGB(H, S, V);
        mat.color = c;
        outputImage.style.backgroundColor = c;
        hue.value = H; saturation.value = S; value.value = V;

        GenerateImages(H, S, V);
        changeColorFunction();
    }
}
