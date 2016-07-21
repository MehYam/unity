using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MapGenerator))]
public sealed class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var mapGenerator = (MapGenerator)target;
        GUILayout.BeginHorizontal();

        GUI.enabled = !mapGenerator.IsGenerated;
        if (GUILayout.Button("Generate"))
        {
            mapGenerator.GenerateMap();
        }
        GUI.enabled = !GUI.enabled;
        if (GUILayout.Button("Clear"))
        {
            mapGenerator.ClearMap();
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }
}
