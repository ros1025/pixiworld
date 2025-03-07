using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharCustomizerUI : MonoBehaviour
{
    [SerializeField]
    private UIDocument document;
    [SerializeField]
    private CharacterCustomizer customizer;
    [SerializeField]
    private ClothingDatabaseSO clothesDB;
    [SerializeField]
    private ClothingCategorySO categoriesDB;
    private VisualElement root;
    private ScrollView categories;
    private ScrollView items;
    private Button clothes;
    private Button themes;
    private Button bodyProportions;
    private Button traits;
    private Button hideButton;

    private void Start()
    {
        root = document.rootVisualElement;
        Initialise();
    }

    public void Initialise()
    {
        root = document.rootVisualElement;
        root.style.visibility = Visibility.Visible;

        customizer.InitializeCustomiser();
        categories = root.Q<VisualElement>("CategoryBar").Q<ScrollView>();
        items = root.Q<VisualElement>("ContentBar").Q<ScrollView>();
        clothes = root.Q<VisualElement>("Header").Q<Button>("Clothes");
        themes = root.Q<VisualElement>("Header").Q<Button>("Themes");
        bodyProportions = root.Q<VisualElement>("Header").Q<Button>("Proportions");
        traits = root.Q<VisualElement>("Header").Q<Button>("Traits");
        hideButton = root.Q<VisualElement>("Header").Q<Button>("Cancel");

        clothes.RegisterCallback<ClickEvent>(TriggerClothesSelection);
        bodyProportions.RegisterCallback<ClickEvent>(TriggerProportionCustomisation);

        TriggerClothesSelection();
    }

    private void TriggerClothesSelection(ClickEvent evt)
    {
        TriggerClothesSelection();
    }

    private void TriggerClothesSelection()
    {
        items.Clear();
        categories.Clear();

        foreach (ClothingCategory category in categoriesDB.clothingCategories)
        {
            Button catButton = new();
            catButton.name = category.name;
            catButton.AddToClassList("category-button");
            catButton.RegisterCallback<ClickEvent, List<ObjectCategory>>(FilterClothingChoice, category.subcategories);
            categories.Add(catButton);
        }
    }


    private void FilterClothingChoice(ClickEvent evt, List<ObjectCategory> categories)
    {
        FilterClothingChoice(categories);
    }

    private void FilterClothingChoice(List<ObjectCategory> categories)
    {
        items.Clear();

        List<ClothingSO> clothes = clothesDB.clothes.FindAll(item => item.clothingCategory.Intersect(categories).Count() > 0);
        foreach (ClothingSO clothing in clothes)
        {
            VisualElement visualElement = new();
            visualElement.name = clothing.name;
            visualElement.AddToClassList("content-group");
            items.Add(visualElement);

            Label label = new();
            label.text = clothing.name;
            label.AddToClassList("group-label");
            visualElement.Add(label);
        }
    }

    private void TriggerThemeCustomisation()
    {

    }

    private void TriggerProportionCustomisation(ClickEvent evt)
    {
        TriggerProportionCustomisation();
    }


    private void TriggerProportionCustomisation()
    {
        items.Clear();
        categories.Clear();

        List<TransformGroups> transformGroups = customizer.GetTransformGroups();
        foreach (TransformGroups group in transformGroups)
        {
            VisualElement visualElement = new();
            visualElement.name = group.name;
            visualElement.AddToClassList("content-group-vertical");
            items.Add(visualElement);

            Label label = new();
            label.text = group.name;
            label.AddToClassList("group-label");
            visualElement.Add(label);

            Slider x = new();
            x.name = "X";
            x.label = "X";
            x.value = group.weightX;
            x.lowValue = -0.01f;
            x.highValue = 0.01f;
            x.showInputField = false;
            visualElement.Add(x);

            Slider y = new();
            y.name = "Y";
            y.label = "Y";
            y.value = group.weightY;
            y.lowValue = -0.01f;
            y.highValue = 0.01f;
            y.showInputField = false;
            visualElement.Add(y);

            Slider z = new();
            z.name = "Z";
            z.label = "Z";
            z.value = group.weightZ;
            z.lowValue = -0.01f;
            z.highValue = 0.01f;
            z.showInputField = false;
            visualElement.Add(z);

            x.RegisterValueChangedCallback(evt => group.SetTransformerWeights(evt.newValue, y.value, z.value));
            y.RegisterValueChangedCallback(evt => group.SetTransformerWeights(x.value, evt.newValue, z.value));
            z.RegisterValueChangedCallback(evt => group.SetTransformerWeights(x.value, y.value, evt.newValue));
        }
    }

    private void TriggerTraitsCustomisation()
    {

    }

    public void Hide()
    {
        root.style.visibility = Visibility.Hidden;
    }
}
