using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class FileDataHandler
{
    private string dataDirPath = "";

    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public WorldSaveData Load()
    {
        //find path
        string fullPath = Path.Combine(dataDirPath, dataFileName);
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
                Debug.LogError($"An error occured while loading data to file: {fullPath}\n{e}");
            }
        }
        return loadedData;
    }

    public void Save(WorldSaveData data)
    {
        //create path
        string fullPath = Path.Combine(dataDirPath, dataFileName);
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
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occured while saving data to file: {fullPath}\n{e}");
        }
    }
}
