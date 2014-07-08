using UnityEngine;
using System.Collections;
using System;

public sealed class TweenPosition : MonoBehaviour
{
    sealed class TweenState
    {
        public readonly Vector3 destination;
        public readonly float startTime;
        public readonly float time;
        public readonly Action<GameObject> onDone;
        public TweenState(Vector3 destination, float time, Action<GameObject> onDone)
        {
            this.destination = destination;
            this.startTime = Time.time;
            this.time = time;
            this.onDone = onDone;
        }
        Vector3 velocities = Vector3.zero;
        public Vector3 Update(Transform transform)
        {
            return Vector3.SmoothDamp(transform.position, destination, ref velocities, time * Consts.SMOOTH_DAMP_MULTIPLIER);
        }
    }

    TweenState _state;
    public void To(Vector3 destination, float time, Action<GameObject> onDone = null)
    {
        _state = new TweenState(destination, time, onDone);
        enabled = true;
    }

    const float FUDGE = 1.5f; // SmoothDamp seems to need a little extra time to get there.  It's not a great tween, or I'm spacing and using it incorrectly
    void Update()
    {
        if (_state != null)
        {
            transform.position = _state.Update(transform);
            if (Time.time > (_state.startTime + _state.time*FUDGE))
            {
                if (_state.onDone != null)
                {
                    _state.onDone(this.gameObject);
                }
                _state = null;
            }
        }
        if (_state == null)
        {
            enabled = false;
        }
	}
}
