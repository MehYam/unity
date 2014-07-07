using UnityEngine;
using System.Collections;

using PvT.Util;

public class Fader : MonoBehaviour  // this is almost the same class as Tween
{
    IFadeSetter _setter;
    void Awake()
    {
        var animatedText = GetComponent<AnimatedText>();
        if (animatedText != null)
        {
            _setter = new AnimatedTextFadeSetter(animatedText);
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
    sealed class AnimatedTextFadeSetter : IFadeSetter
    {
        readonly AnimatedText text;
        public AnimatedTextFadeSetter(AnimatedText target) { this.text = target; }
        public float alpha
        {
            get { return text.alpha; }
            set { text.alpha = value; }
        }
    }

    sealed class FadeState
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
                //Debug.Log(string.Format("Fader {0} going to sleep now.", name));
                gameObject.SetActive(false);

                _fade = null;
            }
        }
    }
}
