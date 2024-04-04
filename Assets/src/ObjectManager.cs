using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectManager : MonoBehaviour
{
    //these are any other important scipts that need to have their references stored
    [Header("Other Scripts")]

    [SerializeField, Tooltip("the IO Handler")]private GameIO gameIO;

    //prefabs are used to quickly add new objects of a given type at runtime
    [Header("Prefabs")]

    [SerializeField, Tooltip("the prefab for an Item object")] private GameObject itemPrefab;

    [SerializeField, Tooltip("the prefab for a Bot object")] private GameObject botPrefab;

    //object holders used to simplify the level structure and ensure that it is easy to find objects of certain types for debugging or testing purposes
    [Header("Object Holders")]

    [SerializeField, Tooltip("the holder for all Item objects")] private Transform itemHolder;

    [SerializeField, Tooltip("the holder for all Bot objects")] private Transform botHolder;

    [Space]

    /*
     * master list that contains all items and bots; 
     * was going to store as a LinkedList due to the slightly improved efficiency of adding to the list, 
     * but it isn't serializable to the editor so I swapped for readability
     */
    [SerializeField, Tooltip("the main list that contains all level objects")] private List<GameObject> levelObjects = new List<GameObject>();

    [Space]

    [SerializeField, Tooltip("A reference to the player object")]private Player player;

    //this is where all materials are stored for both item and bot objects. This allows for editor-time adjustement of colors
    [Header("Materials")]
    
    [SerializeField, Tooltip("the base material for Item objects. Used by default")] private Material baseItemMat;

    [SerializeField, Tooltip("the highlighted material for Item object. Used when close to the player")] private Material highlightItemMat;

    [SerializeField, Tooltip("the base material for Bot objects. Used by default")] private Material baseBotMat;

    [SerializeField, Tooltip("the highlighted material for Bot object. Used when close to the player")] private Material highlightBotMat;

    //this is where the colors for both item and bot objects are stored. This allows for direct manipulation of material colors within the main editor without having to open up the materials directly
    [Header("Colors")]

    [SerializeField, Tooltip("the base color for Item objects. Used by default")] private Color baseItemColor = Color.white;

    [SerializeField, Tooltip("the highlighted color for Item object. Used when close to the player")] private Color highlightItemColor = Color.red;

    [SerializeField, Tooltip("the base color for Bot objects. Used by default")] private Color baseBotColor = Color.white;

    [SerializeField, Tooltip("the highlighted color for Bot object. Used when close to the player")] private Color highlightBotColor = Color.blue;

    //the following fields are used to handle validation for color changes such that they are automatically applied to the materials
    private Color prevBaseItemColor = Color.white;

    private Color prevHighlightItemColor = Color.red;

    private Color prevBaseBotColor = Color.white;

    private Color prevHighlightBotColor = Color.blue;

    //this is where values used for spawning new objects are defined. Allows for the area of spawning to be defined
    [Header("Spawning")]

    [SerializeField, Tooltip("the origin for spawning objects. Set to the world origin by default")] private Vector3 spawnOrigin = Vector3.zero;

    [SerializeField, Tooltip("The radius in which objects can be spawned. Maximum distance from the origin an object can be randomly created")] private float spawnRadius = 20f;

    [SerializeField, Tooltip("the number of items to spawn into the scene")] private int numItemsToSpawn = 1;

    [SerializeField, Tooltip("the number of bots to spawn into the scene")] private int numBotsToSpawn = 1;

    //Below here are a struct and enum I will be using for saving to and writing from files. This will allow me to utilize JSON for my process, as MonoBehaviors and JSON don't quite mix

    /// <summary>
    /// The Type of object that is being stored to be saved to a file. Can represent character, bot, or item
    /// </summary>
    public enum ObjectType { PLAYER, BOT, ITEM }

    /// <summary>
    /// A struct used to store objects for file IO. This will store an object's type and position so it can be respawned into the level at will in the right position
    /// </summary>
    public struct ObjectStruct
    {
        /// <summary>
        /// The type of object that is being stored.
        /// </summary>
        public ObjectType objectType;

        /// <summary>
        /// The worldspace position that this object was located at
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// A constructor for the ObjectStruct class
        /// </summary>
        /// <param name="type">the type of object to be stored</param>
        /// <param name="pos">the position of the object to be stored</param>
        public ObjectStruct(ObjectType type, Vector3 pos)
        {
            objectType = type;
            position = pos;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        
        if (!itemHolder)
        {
            //If the item holder has not been manually set, locate it now
            itemHolder = transform.Find("Item Holder");
            if(!itemHolder)
            {
                //if there is no item holder, create an empty game object, name it properly, and set the reference
                itemHolder = new GameObject("Item Holder").transform;
            }
        }

        if (!botHolder)
        {
            //If the bot holder has not been manually set, locate it now
            botHolder = transform.Find("Bot Holder");
            if (!botHolder)
            {
                //if there is no bot holder, create an empty game object, name it properly, and set the reference
                botHolder = new GameObject("Bot Holder").transform;
            }
        }

        if (!gameIO)
        {
            //if the gameIO reference isn't set, find the component
            gameIO = GetComponent<GameIO>();
            if (!gameIO)
            {
                //if the component doesn't exist, create it
                gameIO = gameObject.AddComponent<GameIO>();
            }
        }



        if (!player)
        {
            //if the player has not been manually set, find it in the scene.
            player = FindObjectOfType<Player>();
        }
    }

    private void Start()
    {
        //a double check method to make sure the level objects have been added to the list. Can add some additional startup overhead, but only in the case that the list is empty
        if(levelObjects == null || levelObjects.Count == 0)
        {
            FindAllObjects();
        }
        //once the game begins, the list of objects should be passed to the player so that it can begin checking for nearest objects
        player.levelObjects = levelObjects;
    }

    /// <summary>
    /// Resets the current list of LevelObjects, then finds and adds all LevelObjects that currently exist within the scene
    /// </summary>
    public void FindAllObjects()
    {
        /*
         * this will allow me to get all LevelObjects and add them to the master list for quick and efficient access. Caching them here prevents the extra overhead of searching for all of them every time,
         * and since it should realistically only be called in editor mode, it does not matter that this particular operation is expensive.
         * additionally, simply storying as an array may save time on this operation, but since the only operations I plan to be doing on the list in general are AddLast and iteration, 
         * this will be efficient at runtime
         */
        levelObjects = new List<GameObject>();
        LevelObject[] objects = FindObjectsOfType<LevelObject>();
        foreach (LevelObject obj in objects)
        {
            levelObjects.Add(obj.gameObject);
        }
    }

    /// <summary>
    /// Spawns one item into the scene at a random position. Position is bounded by the spawnOrigin and spawnRadius fields
    /// </summary>
    private void AddRandomItem()
    {
        //spawn a new Item Prefab copy
        GameObject newItem = AddRandomObject(itemPrefab);

        //set the item's parent to the item holder
        newItem.transform.parent = itemHolder;
    }

    /// <summary>
    /// Adds a number of randomly spawned items equal to the value of the numItemsToSpawn variable
    /// Used for adding items through the inspector.
    /// </summary>
    public void AddRandomItemsEditor()
    {
        for(int i = 0; i < numItemsToSpawn; i++)
        {
            AddRandomItem();
        }
    }

    /// <summary>
    /// Spawns one bot into the scene at a random position. Position is bounded by the spawnOrigin and spawnRadius fields
    /// </summary>
    public void AddRandomBot()
    {
        //spawn a new Bot Prefab copy
        GameObject newBot = AddRandomObject(botPrefab);

        //set the bot's parent to the bot holder
        newBot.transform.parent = botHolder;
    }

    /// <summary>
    /// Adds a number of randomly spawned bots equal to the value of the numBotsToSpawn variable
    /// Used for adding bots through the inspector.
    /// </summary>
    public void AddRandomBotsEditor()
    {
        for (int i = 0; i < numBotsToSpawn; i++)
        {
            AddRandomBot();
        }
    }


    /// <summary>
    /// Instantiates a copy of the given object at a random position and adds it to the list of LevelObjects. 
    /// The random position is bounded by the spawnOrigin and spawnRadius fields
    /// </summary>
    /// <param name="obj">the object to spawn a copy of</param>
    /// <returns>the newly spawned object</returns>
    public GameObject AddRandomObject(GameObject obj)
    {
        //this generates a random position around the spawn origin within the defined spawn radius for the new object to be created
        Vector3 spawnPos = spawnOrigin + Random.onUnitSphere * Random.Range(0, spawnRadius);
        //spawn in the new object at the random position 
        return SpawnObject(obj, spawnPos);
    }

    /// <summary>
    /// Instantiates a copy of the given object at the provided position, and adds it to the list of levelObjects
    /// </summary>
    /// <param name="obj">the object to spawn a copy of</param>
    /// <param name="spawnPos">the position to spawn this object at</param>
    /// <returns>the newly spawned object</returns>
    public GameObject SpawnObject(GameObject obj, Vector3 spawnPos)
    {
        //instantiate the given object at the given position
        GameObject newObj = Instantiate(obj, spawnPos, Quaternion.identity);
        //add the newly spawned object into the master list for quick reference
        levelObjects.Add(newObj);
        //if the game is actively being played, update the player's list, and tell the player to check if this new object is the new closest object
        if (Application.isPlaying)
        {
            player.levelObjects = levelObjects;
            player.CheckObjectDistance(newObj);

        }
        return newObj;
    }

    /// <summary>
    /// Clears out the existing items and bots from the master list and returns it to an empty list. 
    /// Useful for clearing out useless objects for a new test
    /// </summary>
    public void ClearLevelObjects()
    {
        //loop through each object and destroy them
        foreach(GameObject obj in levelObjects)
        {
            if (obj)
            {
                //need to destroy objects differently based on if you are in edit or play mode
                if (!Application.isPlaying)
                {
                    DestroyImmediate(obj);
                }
                else
                {
                    Destroy(obj);
                }
            }
        }
        //create a new list for level objects to ensure there are no null references being used
        levelObjects = new List<GameObject>();
        //set the player's objects to this list
        player.levelObjects = levelObjects;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            //prevents this chunk of code from running while scene loads, as it is unnecessary
            return;
        }
        //if the base item color has been changed, update its material to match
        if (!baseItemColor.Equals(prevBaseItemColor))
        {
            prevBaseItemColor = baseItemColor;
            baseItemMat.color = baseItemColor;
        }

        //if the highlighted item color has been changed, update its material to match
        if (!highlightItemColor.Equals(prevHighlightItemColor))
        {
            prevHighlightItemColor = highlightItemColor;
            highlightItemMat.color = highlightItemColor;
        }

        //if the base bot color has been changed, update its material to match
        if (!baseBotColor.Equals(prevBaseBotColor))
        {
            prevBaseBotColor = baseBotColor;
            baseBotMat.color = baseBotColor;
        }

        //if the highlighted bot color has been changed, update its material to match
        if (!highlightBotColor.Equals(prevHighlightBotColor))
        {
            prevHighlightBotColor = highlightBotColor;
            highlightBotMat.color = highlightBotColor;
        }
    }

    /*
     * This chunk of functions is related to inputs. Normally, I'd have the player handling inputs, 
     * since the player would likely need to also handle other inputs like movement, etc.
     * But in this project the primary method of manipulating the player is by directly dragging them through the scene
     * and most things that would require input are handled through this manager, 
     * so I've decided to allow the manager to handle inputs as well
     */

    /// <summary>
    /// helper function to spawn random item on button press
    /// </summary>
    public void AddRandomItemButton(InputAction.CallbackContext ctx)
    {
        //only call whent the action is fully performed
        if (ctx.started)
        {
            Debug.Log("add random item");
            AddRandomItem();
        }
    }

    /// <summary>
    /// helper function to spawn random bot on button press
    /// </summary>
    public void AddRandomBotButton(InputAction.CallbackContext ctx)
    {
        //only call when the action is fully performed
        if (ctx.started)
        {
            Debug.Log("add random bot");
            AddRandomBot();
        }
    }

    /// <summary>
    /// Helper function for saving the existing level structure on a button press
    /// </summary>
    /// <param name="ctx">the input action</param>
    public void SaveLevel(InputAction.CallbackContext ctx)
    {
        //only call when the action is fully performed
        if (ctx.performed)
        {
            SaveLevelToFile();
        }
    }

    /// <summary>
    /// Helper function for loading the  level structure from a file on a button press
    /// </summary>
    /// <param name="ctx">the input action</param>
    public void LoadLevel(InputAction.CallbackContext ctx)
    {
        //only call when the action is fully performed
        if (ctx.performed)
        {
            LoadFromFile();
        }
    }

    /*
     * This chunk of functions is directly related to file IO.
     * The manager has direct access to all important objects in the game, 
     * so it is the most useful object to manage IO functionality
     */

    /// <summary>
    /// Saves the current state of the level to a file so it can be restored later
    /// </summary>
    public void SaveLevelToFile()
    {
        //First, create a list to hold data of all level objects
        List<ObjectStruct> objs = new List<ObjectStruct>
        {
            //Add in the player's representation, since the player should always exist in the scene
            new ObjectStruct(ObjectType.PLAYER, player.transform.position)
        };

        /*
         * Then, loop through all existing levelobjects and add them to the list. 
         * If MonoBehaviors played better with JSON I could simply pass in levelObjects as a whole, but this is a fairly painless method as well even if less efficient
         */
        foreach (GameObject obj in levelObjects)
        {
            //Assign their data as the proper object type and save their position into the list
            if(obj.GetComponent<LevelObject>() is Item)
            {
                objs.Add(new ObjectStruct(ObjectType.ITEM, obj.transform.position));
            }
            //since I only have two types of objects to be saved, I don't need a second check. In a larger project this would probably need a better structure, but for now it's a simple if-else
            else
            {
                objs.Add(new ObjectStruct(ObjectType.BOT, obj.transform.position));
            }
        }

        //once the list has been created, attempt to save it to the file
        if (gameIO.SaveToFile(objs))
        {
            //if saving was succesful, notify the user
            Debug.Log("Level succesfully saved");
        }
    }

    /// <summary>
    /// Loads in the currently saved level state, if it exists. 
    /// Will overwrite the existing level state
    /// </summary>
    public void LoadFromFile()
    {
        //Try to read the objects from the save file
        List<ObjectStruct> objs;
        if(!gameIO.ReadFromFile(out objs))
        {
            //if the file can't be found or the IO fails to read, end this function
            return;
        }

        //clear out the preexisting objects to make way for the new objects to be spawned
        ClearLevelObjects();

        //create this gameObject reference here 
        GameObject newObj;
        foreach(ObjectStruct obj in objs)
        {
            //for every object struct in the list, check its type and act accordinly
            switch(obj.objectType)
            {
                //the player object will already exist, so just update its position
                case ObjectType.PLAYER:
                    player.transform.position = obj.position;
                    break;
                //for bots, spawn the prefab and set its position and parent
                case ObjectType.BOT:
                    newObj = SpawnObject(botPrefab, obj.position);
                    newObj.transform.parent = botHolder;
                    break;
                //for items, also spawn the prefab and set position and parent
                case ObjectType.ITEM:
                    newObj = SpawnObject(itemPrefab, obj.position);
                    newObj.transform.parent = itemHolder;
                    break;
            }
        }
    }





}
