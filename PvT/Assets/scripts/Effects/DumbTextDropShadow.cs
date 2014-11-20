using UnityEngine;
using System.Collections;

public sealed class DumbTextDropShadow : MonoBehaviour
{
    public TextMesh fg;
    public TextMesh bg;

    public string text
    {
        set
        {
            fg.text = value;
            bg.text = value;
        }
    }
}
