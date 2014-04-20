using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    public int MaxVelocity = 10;
    public int Acceleration = 5;

    // Update is called once per frame
	void Update()
    {
        var horz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        var vel = rigidbody2D.velocity;
        if (Mathf.Abs(vel.x) > MaxVelocity)
        {
            horz = 0;
        }
        if (Mathf.Abs(vel.y) > MaxVelocity)
        {
            vert = 0;
        }
        if (horz != 0 || vert != 0)
        {
            rigidbody2D.AddForce(new Vector2(horz * Acceleration, vert * Acceleration));
        }

        if (rigidbody2D.velocity != Vector2.zero)
        {
            Debug.Log(rigidbody2D.velocity);
        }

        Vector2 lookAt;
        if (Input.GetButton("Fire1"))
        {
            // point towards the mouse when firing
            var mouse = Input.mousePosition;
            var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
            lookAt = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
        }
        else
        {
            // else, point in the direction of travel
            lookAt = rigidbody2D.velocity;
        }
        var angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
