using UnityEngine;
using System.Collections;

public sealed class MainGUI : MonoBehaviour
{
    UnityEngine.UI.Text textField;

    void Start()
    {
        textField = GetComponent<UnityEngine.UI.Text>();

        GlobalEvent.Instance.DebugString += OnDebugString;
    }
    void OnDestroy()
    {
        GlobalEvent.Instance.DebugString -= OnDebugString;
    }
    void OnDebugString(string text)
    {
        textField.text = text;
    }
}
