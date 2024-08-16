using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuObject;
    [SerializeField]
    private GameObject settingsBarObject;
    [SerializeField]
    private GameObject settingsMenuObject;

    private UIDocument MainMenu;  private UIDocument SettingsBar;
    [SerializeField]
    private PlacementSystem placementSystem;
    [SerializeField]
    private SettingsController settingsController;

    public Button buildButton;
    public Button mapButton;
    public Button settingsButton;

    public VisualElement MainMenuRoot; public VisualElement SettingsRoot;

    // Start is called before the first frame update
    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        MainMenu = mainMenuObject.GetComponent<UIDocument>(); SettingsBar = settingsBarObject.GetComponent<UIDocument>();

        //for MainMenu
        MainMenuRoot = MainMenu.rootVisualElement;
        buildButton = MainMenuRoot.Q<Button>("BuildMode");
        mapButton = MainMenuRoot.Q<Button>("MapMode");

        //for SettingsBar
        SettingsRoot = SettingsBar.rootVisualElement;
        settingsButton = SettingsRoot.Q<Button>("Settings");

        buildButton.clicked += BuildButtonPressed;
        mapButton.clicked += MapButtonPressed;
        settingsButton.clicked += InvokeSettings;
    }

    private void BuildButtonPressed()
    {
        mainMenuObject.SetActive(false);
        placementSystem.EnterBuildMode();
    }

    private void MapButtonPressed()
    {
        placementSystem.GoToMainMap();
    }

    private void InvokeSettings()
    {
        mainMenuObject.SetActive(false);
        settingsBarObject.SetActive(false);
        settingsMenuObject.SetActive(true);
        settingsController.InvokeSettings();
    }

    public void SetMainMenu()
    {
        mainMenuObject.SetActive(true);
        settingsBarObject.SetActive(true);
        Setup();
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
            MainMenuRoot.style.marginRight = hotX();
            SettingsRoot.style.marginRight = hotX();
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            MainMenuRoot.style.marginRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
            SettingsRoot.style.marginRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
        }
        MainMenuRoot.style.marginBottom = Screen.height - Screen.safeArea.height;
    }

    private void Update()
    {
        AccountForSafeArea();
    }
}
