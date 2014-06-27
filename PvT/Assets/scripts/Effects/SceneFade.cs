using UnityEngine;
using System.Collections;

using PvT.Util;

public class SceneFade : MonoBehaviour
{
    public float FadeSeconds = 2;
    public bool In = false;
    public bool Animating = true;

    SpriteRenderer sprite;
    float startTime;
    void OnEnable()
    {
        startTime = Time.time;
        sprite = GetComponent<SpriteRenderer>();

        var pixels = Camera.main.pixelRect;
        sprite.transform.localScale = new Vector2(pixels.width * 2, pixels.height * 2);
    }
    void Update()
    {
        if (Animating)
        {
            var pct = (Time.time - startTime) / FadeSeconds;
            if (In)
            {
                pct = 1 - pct;
            }

            Util.SetAlpha(sprite, pct);
            if (pct > 1 || pct < 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
