using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(TileRoomCreator))]
public sealed class TileLevelCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var main = (TileRoomCreator)target;

        GUILayout.BeginHorizontal();
        main.levelFile = (TextAsset)EditorGUILayout.ObjectField("File", main.levelFile, typeof(TextAsset), true);
        if (GUILayout.Button("Load"))
        {
            if (main.levelFile != null)
            {
                main.ParseAndGenerate(main.levelFile.text);
            }
        }
        GUILayout.EndHorizontal();

        main.size = EditorGUILayout.Vector2Field("Size", main.size);
        main.padding = EditorGUILayout.IntField("Padding", main.padding);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            main.Generate(main.size, main.padding);
        }
        if (GUILayout.Button("Clear"))
        {
            main.Clear();
        }
        GUILayout.EndHorizontal();
    }
}
