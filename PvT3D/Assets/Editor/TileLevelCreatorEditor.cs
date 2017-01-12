using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(TileLevelCreator))]
public sealed class TileLevelCreatorEditor : Editor
{
    TextAsset levelFile;
    Vector2 size = new Vector2(5, 5);
    int padding = 1;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var levelCreator = (TileLevelCreator)target;

        GUILayout.BeginHorizontal();
        levelFile = (TextAsset)EditorGUILayout.ObjectField("File", levelFile, typeof(TextAsset), true);
        if (GUILayout.Button("Load"))
        {
            if (levelFile != null)
            {
                levelCreator.ParseAndGenerate(levelFile.text);
            }
        }
        GUILayout.EndHorizontal();

        size = EditorGUILayout.Vector2Field("Size", size);
        padding = EditorGUILayout.IntField("Padding", padding);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            levelCreator.Generate(size, padding);
        }
        if (GUILayout.Button("Clear"))
        {
            levelCreator.Clear();
        }
        GUILayout.EndHorizontal();
    }
}
