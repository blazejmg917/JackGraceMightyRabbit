using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// tests the <see cref="Player"/> class
/// </summary>
public class PlayerTest
{
    Player player;
    ObjectManager manager;
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
        player = GameObject.Find("Player").GetComponent<Player>();

        //clear out the object manager for a clean slate to work from
        manager.ClearLevelObjects();
        
        //reset the player's stored values to start from scratch
        player.Reset();

        //set up item and bot
        item = manager.AddItem(new Vector3(0, 1, 0)).GetComponent<Item>();
        bot = manager.AddBot(new Vector3(0, -2, 0)).GetComponent<Bot>();

        //ensure the player is at the origin
        player.transform.position = Vector3.zero;
    }

    /// <summary>
    /// tests <see cref="Player.FindNearestObjectBasic"/>
    /// </summary>
    [Test]
    public void TestGetNearestBasic()
    {
        //set up a list of the existing level objects and give it to the player
        List<GameObject> levelObjects = new List<GameObject> { item.gameObject, bot.gameObject };
        player.levelObjects= levelObjects;

        //try to find the nearest object
        player.FindNearestObjectBasic();

        //ensure the closest object is highlighted
        Assert.AreEqual(item.GetMat(true).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it still works on a repeat at the same positions

        player.FindNearestObjectBasic();

        Assert.AreEqual(item.GetMat(true).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it properly updates when the player moves

        player.transform.position = new Vector3(0, -1.5f, 0);
        player.FindNearestObjectBasic();

        Assert.AreEqual(bot.GetMat(true).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it properly updates when a new closest item is spawned

        Item item2 = manager.AddItem(new Vector3(.1f,-1.5f,0)).GetComponent<Item>();

        //set the player's objects to include the new item
        levelObjects.Add(item2.gameObject);
        player.levelObjects = levelObjects;
        player.FindNearestObjectBasic();

        Assert.AreEqual(item2.GetMat(true).color, item2.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it stays the same when a new item is spawned that isn't the closest

        Item item3 = manager.AddItem(new Vector3(3f, 1.5f, 0)).GetComponent<Item>();

        //set the player's objects to include the new item
        levelObjects.Add(item3.gameObject);
        player.levelObjects = levelObjects;
        player.FindNearestObjectBasic();

        Assert.AreEqual(item2.GetMat(true).color, item2.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item3.GetMat(false).color, item3.GetComponent<Renderer>().sharedMaterial.color);
    }

    /// <summary>
    /// tests <see cref="Player.FindNearestObjectOverlap"/>
    /// </summary>
    [Test]
    public void TestGetNearestOverlap()
    {
        //set up a list of the existing level objects and give it to the player
        List<GameObject> levelObjects = new List<GameObject> { item.gameObject, bot.gameObject };
        player.levelObjects = levelObjects;

        //try to find the nearest object
        player.FindNearestObjectOverlap();

        //ensure the closest object is highlighted
        Assert.AreEqual(item.GetMat(true).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it still works on a repeat at the same positions

        player.FindNearestObjectOverlap();

        Assert.AreEqual(item.GetMat(true).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it properly updates when the player moves

        player.transform.position = new Vector3(0, -1.5f, 0);
        player.FindNearestObjectOverlap();

        Assert.AreEqual(bot.GetMat(true).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it properly updates when a new closest item is spawned

        Item item2 = manager.AddItem(new Vector3(.1f, -1.5f, 0)).GetComponent<Item>();

        //set the player's objects to include the new item
        levelObjects.Add(item2.gameObject);
        player.levelObjects = levelObjects;
        player.FindNearestObjectOverlap();

        Assert.AreEqual(item2.GetMat(true).color, item2.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);

        //ensure it stays the same when a new item is spawned that isn't the closest

        Item item3 = manager.AddItem(new Vector3(3f, 1.5f, 0)).GetComponent<Item>();

        //set the player's objects to include the new item
        levelObjects.Add(item3.gameObject);
        player.levelObjects = levelObjects;
        player.FindNearestObjectOverlap();

        Assert.AreEqual(item2.GetMat(true).color, item2.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item3.GetMat(false).color, item3.GetComponent<Renderer>().sharedMaterial.color);
    }

    /// <summary>
    /// tests <see cref="Player.CheckObjectDistance(GameObject)"/>
    /// </summary>
    [Test]
    public void TestCheckObject()
    {
        //set up a list of the existing level objects and give it to the player
        List<GameObject> levelObjects = new List<GameObject> { item.gameObject, bot.gameObject };
        player.levelObjects = levelObjects;

        //try to find the nearest object
        player.FindNearestObjectBasic();

        //ensure the closest object is highlighted
        Assert.AreEqual(item.GetMat(true).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);

        //test with a new closest object
        Item item2 = manager.AddItem(new Vector3(.1f, -0.5f, 0)).GetComponent<Item>();
        levelObjects.Add(item2.gameObject);
        player.levelObjects = levelObjects;

        //check the distance of this new object
        player.CheckObjectDistance(item2.gameObject);

        Assert.AreEqual(item2.GetMat(true).color, item2.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);

        //test with an object that is not the closest
        Item item3 = manager.AddItem(new Vector3(3f, 1.5f, 0)).GetComponent<Item>();
        levelObjects.Add(item3.gameObject);
        player.levelObjects = levelObjects;

        //check the distance of this new object
        player.CheckObjectDistance(item3.gameObject);

        Assert.AreEqual(item2.GetMat(true).color, item2.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(bot.GetMat(false).color, bot.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item.GetMat(false).color, item.GetComponent<Renderer>().sharedMaterial.color);
        Assert.AreEqual(item3.GetMat(false).color, item3.GetComponent<Renderer>().sharedMaterial.color);

    }
}
