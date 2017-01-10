using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(SketchupLevelCreator))]
public sealed class SketchupLevelCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var levelCreator = (SketchupLevelCreator)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            levelCreator.Generate();
        }
        if (GUILayout.Button("Clear"))
        {
            levelCreator.Clear();
        }
        GUILayout.EndHorizontal();
    }
}
