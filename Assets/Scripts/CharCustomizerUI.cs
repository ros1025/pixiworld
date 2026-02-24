using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Mono.Cecil.Cil;

public class CharCustomizerUI : MonoBehaviour
{
    [SerializeField]
    private UIDocument document;
    [SerializeField]
    private CharacterCustomizer customizer;
    [SerializeField]
    private ClothingDatabaseSO clothesDB;
    [SerializeField]
    private BodyItemsDatabaseSO bodyItemsDB;
    [SerializeField]
    private ClothingCategoryDatabaseSO clothingCategoriesDB;
    [SerializeField]
    private ClothingCategoryDatabaseSO bodyFeatureCategoriesDB;
    private VisualElement root;
    private ScrollView categories, subcategories;
    private ScrollView items;
    private Button clothes, body, themes, bodyProportions, traits;
    private Button hideButton;
    private VisualElement outfitTypeBox, outfitBox;

    private void Awake()
    {
        root = document.rootVisualElement;
        StartCustomizerUI();
    }

    public void StartCustomizerUI()
    {
        StartCoroutine(Initialise());
    }

    public IEnumerator Initialise()
    {
        root = document.rootVisualElement;
        root.style.visibility = Visibility.Visible;

        //customizer.InitializeCustomiser();
        yield return new WaitUntil(() => customizer.GetCurrentCharacter() != null);
        categories = root.Q<VisualElement>("CategoryBar").Q<ScrollView>();
        subcategories = root.Q<VisualElement>("SubCategoryBar").Q<ScrollView>();
        items = root.Q<VisualElement>("ContentBar").Q<ScrollView>();
        clothes = root.Q<VisualElement>("Header").Q<Button>("Clothes");
        body = root.Q<VisualElement>("Header").Q<Button>("Body");
        themes = root.Q<VisualElement>("Header").Q<Button>("Themes");
        bodyProportions = root.Q<VisualElement>("Header").Q<Button>("Proportions");
        traits = root.Q<VisualElement>("Header").Q<Button>("Traits");
        hideButton = root.Q<VisualElement>("Header").Q<Button>("Cancel");

        outfitTypeBox = new();
        outfitTypeBox.AddToClassList("mini-content-group");
        outfitBox = new();
        outfitBox.AddToClassList("mini-content-group");

        clothes.RegisterCallback<ClickEvent>(TriggerClothesSelection);
        body.RegisterCallback<ClickEvent>(TriggerBodySelection);
        themes.RegisterCallback<ClickEvent>(TriggerThemeCustomisation);
        bodyProportions.RegisterCallback<ClickEvent>(TriggerProportionCustomisation);
        traits.RegisterCallback<ClickEvent>(TriggerTraitsCustomisation);

        hideButton.RegisterCallback<ClickEvent>(Hide);

        TriggerClothesSelection();
        AccountForSafeArea();
    }

    private void TriggerClothesSelection(ClickEvent evt)
    {
        TriggerClothesSelection();
    }

    private void TriggerClothesSelection()
    {
        items.Clear();
        categories.Clear();
        subcategories.Clear();

        foreach (ClothingCategorySO category in clothingCategoriesDB.clothingCategories)
        {
            Button catButton = new();
            catButton.name = category.name;
            catButton.AddToClassList("category-button");
            catButton.RegisterCallback<ClickEvent, ClothingCategorySO>(FilterClothingChoice, category);
            categories.Add(catButton);
        }

        categories.scrollOffset = new Vector2(0, 0);
        subcategories.scrollOffset = new Vector2(0, 0);
        FilterClothingChoice(clothingCategoriesDB.clothingCategories[0]);
    }

    private void FilterClothingChoice(ClickEvent evt, ClothingCategorySO category)
    {
        FilterClothingChoice(category);
    }

    private void FilterClothingChoice(ClothingCategorySO category)
    {
        subcategories.Clear();

        Button catButton2 = new();
        catButton2.name = "All Subcategories";
        catButton2.AddToClassList("category-button");
        catButton2.RegisterCallback<ClickEvent, List<ObjectCategory>>(FilterClothingChoice, category.subcategories);
        subcategories.Add(catButton2);

        foreach (ObjectCategory subcategory in category.subcategories)
        {
            Button subCatButton = new();
            subCatButton.name = subcategory.name;
            subCatButton.AddToClassList("category-button");
            subCatButton.RegisterCallback<ClickEvent, List<ObjectCategory>>(FilterClothingChoice, new List<ObjectCategory> {subcategory});
            subcategories.Add(subCatButton);
        }

        FilterClothingChoice(category.subcategories);
    }

    private void FilterClothingChoice(ClickEvent evt, List<ObjectCategory> categories)
    {
        FilterClothingChoice(categories);
    }

    private void FilterClothingChoice(List<ObjectCategory> categories)
    {
        items.Clear();

        if (customizer.GetCurrentCharacter() != null)
        {
            List<ClothingSO> clothes = clothesDB.clothes.FindAll(item => item.clothingCategory.Intersect(categories).Count() > 0);
            List<VisualElement> elements = new();

            items.Add(outfitTypeBox);
            items.Add(outfitBox);
            GetOutfitTypeBox();

            foreach (ClothingSO clothing in clothes)
            {
                AddClothingButton(elements, clothing);
            }

            if (clothes.Count == 0)
            {
                Label label = new();
                label.text = "No Clothing with Selected Filters! Try resetting your filters.";
                label.AddToClassList("group-label");
                items.Add(label);
            }

            Debug.Log(items.childCount);

            items.scrollOffset = new Vector2(0, 0);
        }
    }

    private async Awaitable AddClothingButton(List<VisualElement> elements, ClothingSO clothing)
    {
        if (elements.Count == 0 || elements[^1].childCount >= 2)
        {
            VisualElement visualElement = new();
            visualElement.name = $"Content Group {elements.Count + 1}";
            visualElement.AddToClassList("content-group");
            items.Add(visualElement);
            elements.Add(visualElement);
        }

        Button button = new();
        button.name = clothing.name;
        button.text = clothing.name;
        button.AddToClassList("content-button");
        elements[^1].Add(button);

        button.RegisterCallback<ClickEvent>(evt => customizer.ChangeCharacterClothing(clothing));
    }

    private void TriggerBodySelection(ClickEvent evt)
    {
        TriggerBodySelection();
    }

    private void TriggerBodySelection()
    {
        items.Clear();
        categories.Clear();
        subcategories.Clear();

        foreach (ClothingCategorySO category in bodyFeatureCategoriesDB.clothingCategories)
        {
            Button catButton = new();
            catButton.name = category.name;
            catButton.AddToClassList("category-button");
            catButton.RegisterCallback<ClickEvent, ClothingCategorySO>(FilterBodyItemChoice, category);
            categories.Add(catButton);
        }

        categories.scrollOffset = new Vector2(0, 0);
        subcategories.scrollOffset = new Vector2(0, 0);
        FilterBodyItemChoice(bodyFeatureCategoriesDB.clothingCategories[0]);
    }

    private void FilterBodyItemChoice(ClickEvent evt, ClothingCategorySO category)
    {
        FilterBodyItemChoice(category);
    }

    private void FilterBodyItemChoice(ClothingCategorySO category)
    {
        subcategories.Clear();

        Button catButton2 = new();
        catButton2.name = "All Subcategories";
        catButton2.AddToClassList("category-button");
        catButton2.RegisterCallback<ClickEvent, List<ObjectCategory>>(FilterBodyItemChoice, category.subcategories);
        subcategories.Add(catButton2);

        foreach (ObjectCategory subcategory in category.subcategories)
        {
            Button subCatButton = new();
            subCatButton.name = subcategory.name;
            subCatButton.AddToClassList("category-button");
            subCatButton.RegisterCallback<ClickEvent, List<ObjectCategory>>(FilterBodyItemChoice, new List<ObjectCategory> {subcategory});
            subcategories.Add(subCatButton);
        }

        FilterBodyItemChoice(category.subcategories);
    }

    private void FilterBodyItemChoice(ClickEvent evt, List<ObjectCategory> categories)
    {
        FilterBodyItemChoice(categories);
    }

    private void FilterBodyItemChoice(List<ObjectCategory> categories)
    {
        items.Clear();

        if (customizer.GetCurrentCharacter() != null)
        {
            List<BodyFeatureSO> clothes = bodyItemsDB.bodyFeatures.FindAll(item => item.featureTypes.Intersect(categories).Count() > 0);
            List<VisualElement> elements = new();

            items.Add(outfitTypeBox);
            items.Add(outfitBox);
            GetOutfitTypeBox();

            foreach (BodyFeatureSO bodyFeature in clothes)
            {
                AddBodyItemButton(elements, bodyFeature);
            }

            if (clothes.Count == 0)
            {
                Label label = new();
                label.text = "No Items with Selected Filters! Try resetting your filters.";
                label.AddToClassList("group-label");
                items.Add(label);
            }

            Debug.Log(items.childCount);

            items.scrollOffset = new Vector2(0, 0);
        }
    }

    private async Awaitable AddBodyItemButton(List<VisualElement> elements, BodyFeatureSO bodyFeature)
    {
        if (elements.Count == 0 || elements[^1].childCount >= 2)
        {
            VisualElement visualElement = new();
            visualElement.name = $"Content Group {elements.Count + 1}";
            visualElement.AddToClassList("content-group");
            items.Add(visualElement);
            elements.Add(visualElement);
        }

        Button button = new();
        button.name = bodyFeature.name;
        button.text = bodyFeature.name;
        button.AddToClassList("content-button");
        elements[^1].Add(button);

        button.RegisterCallback<ClickEvent>(evt => customizer.ChangeCharacterClothing(bodyFeature));
    }

    private void TriggerThemeCustomisation(ClickEvent evt)
    {
        TriggerThemeCustomisation();
    }

    private void TriggerThemeCustomisation()
    {
        items.Clear();
        categories.Clear();
        subcategories.Clear();

        if (customizer.GetCurrentCharacter() != null)
        {
            List<CharacterItem> clothingList = customizer.GetCurrentCharacter().GetAllClothingInCurrentOutfit();

            foreach (CharacterItem cloth in clothingList)
            {
                VisualElement main = new();
                main.name = $"{cloth.clothingItem.Name}";
                main.AddToClassList("content-group-vertical");
                items.Add(main);

                Label label = new();
                label.text = $"Clothing {cloth.clothingItem.Name}";
                label.AddToClassList("group-label");
                main.Add(label);

                VisualElement miniBox = new();
                miniBox.AddToClassList("mini-content-group");
                main.Add(miniBox);

                Button color = new();
                color.name = "Colour";
                color.AddToClassList("texture-button");
                color.style.backgroundColor = cloth.matData.color;
                color.RegisterCallback<ClickEvent, CharacterItem>(ColorPickerUI, cloth);
                miniBox.Add(color);

                Button text = new();
                text.name = "Texture";
                text.AddToClassList("texture-button");
                miniBox.Add(text);
            }
        }
    }

    private void TriggerProportionCustomisation(ClickEvent evt)
    {
        TriggerProportionCustomisation();
    }


    private void TriggerProportionCustomisation()
    {
        items.Clear();
        categories.Clear();
        subcategories.Clear();

        if (customizer.GetCurrentCharacter() != null)
        {
            List<TransformGroups> transformGroups = customizer.GetTransformGroups();
            foreach (TransformGroups group in transformGroups)
            {
                VisualElement visualElement = new();
                visualElement.name = group.name;
                visualElement.AddToClassList("content-group-vertical");
                items.Add(visualElement);

                Label label = new();
                string displayName = group.name.Split("_")[1].Replace("-", " ");
                label.text = displayName;
                label.AddToClassList("group-label");
                visualElement.Add(label);

                Slider weight = new();
                weight.name = "Weight";
                weight.label = "Weight";
                weight.value = group.weight;
                weight.lowValue = 0;
                weight.highValue = 100;
                weight.showInputField = false;
                visualElement.Add(weight);

                weight.RegisterValueChangedCallback(evt => group.SetTransformerWeights(evt.newValue));
            }
        }

        items.scrollOffset = new Vector2(0, 0);
    }

    private void TriggerTraitsCustomisation(ClickEvent evt)
    {
        TriggerTraitsCustomisation();
    }

    private void TriggerTraitsCustomisation()
    {
        items.Clear();
        categories.Clear();
        subcategories.Clear();

        if (customizer.GetCurrentCharacter() != null)
        {
            Character character = customizer.GetCurrentCharacter();

            StringContentGroupCreator(character.GetCharacterName(), character.SetCharacterName);

            VisualElement genderBox = new();
            genderBox.name = "Character Gender";
            genderBox.AddToClassList("content-group-vertical");
            items.Add(genderBox);

            Label genderLabel = new();
            genderLabel.text = "Gender";
            genderLabel.AddToClassList("group-label");
            genderBox.Add(genderLabel);

            VisualElement genderBoxContent = new();
            genderBoxContent.AddToClassList("mini-content-group");
            genderBox.Add(genderBoxContent);

            foreach (GenderSettingSO defaultGenderSetting in CharacterManager.instance.characterRules.defaultGenders.defaultGenders)
            {
                Button grpButton = new();
                grpButton.name = defaultGenderSetting.Name;
                grpButton.text = defaultGenderSetting.Name;
                genderBoxContent.Add(grpButton);

                if (character.GetCharacterGender().Name == defaultGenderSetting.Name)
                {
                    grpButton.AddToClassList("unity-button-selected");
                }

                grpButton.RegisterCallback<ClickEvent>(evt => {
                    character.SetCharacterGender(defaultGenderSetting.ConvertToGenderSettingObject());
                    HighlightButtonInMiniGroup(evt.target as Button, genderBoxContent);
                });
            }

            VisualElement ageBox = new();
            ageBox.name = "Character Age";
            ageBox.AddToClassList("content-group-vertical");
            items.Add(ageBox);

            Label ageLabel = new();
            ageLabel.text = "Age";
            ageLabel.AddToClassList("group-label");
            ageBox.Add(ageLabel);

            VisualElement ageBoxContent = new();
            ageBoxContent.AddToClassList("mini-content-group");
            ageBox.Add(ageBoxContent);

            foreach (AgeGroupSO ageGroup in CharacterManager.instance.characterRules.ageGroups.ageGroups)
            {
                Button grpButton = new();
                grpButton.name = ageGroup.Name;
                grpButton.text = ageGroup.Name;
                ageBoxContent.Add(grpButton);
                
                if (character.GetCharacterAge() == ageGroup)
                {
                    grpButton.AddToClassList("unity-button-selected");
                }

                grpButton.RegisterCallback<ClickEvent>(evt => {
                    character.SetAgeGroup(ageGroup);
                    HighlightButtonInMiniGroup(evt.target as Button, ageBoxContent);
                });
            }
        }

        items.scrollOffset = new Vector2(0, 0);
    }

    private void GetOutfitTypeBox()
    {
        outfitTypeBox.Clear();

        OutfitTypeSO currentOutfitType = customizer.GetCurrentCharacter().GetCurrentOutfit().outfitType;

        foreach (OutfitTypeSO outfitType in CharacterManager.instance.characterRules.outfitTypes.outfitTypes)
        {
            Button grpButton = new();
            grpButton.name = outfitType.Name;
            grpButton.text = outfitType.Name;
            outfitTypeBox.Add(grpButton);

            if (outfitType == currentOutfitType)
            {
                grpButton.AddToClassList("unity-button-selected");
            }

            grpButton.RegisterCallback<ClickEvent>((evt) => {
                GetOutfitBox(outfitType);
                HighlightButtonInMiniGroup(evt.target as Button, outfitTypeBox);
            });
        }

        GetOutfitBox(currentOutfitType);
    }

    private void GetOutfitBox(OutfitTypeSO outfitType)
    {
        outfitBox.Clear();
        List<SavedCharacterOutfits> outfits = customizer.GetCurrentCharacter().outfits.FindAll(outfit => outfit.outfitType == outfitType);

        SavedCharacterOutfits currentOutfit = customizer.GetCurrentCharacter().GetCurrentOutfit();

        foreach (SavedCharacterOutfits outfit in outfits)
        {
            Button grpButton = new();
            grpButton.name = $"{outfits.IndexOf(outfit)}";
            grpButton.text = $"{outfits.IndexOf(outfit)}";
            outfitBox.Add(grpButton);

            if (outfit == currentOutfit)
            {
                grpButton.AddToClassList("unity-button-selected");
            }

            grpButton.RegisterCallback<ClickEvent>((evt) => {
                customizer.GetCurrentCharacter().SwitchOutfit(outfit);
                HighlightButtonInMiniGroup(evt.target as Button, outfitBox);
            });
        }

        if (currentOutfit.outfitType != outfitType)
        {
            customizer.GetCurrentCharacter().SwitchOutfit(outfits[0]);
            HighlightButtonInMiniGroup(outfitBox.ElementAt(0) as Button, outfitBox);
        }
    }

    private void StringContentGroupCreator(string value, Action<string> changeCallback)
    {
        VisualElement nameBox = new();
        nameBox.name = "Character Name";
        nameBox.AddToClassList("content-group-vertical");
        items.Add(nameBox);

        Label nameLabel = new();
        nameLabel.text = "Name";
        nameLabel.AddToClassList("group-label");
        nameBox.Add(nameLabel);

        TextField nameField = new();
        nameField.value = value;
        nameBox.Add(nameField);
        nameField.RegisterValueChangedCallback(evt => changeCallback(evt.newValue));
    }

    public void HighlightButtonInMiniGroup(Button target, VisualElement box)
    {
        foreach (VisualElement grpItem in box.Children())
        {
            if (grpItem.ClassListContains("unity-button-selected"))
            {
                grpItem.RemoveFromClassList("unity-button-selected");
            }
        }

        target.AddToClassList("unity-button-selected");
    }

    public void ColorPickerUI(ClickEvent evt, CharacterItem characterItem)
    {
        Button tCancel = new();
        tCancel.AddToClassList("mini-content-group");
        tCancel.RegisterCallback<ClickEvent>(TriggerTraitsCustomisation);

        items.Clear();
        Texture2D svTexture = new Texture2D(16, 16); 
        Texture2D hueTexture = new Texture2D(1, 10);

        bool hueDragModifyState = false;
        bool svDragModifyState = false;

        MatData mat = characterItem.matData;
        Color.RGBToHSV(mat.color, out float H, out float S, out float V);

        VisualElement parent = new();
        parent.name = $"Color Picker";
        parent.AddToClassList("color-picker");
        items.Add(parent);

        VisualElement preview = new();
        preview.name = $"Color Preview";
        preview.AddToClassList("color-picker-wrapper");
        parent.Add(preview);

        Image previewImage = new();
        previewImage.name = $"Color Preview Image";
        previewImage.AddToClassList("color-picker-bg");
        previewImage.image = svTexture;
        preview.Add(previewImage);

        Image hueImage = new();
        hueImage.name = $"Color Hue Image";
        hueImage.AddToClassList("color-picker-side");
        hueImage.image = hueTexture;
        preview.Add(hueImage);

        Image outputImage = new();
        outputImage.name = $"Color Output Image";
        outputImage.AddToClassList("color-picker-side");
        outputImage.style.backgroundColor = mat.color;
        preview.Add(outputImage);

        VisualElement colorCursor = new();
        colorCursor.name = $"Color Preview Cursor";
        colorCursor.AddToClassList("color-picker-widget");
        previewImage.Add(colorCursor);

        VisualElement hueCursor = new();
        hueCursor.name = $"Hue Cursor";
        hueCursor.AddToClassList("hue-picker-widget");
        hueImage.Add(hueCursor);

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

        root.RegisterCallback<PointerUpEvent>(evt => {
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
        VisualElement hueCursor = items.Q<VisualElement>("Color Picker").Q<VisualElement>($"Hue Cursor");
        Image hueImage = items.Q<VisualElement>("Color Picker").Q<Image>($"Color Hue Image");

        Texture2D hueTexture = new Texture2D(1, 10);
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
        VisualElement colorCursor = items.Q<VisualElement>("Color Picker").Q<VisualElement>($"Color Preview Cursor");
        Image previewImage = items.Q<VisualElement>("Color Picker").Q<Image>($"Color Preview Image");

        Texture2D svTexture = new Texture2D(16, 16);
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

    private void SetHSVByPreview(float H, float mouseX, float mouseY, MatData mat)
    {
        VisualElement colorCursor = items.Q<VisualElement>("Color Picker").Q<VisualElement>($"Color Preview Cursor");
        Image previewImage = items.Q<VisualElement>("Color Picker").Q<Image>($"Color Preview Image");

        float height = previewImage.resolvedStyle.height - colorCursor.resolvedStyle.height - previewImage.resolvedStyle.marginBottom - previewImage.resolvedStyle.marginTop;
        float width = previewImage.resolvedStyle.width - colorCursor.resolvedStyle.width;

        float S = Mathf.Clamp(mouseX / width, 0, 1);
        float V = Mathf.Clamp((height - mouseY) / height, 0, 1);

        SetHSV(H, S, V, mat);
    }

    private void SetHSVByHueSlider(float mouseY, float S, float V, MatData mat)
    {
        VisualElement hueCursor = items.Q<VisualElement>("Color Picker").Q<VisualElement>($"Hue Cursor");
        Image hueImage = items.Q<VisualElement>("Color Picker").Q<Image>($"Color Hue Image");

        float height = hueImage.resolvedStyle.height - hueCursor.resolvedStyle.height - hueImage.resolvedStyle.marginBottom - hueImage.resolvedStyle.marginTop;

        float H = Mathf.Clamp((height - mouseY) / height, 0, 1);
        SetHSV(H, S, V, mat);
    }

    public void SetHSV(float H, float S, float V, MatData mat)
    {
        Image outputImage = items.Q<VisualElement>("Color Picker").Q<Image>($"Color Output Image");
        Slider hue = items.Q<VisualElement>("Color Picker").Q<Slider>("Hue");
        Slider saturation = items.Q<VisualElement>("Color Picker").Q<Slider>("Saturation");
        Slider value = items.Q<VisualElement>("Color Picker").Q<Slider>("Value");
        hue.value = H; saturation.value = S; value.value = V;

        Color c = Color.HSVToRGB(H, S, V);
        mat.color = c;
        outputImage.style.backgroundColor = c;

        customizer.GetCurrentCharacter().RenderOutfit();
        GenerateImages(H, S, V);
    }

    public void Hide(ClickEvent evt)
    {
        Hide();
    }

    public void Hide()
    {
        if (root.resolvedStyle.visibility == Visibility.Visible)
        {
            root.style.visibility = Visibility.Hidden;
        }
        else
        {
            root.style.visibility = Visibility.Visible;
        }
    }

    private void AccountForSafeArea()
    {
        float hotX()
        {
            if (Screen.safeArea.x == 0) return 0;
            else return (Screen.width - Screen.safeArea.width - Screen.safeArea.x);
        }

        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            root.style.marginRight = Screen.safeArea.x;
        }
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            root.style.marginRight = hotX();
        }
        root.style.marginBottom = Screen.height - Screen.safeArea.height;
    }
}
