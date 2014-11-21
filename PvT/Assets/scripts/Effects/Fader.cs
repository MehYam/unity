using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using PvT.Util;

public class Fader : MonoBehaviour  // this is almost the same class as Tween
{
    IAlphaSetter _alphaSetter;
    void Awake()
    {
        if (GetComponent<SpriteRenderer>() != null)
        {
            _alphaSetter = new SpriteRendererAlphaSetter(GetComponent<SpriteRenderer>());
        }
        else if (GetComponent<TextMesh>() != null)
        {
            _alphaSetter = new TextMeshAlphaSetter(GetComponent<TextMesh>());
        }
        else if (GetComponent<Text>() != null)
        {
            _alphaSetter = new UnityTextAlphaSetter(GetComponent<Text>());
        }
        else if (GetComponent<CanvasGroup>() != null)
        {
            _alphaSetter = new CanvasGroupAlphaSetter(GetComponent<CanvasGroup>());
        }
        DebugUtil.Assert(_alphaSetter != null);
    }

    interface IAlphaSetter
    {
        float alpha { get; set; }
    }
    sealed class SpriteRendererAlphaSetter : IAlphaSetter
    {
        readonly SpriteRenderer target;
        public SpriteRendererAlphaSetter(SpriteRenderer target) { this.target = target; }
        public float alpha
        {
            get { return target.color.a; }
            set { Util.SetAlpha(target, value); }
        }
    }
    sealed class TextMeshAlphaSetter : IAlphaSetter
    {
        readonly TextMesh target;
        public TextMeshAlphaSetter(TextMesh target) { this.target = target; }
        public float alpha
        {
            get { return target.color.a; }
            set { Util.SetAlpha(target, value); }
        }
    }
    sealed class UnityTextAlphaSetter : IAlphaSetter
    {
        readonly Text text;
        public UnityTextAlphaSetter(Text target) { this.text = target; }
        public float alpha
        {
            get { return text.color.a; }
            set { Util.SetAlpha(text, value); }
        }
    }
    sealed class CanvasGroupAlphaSetter : IAlphaSetter
    {
        readonly CanvasGroup group;
        public CanvasGroupAlphaSetter(CanvasGroup target) { this.group = target; }
        public float alpha
        {
            get { return group.alpha; }
            set { group.alpha = value; }
        }
    }

    sealed class FadeState
    {
        public readonly float startTime;
        public readonly float endTime;
        public readonly float startAlpha;
        public readonly float targetAlpha;
        public readonly bool  autoActivate;
        public FadeState(float startTime, float endTime, float startAlpha, float targetAlpha, bool activate) 
        { 
            this.startTime = startTime; 
            this.endTime = endTime; 
            this.startAlpha = startAlpha; 
            this.targetAlpha = targetAlpha; 
            this.autoActivate = activate;
        }
    }
    FadeState _currentFade;

    /// <summary>
    /// Fades the GameObject's alpha, whether it's a Sprite, TextMesh, or Text object.
    /// </summary>
    /// <param name="targetAlpha">The alpha to tween to</param>
    /// <param name="seconds">Length of tween in seconds</param>
    /// <param name="children">Set alpha on any child objects as well</param>
    /// <param name="autoActivate">Whether to activate and deactivate the GameObject as it transitions in and out of alpha == 0</param>
    public void Fade(float targetAlpha, float seconds, bool autoActivate = true)
    {
        _currentFade = new FadeState(Time.time, Time.time + seconds, 1 - targetAlpha, targetAlpha, autoActivate);

        if (_currentFade.autoActivate)
        {
            gameObject.SetActive(true);
        }
        enabled = true;
        Update();
    }

    public float alpha { get { return _alphaSetter.alpha; } }
    public bool complete { get { return _currentFade == null; } }
    void Update()
    {
        if (_currentFade != null)
        {
            var totalTime = (_currentFade.endTime - _currentFade.startTime);
            var pctTime = totalTime > 0 ? (Time.time - _currentFade.startTime) / totalTime : 1;

            _alphaSetter.alpha = Mathf.Lerp(_currentFade.startAlpha, _currentFade.targetAlpha, pctTime);
            
            if (pctTime >= 1)
            {
                if (_alphaSetter.alpha == 0 && _currentFade.autoActivate)
                {
                    gameObject.SetActive(false);
                }

                enabled = false;
                _currentFade = null;
            }
        }
    }
}
