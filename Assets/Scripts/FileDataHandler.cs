using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class FileDataHandler
{
    private string dataDirPath = "";

    private string dataFileName = "";

    private readonly string backupExtension = ".bak";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public WorldSaveData Load(string profileId, bool allowRestoreForBackup = true)
    {
        if (profileId == null)
        {
            Debug.LogError("profileId is null!");
            return null;
        }    
        
        //find path
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        WorldSaveData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<WorldSaveData>(dataToLoad);
            }
            catch (Exception e)
            {
                if (allowRestoreForBackup)
                {
                    Debug.LogWarning($"An error occured while loading data to file: {fullPath}. Attempting to roll back.\n{e}");
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess)
                    {
                        loadedData = Load(profileId, false);
                    }
                }
                else
                {
                    Debug.LogError($"An error occured while loading data to file: {fullPath} and backup has failed.\n{e}");
                }
            }
        }
        return loadedData;
    }

    public Dictionary<string, WorldSaveData> LoadAllProfiles()
    {
        Dictionary<string, WorldSaveData> profileDictionary = new Dictionary<string, WorldSaveData>();
        
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileId = dirInfo.Name;

            //make sure that the folder is for storing data
            string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Skipping directory when loading all profiles because it does not contain the data {profileId}");
                continue;
            }

            WorldSaveData profileData = Load(profileId);

            if (profileData != null)
            {
                profileDictionary.Add(profileId, profileData);
            }
            else
            {
                Debug.LogError($"Failed to load profile {profileId}");
            }
        }

        return profileDictionary;
    }

    public void Delete(string profileId)
    {
        if (profileId == null)
        {
            Debug.LogError("profileId is null!");
            return;
        }

        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        try
        {
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.LogWarning("Attempted to delete profile data that did not exist: {fullPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occured while deleting file: {fullPath}\n{e}");
        }
    }

    public void Save(WorldSaveData data, string profileId)
    {
        if (profileId == null)
        {
            Debug.LogError("profileId is null!");
            return;
        }

        //create path
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        string backupFilePath = fullPath + backupExtension;
        try
        {
            //create the file directory
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //serialize
            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            //verify the data to check for corruption
            WorldSaveData verifiedData = Load(profileId);
            //if verified, back up
            if (verifiedData != null)
            {
                File.Copy(fullPath, backupFilePath, true);
            }
            //else, throw exception
            else
            {
                throw new Exception("The file could not be verified and a backup could not be created.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occured while saving data to file: {fullPath}\n{e}");
        }
    }

    private bool AttemptRollback(string fullPath)
    {
        bool success = false;
        string backupFilePath = fullPath + backupExtension;

        try
        {
            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, fullPath, true);
                success = true;
                Debug.LogWarning($"File was corrupted, rolling back file at {backupFilePath}");
            }
            else
            {
                throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error occured when trying to roll back to backup file at {backupFilePath} \n{e}");
        }

        return success;
    }
}
