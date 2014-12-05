using UnityEngine;
using System.Collections;

public sealed class LaserGate : MonoBehaviour
{
    GameObject beam;

    // Use this for initialization
    void Start()
    {
        beam = transform.GetChild(0).gameObject;
    }

    public bool on
    {
        get { return beam.activeSelf; }
        set { beam.SetActive(value); }
    }

    public void Flicker()
    {
    }

    //IEnumerator FlickerScript()
    //{
    //    var renderer = beam.GetComponent<SpriteRenderer>();

    //}
}
