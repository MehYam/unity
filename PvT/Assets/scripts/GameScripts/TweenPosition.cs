using UnityEngine;
using System.Collections;

public sealed class TweenPosition : MonoBehaviour
{
    sealed class TweenState
    {
        public readonly Vector3 destination;
        public readonly float time;
        public TweenState(Vector3 destination, float time)
        {
            this.destination = destination;
            this.time = time;
        }
        Vector3 velocities = Vector3.zero;
        public Vector3 Update(Transform transform)
        {
            return Vector3.SmoothDamp(transform.position, destination, ref velocities, time);
        }
    }

    TweenState _state;
    public void To(Vector3 destination, float time)
    {
        _state = new TweenState(destination, time);
    }

    const float EPSILON = 0.0001f;
    void Update()
    {
        if (_state != null)
        {
            transform.position = _state.Update(transform);
            if (Vector3.Distance(transform.position, _state.destination) < EPSILON)
            {
                _state = null;
            }
        }
        if (_state == null)
        {
            enabled = false;
        }
	}
}
