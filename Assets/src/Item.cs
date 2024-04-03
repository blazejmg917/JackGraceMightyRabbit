using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : LevelObject
{
    //since the level objects require item and bot scripts to be assigned, but not necessarily used, and they require the same functionality just with different colors
    //I decided to make them child classes of a LevelObject class that handles all required functionality, so that I have the necessary scripts but can more easily
    //access all needed files in any needed data structures
}
