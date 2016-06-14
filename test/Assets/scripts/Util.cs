using UnityEngine;
using System.Collections;

namespace PvT3D.Util
{
    public static class Util
    {
        static public float DegreesRotationInZ(Vector2 point)
        {
            // This assumes our camera is looking down on the XZ plane
            return Mathf.Atan2(point.x, point.y) * Mathf.Rad2Deg;
        }
        static public float DegreesRotationInY(Vector3 point)
        {
            return Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
        }
        static public float DegreesRotationToMouseInY(Vector3 point)
        {
            // To do mouse stuff in world coordinates instead:  http://wiki.unity3d.com/index.php?title=LookAtMouse
            // I find it's more intuitive and natural to transform objects to screen coordinates instead of trying
            // to translate mouse coordinates into world space, since that's generally the intent of using mouse
            // coordinates in a top-down 2D-style game.
            var screenPoint = Camera.main.WorldToScreenPoint(point);
            return DegreesRotationInZ(Input.mousePosition - screenPoint);
        }
    }
}