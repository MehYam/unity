using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIProgressBar : MonoBehaviour
{
    public UnityEngine.UI.Image background;
    public UnityEngine.UI.Image foreground;

    public bool horizontal = true;
    public bool fillsForward = true;
    public float percent
    {
        get
        {
            Vector2 anchor = fillsForward ? 
                foreground.rectTransform.anchorMax : 
                foreground.rectTransform.anchorMin;

            var val = horizontal ? anchor.x : anchor.y;
            if (!fillsForward)
            {
                val = 1 - val;
            }
            return val;
        }
        set
        {
            value = Mathf.Clamp(value, 0, 1);
            if (foreground != null)
            {
                Vector2 anchor;
                if (fillsForward)
                {
                    anchor = foreground.rectTransform.anchorMax;
                }
                else
                {
                    value = 1 - value;
                    anchor = foreground.rectTransform.anchorMin;
                }
                if (horizontal)
                {
                    anchor.x = value;
                }
                else
                {
                    anchor.y = value;
                }
                if (fillsForward)
                {
                    foreground.rectTransform.anchorMax = anchor;
                }
                else
                {
                    foreground.rectTransform.anchorMin = anchor;
                }
            }
        }
    }
}
