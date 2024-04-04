using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class defining the various objects within the game.
/// Their distance from the <see cref="Player"/> will be measured and the closest will be highlighted
/// </summary>
public class LevelObject : MonoBehaviour
{
    [SerializeField, Tooltip("This object's renderer. Used to highlight")] private new Renderer renderer;

    [Space]

    [SerializeField, Tooltip("this object's standard material. Used by default")] private Material baseMat;

    [SerializeField, Tooltip("this object's highlighted material. Used when close to the player")] private Material highlightMat;

    private void Awake()
    {
        //ensure that the renderer has been assigned on startup. Should always be the case, but it's a simple enough error that dummy proofing isn't a bad idea
        if (!renderer)
        {
            renderer = GetComponent<Renderer>();
        }
    }

    /// <summary>
    /// Sets whether this object is highlighted or not, and updates its renderer material to match
    /// </summary>
    /// <param name="highlighted">if this object should be highlighted or not</param>
    public void SetHighlighted(bool highlighted) {
        if(highlighted)
        {
            renderer.material = highlightMat;
        }
        else
        {
            renderer.material = baseMat;
        }
    }
}
