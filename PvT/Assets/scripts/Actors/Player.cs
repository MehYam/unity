using UnityEngine;
using System.Collections;

public sealed class Player : MonoBehaviour
{
	// Use this for initialization
    Actor _actor;
	void Start()
    {
        _actor = GetComponent<Actor>();
        if (_actor != null)
        {
            gameObject.layer = (int)Consts.CollisionLayer.FRIENDLY;
            GlobalGameEvent.Instance.FirePlayerSpawned(_actor);
        }
        else
        {
            Debug.LogError("no Actor for Player");
        }
	}

    public void FixedUpdate()
    {
        if (_actor != null)
        {
            var current = MasterInput.impl.CurrentMovementVector;
            if (current != Vector2.zero)
            {
                if (_actor.thrustEnabled)
                {
                    _actor.AddThrust(current * _actor.attrs.acceleration);
                }
            }
        }
    }
}
