using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class LaserGate : MonoBehaviour
{
    GameObject _beam;
    float _originalAlpha;

    // Use this for initialization
    void Start()
    {
        _beam = transform.GetChild(0).gameObject;
        _originalAlpha = _beam.GetComponent<SpriteRenderer>().color.a;
    }

    public bool on
    {
        get { return _beam.activeSelf; }
        set { _beam.SetActive(value); }
    }

    public void Flicker(float seconds)
    {
        StartCoroutine(FlickerScript(seconds));
    }

    IEnumerator FlickerScript(float seconds)
    {
        var renderer = _beam.GetComponent<SpriteRenderer>();
        _beam.collider2D.enabled = false;

        Timer rate = new Timer(seconds);
        while (!rate.reached)
        {
            yield return new WaitForSeconds(Random.Range(0.01f, 0.2f));

            Util.SetAlpha(renderer, Random.Range(0f, 0.4f));
        }
        Util.SetAlpha(renderer, _originalAlpha);

        _beam.collider2D.enabled = true;
    }
}
