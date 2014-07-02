using UnityEngine;
using System.Collections;

using PvT.Util;

public class Fader : MonoBehaviour
{
    public float alpha
    {
        get
        {
            var asText = GetComponent<TextMesh>();
            if (asText != null)
            {
                return asText.color.a;
            }
            var asSprite = GetComponent<SpriteRenderer>();
            return asSprite.color.a;
        }
        set
        {
            gameObject.SetActive(true);
            var asText = GetComponent<TextMesh>();
            if (asText != null)
            {
                Util.SetAlpha(asText, value);
            }
            else
            {
                Util.SetAlpha(GetComponent<SpriteRenderer>(), value);
            }
        }
    }

    class FadeState
    {
        public readonly float startTime;
        public readonly float endTime;
        public readonly float startAlpha;
        public readonly float targetAlpha;
        public FadeState(float startTime, float endTime, float startAlpha, float targetAlpha) { this.startTime = startTime; this.endTime = endTime; this.startAlpha = startAlpha; this.targetAlpha = targetAlpha; }
    }
    FadeState _fade;
    public void Fade(float target, float seconds)
    {
        seconds = Mathf.Max(0.00001f, seconds); // prevent DIVZERO

        _fade = new FadeState(Time.time, Time.time + seconds, 1 - target, target);
        gameObject.SetActive(true);

        Update();
    }
    void Update()
    {
        if (_fade != null)
        {
            var pctTime = (Time.time - _fade.startTime) / (_fade.endTime - _fade.startTime);
            alpha = Mathf.Lerp(_fade.startAlpha, _fade.targetAlpha, pctTime);
            if (pctTime >= 1 && alpha == 0)
            {
                Debug.Log(string.Format("Fader {0} going to sleep now.", name));
                gameObject.SetActive(false);

                _fade = null;
            }
        }
    }
}
