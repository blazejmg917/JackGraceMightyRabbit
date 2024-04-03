using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    //was considering using a ref variable here for more immediate updates, but they aren't allowed in the version of C# I'm using for this project so we're settling for easy accessors
    /// <summary>
    /// A list of all LevelObjects to be checked as the potential closest object
    /// </summary>
    [SerializeField, Tooltip("all objects to check the positions of")] private List<GameObject> _levelObjects = new List<GameObject>();

    /// <summary>
    /// A list of all LevelObjects to be checked as the potential closest object
    /// </summary>
    public List<GameObject> levelObjects
    {
        get { return _levelObjects; }
        set { _levelObjects = value; }
    }

    /// <summary>
    /// The object that is currently the closest to the player
    /// </summary>
    [SerializeField, Tooltip("the current closest object")] private GameObject closestObject;

    //the new closest object; defining this variable here to avoid repeated declaration
    /// <summary>
    /// An object that is used to reference new candidates for the closest object before it they are assigned to the final variable
    /// </summary>
    private GameObject newClosestObject;

    /// <summary>
    /// the distance between the player and its current closest object
    /// </summary>
    [SerializeField, Tooltip("the current distance between the player and the closest object")] private float closestDistance = float.MaxValue;

    //the distance between this transform and another; defining this variable here to avoid repeated declaration
    /// <summary>
    /// the distance between the player and whichever object is currently being checked as the potential new closest object
    /// </summary>
    private float distance;

    
    /// <summary>
    /// Searches through all of the tracked LevelObjects to find the closest one to the player. 
    /// If there is a new closest object, updates object highlighting to reflect that and starts tracking the distance from the new closest object
    /// </summary>
    public void FindNearestObject()
    {
        //do a sanity check to ensure that we don't get any exceptions thrown throughout
        if (levelObjects == null || levelObjects.Count <=0)
        {
            return;
        }

        /*
         * In this version, check every object stored in the player's list. 
         * With more time I'd consider making a sort of sorted List structure such that only object that could possible have become the new closest will be checked 
         * based on distance traveled by the player
         */
        foreach(GameObject levelObject in _levelObjects)
        {
            //compare the distance with the stored greatest distance, and mark the object as the new closest if necessary
            distance = Vector3.Distance(transform.position, levelObject.transform.position);
            if (distance < closestDistance)
            {
                newClosestObject = levelObject;
                closestDistance = distance;
            }
        }

        //Save the highlighting for the end to prevent unnecessary calls to the renderer
        if(newClosestObject && newClosestObject != closestObject) 
        {
            //unhighlight the old object, reassign the variable, and then highlight the new object
            if (closestObject)
            {
                //null check for the very first time this check is run at the start of the game
                closestObject.GetComponent<LevelObject>().SetHighlighted(false);
            }
            closestObject = newClosestObject;
            closestObject.GetComponent<LevelObject>().SetHighlighted(true);
        }
    }

    /// <summary>
    /// Checks if one single LevelObject is the new closest object. Useful for tracking the addition of new objects during the game
    /// </summary>
    /// <param name="levelObject">the new LevelObject which will have its distance checked</param>
    public void CheckObjectDistance(GameObject levelObject)
    {
        //don't waste time checking the object that is already the closest
        if (levelObject == closestObject)
        {
            return;
        }

        //do distance calculations and determine if this is the current closest object
        distance = Vector3.Distance(transform.position, levelObject.transform.position);
        if (distance < closestDistance)
        {
            //if it is, immediately handle reassignment and highlighting since this is only a single-object check
            Debug.Log("object was new closest");
            closestDistance = distance;
            if (closestObject)
            {
                closestObject.GetComponent<LevelObject>().SetHighlighted(false);
            }
            closestObject = levelObject;
            closestObject.GetComponent<LevelObject>().SetHighlighted(true);
        }
    }


    private void Update()
    {
        //update the current distance between the player and the current closest object for correct tracking. This can also be done within the full distance check, but this allows for some extra math
        if (closestObject)
        {
            closestDistance = Vector3.Distance(transform.position, closestObject.transform.position);
        }

        /*
         * In the most basic form of this solution, you check for a new closest object every frame. This will ensure the correct object is always highlighted, and at low object counts won't be a problem
         * But at higher counts this will start to become quite inefficient. Sometimes that inefficiency will be unavoidable, but there are a few tricks to cut down on the checks being made
         */
        FindNearestObject();
    }
}
