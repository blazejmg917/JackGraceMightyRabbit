using Codice.Client.BaseCommands;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField, Tooltip("the current second closest object")] private GameObject secondClosestObject;

    //the new closest object; defining this variable here to avoid repeated declaration
    private GameObject newClosestObject;

    //the new second closest object; defining this variable here to avoid repeated declaration
    private GameObject newSecondClosestObject;

    [SerializeField, Tooltip("the current distance between the player and the closest object")] private float closestDistance = float.MaxValue;
    [SerializeField, Tooltip("the current distance between the player and the second closest object")] private float secondClosestDistance = float.MaxValue;

    //the distance from the player and the closest object last time a new closest object was found
    private float lastDistance;

    //the distance from the player and the second closest object last time a new closest object was found
    private float lastSecondDistance;

    //the distance between this transform and another; defining this variable here to avoid repeated declaration
    private float distance;

    //the position the player was when they last found a new closest object. Used for second place distance calculations
    private Vector3 lastPos;

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
    /// Searches through all of the tracked <see cref="LevelObject"/>s to find the closest two to the <see cref="Player"/>. 
    /// If there is a new closest object, updates object highlighting to reflect that and starts tracking the distance from the new two closest objects
    /// Once a second place has been set, will not run a check again until the player has moved a distance from the position where they were when finding the closest object
    /// equal to half the difference between the distances of first and second place
    /// </summary>
    public void FindNearestObjectSecondPlace()
    {
        //do a sanity check to ensure that we don't get any exceptions thrown throughout
        if (levelObjects == null || levelObjects.Count <= 0)
        {
            return;
        }

        //for the second place implementation, ensure that if there is a second place object and the player has not yet moved far enough that the check is not run
        if (secondClosestObject)
        {
            float distanceTraveled = Vector3.Distance(transform.position, lastPos);
            if (distanceTraveled < (lastSecondDistance - lastDistance) / 2)
            {
                return;
            }

            //calculate the distance to the second closest object
            secondClosestDistance = Vector3.Distance(transform.position, secondClosestObject.transform.position);
        }

       

        /*
         * In this version, check every object stored in the player's list. 
         * With more time I'd consider making a sort of sorted List structure such that only object that could possible have become the new closest will be checked 
         * based on distance traveled by the player
         */
        FindNearestInGroupSecondPlace(levelObjects);
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
    /// Creates an overlapSphere around the player using the distance to the closest object as its radius, 
    /// and checks if any caught <see cref="LevelObject"/>s are closer to the <see cref="Player"/> than the current closest object.
    /// </summary>
    public void FindNearestObjectOverlapSecondPlace()
    {

        //for the second place implementation, ensure that if there is a second place object and the player has not yet moved far enough that the check is not run
        if(secondClosestObject)
        {
            float distanceTraveled = Vector3.Distance(transform.position, lastPos);
            if(distanceTraveled < (lastSecondDistance - lastDistance) / 2)
            {
                return;
            }

            //calculate the distance to the second closest object
            secondClosestDistance = Vector3.Distance(transform.position, secondClosestObject.transform.position);
        }

        //use the closest distance as the radius by default
        FindNearestObjectOverlapSecondPlace(closestDistance);
    }

    /// <summary>
    /// Creates an overlapSphere around the player with the given radius, 
    /// and checks if any caught <see cref="LevelObject"/>s are closer to the <see cref="Player"/> than the current two closest objects.
    /// </summary>
    /// <param name="radius">The radius of the sphere to create</param>
    public void FindNearestObjectOverlapSecondPlace(float radius)
    {
        //If there is no closest object to check from, this check becomes arbitrary and potentially far less efficient, so it should be passed over to the basic version
        if (!closestObject)
        {
            FindNearestObjectSecondPlace();
            return;
        }

        //Create an overlap sphere of the given radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        //check the distance to all colliders found
        FindNearestInGroupSecondPlace(colliders);
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
    /// <param name="objects">A list of all of the <see cref="LevelObject"/>s to check for a potential new closest object</param>
    private void FindNearestInGroupSecondPlace(List<GameObject> objects)
    {
        //set this to null so its easy to determine if there is any second object at all
        newSecondClosestObject = null;

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
             * 
             * Similarly, for code clarity we want to wait to the end to update the second closest,
             * even though there are no renderer updates
             */
            if (distance < closestDistance)
            {
                /*
                 * if this isn't the current closest object, 
                 * it bumps the closest object down to second place
                 */
                if (levelObject != closestObject)
                {
                    newSecondClosestObject = newClosestObject;
                    secondClosestDistance = closestDistance;
                }

                //update the closest
                newClosestObject = levelObject;
                closestDistance = distance;

            }
            /*
             * if this wasn't new new closest object, check it against second place.
             * even if a new closest object isn't found, a new second place could lead to more time without needing additional checks
             */
            else if (distance < secondClosestDistance && levelObject != closestObject)
            {
                newSecondClosestObject = levelObject;
                secondClosestDistance = distance;
            }
        }

        //now update the closest object here at the end
        UpdateClosestSecondPlace();


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
    /// Checks all <see cref="LevelObject"/>s within the specified group to see if any are closer than the current closest object
    /// </summary>
    /// <param name="objects">A list of all of the colliders of objects to check for potential new closest objects</param>
    private void FindNearestInGroupSecondPlace(Collider[] objects)
    {
        //set this to null so its easy to determine if there is any second object at all
        newSecondClosestObject = null;


        //check all objects provided
        foreach (Collider levelObject in objects)
        {
            //compare the distance between this object and the player wiht with the stored greatest distance
            distance = Vector3.Distance(transform.position, levelObject.transform.position);

            /*
             * if this is the new closest object, mark it as such but don't update everything yet, 
             * as that would both potentially alter the rest of the iteration and lead to multiple
             * unecessary operations to happen when only the final decided closest object actually
             * warrants resetting all of the other values
             * 
             * Similarly, for code clarity we want to wait to the end to update the second closest,
             * even though there are no renderer updates
             */
            if (distance < closestDistance)
            {
                /*
                 * if this isn't the current closest object, 
                 * it bumps the closest object down to second place
                 */
                if (levelObject.gameObject != closestObject)
                {
                    newSecondClosestObject = newClosestObject;
                    secondClosestDistance = closestDistance;
                }

                //update the closest
                newClosestObject = levelObject.gameObject;
                closestDistance = distance;

            }
            /*
             * if this wasn't new new closest object, check it against second place.
             * even if a new closest object isn't found, a new second place could lead to more time without needing additional checks
             */
            else if (distance < secondClosestDistance && levelObject.gameObject != closestObject)
            {
                newSecondClosestObject = levelObject.gameObject;
                secondClosestDistance = distance;
            }
        }

        //now update the closest object here at the end
        UpdateClosestSecondPlace();


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
    /// If there is a new closest <see cref="LevelObject"/>, updates the visual highlight and sets a new object to track.
    /// Additionally, sets the new second closest object and updates related fields
    /// </summary>
    private void UpdateClosestSecondPlace()
    {
        //if either of the two change, updating the recorded distances is required
        bool changedObjects = false;

        //if there is a new closest object, update visuals to match
        if (newClosestObject && newClosestObject != closestObject)
        {
            changedObjects = true;

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

        //if the second closest object has changed, update that here
        if(newSecondClosestObject != secondClosestObject)
        {
            changedObjects = true;
            secondClosestObject = newSecondClosestObject;
        }

        //if the second closest object doesn't exist anymore, set its distance to the float max for easier calculations
        if (!secondClosestObject)
        {
            secondClosestDistance = float.MaxValue;
            return;
        }

        //even if neither object has changed, if this moves us in a favorable direction such that we can extend our safe zone, do so
        if(!changedObjects)
        {
            //calculate the current and stored distance differences
            float secondDist = Vector3.Distance(secondClosestObject.transform.position, transform.position);
            float currentDistDiff = secondDist - distance;
            float lastDistDiff = lastSecondDistance - lastDistance;

            //if the current difference is greater, then we should update fields to extend our safe zone
            if(currentDistDiff > lastDistDiff)
            {
                changedObjects = true;
            }
        }

        //if something has changed (or we've manually set this to true), update the stored fields for safe zone calculations
        if(changedObjects)
        {
            lastDistance = closestDistance;
            lastSecondDistance = Vector3.Distance(secondClosestObject.transform.position, transform.position);
            lastPos = transform.position;
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
            newClosestObject = closestObject;

            /*
             * unfortunately, without looping through the whole list here, we cannot properly check on the status of the second place object or the safe zone,
             * (and doing so here would not be particularly thread-safe) so we need to turn off the safe zone to trigger a full calculation next turn if using secondplace algorithms
             */
            secondClosestObject = null;
            secondClosestDistance = float.MaxValue;
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
         * 
         * In all of these cases, the objects are half bot, half item
         */

        /*
         * OPTION 1: BASIC
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
         * OPTION 2: SECONDPLACE
         * In this form of the solution, I've taken the basic algorithm and added a bit of extra information to be tracked.
         * By also tracking the second closest object, I'm able to determine a safe zone around the player's position
         * such that the closest object cannot change unless they move outside of the zone or a new object is spawned.
         * This zone is a sphere with a radius equal to half of the difference between the distance from the closest object to the player and from the second closest to the player
         * I can prove this since that radius is the absolute minimum distance that could be traveled until the closest and second closest objects are equidistant
         * from the player (in the scenario that the 3 objects fall on a line like so: 1--P---2), and since no object is closer than the first or second object,
         * that sphere is entirely safe. 
         * 
         * This math is incredibly useful, as it allows me to complete ignore checking unless the player moves that far away from their origin point, outside of the safe zone
         * This is optimal for solutions where the player is moving slowly or in a light environment, as it will go for longer periods of time without having to rerun its checks.
         * 
         * However, with lots of objects or when moving quickly, it will run slightly slower than the basic version.
         * 
         * I was actually a bit surprised by how poorly this one fared. Granted, it *could* be an efficient solution for certain systems/games/scenarios,
         * but when covering all arbitrary solutions and receiving high rates of movement, it just couldn't keep up
         * 
         * **For these tests, I continually moved the player around. When not moving, the operation time typically falls somewhere around the .001ms-.01ms range, 
         * and drastically reduces the average time. But I wanted to test how much it would drag the system down if it was faster moving.
         * 
         * Analysis results:
         * 10 objects: 1.151814e-05s average
         * 2000 objects in cluster: 0.000557566s average
         * 2000 objects outside of cluster: 0.0008098574s average
         * 2000 sparse: 0.0003685465S average
         * 20000 objects in cluster: 0.001778917s average
         * 20000 objects outside of cluster: 0.002842159s average
         * 20000 sparse: 0.002938255s average
         */
        //FindNearestObjectSecondPlace();

        /*
         * OPTION 3: OVERLAP
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
        //FindNearestObjectOverlap();

        /*
         * Option 4: OVERLAP SECONDPLACE - CURRENT SOLUTION
         * 
         * In this version, I combine options 2 and 3 into a hybrid solution. 
         * I use overlapspheres to check for the closest objects, but still implement the safe zone checks to reduce the frequency of my calls
         * 
         * This does benefit me when moving slowly and in sparse environments especially, and reducing the need to make overlapsphere calls is very valuable,
         * but I was very surprised by just how much this sped up my system. It ran twice as fast as the base overlap version in some cases which was shocking to me, 
         * as I expected only a marginal increase in performance. However, this turned out to be the most effective method, and as such is the one I will be using in my final submission
         * The combination of the benefits from swapping to overlap and adding the secondplace check are an effective pair
         * 
         * (I will admit, with the time I had I cannot guarantee a static testing environment. 
         * There very well could have been fluctuations in memory usage/cpu performance that I was not tracking which could have affected my results.
         * With more time I would have done a more in-depth test, and probably printed to CSV files for further analysis, as I've done for projects before
         * 
         * **Once again, I am going to try to keep the player moving fairly regularly for these tests
         * Analysis results
         * 10 objects: 1.700793e-05s average
         * 2000 objects in cluster: 2.381538e-052 average
         * 2000 objects outside of cluster: 1.989095e-05s average
         * 2000 objects sparse: 2.278455e-05s average
         * 20000 objects in cluster: 4.200165e-05s average
         * 20000 objects outside of cluster: 3.614843e-05s average
         * 20000 objects sparse: 4.112445e-05s average
         */
        FindNearestObjectOverlapSecondPlace();

        //getting the end time of this frame to determine processing time
        float after = Time.realtimeSinceStartup;

        //get the processing time for this frame
        frameProcessTime = after - before;

        //update the average processing time
        averageProcessTime = (averageProcessTime * numFramesRun + frameProcessTime) / ++numFramesRun;
    }

    /*
     * A few search styles a considered but didn't implement and why:
     */


    /* 
     * Option 5: GROUPING
     * 
     * This idea was to split the objects into groups on startup based on their position, 
     * and then only check the distance of objects in the group the player was in and the adjacent groups.
     * This would have cut down on the number of objects that would need to be checked from the basic version
     * 
     * I decided not to implement this as the overlap sphere was simply a far more effective method of accomplishing the same task.
     * Additionally, this method would work very well for a game with a fixed level structure where I knew what objects would be 
     * important to have loaded into certain level segments based on the player's position,
     * But in this project where it was an arbitrary number of objects placed randomly, 
     * it would either be a far more costly operation on startup to properly split the objects into equal groups, 
     * Or I would have incredibly unbalanced groups where some are more effective than others
     * 
     * So in the end, I decided to drop this idea and simply implement the overlap sphere version instead
     */


    /*
     * Option 6: GRAPH
     * 
     * This idea was to create a graph structure such that every object was a node with dynamically created edges between them. 
     * On startup, this graph would be created between the existing objects based on their spacing from each other to make the graph, 
     * and then whenever the player moved, the objects within the graph would pass along the next object in the direction the player is moving
     * This would prevent the need for long loops through objects to compare distance, as it would be a simple set of checks along edges.
     * 
     * Ultimately, I decided not to implement this for a few reasons.
     * Firstly, while I could guarantee an effective implementation on a small set of objects, 
     * I was never able to suitably convince myself of this method's benefit over the overlap sphere version, 
     * especially given how much longer it could take to create.
     * In large, dense areas the grid would have to have an incredibly dense network of edges to work with, 
     * and since the player can be moved at incredibly high speeds, 
     * you could still end up in a situation where you have to loop through a lot of edges to get to your destinatino
     * But I think the final nail in the coffin for this method was being able to add new objects. 
     * Dynamically updating the graph at runtime to allow for new objects to be created would really be a pain, 
     * as each added object would need to be able to properly orient itself in the grid,
     * Break any Edges it interfered with, and then create new edges between itself and other objects
     * 
     * Of all the methods, if I had infinite time to work on one this would be the one I would be most interested in going back to try creating.
     * But with how much school work I've already procrastinated to put time into this project it would simply have taken too much time
     */

    /*
     * Option 7: SORTED
     * 
     * This idea was where the motivation for the second place implementations started.
     * Originally the idea was that whenever a new closest object was found, 
     * it would sort all of the existing level objects by the distance from the player when the new closest object was set
     * And then when you would go to search for a new closest object, you would loop through the list and check if that particular object's safe zone had been exited
     * If that were the case, you would now check if that object was a new closest object
     * And if it weren't the case you would instantly end execution with the knowledge that none after could be the new closest.
     * 
     * This was an interesting idea, and would have potentially been an improvement to the basic method under certain circumstances
     * But there were some pretty major flaws in this optimization being applicable to arbitrary situations
     * Firstly, this is effective when I can guarantee that far more time will be spent simply moving around small areas than actually finding new closest objects.
     * Because while this cuts down the frame by frame operation cost, sorting the list would be an O(nlog(n)) operation.
     * This meant that if I could guarantee you'd go at least log(n) frames between finding new objects I could prove this to be an effective method,
     * But when you can move the player at arbitrary speeds through an arbitrary number of objects at an arbitrary density, I couldn't be sure that would happen
     * Also, right around the time I came up with this idea I was also thinking through the overlap idea, which seemed both far simpler and a far better
     * 
     * However, I did like the idea of a safe zone, and decided that the O(1) operation cost of a single distance check at the start of a loop to at least 
     * check on second place would be a worthwhile idea, so I decided that while I would drop the SORTED option, I would keep that second place safe zone optimization at least
     */



}
