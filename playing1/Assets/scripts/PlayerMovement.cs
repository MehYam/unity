using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Camera gameCamera;

    const float KEYBOARD_SPEED = 1;
    const int LEFT_BUTTON = 0;
    const int RIGHT_BUTTON = 1;
    const int MIDDLE_BUTTON = 2;


    void Update()
    {
        // Keyboard
        var keyboardX = Input.GetAxisRaw("Horizontal") * KEYBOARD_SPEED * Time.fixedDeltaTime;
        var keyboardY = Input.GetAxisRaw("Vertical") * KEYBOARD_SPEED * Time.fixedDeltaTime;

        transform.localPosition += new Vector3(keyboardX, keyboardY, 0f);

        // Mouse
        if (Input.GetMouseButton(LEFT_BUTTON))
        {
            var mouseWorld = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            var delta = transform.localPosition - mouseWorld;

            var rotation = Mathf.Atan2(delta.x, -delta.y) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, 0, rotation);
        }
        else
        {
            transform.localEulerAngles = Vector3.zero;
        }
    }
}
