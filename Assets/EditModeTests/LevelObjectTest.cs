using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// tests the <see cref="LevelObject"/> class
/// </summary>
public class LevelObjectTest
{
    /// <summary>
    /// tests <see cref="LevelObject.SetHighlighted(bool)"/>
    /// </summary>
    [Test]
    public void TestHighlight()
    {
        //get a reference to the object manager
        ObjectManager manager = GameObject.FindObjectOfType<ObjectManager>();
        //add a random item to test with
        GameObject obj = manager.AddRandomItem();
        //get the level object component from the item
        LevelObject levelObj = obj.GetComponent<LevelObject>();
        //get the renderer component from the item
        Renderer renderer= obj.GetComponent<Renderer>();

        //set the object to be highlighted
        levelObj.SetHighlighted(true);

        //assert that the highlighted material is displayed
        Assert.AreEqual(levelObj.GetMat(true).color, renderer.sharedMaterial.color);

        //set the object to be not highlighted
        levelObj.SetHighlighted(false);

        //assert that the base material is displayed
        Assert.AreEqual(levelObj.GetMat(false).color, renderer.sharedMaterial.color);
    }
}
