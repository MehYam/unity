//#define THIS_STUFF_IS_SO_STUPID
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Main))]
public class MainEditor : Editor
{
    void Awake()
    {
#if THIS_STUFF_IS_SO_STUPID
        if (EditorPrefs.HasKey("Main_debug_info"))
        {
            var main = (Main)target;

            main.defaultPlane = EditorPrefs.GetString("Main_debug_info_default_plane");
            Debug.Log("read " + main.defaultPlane);
            main.defaultTank = EditorPrefs.GetString("Main_debug_info_default_tank_hull");
            main.defaultTurret = EditorPrefs.GetString("Main_debug_info_default_tank_turret");
            main.defaultIsPlane = EditorPrefs.GetBool("Main_debug_info_default_is_plane");
        }
#endif
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //var ship = EditorGUILayout.TextField("Ship", "BEE");
        //var hull = EditorGUILayout.TextField("Tank Hull", "tankhull0");
        //var turret = EditorGUILayout.TextField("Tank Turret", "tankturret0");
        var main = (Main) target;
        main.startWave = (int)GUILayout.HorizontalSlider(main.startWave, 0, 17);
        if (GUILayout.Button("Respawn"))
        {
            main.Debug_Respawn();
        }
#if THIS_STUFF_IS_SO_STUPID
        if (GUILayout.Button("Reload"))
        {
            Application.LoadLevel(0);

            EditorPrefs.SetString("Main_debug_info", "");
            EditorPrefs.SetString("Main_debug_info_default_plane", main.defaultPlane);
            EditorPrefs.SetString("Main_debug_info_default_tank_hull", main.defaultTank);
            EditorPrefs.SetString("Main_debug_info_default_tank_turret", main.defaultTurret);
            EditorPrefs.SetBool("Main_debug_info_default_is_plane", main.defaultIsPlane);
        }
#endif
    }
}
