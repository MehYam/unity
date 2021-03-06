using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using PvT.Util;

public static class AnimatedText
{
    static public void FadeIn(Text textField, string text, float seconds)
    {
        var fader = textField.gameObject.GetOrAddComponent<Fader>();
        textField.text = text;

        fader.Fade(1, seconds);
    }

    static public void FadeOut(Text textField, float seconds)
    {
        var fader = textField.gameObject.GetOrAddComponent<Fader>();
        fader.Fade(0, seconds);
    }
}
