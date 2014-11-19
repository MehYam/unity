using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    public GameObject BackgroundSprite;
    public GameObject ForegroundSprite;
    public GameObject BorderSprite;
    public TextMesh   Text;
    public TextMesh   TextBackground;

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
            value = Mathf.Clamp(value, 0, 1);
            if (ForegroundSprite != null)
            {
                ForegroundSprite.transform.localScale = new Vector3(value, 1, 1);

                var fg = ForegroundSprite.GetComponent<SpriteRenderer>();
                fg.color = Color.Lerp(LowColor, HighColor, value);
            }
        }
    }
    public string text
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                Text.text = value;
                TextBackground.text = value;

                Text.gameObject.SetActive(true);
            }
            else
            {
                Text.gameObject.SetActive(false);
            }
        }
    }

    // implementation
    void Awake()
    {
        Layout();
    }
    void OnEnable()
    {
        Layout();
    }
    readonly Vector2 BORDER = new Vector2(1.04f, 1.32f);
    void Layout()
    {
        Center(BackgroundSprite);
        Center(ForegroundSprite);

        if (BorderSprite != null)
        {
            BorderSprite.transform.localScale = BORDER;
        }
        Center(BorderSprite, 0.01f);
    }
    static void Center(GameObject spriteGO, float offset = 0)
    {
        if (spriteGO != null)
        {
            var sprite = spriteGO.GetComponent<SpriteRenderer>();
            var bounds = sprite.sprite.bounds;

            sprite.transform.localPosition = new Vector3((bounds.min.x - bounds.max.x) / 2 - offset/2, 0);
        }
    }
}
