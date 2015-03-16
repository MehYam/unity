using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(LevelScript))]
public sealed class LevelScriptEditor : Editor
{
    [SerializeField]
    string mobType = "tankturret0";
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn One"))
        {
            var levelScript = (LevelScript)target;
            levelScript.SpawnOne(mobType);
        }
        mobType = GUILayout.TextField(mobType);
        GUILayout.EndHorizontal();
    }
}
