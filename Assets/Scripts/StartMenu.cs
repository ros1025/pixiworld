using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private UIDocument ui;
    [SerializeField] private SlotMenu slotMenu;
    private Button newGame;
    private Button resumeGame;
    private Button loadGame;
    private Button settings;
    
    public void Start()
    {
        CallMenu();
    }

    public void OnNewGameClicked(ClickEvent evt)
    {
        CreateNewGame();
    }

    public void CreateNewGame()
    {
        DataPersistenceManager.instance.NewGame();
        SaveGameAndLoadScene();
    }

    public void OnResumeGameClicked(ClickEvent evt)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        SaveGameAndLoadScene();
    }

    public void OnLoadGameClicked(ClickEvent evt)
    {
        slotMenu.CallMenu();
        HideMenu();
    }

    private void SaveGameAndLoadScene()
    {
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadSceneAsync("Town");
    }

    public void DisableMenuButtons()
    {
        newGame.SetEnabled(false);
        resumeGame.SetEnabled(false);
        loadGame.SetEnabled(false);
        settings.SetEnabled(false);
    }

    public void EnableMenuButtons()
    {
        newGame.SetEnabled(true);
        resumeGame.SetEnabled(true);
        loadGame.SetEnabled(true);
        settings.SetEnabled(true);
    }

    public void HideMenu()
    {
        VisualElement root = ui.rootVisualElement;
        root.visible = false;
        root.SetEnabled(false);
    }

    public void CallMenu()
    {
        VisualElement root = ui.rootVisualElement;
        root.SetEnabled(true);
        root.visible = true;
        VisualElement mainBar = root.Q<VisualElement>("MainBar");

        newGame = mainBar.Q<Button>("NewGame");
        resumeGame = mainBar.Q<Button>("ResumeGame");
        loadGame = mainBar.Q<Button>("LoadGame");
        settings = mainBar.Q<Button>("Settings");

        newGame.RegisterCallback<ClickEvent>(OnNewGameClicked);
        if (DataPersistenceManager.instance.HasGameData())
        {
            resumeGame.RegisterCallback<ClickEvent>(OnResumeGameClicked);
            loadGame.RegisterCallback<ClickEvent>(OnLoadGameClicked);
        }
        else
        {
            resumeGame.SetEnabled(false);
            loadGame.SetEnabled(false);
        }
    }
}
