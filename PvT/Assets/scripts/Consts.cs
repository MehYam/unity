using UnityEngine;

/// <summary>
/// Constants and basic utilities
/// </summary>
public static class Consts
{
    // Scene layers
    public enum Layer : int
    {
        MOB = 8, 
        MOB_AMMO = 9,
        FRIENDLY_AMMO = 10,
        FRIENDLY = 11,
        ENVIRONMENT = 12
    };
    static public readonly int AMMO_SORT_ORDER = -1;

    static public readonly int PixelsToUnits = 100;

    static public readonly float ACTOR_NOSE_OFFSET = -90;

    /// <summary>
    /// Flips a coin.  You can set the odds
    /// </summary>
    /// <param name="odds">The % of chance of heads</param>
    /// <returns></returns>
    static public bool CoinFlip(float odds = 0.5f)
    {
        return Random.value >= odds;
    }

    static public Quaternion GetLookAtAngle(Transform transform, Vector2 point)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg + ACTOR_NOSE_OFFSET);
    }
    static public Vector2 GetLookAtVector(float angle, float magnitude)
    {
        var vector = new Vector2(0, magnitude);
        return RotatePoint(vector, -Consts.ACTOR_NOSE_OFFSET - angle);
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
    static public float DegreesBetweenPoints(Vector2 a, Vector2 b)
    {
        return DegreesRotation(a - b);
    }
    static public float DegreesRotation(Vector2 point)
    {
        return Mathf.Atan2(point.x, point.y) * Mathf.Rad2Deg;
    }
    		// returns a signed difference between two angles useful for evolving one to the other
	static public float diffRadians(float source, float target)
	{
		var raw = target - source;
		return Mathf.Atan2(Mathf.Sin(raw), Mathf.Cos(raw));
	}
    static public float diffAngle(float source, float target)
    {
        return diffRadians(source * Mathf.Deg2Rad, target * Mathf.Deg2Rad) * Mathf.Rad2Deg;
    }
    static public Vector3 Add(Vector2 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, b.z);
    }
    static public Vector2 Add(Vector3 a, Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }
    static public Vector3 ScatterRandomly(float radius)
    {
        return new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius));
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
    static public Rect GetScreenRectInWorldCoords(Camera camera)
    {
        var pixels = camera.pixelRect;
        var screenMin = camera.ScreenToWorldPoint(Vector3.zero);
        var screenMax = camera.ScreenToWorldPoint(new Vector3(pixels.xMax, pixels.yMax));

        return new Rect(screenMin.x, screenMax.y, screenMax.x - screenMin.x, screenMax.y - screenMin.y);;
    }

    static public void Log(string fmt, params object[] args)
    {
        Debug.Log(string.Format(fmt, args));
    }

}
