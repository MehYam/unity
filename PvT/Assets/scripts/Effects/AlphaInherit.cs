using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class AlphaInherit : MonoBehaviour
{
    SpriteRenderer parentRenderer;
	void Start()
    {
        if (transform.parent != null)
        {
            parentRenderer = transform.parent.GetComponent<SpriteRenderer>();
        }
	}
	
	void Update()
    {
        if (transform.parent != null)
        {
	        var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && parentRenderer != null && renderer.color.a != parentRenderer.color.a)
            {
                Util.SetAlpha(renderer, parentRenderer.color.a);
            }
        }
	}
}
