using UnityEngine;
using System.Collections;

public sealed class Thrust : MonoBehaviour
{
    Actor _actor;
    void Start()
    {
        _actor = GetComponent<Actor>();
    }
    void FixedUpdate()
    {
        var direction = gameObject.transform.forward;
        direction.y = 0;

        gameObject.GetComponent<Rigidbody>().AddForce(direction * _actor.acceleration);
    }
}
