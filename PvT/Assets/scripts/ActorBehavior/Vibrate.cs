using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Vibrate : MonoBehaviour
{
    Timer _rate;
    void Awake()
    {
         _rate = new Timer(0.03f, 0.25f);
    }

    float _lastOffset = 0.1f;
	void Update()
    {
        if (_rate.reached)
        {
            transform.Translate(_lastOffset, 0, 0);

            _lastOffset = -_lastOffset;

            _rate.Start();
        }
	}
}
