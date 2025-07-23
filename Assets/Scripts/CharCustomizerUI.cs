using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

public class CharCustomizerUI : MonoBehaviour
{
    [SerializeField]
    private UIDocument document;
    [SerializeField]
    private CharacterCustomizer customizer;
    [SerializeField]
    private ClothingDatabaseSO clothesDB;
    [SerializeField]
    private ClothingCategoryDatabaseSO categoriesDB;
    private VisualElement root;
    private ScrollView categories;
    private ScrollView items;
    private Button clothes;
    private Button themes;
    private Button bodyProportions;
    private Button traits;
    private Button hideButton;

    private void Awake()
    {
        root = document.rootVisualElement;
        StartCoroutine(Initialise());
    }

    public IEnumerator Initialise()
    {
        root = document.rootVisualElement;
        root.style.visibility = Visibility.Visible;

        //customizer.InitializeCustomiser();
        yield return new WaitUntil(() => customizer.GetCurrentCharacter() != null);
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

        foreach (ClothingCategorySO category in categoriesDB.clothingCategories)
        {
            Button catButton = new();
            catButton.name = category.name;
            catButton.AddToClassList("category-button");
            catButton.RegisterCallback<ClickEvent, List<ObjectCategory>>(FilterClothingChoice, category.subcategories);
            categories.Add(catButton);
        }

        FilterClothingChoice(categoriesDB.clothingCategories[0].subcategories);
    }


    private void FilterClothingChoice(ClickEvent evt, List<ObjectCategory> categories)
    {
        FilterClothingChoice(categories);
    }

    private void FilterClothingChoice(List<ObjectCategory> categories)
    {
        items.Clear();

        List<ClothingSO> clothes = clothesDB.clothes.FindAll(item => item.clothingCategory.Intersect(categories).Count() > 0);
        List<VisualElement> elements = new();

        foreach (ClothingSO clothing in clothes)
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

        items.scrollOffset = new Vector2(0, 0);
    }

    private void TriggerTraitsCustomisation()
    {

    }

    public void Hide()
    {
        root.style.visibility = Visibility.Hidden;
    }
}
