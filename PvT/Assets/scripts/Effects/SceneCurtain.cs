using UnityEngine;
using System.Collections;

using PvT.Util;

public class SceneCurtain : MonoBehaviour
{
    SpriteRenderer sprite;
    void Awake()
    {
        Debug.Log("SceneCurtain.Awake");
        sprite = GetComponent<SpriteRenderer>();

        var pixels = Camera.main.pixelRect;
        sprite.transform.localScale = new Vector2(pixels.width * 2, pixels.height * 2);
    }
    public float alpha
    {
        get
        {
            return sprite.color.a;
        }
        set
        {
            gameObject.SetActive(true);
            Util.SetAlpha(sprite, value);
        }
    }

    class FadeState
    {
        public readonly float startTime;
        public readonly float endTime;
        public readonly float startAlpha;
        public readonly float targetAlpha;
        public FadeState(float startTime, float endTime, float startAlpha, float targetAlpha) { this.startTime = startTime; this.endTime = endTime;  this.startAlpha = startAlpha; this.targetAlpha = targetAlpha; }
    }
    FadeState _fade;
    void Fade(float target, float seconds)
    {
        seconds = Mathf.Max(0.00001f, seconds); // prevent DIVZERO

        _fade = new FadeState(Time.time, Time.time + seconds, alpha, target);
        gameObject.SetActive(true);
    }
    void Update()
    {
        if (_fade != null)
        {
            var pct = Time.time - _fade.startTime / (_fade.endTime - _fade.startTime);

            Debug.Log(pct);

            alpha = Mathf.Lerp(_fade.startAlpha, _fade.targetAlpha, pct);
            if (pct > 1 || pct < 0)
            {
                gameObject.SetActive(false);

                _fade = null;
            }
        }
    }
}
