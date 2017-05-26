using UnityEngine;
using System.Collections;

using kaiGameUtil;

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
        static public float Angle(Vector2 point)
        {
            return Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
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
        /// <summary>
        /// Extension method on GameObject that adds a component of type T, or returns
        /// an existing if a T is already attached.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <param name="obj">The implicit "this" reference of the GameObject. This is an extension method</param>
        /// <returns>Returns the new or pre-existing component of type T</returns>
        static public T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T retval = obj.GetComponent<T>();
            if (retval == null)
            {
                retval = obj.AddComponent<T>();
            }
            return retval;
        }
    }
    static class PointSpecialization
    {
        public static Vector2 ToVector2(this Point<float> point)
        {
            return new Vector2(point.x, point.y);
        }
        public static Vector2 ToVector2(this Point<int> point)
        {
            return new Vector2(point.x, point.y);
        }
    }
}
