using UnityEngine;

/// <summary>
/// Constants and basic utilities
/// </summary>
public static class Consts
{
    static public readonly float ACTOR_NOSE_OFFSET = -90;

    static public Quaternion GetLookAtAngle(Transform transform, Vector2 point)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg + ACTOR_NOSE_OFFSET);
    }
    static public Vector2 RotatePoint(Vector2 point, float degrees)
    {
        var radians = (degrees - ACTOR_NOSE_OFFSET) * Mathf.Deg2Rad;
        var cos = Mathf.Cos(radians);
        var sin = Mathf.Sin(radians);

        var retval = new Vector2();
        retval.x = cos * point.x - sin * point.y;
        retval.y = sin * point.x - cos * point.y;

        return retval;
    }
    static public void RemoveAllChildren(Transform transform)
    {
        Debug.Log("Destroying children: " + transform.childCount);

        var children = new GameObject[transform.childCount];
        for (var i = 0; i < transform.childCount; ++i)
        {
            children[i] = transform.GetChild(i).gameObject;
        }
        foreach (var child in children)
        {
            GameObject.Destroy(child);
        }
    }
}
