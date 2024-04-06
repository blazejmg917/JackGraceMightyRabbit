# JackGraceMightyRabbit

This is my submission for the Mighty Rabbit Code Test.

Thank you so much for giving me this test to work on! I thought it was a pretty interesting one to work on with a very simple base idea and more complex optional requirements to build on.
I am excited to share my finished project with you, and I hope that it meets your expectations!

## User Guide ##

**To Set up the level**

The main way to set up the level is with the ObjectManager class.
If you select the object in the scene and look at its component in the inspector view, you'll see 4 fields and 3 buttons at the bottom under the "Spawning" header.
* NumBotsToSpawn is the number of bots to spawn randomly when you press the Spawn Bots button
* NumItemsToSpawn is the number of items to spawn randomly when you press the Spawn Items button
* SpawnOrigin is the starting point from where spawn positions are calculated
* SpawnRadius is how far away from the spawn origin an object can be spawned

* Spawn Bots will spawn random bots based on the parameters you set
* Spawn Items will spawn random items based on the parameters you set
* Clear Level will destroy all existing bots and items hooked up to the object manager


**To adjust colors in edit mode**

Just above these, under the "Colors" header, you will see 4 colors. By changing these colors in edit mode you will dynamically update the materials shared by objects in the project, manipulating their base and highlighted colors


**To Spawn new objects in game:**
* Press "I" to spawn a new item
* Press "B" to spawn a new bot
* These functions still are bound by the limits set by spawnOrigin and spawnRadius\


**To Save and load game**
* Press "S" to save the current scene to your save file (you only have one file)
* Press "L" to load the current scene from the save file (if it exists)


## Functional Goals ##

1. Complete
2. Complete
3. Complete (tested up to 100,000 items)
4. Complete (To easily update colors, select the ObjectManager object from the hierarchy, and then adjust the colors in the inspector view. The shared material colors for all of the objects will update automatically to match the colors you select.)


## Optional ##

* Unit Tests: Edit mode Unit Tests have been added to >70% line coverage across 22 tests. This tests all important functionality for the project. I Would have added play mode tests, but it felt largely redundant to do so. All the necessary testing could be done in edit mode. The only parts of scripts that weren't tested were things like null checks in Awake functions, and testing for those aren't particularly useful.
  
* XML Docs: I actually already use XML Docs in Unity coding as my own personal standard, as I think the hookup to Visual Studio is incredibly useful both to help remind myself of what functions do and to assist further developers in learning how to work within my codebases. So I added XML Docs throughout the code while originally programming it, and only had to do some small tweaks here and there. I did try to use the <see> tags more frequently in this project than I usually have, which I think is something useful that I should try to do more often in my own code. But anyway, I've added XML Docs throughout.
  
* Optimize finding nearest: I spent a considerable amount of time on this problem, and you can read a lot more about my particular processes in the oversized comment blocks I added to the player script. But for a brief overview, I tried 4 solutions to the find nearest problem: Basic, SecondPlace, Overlap, and OverlapSecondPlace, and found that OverlapSecondPlace was the most efficient through a small set of efficiency testing I ran on different types of object environments. For further details on both the methods used and the analysis done, I'd suggest reading through my comments on the player script. I'd also be more than happy to give any needed further explanations if you reach out to me
  
* Add new Items/Bots automatically on key press: This one has been completed. Pressing the "I" key at runtime spawns an item randomly within the defined spawn area set by the object manager, and pressing the "B" key at runtime does the same for spawning a bot.
  
* Read/write Item/Bot/Player state to a file and restore on launch: I completed this but tweaked it slightly so it doesn't technically perfectly match the description. Instead of restoring the level on launch, you can save and load level structures by pressing the "S" and "L" keys at runtime. This means that you *can* load your saved level right at game launch if you click right away, but it also gives you the freedom to work in a fully new level without it being overridden by your saved level every time


## Questions ##
  1. How can your implementation be optimized? I put a considerable amount of time into the optimization of my project, so I feel it's in a good place optimization-wise, but I know there are a few tricks I could do to make it run faster with enough time and analysis. For example, currently, I have a bit of a convoluted execution order in the Player script which requires multiple function calls and extra checks that are only useful for certain find nearest implementations.

     If I went through to reorder things to only fit one solution, it would definitely improve its speed. I'd also go through each of the unused optimizations I listed in the player script and analyze them to see which could actually be used to improve runtime speeds.  Lastly, I made all of these systems with an arbitrary number of objects placed in an arbitrarily sized area with an arbitrary density while a player moves around at an arbitrary speed.

      If I had a better idea of a more concrete goal for the project I could definitely rework parts of my find nearest to more effectively match that structure, I also think I could probably find some more minor optimizations with just a few more passes over my code once I've had to chance to step back from the project a bit more.

     
  3. How much time did you spend on your implementation? I spent roughly 10 hours & 15 minutes in total. The breakdown looks like this:
     * ~2:40 getting the functional implementation and key-press spawning complete.
     * ~2:35 to create the overlap optimization and implement save/load functionality.
     * ~3:10 to implement unit testing and improve my XML docs.
     * ~1:50 to implement the second-place optimizations and complete my analysis of different types of optimizations.

     
  5. What was most challenging for you? I think the most challenging part was probably just the conceptual challenge of optimizing this problem when the number of objects, position of objects, and speed of the player can all be arbitrary. I had multiple useful optimization ideas that I eneed up not using at all because they were incredibly selective in their utility, and I had to build a kind of one-size-fits-all solution.
     
     It was a very interesting problem that I spent a lot of time thinking about, especially when I was supposed to be doing other things for school (whoops), and made me really consider all of the possibilities more so than if I only had one particular problem set to think about. It was a fun exercise, and led me to create the analysis on the different optimization types that I hope you find interesting/useful!

     
  7. What else would you add to this exercise? If I could add one thing to this project it would be to add different checkers for different types of "closest" checking. Currently, I only calculate the distance between the centers of objects, rather than the absolute nearest pieces of objects. This means that in my current implementation, many factors like object size and shape are not taken into account when determining the closest object. If I wanted to resolve this problem I'd have to implement some sort of Raycasting or Spherecasting when determining closest objects.
    
     Additionally, I think this project is a very simple yet extra convoluted version of a system that determines what object you interact with when interacting with objects in a game. The closest object gets highlighted, and the game now knows that is the object to interact with. Turning this project into that would involve checking some angles for FOV and doing some ray casts to make sure the object is not blocked from the player. Additionally, adding some more in-game types of movement to the player would be good to emulate that kind of system more effectively.

     Lastly, I appreciated that this project had tasks from very different areas of development (Unit Tests->testing, XML Docs->Documentation, Optimization->backend programming, Adding Items on Key Press->a bit of gameplay programming, read/write to file->backend systems, allowing color change->making designer tools), but I think could potentially have dug a little bit deeper into some areas like tools or graphics with some more optional goals.


If you'd like to view the full initial document with my answers attached, you can click [here](UnityTest.txt)
