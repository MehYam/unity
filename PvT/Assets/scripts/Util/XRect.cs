using UnityEngine;
using System.Collections;

/// <summary>
/// I reject Unity3D's incomplete Bounds and Rect types and substitute my own
/// </summary>
public class XRect
{
    static public readonly XRect zero = new XRect(0, 0, 0, 0);

    public float left;
    public float bottom;
    public float right;
    public float top;

    public float width { get { return right - left; } }
    public float height { get { return top - bottom; } }

    public Vector2 size { get { return new Vector2(left - right, top - bottom); } }
    public Vector2 min { get { return new Vector2(left, bottom); } }
    public Vector2 max { get { return new Vector2(right, top); } }

    public void Inflate(float amount)
    {
        left -= amount;
        bottom -= amount;
        right += amount;
        top += amount;
    }
    public bool Contains(float x, float y) { return left <= x && x <= right && bottom <= y && y <= top; }
    public bool Contains(Vector2 pt) { return Contains(pt.x, pt.y); }

    public XRect(float left, float bottom, float right, float top)
    {
        this.left = left;
        this.bottom = bottom;
        this.right = right;
        this.top = top;
    }
    public XRect(Vector2 min, Vector2 max)
    {
        this.left = min.x;
        this.bottom = min.y;
        this.right = max.x;
        this.top = max.y;
    }
    public XRect(XRect rhs)
    {
        this.left = rhs.left;
        this.bottom = rhs.bottom;
        this.right = rhs.right;
        this.top = rhs.top;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1}) - ({2}, {3})", left, bottom, right, top);
    }
}
