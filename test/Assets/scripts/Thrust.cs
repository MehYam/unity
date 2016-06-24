using UnityEngine;
using System.Collections;

public sealed class Thrust : MonoBehaviour
{
    public float acceleration = 1;
    void FixedUpdate()
    {
        var direction = gameObject.transform.forward;
        direction.y = 0;

        gameObject.GetComponent<Rigidbody>().AddForce(direction * acceleration);
    }
}
