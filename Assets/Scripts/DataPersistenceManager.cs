using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    
    public static DataPersistenceManager instance { get; private set; }
    private List<IDataPersistence> dataPersistenceObjects;
    private WorldSaveData gameData;
    private FileDataHandler dataHandler;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new WorldSaveData();
    }

    public void LoadGame()
    {
        //TODO - load any saved data from a file using a data handler
        this.gameData = dataHandler.Load();

        //if there is no data to load, initialize a new game
        if (this.gameData == null)
        {
            Debug.Log("No data was found. Initializing a new game...");
            NewGame();
        }
        
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        //Pass the data to other scripts so it can update
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }

        //Save the data to a file using a data handler
        dataHandler.Save(gameData);
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
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
