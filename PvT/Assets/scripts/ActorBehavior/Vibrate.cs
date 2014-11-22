using UnityEngine;
using System.Collections;

public sealed class Vibrate : MonoBehaviour
{
    Rate _rate;
    void Awake()
    {
         _rate = new Rate(0.03f, 0.25f);
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
