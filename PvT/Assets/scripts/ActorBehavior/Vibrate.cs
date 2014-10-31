using UnityEngine;
using System.Collections;

public sealed class Vibrate : MonoBehaviour
{
    RateLimiter _rate;
    void Awake()
    {
         _rate = new RateLimiter(0.03f);
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
