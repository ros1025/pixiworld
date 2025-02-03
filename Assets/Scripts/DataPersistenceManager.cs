using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debug Mode")]
    [SerializeField] private bool initialiseDataWhenNull;
    [SerializeField] private bool disableDataPersistence;
    [SerializeField] private bool overrideProfileId;
    [SerializeField] private string testSelectedProfileId;


    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    [Header("Auto Saving Configuration")]
    [SerializeField] private float autoSaveCoroutineTime = 60f;

    public static DataPersistenceManager instance { get; private set; }
    private List<IDataPersistence> dataPersistenceObjects;
    private WorldSaveData gameData;
    private FileDataHandler dataHandler;
    private Coroutine autoSaveCoroutine;
    private string currentProfileId = "";

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;

        if (disableDataPersistence)
        {
            Debug.LogWarning("Data persistence is disabled in this mode!");
        }

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        InitialiseProfileId();
    }

    private void InitialiseProfileId()
    {
        this.currentProfileId = GetMostRecentlyUpdatedProfileId();
        if (overrideProfileId)
        {
            this.currentProfileId = testSelectedProfileId;
            Debug.LogWarning($"You are currently overriding the ID with the test ID: {testSelectedProfileId}");
        }
    }

    public void NewGame()
    {
        currentProfileId = GenerateNewId();
        this.gameData = new WorldSaveData();
        Debug.Log(gameData);
    }

    public void LoadGame()
    {
        if (disableDataPersistence)
        {
            return;
        }
        
        //TODO - load any saved data from a file using a data handler
        this.gameData = dataHandler.Load(currentProfileId);

        //if there is no data to load, return
        if (this.gameData == null)
        {
            if (initialiseDataWhenNull)
            {
                NewGame();
            }
            else
            {
                Debug.Log("No data was found. A new game needs to be started before data can be loaded.");
                return;
            }
        }
        
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        if (disableDataPersistence)
        {
            return;
        }

        //if there is no data to save, return
        if (this.gameData == null)
        {
            Debug.Log("No data was found. A new game needs to be started before data can be saved.");
            return;
        }

        //Pass the data to other scripts so it can update
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }

        gameData.lastSaveDate = DateTime.Now.ToBinary();

        //Save the data to a file using a data handler
        dataHandler.Save(gameData, currentProfileId);
    }

    public void DeleteGameProfile(string profileId)
    {
        dataHandler.Delete(profileId);

        InitialiseProfileId();

        LoadGame();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        //SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        //SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();

        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
        autoSaveCoroutine = StartCoroutine(AutoSave());
    }

    public void ChangeSelectedProfileId(string newProfileId)
    {
        this.currentProfileId = newProfileId;
        LoadGame();
    }

    private void OnApplicationPause()
    {
        SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public string GetMostRecentlyUpdatedProfileId()
    {
        string mostRecentId = null;
        Dictionary<string, WorldSaveData> profilesGameData = dataHandler.LoadAllProfiles();

        foreach (KeyValuePair<string, WorldSaveData> pair in profilesGameData)
        {
            string profileId = pair.Key;
            WorldSaveData saveData = pair.Value;

            //skip if null
            if (saveData == null)
            {
                continue;
            }

            //if found
            if (mostRecentId == null)
            {
                mostRecentId = profileId;
            }
            else
            {
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGameData[mostRecentId].lastSaveDate);
                DateTime newDateTime = DateTime.FromBinary(profilesGameData[profileId].lastSaveDate);

                if (newDateTime > mostRecentDateTime)
                {
                    mostRecentId = profileId;
                }
            }
        }
        return mostRecentId;
    }

    public string GenerateNewId()
    {
        int newId = 0;
        Dictionary<string, WorldSaveData>  data = GetAllProfilesGameData();
        List<string> ids = data.Keys.ToList();
        while (ids.FindIndex(str => str == newId.ToString()) != -1)
        {
            newId++;
        }
        return newId.ToString();
    }

    public bool HasGameData()
    {
        return gameData != null;
    }

    public Dictionary<string, WorldSaveData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveCoroutineTime);
            SaveGame();
            Debug.Log("Auto Saving Configurator...");
        }
    }
}
