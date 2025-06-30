using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TaskbarController : MonoBehaviour
{
    [SerializeField]
    private TimeManager timeManager;
    [SerializeField]
    private MainMenuController uiController;

    [SerializeField]
    private UIDocument MainMenu;
    [SerializeField]
    private PlacementSystem placementSystem;
    [SerializeField]
    private SettingsController settingsController;

    public Button buildButton;
    public Button mapButton;

    public VisualElement MainMenuRoot;

    // Start is called before the first frame update
    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        //for MainMenu
        MainMenuRoot = MainMenu.rootVisualElement;

        buildButton = MainMenuRoot.Q<Button>("BuildMode");
        mapButton = MainMenuRoot.Q<Button>("MapMode");

        buildButton.ClearBindings();
        mapButton.ClearBindings();

        buildButton.clicked += BuildButtonPressed;
        mapButton.clicked += MapButtonPressed;
    }

    private void BuildButtonPressed()
    {
        uiController.ExitMainEvents.Invoke();
        uiController.EnterBuildModeEvents.Invoke();
    }

    private void MapButtonPressed()
    {
        placementSystem.GoToMainMap();
    }

    public void SetMainMenu()
    {
        MainMenu.rootVisualElement.style.visibility = Visibility.Visible;
        Setup();
    }

    public void Hide()
    {
        MainMenu.rootVisualElement.style.visibility = Visibility.Hidden;
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
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            MainMenuRoot.style.marginRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
        }
        MainMenuRoot.style.marginBottom = Screen.height - Screen.safeArea.height;
    }

    private void Update()
    {
        AccountForSafeArea();
    }
}
