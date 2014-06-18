using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    public GameObject BackgroundSprite;
    public GameObject ForegroundSprite;

    public Color HighColor;
    public Color LowColor;
    public float percent
    {
        get
        {
            return ForegroundSprite.transform.localScale.x;
        }
        set
        {
            if (ForegroundSprite != null)
            {
                ForegroundSprite.transform.localScale = new Vector3(value, 1, 1);

                var fg = ForegroundSprite.GetComponent<SpriteRenderer>();
                fg.color = Color.Lerp(LowColor, HighColor, value);
            }
        }
    }

    // implementation
    void Awake()
    {
        Layout();
    }
    void Layout()
    {
        Center(BackgroundSprite);
        Center(ForegroundSprite);
    }
    static void Center(GameObject spriteGO)
    {
        if (spriteGO != null)
        {
            var sprite = spriteGO.GetComponent<SpriteRenderer>();
            var bounds = sprite.sprite.bounds;

            sprite.transform.localPosition = new Vector3((bounds.min.x - bounds.max.x) / 2, 0);
        }
    }
}
