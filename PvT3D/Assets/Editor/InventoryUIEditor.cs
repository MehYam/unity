using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryUI))]
public class InventoryUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var ui = (InventoryUI)target;
        ui.slots = EditorGUILayout.IntField("slots", ui.slots);
    }
}
