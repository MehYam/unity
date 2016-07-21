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
        if (GUILayout.Button("Generate"))
        {
            mapGenerator.GenerateMap();
        }
        if (GUILayout.Button("Clear"))
        {
            //mapGenerator.ClearMap();
        }
        GUILayout.EndHorizontal();
    }
}
