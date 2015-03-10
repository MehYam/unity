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

        var main = (Main) target;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear player data"))
        {
            PlayerData.ClearAll();
        }
        if (GUILayout.Button("Gain 50% XP"))
        {
            PlayerData.Instance._debug();
        }
        GUILayout.EndHorizontal();
        if (Application.isPlaying)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Respawn"))
            {
                main.Debug_Respawn();
            }
            if (GUILayout.Button("Die"))
            {
                main.game.player.GetComponent<Actor>().TakeDamage(10000);
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Add Weapon Mod"))
            {
                var actor = Main.Instance.game.player.GetComponent<Actor>();
                actor.AddWeaponModifier(new ActorType.WeaponAttrs(0.5f, 0.5f, 0.5f, 0.5f));
            }
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
