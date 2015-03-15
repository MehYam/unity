using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(LevelScript))]
public sealed class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Spawn One"))
        {
            var levelScript = (LevelScript)target;
            levelScript.SpawnOne();
        }
    }
}
