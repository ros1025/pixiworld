using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AdaptivePerformance;

public class SettingsController : MonoBehaviour
{
    [SerializeField]
    private UIDocument SettingsMenu;
    [SerializeField]
    private MainMenuController mainController;
    [SerializeField]
    private ModifySettings settings;
    [SerializeField]
    private List<Texture> buttonImages;

    VisualElement root; VisualElement container; VisualElement homebar;
    Button graphicsToggle; Button audioToggle; Button progressToggle; Button exitSettings;

    public void InvokeSettings()
    {
        root = SettingsMenu.rootVisualElement;
        container = root.Q<VisualElement>("Content").Q<VisualElement>("unity-content-container");
        homebar = root.Q<VisualElement>("TopBarContainer");
        container.Clear();

        graphicsToggle = homebar.Q<Button>("GraphicsSettings");
        audioToggle = homebar.Q<Button>("AudioSettings");
        progressToggle = homebar.Q<Button>("ProgressSettings");
        exitSettings = root.Q<Button>("Cancel");
        graphicsToggle.AddToClassList("selected-bar-control");

        graphicsToggle.RegisterCallback<ClickEvent, int>(UpdateMenus, 0);
        audioToggle.RegisterCallback<ClickEvent, int>(UpdateMenus, 1);
        progressToggle.RegisterCallback<ClickEvent, int>(UpdateMenus, 2);
        exitSettings.clicked += ExitSettings;

        InvokeGraphicsSettings();
    }

    void InvokeGraphicsSettings()
    {
        List<string> QualityLevels = new();
        for (int i = 0; i < QualitySettings.names.Length; i++)
            QualityLevels.Add(QualitySettings.names[i]);
        MakeDropdownBar("graphics_qualitylevel", "Graphics Quality Level", QualitySettings.GetQualityLevel(), QualityLevels, settings.SetQualityLevel);

        settings.SetupResolutions();
        MakeIntDropdownBar("graphics_renderresolution", "Render Resolution", settings.Resolutions.IndexOf(settings.currentRes), settings.Resolutions, "p", settings.SetRenderingResolution);

        List<string> PerformanceModes = new List<string> { "Performance", "Optimised", "Standard", "Battery" };
        MakeDropdownBar("graphics_performancemodes", "Performance Mode", settings.DetermineCurrentPerformanceMode(), PerformanceModes, settings.SetPerformanceMode);

        MakeIntSliderBar("graphics_targetfps", "Target FPS", settings.DetermineTargetFPS(), 24, 166, " FPS", settings.SetTargetFPS);
    }

    void InvokeProgressSettings()
    {
        MakeButton("progress_quit", "Exit", buttonImages.Find(button => button.name == "power"), ExitGame);
    }

    public void ExitSettings()
    {
        container.Clear();
        gameObject.SetActive(false);
        mainController.SetMainMenu();
    }

    public void ExitGame()
    {
        container.Clear();
        gameObject.SetActive(false);
        Application.Quit();
    }

    void UpdateMenus(ClickEvent evt, int mode)
    {
        container.Clear();
        var button = evt.target as Button;
        foreach (Button b in homebar.Q<VisualElement>("unity-content-container").Children())
            if (b.ClassListContains("selected-bar-control"))
                b.RemoveFromClassList("selected-bar-control");
        button.AddToClassList("selected-bar-control");

        if (mode == 0) InvokeGraphicsSettings();
        if (mode == 2) InvokeProgressSettings();
    }

    void MakeDropdownBar(string name, string label, int defaultChoice, List<string> options, EventCallback<ChangeEvent<string>, List<string>> method)
    {
        VisualElement box = new VisualElement();
        box.name = name;
        box.AddToClassList("list-object");
        container.Add(box);

        DropdownField field = new DropdownField();

        field.label = label;
        field.choices = options;
        field.index = defaultChoice;
        box.Add(field);

        field.RegisterCallback(method, options);
    }

    void MakeIntDropdownBar(string name, string label, int defaultChoice, List<int> options, string units, EventCallback<ChangeEvent<string>, List<int>> method)
    {
        List<string> optionsStr = new();
        foreach (int item in options)
            optionsStr.Add($"{item}{units}");

        VisualElement box = new VisualElement();
        box.name = name;
        box.AddToClassList("list-object");
        container.Add(box);

        DropdownField field = new DropdownField();
        field.label = label;
        field.choices = optionsStr;
        field.index = defaultChoice;
        box.Add(field);

        field.RegisterCallback(method, options);
    }

    void MakeIntSliderBar(string name, string label, int defaultChoice, int minValue, int maxValue, string units, EventCallback<ChangeEvent<int>, Label> method)
    {
        VisualElement box = new VisualElement();
        box.name = name;
        box.AddToClassList("list-object");
        container.Add(box);

        SliderInt slider = new SliderInt();
        slider.label = label;
        slider.lowValue = minValue;
        slider.highValue = maxValue;
        slider.value = defaultChoice;
        box.Add(slider);

        Label valueDisplay = new Label();
        valueDisplay.text = $"{defaultChoice}{units}";
        valueDisplay.AddToClassList("slider-value");
        slider.Add(valueDisplay);

        slider.RegisterCallback(method, valueDisplay);
    }

    void MakeButton(string name, string label, Texture sprite, System.Action method)
    {
        Button button = new Button();
        button.name = name;
        button.text = label;
        button.AddToClassList("settings-options-button");
        container.Add(button);

        Image image = new Image();
        image.image = sprite;
        image.AddToClassList("settings-options-button__image");
        button.Add(image);

        button.clicked += method;
    }

    void MakeToggleButton(string name, string label, bool defaultChoice, EventCallback<ChangeEvent<bool>> method)
    {
        VisualElement box = new VisualElement();
        box.name = name;
        box.AddToClassList("list-object");
        container.Add(box);

        Toggle toggle = new Toggle();
        toggle.label = label;
        toggle.value = defaultChoice;
        box.Add(toggle);

        toggle.RegisterCallback(method);
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
            homebar.style.marginLeft = 30 + Screen.safeArea.x;
            //homebar.style.marginRight = 30 + hotX();
            //root.Q<VisualElement>("Content").style.marginLeft = 30 + Screen.safeArea.x;
            root.Q<VisualElement>("Content").style.marginRight = 30 + hotX();
            exitSettings.style.marginRight = 30 + hotX();
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            homebar.style.marginLeft = 30 + hotX();
            //homebar.style.marginRight = 30 + Screen.width - Screen.safeArea.width;
            //root.Q<VisualElement>("Content").style.marginLeft = 30 + hotX();
            root.Q<VisualElement>("Content").style.marginRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
            exitSettings.style.marginRight = 30 + Screen.width - Screen.safeArea.width - Screen.safeArea.x;
        }
        homebar.style.marginBottom = 30 + Screen.safeArea.y;
        root.Q<VisualElement>("Content").style.marginBottom = 30 + Screen.safeArea.y;
    }

    public void UpdateValues(string change, List<string> optionsStr)
    {
        if (change == "renderres")
        {
            container.Q<VisualElement>("graphics_renderresolution").Q<DropdownField>().choices = optionsStr;
            container.Q<VisualElement>("graphics_renderresolution").Q<DropdownField>().index = settings.Resolutions.IndexOf(settings.currentRes);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ExitSettings();
        if (graphicsToggle.ClassListContains("selected-bar-control"))
            root.Q<VisualElement>("graphics_targetfps").visible = settings.DetermineFPSSliderAvailability();
        AccountForSafeArea();
        settings.UpdateResValues();
    }

    void OnApplicationQuit()
    {
        if (gameObject.activeInHierarchy == true)
            container.Clear();
    }
}