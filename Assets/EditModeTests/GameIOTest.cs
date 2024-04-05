using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using static UnityEditor.PlayerSettings;

/// <summary>
/// tests the <see cref="GameIO"/> class
/// </summary>
public class GameIOTest
{
    ObjectManager manager;
    GameIO io;
    GameObject player;
    Item item;
    Bot bot;


    /// <summary>
    /// sets up values for testing
    /// </summary>
    [SetUp]
    public void Setup()
    {
        //get a reference to the object manager
        manager = GameObject.FindObjectOfType<ObjectManager>();

        //get a reference to the player
        player = GameObject.Find("Player");

        //get a reference to the io class
        io = GameObject.FindObjectOfType<GameIO>();

        //clear out the object manager for a clean slate to work from
        manager.ClearLevelObjects();

        //set up item and bot
        item = manager.AddItem(new Vector3(0,1,0)).GetComponent<Item>();
        bot = manager.AddBot(new Vector3(0, -1, 0)).GetComponent<Bot>();

        //ensure the player is at the origin
        player.transform.position = Vector3.zero;
    }
    
    /// <summary>
    /// tests <see cref="GameIO.SaveToFile(ObjectManager.ObjectStructHolder)"/>
    /// </summary>
    [Test]
    public void TestSaveBasicScene()
    {
        //Set up a basic struct holder that only contains the player
        ObjectManager.ObjectStruct objectStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.PLAYER, Vector3.zero);
        ObjectManager.ObjectStructHolder structHolder = new ObjectManager.ObjectStructHolder(new List<ObjectManager.ObjectStruct> { objectStruct });

        //save the structure to a file
        io.SaveToFile(structHolder);

        //attempt to read the contents of the file to a string
        string fileContents = "";
        try
        {
            StreamReader sr = new StreamReader(io.GetFilePath());
            fileContents = sr.ReadToEnd();
            sr.Close();
        }
        catch(Exception e)
        {
            //if reading the file fails, fail the method
            Assert.Fail(e.Message);
        }

        //assert that the file string is correct
        Assert.AreEqual("{\"objects\":[{\"objectType\":0,\"position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0}}]}", fileContents);
    }

    /// <summary>
    /// tests <see cref="GameIO.SaveToFile(ObjectManager.ObjectStructHolder)"/>
    /// </summary>
    [Test]
    public void TestSaveFullScene()
    {
        //create an object struct holder with a player, and item, and a bot
        ObjectManager.ObjectStruct objectStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.PLAYER, Vector3.zero);
        ObjectManager.ObjectStruct itemStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.ITEM, item.transform.position);
        ObjectManager.ObjectStruct botStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.BOT, bot.transform.position);
        ObjectManager.ObjectStructHolder structHolder = new ObjectManager.ObjectStructHolder(new List<ObjectManager.ObjectStruct> { objectStruct, itemStruct, botStruct });

        //save the struct holder to the file
        io.SaveToFile(structHolder);


        //attempt to read the contents of the file to a string
        string fileContents = "";
        try
        {
            StreamReader sr = new StreamReader(io.GetFilePath());
            fileContents = sr.ReadToEnd();
            sr.Close();
        }
        catch (Exception e)
        {
            //if reading the file fails, fail the method
            Assert.Fail(e.Message);
        }

        //assert that the file string is correct
        Assert.AreEqual("{\"objects\":[{\"objectType\":0,\"position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0}},{\"objectType\":2,\"position\":{\"x\":0.0,\"y\":1.0,\"z\":0.0}},{\"objectType\":1,\"position\":{\"x\":0.0,\"y\":-1.0,\"z\":0.0}}]}", fileContents);
    }

    /// <summary>
    /// tests <see cref="GameIO.SaveToFile(ObjectManager.ObjectStructHolder)"/>
    /// </summary>
    [Test]
    public void TestSaveNull()
    {
        //prepare to receive the error message for giving a null list
        LogAssert.Expect(LogType.Error, "Could not save file: List was null");

        //ensure that attempting to save an empty struct holder returns false
        Assert.IsFalse(io.SaveToFile(new ObjectManager.ObjectStructHolder()));
    }

    /// <summary>
    /// tests <see cref="GameIO.ReadFromFile(out ObjectManager.ObjectStructHolder)"/>
    /// </summary>
    [Test]
    public void TestLoadBasicScene()
    {
        //Set up a basic struct holder that only contains the player
        ObjectManager.ObjectStruct objectStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.PLAYER, Vector3.zero);
        ObjectManager.ObjectStructHolder structHolder = new ObjectManager.ObjectStructHolder(new List<ObjectManager.ObjectStruct> { objectStruct });

        //save the structure to a file
        io.SaveToFile(structHolder);

        //empty the struct holder in preparation for receiving data from the file
        structHolder = new ObjectManager.ObjectStructHolder();

        //try to read data from the file
        try
        {
            Assert.IsTrue(io.ReadFromFile(out structHolder));
        }
        catch(Exception e)
        {
            //if it fails to read data, fail the test
            Assert.Fail(e.Message);
        }

        //Ensure that there is only one struct in the holder
        Assert.AreEqual(1, structHolder.objects.Count);

        //ensure that one struct is a player struct
        Assert.AreEqual(ObjectManager.ObjectType.PLAYER, structHolder.objects[0].objectType);

        //ensure it has stored the position properly
        Assert.AreEqual(Vector3.zero, structHolder.objects[0].position);

    }

    /// <summary>
    /// tests <see cref="GameIO.ReadFromFile(out ObjectManager.ObjectStructHolder)"/>
    /// </summary>
    [Test]
    public void TestLoadFullScene()
    {
        //create an object struct holder with a player, and item, and a bot
        ObjectManager.ObjectStruct objectStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.PLAYER, Vector3.zero);
        ObjectManager.ObjectStruct itemStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.ITEM, item.transform.position);
        ObjectManager.ObjectStruct botStruct = new ObjectManager.ObjectStruct(ObjectManager.ObjectType.BOT, bot.transform.position);
        ObjectManager.ObjectStructHolder structHolder = new ObjectManager.ObjectStructHolder(new List<ObjectManager.ObjectStruct> { objectStruct, itemStruct, botStruct });

        //save the struct holder to the file
        io.SaveToFile(structHolder);

        //empty the struct holder in preparation for receiving data from the file
        structHolder = new ObjectManager.ObjectStructHolder();

        //try to read data from the file
        try
        {
            Assert.IsTrue(io.ReadFromFile(out structHolder));
        }
        catch (Exception e)
        {
            //if it fails to read data, fail the test
            Assert.Fail(e.Message);
        }

        //ensure that there are three structs in the holder
        Assert.AreEqual(3, structHolder.objects.Count);

        //the first entry should be the player
        Assert.AreEqual(ObjectManager.ObjectType.PLAYER, structHolder.objects[0].objectType);
        //check its position
        Assert.AreEqual(Vector3.zero, structHolder.objects[0].position);

        //the second entry should be the item
        Assert.AreEqual(ObjectManager.ObjectType.ITEM, structHolder.objects[1].objectType);
        //check its position
        Assert.AreEqual(new Vector3(0,1,0), structHolder.objects[1].position);

        //the third entry should be the bot
        Assert.AreEqual(ObjectManager.ObjectType.BOT, structHolder.objects[2].objectType);
        //check its position
        Assert.AreEqual(new Vector3(0, -1, 0), structHolder.objects[2].position);
    }

    /// <summary>
    /// tests <see cref="GameIO.ReadFromFile(out ObjectManager.ObjectStructHolder)"/>
    /// </summary>
    [Test]
    public void TestReadFromNull()
    {
        //Delete the save file
        System.IO.File.Delete(io.GetFilePath());

        //prepare to receive the error message for reading from a nonexistent file
        LogAssert.Expect(LogType.Error, "file does not exist at filepath " + io.GetFilePath());

        //Setup the structholder to be empty and ready to receive data
        ObjectManager.ObjectStructHolder structHolder = new ObjectManager.ObjectStructHolder();

        //ensure that reading from a nonexistent file returns false
        Assert.IsFalse(io.ReadFromFile(out structHolder));

        //ensure that the struct holder is still empty
        Assert.IsNull(structHolder.objects);
    }
}
