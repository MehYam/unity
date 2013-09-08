using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Camera gameCamera;

    const float KEYBOARD_SPEED = 1;

        //    static private const RADIANS_TO_DEGREES:Number = 180/Math.PI;
        //static private const DEGREES_TO_RADIANS:Number = Math.PI/180;
        //static public function getDegreesRotation(deltaX:Number, deltaY:Number):Number
        //{
        //    return Math.atan2(deltaX, -deltaY) * RADIANS_TO_DEGREES;
        //}

    void Update()
    {
        // This is where we move the object.

        // Get input from the keyboard, with automatic smoothing (GetAxis instead of GetAxisRaw).
        // We always want the movement to be framerate independent, so we multiply by Time.deltaTime.
        var keyboardX = Input.GetAxisRaw("Horizontal") * KEYBOARD_SPEED * Time.deltaTime;
        var keyboardY = Input.GetAxisRaw("Vertical") * KEYBOARD_SPEED * Time.deltaTime;

        // Calculate the new position based on the above input.
        // If you want to limit the movement, you can use Mathf.Clamp
        // to limit the allowed range of newPos.x or newPos.y.
        transform.localPosition += new Vector3(keyboardX, keyboardY, 0f);

        var mouseWorld = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        var delta = transform.localPosition - mouseWorld;

        var rotation = Mathf.Atan2(delta.x, -delta.y) * Mathf.Rad2Deg;
        transform.localEulerAngles = new Vector3(0, 0, rotation);
    }
}
