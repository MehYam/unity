using UnityEngine;
using System.Collections;

using PvT.Util;

public class AnimatedText : MonoBehaviour
{
    public TextMesh Text;
    public TextMesh DropShadow;
    public string SortingLayerName = "UI";

    public string text
    {
        get
        {
            return Text.text;
        }
        set
        {
            Text.text = value;
            if (DropShadow != null)
            {
                DropShadow.text = text;
            }
        }
    }
    void Start()
    {
        renderer.sortingLayerName = SortingLayerName;
        if (DropShadow != null)
        {
            DropShadow.renderer.sortingLayerName = SortingLayerName;
            DropShadow.renderer.sortingOrder = renderer.sortingOrder - 1;
        }
        if (text.Length > 0)
        {
            text = text;
        }
    }

    static public void FadeIn(AnimatedText textField, string text, float seconds)
    {
        var fader = textField.GetComponent<Fader>();
        textField.text = text;

        fader.Fade(1, seconds);
    }

    static public void FadeOut(AnimatedText textField, float seconds)
    {
        var fader = textField.GetComponent<Fader>();
        fader.Fade(0, seconds);
    }
}
