using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Main))]
public class MainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //var ship = EditorGUILayout.TextField("Ship", "BEE");
        //var hull = EditorGUILayout.TextField("Tank Hull", "tankhull0");
        //var turret = EditorGUILayout.TextField("Tank Turret", "tankturret0");

        if (GUILayout.Button("Respawn"))
        {
            Main.Instance.Debug_Respawn();
        }
        if (GUILayout.Button("Reload"))
        {
            Application.LoadLevel(0);
        }
    }
}
