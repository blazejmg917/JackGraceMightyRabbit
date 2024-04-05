using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class handling writing to and reading from files. Used to save and load levels
/// </summary>
public class GameIO : MonoBehaviour
{
    [SerializeField, Tooltip("the name of the file to save")] private string savedFileName = "stored_level";

    /// <summary>
    /// Saves the given set of <see cref="ObjectManager.ObjectStruct"/>s to the main game save file
    /// </summary>
    /// <param name="objectStructs">the <see cref="ObjectManager.ObjectStructHolder"/> containing all structs to be saved</param>
    /// <returns>whether the file saved succesfully</returns>
    public bool SaveToFile(ObjectManager.ObjectStructHolder objectStructs)
    {
        //null check just to be sure
        if (objectStructs.objects == null)
        {
            Debug.LogError("Could not save file: List was null");
            return false;
        }
        //setup the filepath for the main game save using persistentDataPath. Generally just a good place to save data
        string filepath = Application.persistentDataPath + "/" + savedFileName;
        //convert the object struct holder to a string using JSON
        string js = JsonUtility.ToJson(objectStructs);
        //attempt to write the JSON string to the save file
        try
        {
            StreamWriter sw = new StreamWriter(filepath, false);
            //Debug.Log(js);
            sw.Write(js);
            sw.Close();
        }
        catch (Exception e)
        {
            //if it fails in any way, display error message and return false
            Debug.LogWarning("Failure writing to file: " + e.Message);
            return false;
        }

        //if you got here, it succeeded. Return true
        return true;
    }

    /// <summary>
    /// Reads the data stored in the game save file and creates an <see cref="ObjectManager.ObjectStructHolder"/> to be used in restoring that game state
    /// </summary>
    /// <param name="objectStructs">the <see cref="ObjectManager.ObjectStructHolder"/> that the save data will be passed into</param>
    /// <returns>whether the save was succesfully read or not</returns>
    public bool ReadFromFile(out ObjectManager.ObjectStructHolder objectStructs)
    {
        //create a base struct so there's something to return in case of a failure
        objectStructs = new ObjectManager.ObjectStructHolder();
        //setup the filepath for the save data
        string filepath = Application.persistentDataPath + "/" + savedFileName;
        //if that file doesn't already exist, data cannot be read. return false
        if (!System.IO.File.Exists(filepath))
        {
            Debug.LogError("file does not exist at filepath " + filepath);
            return false;
        }

        /*
         * create the basic string and attempt to read the save file to it.
         * Technically there should probably be some extra encryption/sanitization steps here to prevent problems, 
         * but given that this product will only see a few people, does not store sensitive data, 
         * and is not in a position to cause any real hard it's not a major concern
         */
        string js;
        try
        {
            StreamReader sr = new StreamReader(filepath);
            js = sr.ReadToEnd();
            sr.Close();
        }
        catch (Exception e)
        {
            //if reading from the file failed print error and return false
            Debug.Log("Failure reading from file: " + e.Message);
            return false;
        }

        //try to convert the string back into an object holder using JSON converter
        try
        {
            objectStructs = JsonUtility.FromJson<ObjectManager.ObjectStructHolder>(js);
        }
        catch (Exception e)
        {
            //if JSON conversion fails print error and return false
            Debug.Log("Failure converting file from json: " + e.Message);
            return false;
        }

        //if you made it here the file should be all good to go, so return true
        return true;
    }

    /// <summary>
    /// Gets the full filepath of the file that saved games are saved to
    /// </summary>
    /// <returns>the file where saved games are stored</returns>
    public string GetFilePath()
    {
        return Application.persistentDataPath + "/" + savedFileName;
    }
}
