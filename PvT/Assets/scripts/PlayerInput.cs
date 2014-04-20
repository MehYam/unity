using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
	// Update is called once per frame
	void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            var mouse = Input.mousePosition;
            var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
            var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
            var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        var horz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        if (horz != 0 || vert != 0)
        {
            rigidbody2D.AddForce(new Vector2(horz, vert));
        }
    }
}
