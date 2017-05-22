using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class TimeToLive : MonoBehaviour
{
    public float seconds = 0;
    public bool fadeHealthOverTime = false;

    Actor actor = null;
    Timer timer = new Timer();
	void Start()
    {
        timer.Start(seconds);

        actor = GetComponent<Actor>();
	}
	void Update()
    {
        if (timer.reached)
        {
            Destroy(gameObject);
        }
        else if (fadeHealthOverTime)
        {
            float pct = timer.remaining / seconds;
            actor.SetHealthPct(pct);
        }
	}
}
