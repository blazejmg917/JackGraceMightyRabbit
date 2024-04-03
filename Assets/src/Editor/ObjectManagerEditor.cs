using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.Client.BaseCommands;
using Unity.VisualScripting;

[CustomEditor(typeof(ObjectManager))]
public class ObjectManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectManager objMan = (ObjectManager)target;

        base.OnInspectorGUI();
        if (GUILayout.Button("Spawn Items"))
        {
            if (objMan.IsSceneBound())
            {
                objMan.AddRandomItemsEditor();
                EditorUtility.SetDirty(objMan);
            }
        }
        if (GUILayout.Button("Spawn Bots"))
        {
            if (objMan.IsSceneBound())
            {
                objMan.AddRandomBotsEditor();
                EditorUtility.SetDirty(objMan);
            }
        }
        if (GUILayout.Button("Clear Level"))
        {
            if (objMan.IsSceneBound())
            {
                objMan.ClearLevelObjects();
                EditorUtility.SetDirty(objMan);
            }
        }
    }
}
