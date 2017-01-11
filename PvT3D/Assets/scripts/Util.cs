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
        // To do mouse stuff in world coordinates instead:  http://wiki.unity3d.com/index.php?title=LookAtMouse
        // I find it's more intuitive and natural to transform objects to screen coordinates instead of trying
        // to translate mouse coordinates into world space, since that's generally the intent of using mouse
        // coordinates in a top-down 2D-style game.
        static public float DegreesRotationToMouseInY(Vector3 point)
        {
            var screenPoint = Camera.main.WorldToScreenPoint(point);
            return DegreesRotationInZ(Input.mousePosition - screenPoint);
        }
        static public Vector3 MouseVector(Vector3 worldPoint)
        {
            var screenPoint = Camera.main.WorldToScreenPoint(worldPoint);
            var direction = Input.mousePosition - screenPoint;

            // The screen is in X/Y, but the camera looks down on the world towards X/Z, so transpose.
            direction.z = direction.y;
            direction.y = 0;
            return direction;
        }
        static public Material GetMaterialInChildren(GameObject obj)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            return renderer != null ? renderer.material : null;
        }
    }
}
