using UnityEngine;
using System.Collections;

public class testphysics : MonoBehaviour
{
    public float acceleration;

	// Update is called once per frame
	void Update()
    {
        if (transform.position.x > 0)
        {
            rigidbody2D.AddForce(new Vector2(-acceleration, 0));
        }
	}
}
