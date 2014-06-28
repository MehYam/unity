using UnityEngine;
using System.Collections;

using PvT.Util;

public class AnimatedText : MonoBehaviour
{
    public float FadeRate = 1;
    public TextMesh Text;
    public TextMesh DropShadow;
    public string SortingLayerName = "UI";

    float targetAlpha = 1;
    public string text
    {
        get
        {
            return Text.text;
        }
        set
        {
            Util.SetAlpha(Text, 0);
            targetAlpha = 1;

            Text.text = value; 
            Text.gameObject.SetActive(true);
            if (DropShadow != null)
            {
                DropShadow.text = text;
            }
        }
    }
    public void Clear()
    {
        targetAlpha = 0;
    }

    void Start()
    {
        renderer.sortingLayerName = SortingLayerName;
        if (DropShadow != null)
        {
            DropShadow.renderer.sortingLayerName = SortingLayerName;
            DropShadow.renderer.sortingOrder = renderer.sortingOrder - 1;
        }
    }

    void Update()
    {
        var alpha = Text.color.a;
        if (targetAlpha != alpha)
        {
            var change = (FadeRate * Time.deltaTime);
            alpha += (alpha > targetAlpha) ? -change : change;

            Util.SetAlpha(Text, alpha);
            if (DropShadow != null)
            {
                Util.SetAlpha(DropShadow, alpha);
            }

            if (Text.color.a == 0)
            {
                Text.gameObject.SetActive(false);
            }
        }
    }
}
