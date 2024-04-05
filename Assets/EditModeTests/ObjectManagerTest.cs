using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

/// <summary>
/// tests the <see cref="ObjectManager"/> class
/// </summary>
public class ObjectManagerTest
{
    Player player;
    ObjectManager manager;
    Item item;
    Bot bot;
    Transform itemHolder;
    Transform botHolder;
    GameIO io;

    /// <summary>
    /// sets up values for testing
    /// </summary>
    [SetUp]
    public void Setup()
    {
        //get a reference to the object manager
        manager = GameObject.FindObjectOfType<ObjectManager>();

        //get a reference to the player
        player = GameObject.Find("Player").GetComponent<Player>();

        //get a reference to the io class
        io = GameObject.FindObjectOfType<GameIO>();

        //get a reference to both holders
        itemHolder = GameObject.Find("Item Holder").transform;
        botHolder = GameObject.Find("Bot Holder").transform;

        //clear out the object manager for a clean slate to work from
        manager.ClearLevelObjects();
        player.Reset();

        //set up item and bot
        item = manager.AddItem(new Vector3(0, 1, 0)).GetComponent<Item>();
        bot = manager.AddBot(new Vector3(0, -2, 0)).GetComponent<Bot>();

        //ensure the player is at the origin
        player.transform.position = Vector3.zero;
    }

    /// <summary>
    /// tests <see cref="ObjectManager.FindAllObjects"/>
    /// </summary>
    [Test]
    public void TestFindObjects()
    {
        //first, clear out the existing objects
        manager.ClearLevelObjects();

        //then create one base level object, one item, and one bot
        GameObject levelObject = new GameObject("LevelObject");
        levelObject.AddComponent<LevelObject>();

        GameObject itemObject = new GameObject("Item");
        itemObject.AddComponent<Item>();

        GameObject botObject = new GameObject("Bot");
        botObject.AddComponent<Bot>();

        //tell the manager to find all objects
        manager.FindAllObjects();

        //assert that all 3 were found
        List<GameObject> objects = manager.GetLevelObjects();
        Assert.AreEqual(3, objects.Count);
        Assert.IsTrue(objects.Contains(levelObject));
        Assert.IsTrue(objects.Contains(itemObject));
        Assert.IsTrue(objects.Contains(botObject));
    }

    /// <summary>
    /// tests <see cref="ObjectManager.AddItem(Vector3)"/>
    /// </summary>
    [Test]
    public void TestSpawnItem()
    {
        //spawn a new item
        GameObject newItem = manager.AddItem(new Vector3(-77, 0, 13.54f));

        //check its position
        Assert.AreEqual(new Vector3(-77, 0, 13.54f), newItem.transform.position);

        //check its name
        Assert.AreEqual("Item", newItem.name);

        //check its parent
        Assert.AreEqual(itemHolder, newItem.transform.parent);

    }

    /// <summary>
    /// tests <see cref="ObjectManager.AddBot(Vector3)"/>
    /// </summary>
    [Test]
    public void TestSpawnBot()
    {
        //spawn a new bot
        GameObject newBot = manager.AddBot(new Vector3(-77, 0, 13.54f));

        //check its position
        Assert.AreEqual(new Vector3(-77, 0, 13.54f), newBot.transform.position);

        //check its name
        Assert.AreEqual("Bot", newBot.name);

        //check its parent
        Assert.AreEqual(botHolder, newBot.transform.parent);

    }

    /// <summary>
    /// tests <see cref="ObjectManager.AddRandomItem"/>
    /// </summary>
    [Test]
    public void TestSpawnRandomItem()
    {
        //spawn a new item
        GameObject newItem = manager.AddRandomItem();

        //check it was spawned within max radius
        Assert.LessOrEqual(Vector3.Distance(newItem.transform.position, manager.GetSpawnOrigin()), manager.GetSpawnRadius());

        //check its name
        Assert.AreEqual("Item", newItem.name);

        //check its parent
        Assert.AreEqual(itemHolder, newItem.transform.parent);

    }

    /// <summary>
    /// tests <see cref="ObjectManager.AddRandomBot"/>
    /// </summary>
    [Test]
    public void TestSpawnRandomBot()
    {
        //spawn a new bot
        GameObject newBot = manager.AddRandomBot();

        //check it was spawned within max radius
        Assert.LessOrEqual(Vector3.Distance(newBot.transform.position, manager.GetSpawnOrigin()), manager.GetSpawnRadius());

        //check its name
        Assert.AreEqual("Bot", newBot.name);

        //check its parent
        Assert.AreEqual(botHolder, newBot.transform.parent);

    }

    /// <summary>
    /// tests <see cref="ObjectManager.AddRandomItemsEditor"/>
    /// </summary>
    [Test]
    public void TestSpawnRandomItems()
    {
        //clear the existing objects
        manager.ClearLevelObjects();

        //set the number to spawn
        manager.SetItemSpawnCount(5);

        //spawn items
        manager.AddRandomItemsEditor();

        List<GameObject> items = manager.GetLevelObjects();

        //check the proper number were spawned
        Assert.AreEqual(5, items.Count);

        //foreach item do assertions
        foreach(GameObject newItem in items)
        {
            //check it was spawned within max radius
            Assert.LessOrEqual(Vector3.Distance(newItem.transform.position, manager.GetSpawnOrigin()), manager.GetSpawnRadius());

            //check its name
            Assert.AreEqual("Item", newItem.name);

            //check its parent
            Assert.AreEqual(itemHolder, newItem.transform.parent);
        }

        

    }

    /// <summary>
    /// tests <see cref="ObjectManager.AddRandomBotsEditor"/>
    /// </summary>
    [Test]
    public void TestSpawnRandomBots()
    {
        //clear the existing objects
        manager.ClearLevelObjects();

        //set the number to spawn
        manager.SetBotSpawnCount(5);

        //spawn items
        manager.AddRandomBotsEditor();

        List<GameObject> bots = manager.GetLevelObjects();

        //check the proper number were spawned
        Assert.AreEqual(5, bots.Count);

        //foreach item do assertions
        foreach (GameObject newBot in bots)
        {

            //check it was spawned within max radius
            Assert.LessOrEqual(Vector3.Distance(newBot.transform.position, manager.GetSpawnOrigin()), manager.GetSpawnRadius());

            //check its name
            Assert.AreEqual("Bot", newBot.name);

            //check its parent
            Assert.AreEqual(botHolder, newBot.transform.parent);
        }

    }

    /// <summary>
    /// tests <see cref="ObjectManager.ClearLevelObjects"/>
    /// </summary>
    [Test]
    public void TestClear()
    {
        //ensure there are existing objects to start
        Assert.AreEqual(2, manager.GetLevelObjects().Count);

        //clear all objects
        manager.ClearLevelObjects();

        //ensure there are no objects left
        Assert.AreEqual(0, manager.GetLevelObjects().Count);

        //ensure the two starting items were destroyed
        Assert.IsTrue(item == null);
        Assert.IsTrue(bot == null);
    }

    /// <summary>
    /// tests <see cref="ObjectManager.SaveLevelToFile"/>
    /// </summary>
    [Test]
    public void TestSaveLevel()
    {
        // adjust the player position
        player.transform.position = new Vector3(1, 1, 1);

        //save the level to a file
        manager.SaveLevelToFile();

        //attempt to read the file contents
        string fileContents = "";
        try
        {
            StreamReader sr = new StreamReader(io.GetFilePath());
            fileContents = sr.ReadToEnd();
            sr.Close();
        }
        catch (Exception e)
        {
            //fail if it can't be read
            Assert.Fail(e.Message);
        }

        //ensure the file contents are correct
        Assert.AreEqual("{\"objects\":[{\"objectType\":0,\"position\":{\"x\":1.0,\"y\":1.0,\"z\":1.0}},{\"objectType\":2,\"position\":{\"x\":0.0,\"y\":1.0,\"z\":0.0}},{\"objectType\":1,\"position\":{\"x\":0.0,\"y\":-2.0,\"z\":0.0}}]}", fileContents);
    }

    /// <summary>
    /// tests <see cref="ObjectManager.LoadFromFile"/>
    /// </summary>
    [Test]
    public void LoadLevel()
    {
        //adjust player position
        player.transform.position = new Vector3(1, 1, 1);

        //save the level to a file
        manager.SaveLevelToFile();

        //move the player
        player.transform.position = new Vector3(37, 54, 9);

        //Clear the existing level objects
        manager.ClearLevelObjects();

        //spawn multiple bots in their place
        manager.SetBotSpawnCount(4);
        manager.AddRandomBotsEditor();

        //load the level from the file
        manager.LoadFromFile();

        //ensure the player is in the correct position
        Assert.AreEqual(new Vector3(1,1,1), player.transform.position);

        //get the manager's level objects
        List<GameObject> levelObjects = manager.GetLevelObjects();
        
        //ensure there are exactly 2 objects
        Assert.AreEqual(2, levelObjects.Count);

        //the first entry should be the item:
        //check its position
        Assert.AreEqual(new Vector3(0,1,0), levelObjects[0].transform.position);

        //check its name
        Assert.AreEqual("Item", levelObjects[0].name);

        //check its parent
        Assert.AreEqual(itemHolder, levelObjects[0].transform.parent);

        //the second entry should be the bot:
        //check its position
        Assert.AreEqual(new Vector3(0, -2, 0), levelObjects[1].transform.position);

        //check its name
        Assert.AreEqual("Bot", levelObjects[1].name);

        //check its parent
        Assert.AreEqual(botHolder, levelObjects[1].transform.parent);
    }
}
