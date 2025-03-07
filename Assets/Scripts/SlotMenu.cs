using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SlotMenu : MonoBehaviour
{
    [SerializeField] private UIDocument ui;
    [SerializeField] private GameObject gizmo;
    [SerializeField] private StartMenu start;
    [SerializeField] private Texture2D deleteImage;
    [SerializeField] private Texture2D settingsImage;
    [SerializeField] private Texture2D playimage;
    private ScrollView scroller;

    void Start()
    {
        HideMenu();
    }

    public void HideMenu()
    {
        VisualElement root = ui.rootVisualElement;
        root.style.visibility = Visibility.Hidden;
    }

    private void LoadGame(string profileId)
    {
        DataPersistenceManager.instance.ChangeSelectedProfileId(profileId);
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadSceneAsync("Town");
    }

    private void OnSlotSelected(ClickEvent evt, string profileId)
    {
        LoadGame(profileId);
    }

    private void OnSlotDelete(ClickEvent evt, string profileId)
    {
        GameObject popup = Instantiate(gizmo);
        GizmoConfigurator config = popup.GetComponent<GizmoConfigurator>();
        config.ChangeHeader("Delete Save");
        config.ChangeContent($"Are you sure you want to delete save {profileId}?");
        config.ClearButtons();
        config.AddButton("Yes", () => { 
            ConfirmSlotDelete(profileId);
            CancelGizmo(popup);
        });
        config.AddButton("No", () => CancelGizmo(popup));

    }

    private void ConfirmSlotDelete(string profileId)
    {
        DataPersistenceManager.instance.DeleteGameProfile(profileId);
        LoadSlots();
    }

    private void CancelGizmo(GameObject gizmo)
    {
        Destroy(gizmo);
    }

    private void LoadSlots()
    {
        scroller.Clear();

        Dictionary<string, WorldSaveData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        foreach (string profileId in profilesGameData.Keys)
        {
            if (profilesGameData[profileId] != null)
            {
                CreateSlotElement(profileId, profilesGameData[profileId]);
            }
        }
    }

    private void CreateSlotElement(string profileId, WorldSaveData saveData)
    {
        VisualElement main = new();
        main.AddToClassList("content-bar");
        scroller.Add(main);

        VisualElement pictureFrame = new();
        pictureFrame.AddToClassList("content-picture");
        main.Add(pictureFrame);

        VisualElement labelFrame = new();
        main.Add(labelFrame);

        Label title = new();
        title.text = profileId;
        title.AddToClassList("save-name");
        labelFrame.Add(title);

        Label desc = new();
        desc.text = 
            $"Last Saved:\n{System.DateTime.FromBinary(saveData.lastSaveDate).ToShortDateString()} {System.DateTime.FromBinary(saveData.lastSaveDate).ToLongTimeString()}";
        desc.AddToClassList("save-description");
        labelFrame.Add(desc);

        VisualElement gameControls = new();
        gameControls.AddToClassList("game-controls");
        main.Add(gameControls);

        Button play = new();
        play.AddToClassList("play-button");
        play.RegisterCallback<ClickEvent, string>(OnSlotSelected, profileId);
        gameControls.Add(play);

        VisualElement playImg = new();
        playImg.style.backgroundImage = Background.FromTexture2D(playimage);
        playImg.style.width = 50f;
        playImg.style.height = 50f;
        play.Add(playImg);

        Button edit = new();
        edit.AddToClassList("edit-button");
        gameControls.Add(edit);

        VisualElement editImg = new();
        editImg.style.backgroundImage = Background.FromTexture2D(settingsImage);
        editImg.style.width = 50f;
        editImg.style.height = 50f;
        edit.Add(editImg);

        Button delete = new();
        delete.AddToClassList("delete-button");
        delete.RegisterCallback<ClickEvent, string>(OnSlotDelete, profileId);
        gameControls.Add(delete);

        VisualElement deleteImg = new();
        deleteImg.style.backgroundImage = Background.FromTexture2D(deleteImage);
        deleteImg.style.width = 50f;
        deleteImg.style.height = 50f;
        delete.Add(deleteImg);
    }

    public void CallMenu()
    {
        VisualElement root = ui.rootVisualElement;
        root.style.visibility = Visibility.Visible;
        scroller = root.Q<VisualElement>("MainBar").Q<ScrollView>();

        Button cancelButton = root.Q<VisualElement>("TopBar").Q<Button>("Cancel");
        cancelButton.RegisterCallback<ClickEvent>(OnExitButtonClicked);
        LoadSlots();
    }

    public void OnExitButtonClicked(ClickEvent evt)
    {
        start.CallMenu();
        HideMenu();
    }
}
