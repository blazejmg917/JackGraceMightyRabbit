using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The main player class. Will be moved around the scene manually and checks for the closest <see cref="LevelObject"/>
/// </summary>
public class Player : MonoBehaviour
{
    //was considering using a ref variable here for more immediate updates, but they aren't allowed in the version of C# I'm using for this project so we're settling for easy accessors
    [SerializeField, Tooltip("all objects to check the positions of")] private List<GameObject> _levelObjects = new List<GameObject>();

    /// <summary>
    /// public accessor for the list of <see cref="LevelObject"/>s that the player is tracking and checking for the nearest object from
    /// </summary>
    public List<GameObject> levelObjects
    {
        get { return _levelObjects; }
        set { _levelObjects = value; }
    }


    [SerializeField, Tooltip("the current closest object")] private GameObject closestObject;

    //the new closest object; defining this variable here to avoid repeated declaration
    private GameObject newClosestObject;

    [SerializeField, Tooltip("the current distance between the player and the closest object")] private float closestDistance = float.MaxValue;

    //the distance between this transform and another; defining this variable here to avoid repeated declaration
    private float distance;

    /*
     * this is a set of publicly viewable metrics keeping track of the number of milliseconds it takes to complete a full update frame's worth of checks on average
     * I think this will be a simple but useful tool for testing my different optimization strategies
     */
    [SerializeField, Tooltip("the average time it takes for one frame of FixedUpdate to run while searching for the closest object")]private float averageProcessTime = 0;

    //keeps track of how many frames have been run to handle average time
    [SerializeField, Tooltip("the number of frames the game has been running for")]private long numFramesRun = 0;

    //keeps track of how long the current frame runs
    [SerializeField, Tooltip("the time it is taking for the current frame of FixedUpdate to run while searching for the closest object")] private float frameProcessTime = 0;


    /// <summary>
    /// Resets the player's closest object and distance to their base values to allow full reset of structure.
    /// This helps to avoid issues with the overlapsphere method of finding closest files when loading from a file
    /// </summary>
    public void Reset()
    {
        //reset the starting distances
        closestDistance = float.MaxValue;
        distance = 0;

        //set the closest objects to null
        closestObject = null;
        newClosestObject = null;
    }


    /// <summary>
    /// Searches through all of the tracked <see cref="LevelObject"/>s to find the closest one to the <see cref="Player"/>. 
    /// If there is a new closest object, updates object highlighting to reflect that and starts tracking the distance from the new closest object
    /// </summary>
    public void FindNearestObjectBasic()
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
        FindNearestInGroup(levelObjects);
    }

    /// <summary>
    /// Creates an overlapSphere around the player using the distance to the closest object as its radius, 
    /// and checks if any caught <see cref="LevelObject"/>s are closer to the <see cref="Player"/> than the current closest object.
    /// </summary>
    public void FindNearestObjectOverlap()
    {
        //use the closest distance as the radius by default
        FindNearestObjectOverlap(closestDistance);
    }

    /// <summary>
    /// Creates an overlapSphere around the player with the given radius, 
    /// and checks if any caught <see cref="LevelObject"/>s are closer to the <see cref="Player"/> than the current closest object.
    /// </summary>
    /// <param name="radius">The radius of the sphere to create</param>
    public void FindNearestObjectOverlap(float radius)
    {
        //If there is no closest object to check from, this check becomes arbitrary and potentially far less efficient, so it should be passed over to the basic version
        if (!closestObject)
        {
            FindNearestObjectBasic();
            return;
        }

        //Create an overlap sphere of the given radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        //check the distance to all colliders found
        FindNearestInGroup(colliders);
    }

    /// <summary>
    /// Checks all <see cref="LevelObject"/>s within the specified group to see if any are closer than the current closest object
    /// </summary>
    /// <param name="objects">A list of all of the <see cref="LevelObject"/>s to check for a potential new closest object</param>
    private void FindNearestInGroup(List<GameObject> objects)
    {
        //check all objects provided
        foreach (GameObject levelObject in objects)
        {
            //compare the distance between this object and the player wiht with the stored greatest distance
            distance = Vector3.Distance(transform.position, levelObject.transform.position);

            /*
             * if this is the new closest object, mark it as such but don't update everything yet, 
             * as that would both potentially alter the rest of the iteration and lead to multiple
             * unecessary operations to happen when only the final decided closest object actually
             * warrants resetting all of the other values
             */
            if (distance < closestDistance)
            {
                newClosestObject = levelObject;
                closestDistance = distance;
            }
        }

        //now update the closest object here at the end
        UpdateClosest();
    }

    /// <summary>
    /// Checks all <see cref="LevelObject"/>s within the specified group to see if any are closer than the current closest object
    /// </summary>
    /// <param name="objects">A list of all of the colliders of objects to check for a potential new closest object</param>
    private void FindNearestInGroup(Collider[] objects)
    {
        //check all objects provided
        foreach (Collider levelObject in objects)
        {
            //compare the distance between this collider and the player with with the stored greatest distance
            distance = Vector3.Distance(transform.position, levelObject.transform.position);

            /*
             * if this is the new closest object, mark it as such but don't update everything yet, 
             * as that would both potentially alter the rest of the iteration and lead to multiple
             * unecessary calls to object renderers to happen when only the final decided closest 
             * object actually warrants resetting the renderer updates
             */
            if (distance < closestDistance)
            {
                newClosestObject = levelObject.gameObject;
                closestDistance = distance;
            }
        }

        //now update the closest object here at the end
        UpdateClosest();
    }

    /// <summary>
    /// If there is a new closest <see cref="LevelObject"/>, updates the visual highlight and sets a new object to track
    /// </summary>
    private void UpdateClosest()
    {
        //if there is a new closest object, update visuals to match
        if (newClosestObject && newClosestObject != closestObject)
        {
            //unhighlight the old object if it exists
            if (closestObject)
            {
                //null check for the very first time this check is run at the start of the game
                closestObject.GetComponent<LevelObject>().SetHighlighted(false);
            }

            //update the variable and highlight the new closest object
            closestObject = newClosestObject;
            closestObject.GetComponent<LevelObject>().SetHighlighted(true);
        }
    }

    /// <summary>
    /// Checks if one single <see cref="LevelObject"/> is the new closest object. Useful for tracking the addition of new objects during the game
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
            closestDistance = distance;

            //unhighlight the old object if it exists
            if (closestObject)
            {
                //null check for the very first time this check is run at the start of the game
                closestObject.GetComponent<LevelObject>().SetHighlighted(false);
            }

            //update the variable and highlight the new closest object
            closestObject = levelObject;
            closestObject.GetComponent<LevelObject>().SetHighlighted(true);
        }
    }

    
    private void FixedUpdate()
    {
        /*
         * keeping track of the start time of this frame to determine processing time. Was originally using DateTime,
         * but found this information far more useful in terms of seconds that could be broken into decimals
         */
        float before = Time.realtimeSinceStartup;

        /*
         * update the current distance between the player and the current closest object for correct tracking. 
         * This can also be done within the full distance check, but this allows for some extra math 
         * Some of my options for optimization will use this math, others won't
         */
        if (closestObject)
        {
            closestDistance = Vector3.Distance(transform.position, closestObject.transform.position);
        }

        /*
         * below I will list a few optimization options, their strengths and benefits, and a bit of analysis I did on them
         * I let the program run for roughly 1000 frames while moving the player around and calculated the average length of a FixedUpdate call during that time
         * In a longer setting I would have liked more time to give towards various trials of this kind of analysis, but I did what I could with the time I have
         * 
         * Additionally, here are a few analysis definitions:
         * in cluster: I spawn all objects within a small radius (20 for 2000 objects, 50 for 20000) and keep player inside of that radius
         * outside of cluster: I spawn all objects within a small radius and keep player outside of that radius
         * sparse: I spawn all objects within a wide radius (200 for 2000 objects, 5000 for 20000) and keep player inside of that radius
         */

        /*
         * OPTION 1
         * In the most basic form of this solution, you check for a new closest object every frame. 
         * This will ensure the correct object is always highlighted, and at low object counts won't be a problem
         * But at higher counts this will start to become quite inefficient. 
         * Sometimes that inefficiency will be unavoidable, but there are a few tricks to cut down on the checks being made
         * 
         * This solution's major strength is that it's best and worst case efficiencies are the same. 
         * Regardless of distance, this will always be exactly an O(n) operation, so it may become more effective than others in very sparse setups
         * However, this solution also has no strong areas either. It will ALWAYS require checking every single level object, which will cause much worse best-case times, 
         * especially in dense areas
         * 
         * analysis results:
         * 10 objects: 2.966794e-05s average
         * 2000 objects in cluster: 0.0001179194s average
         * 2000 objects ouside of cluster: 5.139307e-05s average
         * 2000 objects sparse: 4.642201e-05s average
         * 20000 objects in cluster: 0.0002596787s average
         * 20000 objects outside of cluster: 0.0001109541s average
         * 20000 objects sparse: 0.000115283s average
         */
        //FindNearestObjectBasic();

        /*
         * OPTION 2 - CURRENT SOLUTION
         * In this version, I simply leverage Unity's physics and collisions systems to be my best friend.
         * I have lots of ideas of how I could optimize this system, and may still implement some, 
         * But I also imagine that 
         * 1. many of these ideas would take far too long, 
         * 2. Unity's physics system has already set up some extra things that will outperform my checks most of the time anyways
         * 
         * So this system that simply leverages that could be notably effective
         * 
         * This system will excel with higher numbers of objects, since it will drastically cut down on the number of checks being made
         * 
         * But when all objects are nearby or the player is moving rapidly, this benefit will be reduced as the number of objects to be checked will rise
         * Additionally, this solution requires colliders for all objects, which means that background collision checking 
         * will always be running for all of these objects, which will lead to increased processing overhead on Unity's side.
         * I have all objects set as triggers, and have turned off collision between them which will hopefully decrease that overhead though
         * 
         * Also, on startup, this will be an essentially nonsensical solution requiring an almost infinitely sized sphere, 
         * so the very first check will have to be run the basic way
         * 
         * Analysis results:
         * 10 objects: 2.594141e-05s average
         * 2000 objects in cluster: 7.495193e-05s average
         * 2000 objects outside of cluster: 3.507084e-05s average
         * 2000 objects sparse: 3.609143e-05s average
         * 20000 objects in cluster: 0.0001995913s average
         * 20000 objects outside of cluster: 7.163183e-05s average
         * 20000 objects sparse: 9.157765e-05s average
         */
        FindNearestObjectOverlap(closestDistance);

        //getting the end time of this frame to determine processing time
        float after = Time.realtimeSinceStartup;

        //get the processing time for this frame
        frameProcessTime = after - before;

        //update the average processing time
        averageProcessTime = (averageProcessTime * numFramesRun + frameProcessTime) / ++numFramesRun;
    }

}
