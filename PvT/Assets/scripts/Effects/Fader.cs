using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using PvT.Util;

public class Fader : MonoBehaviour  // this is almost the same class as Tween
{
    IFadeSetter _setter;
    void Awake()
    {
        var unityText = GetComponent<Text>();
        if (unityText != null)
        {
            _setter = new UnityTextFadeSetter(unityText);
        }
        else
        {
            var text = GetComponent<TextMesh>();
            if (text != null)
            {
                _setter = new TextMeshFadeSetter(text);
            }
            else
            {
                var sprite = GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    _setter = new SpriteRendererFadeSetter(sprite);
                }
            }
        }
    }

    interface IFadeSetter
    {
        float alpha { get; set; }
    }
    sealed class TextMeshFadeSetter : IFadeSetter
    {
        readonly TextMesh target;
        public TextMeshFadeSetter(TextMesh target) { this.target = target; }
        public float alpha
        {
            get { return target.color.a; }
            set { Util.SetAlpha(target, value); }
        }
    }
    sealed class SpriteRendererFadeSetter : IFadeSetter
    {
        readonly SpriteRenderer target;
        public SpriteRendererFadeSetter(SpriteRenderer target) { this.target = target; }
        public float alpha
        {
            get { return target.color.a; }
            set { Util.SetAlpha(target, value); }
        }
    }
    sealed class UnityTextFadeSetter : IFadeSetter
    {
        readonly Text text;
        public UnityTextFadeSetter(Text target) { this.text = target; }
        public float alpha
        {
            get { return text.color.a; }
            set { Util.SetAlpha(text, value); }
        }
    }

    sealed class FadeState
    {
        public readonly float startTime;
        public readonly float endTime;
        public readonly float startAlpha;
        public readonly float targetAlpha;
        public readonly bool  autoActivate;
        public FadeState(float startTime, float endTime, float startAlpha, float targetAlpha, bool activate) { this.startTime = startTime; this.endTime = endTime; this.startAlpha = startAlpha; this.targetAlpha = targetAlpha; this.autoActivate = activate;}
    }
    FadeState _fade;
    /// <summary>
    /// Fades the GameObject's alpha, whether it's a Sprite, TextMesh, or Text object.
    /// </summary>
    /// <param name="targetAlpha">The alpha to tween to</param>
    /// <param name="seconds">Time over which to tween</param>
    /// <param name="autoActivate">Whether to activate and deactivate the GameObject as it transitions in and out of alpha == 0</param>
    public void Fade(float targetAlpha, float seconds, bool autoActivate = true)
    {
        seconds = Mathf.Max(0.00001f, seconds); // prevent DIVZERO

        _fade = new FadeState(Time.time, Time.time + seconds, 1 - targetAlpha, targetAlpha, autoActivate);

        if (_fade.autoActivate)
        {
            gameObject.SetActive(true);
        }
        DebugUtil.Assert(_setter != null);

        Update();
    }

    void Update()
    {
        if (_fade != null)
        {
            var pctTime = (Time.time - _fade.startTime) / (_fade.endTime - _fade.startTime);
            _setter.alpha = Mathf.Lerp(_fade.startAlpha, _fade.targetAlpha, pctTime);
            if (pctTime >= 1 && _setter.alpha == 0)
            {
                if (_fade.autoActivate)
                {
                    gameObject.SetActive(false);
                }

                _fade = null;
            }
        }
    }
}
