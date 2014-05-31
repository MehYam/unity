using UnityEngine;
using System.Collections;

public sealed class Ammo_tmp : MonoBehaviour
{
    public float Acceleration;
    Vector2 _force = Vector2.zero;
	void Start()
    {
        _force = Consts.RotatePoint(new Vector2(Acceleration, 0), transform.rotation.eulerAngles.z);
	}

	void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.AddForce(_force);
	}
}
