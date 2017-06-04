using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FadeWithActorHealth : MonoBehaviour
{
    Actor _actor;
    Renderer _renderer;
	void Start()
    {
	    _actor = GetComponent<Actor>();
        _renderer = GetComponent<Renderer>();

        Debug.AssertFormat(_actor != null, "{0} has no Actor component", name);

        Fade();
    }
	
	// Update is called once per frame
    float _optimize_lastPct = 0;
	void Update()
    {
	    if (_actor != null)
        {
            Fade();
        }
	}
    void Fade()
    {
        var healthPct = _actor.healthPct;

        if (_optimize_lastPct != healthPct)
        {
            var faded = _renderer.material.color;
            faded.a = _actor.healthPct;

            _renderer.material.color = faded;
        }
    }
}
