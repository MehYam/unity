using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ProgressBar))]
public class ProgressBarEditor : Editor
{
    int percent = 0;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // instead of reading and writing the percentage directly to the bar, store it locally and cast
        // it - makes the UI smoother.
        var bar = (ProgressBar)target;
        if (percent == 0)
        {
            percent = (int)(bar.percent * 100f);
        }
        percent = EditorGUILayout.IntSlider("Percent", percent, 0, 100);
        bar.percent = percent / 100f;
    }
}
