using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameIO : MonoBehaviour
{
    [SerializeField, Tooltip("the name of the file to save")] private string savedFileName = "stored_level";
    public bool SaveToFile(List<ObjectManager.ObjectStruct> objectStructs)
    {
        if (objectStructs == null)
        {
            Debug.LogError("Could not save file: List was null");
            return false;
        }
        string filepath = Application.persistentDataPath + "/" + savedFileName;
        string js = JsonUtility.ToJson(objectStructs);
        try
        {
            StreamWriter sw = new StreamWriter(filepath, false);
            sw.Write(js);
            sw.Close();
        }
        catch (Exception e)
        {
            Debug.Log("Failure writing to file: " + e.Message);
            return false;
        }
        return true;
    }

    public bool ReadFromFile(out List<ObjectManager.ObjectStruct> objectStructs)
    {
        objectStructs = null;
        string filepath = Application.persistentDataPath + "/" + savedFileName;
        if (!System.IO.File.Exists(filepath))
        {
            Debug.LogWarning("file does not exist at filepath " + filepath);
            return false;
        }
        string js;
        try
        {
            StreamReader sr = new StreamReader(filepath);
            js = sr.ReadToEnd();
            sr.Close();
        }
        catch (Exception e)
        {
            Debug.Log("Failure reading from file: " + e.Message);
            return false;
        }
        try
        {
            objectStructs = JsonUtility.FromJson<List<ObjectManager.ObjectStruct>>(js);
        }
        catch (Exception e)
        {
            Debug.Log("Failure converting file from json: " + e.Message);
            return false;
        }

        return true;
    }
}
