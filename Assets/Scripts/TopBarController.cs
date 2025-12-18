using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TopBarController : MonoBehaviour
{
    [SerializeField]
    private MainMenuController uiController;
    [SerializeField]
    private UIDocument SettingsBar;
    [SerializeField]
    private SettingsController settingsController;
    public Button settingsButton;

    public VisualElement SettingsRoot;

    // Start is called before the first frame update
    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        //for SettingsBar
        SettingsRoot = SettingsBar.rootVisualElement;
        settingsButton = SettingsRoot.Q<Button>("Settings");

        settingsButton.RegisterCallback<ClickEvent>(evt => InvokeSettings());
    }

    private void InvokeSettings()
    {
        uiController.ExitMainEvents.Invoke();
        uiController.EnterSettingsEvents.Invoke();
    }

    public void Hide()
    {
        SettingsBar.rootVisualElement.style.visibility = Visibility.Hidden;
    }

    public void SetMainMenu()
    {
        SettingsBar.rootVisualElement.style.visibility = Visibility.Visible;
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
            SettingsRoot.style.marginRight = hotX();
        }
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            SettingsRoot.style.marginRight = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
        }
    }

    private void Update()
    {
        AccountForSafeArea();
    }
}
