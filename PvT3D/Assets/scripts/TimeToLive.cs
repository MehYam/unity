using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class TimeToLive : MonoBehaviour
{
    public float seconds = 0;

    Timer timer = new Timer();
	void Start()
    {
        timer.Start(seconds);
	}
	void Update()
    {
        if (timer.reached)
        {
            Destroy(gameObject);
        }
	}
}
